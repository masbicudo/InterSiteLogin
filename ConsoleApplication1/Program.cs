using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masb.Yai.AttributeSources;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadWriteLock_Tests.Main(Console.WriteLine);
            Console.ReadKey();
        }
    }
}
