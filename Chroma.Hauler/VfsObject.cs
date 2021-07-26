using System.Collections.Generic;
using System.Linq;

namespace Chroma.Hauler
{
    public abstract class VfsObject
    {
        public string Name { get; protected set; }
        public VfsObject Parent { get; protected set; }

        public string AbsolutePath
        {
            get
            {
                var list = new Stack<string>();
                var current = this;

                while (current.Parent != null)
                {
                    list.Push(current.Name);
                    current = current.Parent;
                }

                var path = string.Join('/', list);

                if (this is VfsDirectory && path.Any())
                    path += '/';

                return "/" + path;
            }
        }

        internal VfsObject(string name, VfsObject parent = null)
        {
            Name = name;
            Parent = parent;
        }
    }
}