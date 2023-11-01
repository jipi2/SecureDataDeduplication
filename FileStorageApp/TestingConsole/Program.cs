using CryptoLib;
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

            Stream streamFile = new FileStream("C:\\Users\\Jipi\\Desktop\\test.txt", FileMode.Open);
            Console.WriteLine(streamFile.Length);
            //string hash = CryptoLib.Utils.GetHashOfFile(streamFile);
            //Console.WriteLine(hash);

            //streamFile = new FileStream("C:\\Users\\Jipi\\Desktop\\test.txt", FileMode.Open);
            MerkleTree MT = CryptoLib.Utils.GetMerkleTree(streamFile);
            foreach (MTMember b in MT.HashTree)
            {
                Console.WriteLine("Level: "+b._level.ToString()+" -> "+CryptoLib.Utils.ByteToHex(b._hash));
            }

            Console.ReadKey();
        }
    }
}
