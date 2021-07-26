using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Chroma.Hauler
{
    public class VirtualFileSystem
    {
        private Dictionary<string, ZipArchive> _mountedZips = new();

        public VfsDirectory Root { get; }

        public VirtualFileSystem()
        {
            Root = new VfsDirectory("/");
        }

        public void Mount(string zipFilePath, string vfsDirectory)
        {
            var fileStream = File.OpenRead(zipFilePath);
            var zipArchive = new ZipArchive(fileStream);

            Root.Mount(vfsDirectory.Split('/').LastOrDefault(), zipArchive);
        }
    }
}