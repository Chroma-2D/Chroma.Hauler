using System.IO.Compression;

namespace Chroma.Hauler
{
    public class VfsMountPoint : VfsDirectory
    {
        public ZipArchive ZipArchive { get; }

        internal VfsMountPoint(ZipArchive zipArchive, string name, VfsDirectory parent)
            : base(name, parent)
        {
            ZipArchive = zipArchive;
        }

        public virtual void MapEntries()
        {
            foreach (var entry in ZipArchive.Entries)
            {
                if (entry.FullName.EndsWith('/'))
                {
                    CreateDirectory(entry.FullName.TrimEnd('/'));
                }
                else
                {
                    var idx = entry.FullName.LastIndexOf('/');
                    var fileName = entry.FullName.Substring(idx + 1, entry.FullName.Length - idx - 1);
                    var directoryPath = entry.FullName.Substring(0, idx);
                    var dir = GetDirectory(directoryPath);
                    dir.CreateFileEntry(fileName, new VfsMountedFile(entry, fileName, dir));
                }
            }
        }
    }
}