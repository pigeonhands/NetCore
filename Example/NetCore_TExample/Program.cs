using NetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_TExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NetCore Loaded: {0}", NetCoreClient.Connect("127.0.0.1", 3345));
            Console.WriteLine("Value: {0}", TestExtern("Derp", "Kek"));
            Console.ReadLine();
        }

        [RemoteCall]
        static string TestExtern(string s1, string s2)
        {
            return TestExtern2(string.Format("S1: {0} | S2: {1}", s1, s2));
        }

        [RemoteMove]
        static string TestExtern2(string s)
        {
            return string.Format("Ayy Lamow: {0}", s);
        }
        
    }
}
