using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    public class Utils
    {
        static public byte[] generateRandomByres(int length)
        {
            var randomGenerator = new SecureRandom();
            var randomBytes = new byte[length];
            randomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        static public byte[] HexToByte(string hexString)
        {
            return Hex.Decode(hexString);
        }

        static public string ByteToHex(byte[] bytes)
        {
            return Hex.ToHexString(bytes);
        }
        static public string HashTextWithSalt(string text, byte[] salt)
        {
            Encoding encoding = Encoding.UTF8;
            byte[] textBytes = encoding.GetBytes(text);

            var sha512 = new Sha512Digest();


            var combinedBytes = new byte[salt.Length + textBytes.Length];
            Array.Copy(salt, combinedBytes, salt.Length);
            Array.Copy(textBytes, 0, combinedBytes, salt.Length, text.Length);

            sha512.BlockUpdate(combinedBytes, 0, combinedBytes.Length);

            byte[] hash = new byte[sha512.GetDigestSize()];
            sha512.DoFinal(hash, 0);

            return Hex.ToHexString(hash);

        }

        static public string GetHashOfFile(Stream file)
        {

            byte[] h = new byte[64];
            var sha3_512 = new Sha3Digest(512);

            long fileSize = file.Length;
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            while((bytesRead = file.Read(buffer, 0, buffer.Length))>0)
            {
                sha3_512.BlockUpdate(buffer, 0, bytesRead);
            }

            sha3_512.DoFinal(h, 0);
            
            return Hex.ToHexString(h);
        }

        static private void GetLeaves(ref MerkleTree MT, int count)
        {
            while (count > 1)
            {
                count = 0;
                int level = MT.Levels+1;
                int n = MT.HashTree.Count;
                MerkleTree aux = new MerkleTree();


                for(int i = MT.IndexOfLevel[MT.Levels]; i<n; i+=2)
                {
                    byte[] h = new byte[32];
                    var sha3_256 = new Sha3Digest(256);
                    sha3_256.BlockUpdate(MT.HashTree[i]._hash, 0, MT.HashTree[i]._hash.Length);
                    sha3_256.BlockUpdate(MT.HashTree[i + 1]._hash, 0, MT.HashTree[i + 1]._hash.Length);
                    sha3_256.DoFinal(h, 0);
                    aux.HashTree.Add(new MTMember(level, h));
                    count++;
                }
                foreach(MTMember m in aux.HashTree)
                {
                    MT.HashTree.Add(m);
                }
                MT.IndexOfLevel.Add(MT.HashTree.Count - count);
                MT.Levels++;
            }
        }

        static public MerkleTree GetMerkleTree(Stream file)
        {
            MerkleTree MT = new MerkleTree();
            long fileSize = file.Length;
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            int count = 0;
            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] h = new byte[32];
                var sha3_256 = new Sha3Digest(256);
                sha3_256.BlockUpdate(buffer, 0, bytesRead);
                sha3_256.DoFinal(h, 0);
                MT.HashTree.Add(new MTMember(0, h));
                count++;
            }
            if (count % 2 != 0)
            {
                MT.HashTree.Add(MT.HashTree[0]);
                count++;
            }
            MT.IndexOfLevel.Add(0);
            GetLeaves(ref MT, count);

            return MT;
        }

        static public AsymmetricCipherKeyPair GenerateRSAKeyPair()
        {
            int modulus = 2048;
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            g.Init(new KeyGenerationParameters(new SecureRandom(), modulus));
            AsymmetricCipherKeyPair keys = g.GenerateKeyPair();

            return keys;
        }

        static public void WritePrivateKeyToFile(AsymmetricKeyParameter privKey, string privFileName)
        {
            TextWriter textWriterPrivate = new StringWriter();
            PemWriter pemWriterPrivate = new PemWriter(textWriterPrivate);
            pemWriterPrivate.WriteObject(privKey);
            File.WriteAllText(privFileName, textWriterPrivate.ToString());
        }

        static public void WritePublicKeyToFile(AsymmetricKeyParameter pubKey, string pubFileName)
        {
            TextWriter textWriterPrivate = new StringWriter();
            PemWriter pemWriterPrivate = new PemWriter(textWriterPrivate);
            pemWriterPrivate.WriteObject(pubKey);
            File.WriteAllText(pubFileName, textWriterPrivate.ToString());
        }

        static public AsymmetricKeyParameter ReadRSAPrivateKeyFromFile(string privFileName)
        {
            TextReader textReaderPrivate = new StringReader(File.ReadAllText(privFileName));
            PemReader pemReaderPrivate = new PemReader(textReaderPrivate);
            AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReaderPrivate.ReadObject();
            return keyPair.Private;
        }

        static public AsymmetricKeyParameter ReadRSAPublicKeyFromFile(string pubFileName)
        {
            TextReader textReaderPublic = new StringReader(File.ReadAllText(pubFileName));
            PemReader pemReaderPublic = new PemReader(textReaderPublic);
            AsymmetricKeyParameter pubKey = (AsymmetricKeyParameter)pemReaderPublic.ReadObject();
            return pubKey;
        }   

        static public IAsymmetricCipherKeyPairGenerator GenerateECKeysGenerator()
        {
            var generator = new DHParametersGenerator();
            generator.Init(512, 30, new SecureRandom());
            DHParameters parameters = generator.GenerateParameters();
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);

            return keyGen;
        }

        static public BigInteger GenerateSharedSecret(DHPublicKeyParameters pubKey, AsymmetricKeyParameter privKey)
        {
            var keyAgreement = AgreementUtilities.GetBasicAgreement("DH");
            keyAgreement.Init(privKey);
            return keyAgreement.CalculateAgreement(pubKey);
            

            //Exemplu de folosire DH
            //Console.WriteLine("Hello World!");

            //var keyGen = Utils.GenerateECKeysGenerator();
            //AsymmetricCipherKeyPair aliceDHKeys = keyGen.GenerateKeyPair();
            //AsymmetricCipherKeyPair bobDHKeys = keyGen.GenerateKeyPair();

            //Utils.WritePrivateKeyToFile(aliceDHKeys.Private, "Aliceprivate.pem");
            //Utils.WritePrivateKeyToFile(bobDHKeys.Private, "Bobprivate.pem");

            //Utils.WritePublicKeyToFile(aliceDHKeys.Public, "Alicepublic.pem");
            //Utils.WritePublicKeyToFile(bobDHKeys.Public, "Bobpublic.pem");

            //DHPublicKeyParameters alicePublicKey = (DHPublicKeyParameters)aliceDHKeys.Public;
            //DHPublicKeyParameters bobPublicKey = (DHPublicKeyParameters)bobDHKeys.Public;

            //BigInteger aliceSharedSecret = Utils.GenerateSharedSecret(bobPublicKey, aliceDHKeys.Private);
            //BigInteger bobSharedSecret = Utils.GenerateSharedSecret(alicePublicKey, bobDHKeys.Private);


            //// Ensure that both parties have derived the same shared secret
            //if (aliceSharedSecret.Equals(bobSharedSecret))
            //{
            //    Console.WriteLine("Shared Secret: " + aliceSharedSecret.ToString(16));
            //}
            //else
            //{
            //    Console.WriteLine("Key agreement failed: Alice and Bob have different shared secrets.");
            //}

            //Console.WriteLine("gata");
            //Console.ReadKey();
        }
    }
}
