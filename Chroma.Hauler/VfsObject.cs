using System.Collections.Generic;
using System.Linq;

namespace Chroma.Hauler
{
    public abstract class VfsObject
    {
        private string _absolutePath;
        
        public string Name { get; protected set; }
        public VfsObject Parent { get; protected set; }

        public string AbsolutePath
        {
            get
            {
                if (_absolutePath == null)
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

                    _absolutePath = "/" + path;
                }
                
                return _absolutePath;
            }
        }

        internal VfsObject(string name, VfsObject parent = null)
        {
            Name = name;
            Parent = parent;
        }
    }
}