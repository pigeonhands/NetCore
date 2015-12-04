using NetCore.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public static class NetCoreClient
    {
        private static eSock.Client _client = null;

        public static bool Connect(string ip, int port)
        {
            _client = new eSock.Client();
            if (!_client.Connect(ip, port))
                return false;

            try
            {
                object[] data = _client.Send((byte)NetworkHeaders.Handshake);
                if ((NetworkHeaders)data[0] != NetworkHeaders.AcceptHandshake)
                    return false;
                string encryptionKey = (string)data[1];

                _client.Encryption.EncryptionKey = encryptionKey;
                _client.Encryption.Enabled = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static object CreateRemoteCall(string function, object[] args)
        {
            if (_client == null)
            {
                Console.WriteLine("[NetCore] Attempted RemoteCall with null client");
                return null;
            }

            if (!_client.Connected)
            {
                Console.WriteLine("[NetCore] Attempted RemoteCall without connection");
                return null;
            }

            try
            {
                object[] derp = _client.Send((byte)NetworkHeaders.RemoteCall, function, args);
                return derp[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }
    }
}