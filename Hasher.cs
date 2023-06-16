using System;
using System.Text;
using System.Security.Cryptography;
namespace Accountable
{
    public class Hasher
    {
        public static string Hash(string s)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 shaHash = SHA256.Create())
            {
                byte[] bytes = shaHash.ComputeHash(Encoding.UTF8.GetBytes(s));
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}
