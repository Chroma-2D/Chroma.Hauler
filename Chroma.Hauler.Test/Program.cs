using System;
using System.IO;
using System.Text;

namespace Chroma.Hauler.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var vfs = new VirtualFileSystem();
            vfs.Mount("/home/vdd/Documents/test.zip", "/blah");
            vfs.Mount("/home/vdd/Documents/test2.zip", "/blah");
            vfs.Root.PutFile<CountFile>("/blah/counter");

            using (var fs = vfs.Root.OpenFile("/blah/test/test.txt"))
            using (var sr = new StreamReader(fs))
            {
                Console.WriteLine(sr.ReadToEnd());
            }

            using (var fs = vfs.Root.OpenFile("/blah/test/gfx/nobody_here.txt"))
            using (var sr = new StreamReader(fs))
            {
                Console.WriteLine(sr.ReadToEnd());
            }

            for (var i = 0; i < 10; i++)
            {
                var value = BitConverter.ToInt32(vfs.Root.GetFile("/blah/counter").Read());
                Console.WriteLine(value);
            }
        }

        private string EnumerateDirectory(VfsDirectory directory)
        {
            var sb = new StringBuilder();

            foreach (var dir in directory.GetDirectories())
            {
                sb.Append()
            }
            
            return sb.ToString();
        }
    }
}