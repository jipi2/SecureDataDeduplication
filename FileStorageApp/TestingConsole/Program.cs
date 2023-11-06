using CryptoLib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    internal class Program
    {
        static public DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(256, 30, new SecureRandom());
            return generator.GenerateParameters();
        }

        static public string GetG(DHParameters parameters)
        {
            return parameters.G.ToString();
        }

        static public string GetP(DHParameters parameters)
        {
            return parameters.P.ToString();
        }

        
        static public AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        // This returns A
        static public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null)
            {
                return dhPublicKeyParameters.Y.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }
        // This returns a
        static public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null)
            {
                return dhPrivateKeyParameters.X.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        static public BigInteger ComputeSharedSecret(string A, AsymmetricKeyParameter bPrivateKey, DHParameters internalParameters)
        {
            var importedKey = new DHPublicKeyParameters(new BigInteger(A), internalParameters);
            var internalKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgree.Init(bPrivateKey);
            return internalKeyAgree.CalculateAgreement(importedKey);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Incepem");

            DHParameters parameters = Utils.GenerateParameters();

            string G = Utils.GetG(parameters);
            string P = Utils.GetP(parameters);

            DHParameters parameters2 = Utils.GenerateParameters(G, P);

            AsymmetricCipherKeyPair bobKeys = Utils.GenerateDFKeys(parameters);
            AsymmetricCipherKeyPair aliceKeys = Utils.GenerateDFKeys(parameters2);

            string bobPublicKey = Utils.GetPublicKey(bobKeys);
            string alicePublicKey = Utils.GetPublicKey(aliceKeys);

            string bobPrivString = Utils.GetPemAsString(bobKeys.Private);
            string alicePrivString = Utils.GetPemAsString(aliceKeys.Private);
                
            AsymmetricKeyParameter aux = (AsymmetricKeyParameter)Utils.ReadPrivateKeyFromPemString(bobPrivString);
            AsymmetricKeyParameter aux2 = (AsymmetricKeyParameter)Utils.ReadPrivateKeyFromPemString(alicePrivString);

            BigInteger secretBob = Utils.ComputeSharedSecret(alicePublicKey, aux, parameters2);
            BigInteger secretAlice =Utils.ComputeSharedSecret(bobPublicKey, aux2, parameters2);

            if (secretBob.Equals(secretAlice))
                Console.WriteLine("The secret is oke");
            else
                Console.WriteLine("Error secret");

            Console.WriteLine(Utils.GetPemAsString( bobKeys.Private));
           

            Console.Read();
        }
    }
}
