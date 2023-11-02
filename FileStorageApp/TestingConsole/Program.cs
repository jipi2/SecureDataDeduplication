using CryptoLib;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    internal class Program
    {
      

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var keyGen = Utils.GenerateECKeysGenerator();
            AsymmetricCipherKeyPair aliceDHKeys = keyGen.GenerateKeyPair();
            AsymmetricCipherKeyPair bobDHKeys = keyGen.GenerateKeyPair();

            Utils.WritePrivateKeyToFile(aliceDHKeys.Private, "Aliceprivate.pem");
            Utils.WritePrivateKeyToFile(bobDHKeys.Private, "Bobprivate.pem");

            Utils.WritePublicKeyToFile(aliceDHKeys.Public, "Alicepublic.pem");
            Utils.WritePublicKeyToFile(bobDHKeys.Public, "Bobpublic.pem");

            DHPublicKeyParameters alicePublicKey = (DHPublicKeyParameters)aliceDHKeys.Public;
            DHPublicKeyParameters bobPublicKey = (DHPublicKeyParameters)bobDHKeys.Public;

            BigInteger aliceSharedSecret = Utils.GenerateSharedSecret(bobPublicKey, aliceDHKeys.Private);
            BigInteger bobSharedSecret = Utils.GenerateSharedSecret(alicePublicKey, bobDHKeys.Private);


            // Ensure that both parties have derived the same shared secret
            if (aliceSharedSecret.Equals(bobSharedSecret))
            {
                Console.WriteLine("Shared Secret: " + aliceSharedSecret.ToString(16));
            }
            else
            {
                Console.WriteLine("Key agreement failed: Alice and Bob have different shared secrets.");
            }

            Console.WriteLine("gata");
            Console.ReadKey();

        }
    }
}
