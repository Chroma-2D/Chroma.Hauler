using System;

namespace Chroma.Hauler
{
    public class VirtualFileSystemException : Exception
    {
        public VirtualFileSystemException(string message)
            : base(message)
        {
        }
    }
}