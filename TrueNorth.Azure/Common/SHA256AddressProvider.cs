using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TrueNorth.Azure.Common
{
    public class SHA256AddressProvider 
    {
        public string GenerateHash(System.IO.Stream stream)
        {
            return generateHash(stream, true);
        }

        private string generateHash(System.IO.Stream stream, bool rewind)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(stream);
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                if (rewind) stream.Position = 0;
                return hash;
            }
        }
    }
}
