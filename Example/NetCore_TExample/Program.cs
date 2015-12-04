using System;
using NetCore;

namespace NetCore_TExample
{
    class Program
    {
        static int Kek = 10;
        static void Main(string[] args)
        {
            if(!NetCoreClient.Connect("127.0.0.1", 3345))
            {
                Console.WriteLine("Failed to connect to NetCore server.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Hidden value: {0}", ClassTester.Check());

            Console.ReadLine();
        }
        
    }
}
