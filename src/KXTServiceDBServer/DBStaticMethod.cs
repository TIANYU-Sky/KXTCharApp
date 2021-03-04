using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace KXTServiceDBServer
{
    public class DBStaticMethod
    {
        public static string SHA256(string pw, string token)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] hash = Encoding.UTF8.GetBytes(pw + token);
            hash = sha256.ComputeHash(hash);
            return BitConverter.ToString(hash).Replace("-", "");
        }
        public static string GetToken()
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = Guid.NewGuid().ToByteArray();
            hash = md5.ComputeHash(hash);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
