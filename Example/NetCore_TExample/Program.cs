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
            Console.WriteLine("Value: {0}", TestExtern());
            Console.ReadLine();
        }

        [RemoteCall]
        static int TestExtern()
        {
            return 1337;
        }

        
    }
}
