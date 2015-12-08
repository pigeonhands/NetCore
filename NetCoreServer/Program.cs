using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NetCore.Networking;
using System.Text;
using NetCore;

namespace NetCoreServer
{
    class Program
    {
        static Dictionary<string, MethodInfo> LoadedFunctions = new Dictionary<string, MethodInfo>();
        static eSock.Server _server;
        static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo("Modules");
            if (!di.Exists)
                di.Create();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("NetCore Server - BahNahNah");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            foreach (FileInfo fi in di.GetFiles("*.ncm"))
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(fi.FullName);
                    foreach (Type t in asm.GetTypes())
                    {

                        foreach (MethodInfo mi in t.GetMethods())
                        {
                            if (!Attribute.IsDefined(mi, typeof(RemoteCallAttribute)))
                                continue;
                            string name = string.Format("{0}.{1}", t.FullName, mi.Name);
                            string hash = Hashing.SHA(name);
                            if (LoadedFunctions.ContainsKey(hash))
                            {
                                Console.WriteLine("Duplicate name: {0}", name);
                            }
                            else
                            {
                                LoadedFunctions.Add(hash, mi);
                                Console.WriteLine("Loaded {0}", name);
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    string errorMessage = sb.ToString();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(errorMessage);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Loaded {0} remote functions.", LoadedFunctions.Count);
            Console.WriteLine();

            int port = 0;
            Console.ResetColor();
            Console.Write("Listening port (default 3345): ");


            if (!int.TryParse(Console.ReadLine(), out port))
                port = 3345;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Starting on port {0}...", port);
            _server = new eSock.Server();

            _server.OnDataRetrieved += _server_OnDataRetrieved;

            if (!_server.Start(port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to start on port {0}, press enter to exit.", port);
                Console.ReadLine();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Server started!");
            Console.WriteLine();
            Console.ResetColor();

            while (true)
                Console.ReadLine();

        }

        private static void _server_OnDataRetrieved(eSock.Server sender, eSock.Server.eSockClient client, object[] data)
        {
            try
            {
                NetworkHeaders header = (NetworkHeaders)data[0];

                if (header == NetworkHeaders.Handshake)
                {
                    string key = Guid.NewGuid().ToString();
                    client.Send((byte)NetworkHeaders.AcceptHandshake, key);
                    client.Encryption.EncryptionKey = key;
                    client.Encryption.Enabled = true;
                    return;
                }

                if (header == NetworkHeaders.RemoteCall)
                {
                    string function = (string)data[1];

                    if (!LoadedFunctions.ContainsKey(function))
                    {
                        Console.WriteLine("Invalid call ({0})", function);
                        client.Send(null);
                        return;
                    }

                    object[] args = (object[])data[2];

                    object result = LoadedFunctions[function].Invoke(null, args);
                    client.Send(result);
                    Console.WriteLine("Function Call ({0}) Value={1}", function, result);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    client.Send(null);
                }
                catch { }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", ex.Message);
                Console.ResetColor();
            }
        }
    }
}