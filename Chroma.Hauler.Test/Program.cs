using System;
using System.IO;

namespace Chroma.Hauler.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var vfs = new VirtualFileSystem();
            vfs.Mount("/home/vdd/Documents/test.zip", "/blah");
            vfs.Mount("/home/vdd/Documents/test2.zip", "/blah");


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
        }
    }
}