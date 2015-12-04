using System;
using NetCore;

namespace NetCore_TExample
{
    class Program
    {
        static void Main(string[] args)
        {
            if(!NetCoreClient.Connect("127.0.0.1", 3345))
            {
                Console.WriteLine("Failed to connect to netcore server.");
                Console.ReadLine();
                return;
            }

            int number = 10;
            Console.WriteLine("Original Number: {0}", number);
            Console.WriteLine("Processed Number: {0}", ProtectedMethod(number));
            Console.ReadLine();
        }

        [RemoteCall]
        static int ProtectedMethod(int num)
        {
            return XOR(num, num * num);
        }

        [RemoteMove]
        static int XOR(int num1, int num2)
        {
            return num1 ^ num2;
        }
        
    }
}
