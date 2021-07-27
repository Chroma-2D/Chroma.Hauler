using System;

namespace Chroma.Hauler.Test
{
    public class CountFile : VfsFile
    {
        private int _count;
        
        public CountFile(string name, VfsObject parent) 
            : base(name, parent)
        {
        }

        public override byte[] Read()
        {
            return BitConverter.GetBytes(_count++);
        }
    }
}