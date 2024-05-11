using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.KeysService
{
    public class RSAKeyService
    {
        static public RSA? rsaPrivateKey { get; set; }
        static public RSA? rsaPublicKey { get; set; }
    }
}
