using System.IO;
using System.IO.Compression;

namespace Chroma.Hauler
{
    public class VfsMountedFile : VfsFile
    {
        private VfsMountPoint ParentMountPoint => Parent as VfsMountPoint;
        private ZipArchiveEntry ZipEntry { get; }

        internal VfsMountedFile(ZipArchiveEntry zipEntry, string name, VfsDirectory parent)
            : base(name, parent)
        {
            ZipEntry = zipEntry;
        }

        public override Stream Open()
        {
            return ZipEntry.Open();
        }
    }
}