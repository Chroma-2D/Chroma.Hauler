using System;
using System.IO;

namespace Chroma.Hauler
{
    public abstract class VfsFile : VfsObject
    {
        protected VfsFile(string name, VfsObject parent)
            : base(name, parent)
        {
        }

        public virtual Stream Open()
        {
            throw new NotSupportedException("This file object does not support data as a stream.");
        }

        public virtual byte[] Read()
        {
            throw new NotSupportedException("This file object does not support data as an array.");
        }
    }
}