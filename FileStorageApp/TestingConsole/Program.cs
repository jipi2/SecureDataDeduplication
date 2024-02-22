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
using System.Collections.Specialized;
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
            //string str = "sakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsakldnfk;anfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenranfk asdkfnkjasdnfjka kjasdnkjdasnf askjdnfkjasndkfn askjdnkjansdfkja kasjdnkjasdn sakjdnfkajsndfkja aksjdnkasjnd askdjnaksjnfkjas kasjdnkdajsdnsdfafasdfakjnfblnwfhgjrsiojeriugjnirukjehgbrikjfbgiourtbgiukahgenr";
            //string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            //Stream stream = new MemoryStream(Convert.FromBase64String(base64));
            //MerkleTree mt = Utils.GetMerkleTree(stream);

            //for(int i = 0; i < mt.HashTree.Count; i++)
            //{
            //    //Console.WriteLine("Level: " + mt.HashTree[i]._level);
            //    Console.WriteLine("Hash: " + Utils.ByteToHex(mt.HashTree[i]._hash));
            //}
            //Console.ReadKey();

            byte[] key = Utils.GenerateRandomBytes(32);

            string plaintextString = "This is a test";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintextString);
            string base64encString = Utils.EncryptAes(plaintextBytes, key);
            Console.WriteLine("Encrypted: " + base64encString);

            //key[1]= 0x00;
            //key = Utils.GenerateRandomBytes(32);
            byte[]cyphertextBytes = Convert.FromBase64String(base64encString);
            string decryptedStringBase64 = Utils.DecryptAes(cyphertextBytes, key);
            if(decryptedStringBase64 == null)
            {
                Console.WriteLine("Decryption failed");
                Console.ReadKey();
                return;
            }
            string decryptedString = Encoding.UTF8.GetString(Convert.FromBase64String(decryptedStringBase64));
            Console.WriteLine("Decrypted: " + decryptedString);

            Console.ReadKey();
        }
    }
}
