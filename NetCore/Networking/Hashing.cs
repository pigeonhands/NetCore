using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetCore.Networking
{
    public class Hashing
    {
        public static string SHA(string input)
        {
            using (SHA256 sha = new SHA256CryptoServiceProvider())
            {
                byte[] bPl = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bPl);
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach(byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
