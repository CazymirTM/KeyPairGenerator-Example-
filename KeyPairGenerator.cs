// KeyPairGenerator.cs
using System;
using System.Security.Cryptography;
using System.IO;

class KeyPairGenerator
{
    static void Main()
    {
        using (var rsa = RSA.Create(2048))
        {
            var privateXml = rsa.ToXmlString(true);  // private + public
            var publicXml  = rsa.ToXmlString(false); // public only

            File.WriteAllText("private_key.xml", privateXml);
            File.WriteAllText("public_key.xml", publicXml);

            Console.WriteLine("Generated keys:");
            Console.WriteLine(" - private_key.xml (KEEP SECRET)");
            Console.WriteLine(" - public_key.xml  (embed this in the app)");
        }
    }
}
