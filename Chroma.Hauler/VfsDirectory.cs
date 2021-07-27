using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Chroma.Hauler
{
    public class VfsDirectory : VfsObject
    {
        protected Dictionary<string, Stack<VfsObject>> Children { get; } = new();

        public bool IsRoot => Parent == null;

        internal VfsDirectory(string name, VfsDirectory parent = null)
            : base(name, parent)
        {
        }

        public string[] GetDirectories()
        {
            var list = new List<string>();

            foreach (var child in Children)
            {
                if (!child.Value.Any())
                    continue;

                if (child.Value.Peek() is VfsDirectory dir)
                    list.Add(dir.AbsolutePath);
            }

            return list.ToArray();
        }

        public Stream OpenFile(string relativePath)
        {
            var dir = GetDirectory(Path.GetDirectoryName(relativePath));
            return dir.Get<VfsFile>(relativePath.Split('/').Last()).Open();
        }

        public virtual VfsDirectory CreateDirectory(string relativePath)
        {
            var directories = relativePath.Split('/').ToList();
            directories.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            var current = this;

            foreach (var segment in directories)
            {
                if (!current.HasChild(segment))
                    current.CreateDirectoryEntry(segment);

                if (current.Has<VfsFile>(segment))
                {
                    throw new VirtualFileSystemException(
                        $"'{AbsolutePath}{segment}' already exists in the provided path and is not a directory."
                    );
                }


                current = (VfsDirectory)current.Children[segment].Peek();
            }

            return current;
        }

        public virtual VfsDirectory GetDirectory(string relativePath)
        {
            var directories = relativePath.Split('/').ToList();
            directories.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            var current = this;

            foreach (var segment in directories)
            {
                if (!current.Has<VfsDirectory>(segment))
                {
                    throw new VirtualFileSystemException(
                        $"'{AbsolutePath}{relativePath.TrimStart('/')}' does not exist in the file system."
                    );
                }

                current = (VfsDirectory)current.Children[segment].Peek();
            }

            return current;
        }

        public virtual VfsFile GetFile(string relativePath)
        {
            var targetDirPath = Path.GetDirectoryName(relativePath);
            var targetFileName = Path.GetFileName(relativePath);

            var directory = GetDirectory(targetDirPath);

            if (!directory.Has<VfsFile>(targetFileName))
            {
                throw new VirtualFileSystemException(
                    $"'{directory.AbsolutePath}{targetFileName}' does not exist."
                );
            }

            return directory.Get<VfsFile>(targetFileName);
        }

        public virtual T PutFile<T>(string relativePath) where T : VfsFile
        {
            var targetDirPath = Path.GetDirectoryName(relativePath);
            var targetFileName = Path.GetFileName(relativePath);

            var directory = GetDirectory(targetDirPath);

            var file = (T)Activator.CreateInstance(typeof(T), new object[] {targetFileName, directory});
            directory.CreateFileEntry(targetFileName, file);

            return file;
        }
        
        protected virtual void Remove(string relativePath)
        {
            var targetDirPath = Path.GetDirectoryName(relativePath);
            var targetName = Path.GetFileName(relativePath);

            var directory = GetDirectory(targetDirPath);
            directory.RemoveEntry(targetName);
        }

        public virtual VfsMountPoint Mount(string relativePath, ZipArchive archive)
        {
            var targetDirectory = Path.GetFileName(relativePath);
            var dir = CreateDirectory(relativePath);
                
            var mountPoint = ((VfsDirectory)dir.Parent).MountEntry(targetDirectory, archive);
            mountPoint.MapEntries();
            
            return mountPoint;
        }

        public virtual bool HasChild(string name)
            => Children.ContainsKey(name);

        public virtual bool Has<T>(string name) where T : VfsObject
            => Children.ContainsKey(name) && Children[name].Count > 0 && Children[name].Peek() is T;
        
        internal virtual VfsDirectory CreateDirectoryEntry(string name)
        {
            if (!Children.ContainsKey(name))
                Children.Add(name, new());

            var directory = new VfsDirectory(name, this);

            Children[name].Push(directory);
            return directory;
        }
        
        internal virtual void RemoveEntry(string name)
        {
            if (!HasChild(name))
                throw new VirtualFileSystemException($"Directory '{AbsolutePath}{name}' does not exist.");

            if (!Children[name].TryPop(out _))
                Children.Remove(name);
        }
        
        internal virtual VfsMountPoint MountEntry(string name, ZipArchive archive)
        {
            if (!Children.ContainsKey(name))
                Children.Add(name, new());

            var mountPoint = new VfsMountPoint(archive, name, this);

            if (!Children[name].Any())
            {
                Children[name].Push(mountPoint);
            }
            else
            {
                if (!(Children[name].Peek() is VfsDirectory vfsDirectory))
                    throw new VirtualFileSystemException($"Can only mount into directories.");

                vfsDirectory.MergeDirectories(mountPoint);
            }

            return mountPoint;
        }

        internal virtual VfsFile CreateFileEntry(string name, VfsFile file)
        {
            if (!Children.ContainsKey(name))
                Children.Add(name, new());

            Children[name].Push(file);
            return file;
        }

        private T Get<T>(string name) where T : VfsObject
        {
            if (!Has<T>(name))
                throw new VirtualFileSystemException($"'{AbsolutePath}{name}' does not exist.");

            return Children[name].Peek() as T;
        }
        
        private void MergeDirectories(VfsDirectory other)
        {
            foreach (var child in other.Children)
            {
                if (HasChild(child.Key))
                {
                    for (var i = child.Value.Count - 1; i >= 0; i--)
                    {
                        if (Children[child.Key].Peek() is VfsDirectory ours &&
                            child.Value.ElementAt(i) is VfsDirectory theirs)
                        {
                            ours.MergeDirectories(theirs);
                        }
                        else
                        {
                            Children[child.Key].Push(child.Value.ElementAt(i));
                        }
                    }
                }
                else
                {
                    Children.Add(child.Key, child.Value);
                }
            }
        }
    }
}