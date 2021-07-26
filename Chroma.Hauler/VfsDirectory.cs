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
            var dir = GetDirectoryAtPath(Path.GetDirectoryName(relativePath));
            return dir.Get<VfsFile>(relativePath.Split('/').Last()).Open();
        }

        public virtual VfsDirectory CreateDirectory(string name)
        {
            if (!Children.ContainsKey(name))
                Children.Add(name, new());

            var directory = new VfsDirectory(name, this);

            Children[name].Push(directory);
            return directory;
        }

        public virtual VfsDirectory CreateDirectoryPath(string relativePath)
        {
            var directories = relativePath.Split('/').ToList();
            directories.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            var current = this;

            foreach (var segment in directories)
            {
                if (!current.HasChild(segment))
                    current.CreateDirectory(segment);

                if (current.Has<VfsFile>(segment))
                    throw new VirtualFileSystemException(
                        $"'{AbsolutePath}{segment}' already exists in the provided path and is not a directory.");

                current = (VfsDirectory)current.Children[segment].Peek();
            }

            return current;
        }

        public virtual VfsDirectory GetDirectoryAtPath(string relativePath)
        {
            var directories = relativePath.Split('/').ToList();
            directories.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            
            var current = this;

            foreach (var segment in directories)
            {
                if (!current.Has<VfsDirectory>(segment))
                    throw new VirtualFileSystemException(
                        $"'{AbsolutePath}{relativePath.TrimStart('/')}' does not exist in the file system.");

                current = (VfsDirectory)current.Children[segment].Peek();
            }

            return current;
        }

        public virtual VfsFile CreateFile(string name, VfsFile file)
        {
            if (!Children.ContainsKey(name))
                Children.Add(name, new());

            Children[name].Push(file);
            return file;
        }

        public virtual VfsMountPoint Mount(string name, ZipArchive archive)
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

                vfsDirectory.Merge(mountPoint);
            }

            return mountPoint;
        }

        public virtual void Unmount(string name)
        {
            if (!Has<VfsMountPoint>(name))
                throw new VirtualFileSystemException($"Mount point '{AbsolutePath}{name}' does not exist.");

            if (!Children[name].TryPop(out _))
                Children.Remove(name);
        }

        public virtual void RemoveDirectory(string name)
        {
            if (!Has<VfsDirectory>(name))
                throw new VirtualFileSystemException($"Directory '{AbsolutePath}{name}' does not exist.");

            Children.Remove(name);
        }

        public virtual void RemoveFile(string name)
        {
            if (!Has<VfsFile>(name))
                throw new VirtualFileSystemException($"File '{AbsolutePath}{name}' does not exist.");

            Children.Remove(name);
        }

        public virtual bool HasChild(string name)
            => Children.ContainsKey(name);

        public virtual bool Has<T>(string name) where T : VfsObject
            => Children.ContainsKey(name) && Children[name].Peek() is T;

        internal void Merge(VfsDirectory other)
        {
            foreach (var child in other.Children)
            {
                if (HasChild(child.Key))
                {
                    for (var i = child.Value.Count - 1; i >= 0; i--)
                    {
                        if (Children[child.Key].Peek() is VfsDirectory ours && child.Value.ElementAt(i) is VfsDirectory theirs)
                        {
                            ours.Merge(theirs);
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
        
        private T Get<T>(string name) where T : VfsObject
        {
            if (!Has<T>(name))
                throw new VirtualFileSystemException($"'{AbsolutePath}{name}' does not exist.");

            return Children[name].Peek() as T;
        }
    }
}