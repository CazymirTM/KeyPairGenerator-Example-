using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class LicenseGenerator
{
    // ---- base64url helpers ----
    static string B64UrlEncode(byte[] data)
    {
        var s = Convert.ToBase64String(data);
        return s.Replace('+','-').Replace('/','_').TrimEnd('=');
    }

    static void Main(string[] args)
    {
        // Usage: LicenseGenerator <privateKey.xml> <minutesValid> [hwid]
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: LicenseGenerator <privateKey.xml> <minutesValid> [hwid]");
            return;
        }

        var privateKeyPath = args[0];
        var minutesValid = int.Parse(args[1]);
        var hwid = args.Length >= 3 ? args[2] : "";

        var privXml = File.ReadAllText(privateKeyPath);

        // Build a tiny payload: "exp=<unix>&role=owner&hwid=<...>"
        var now = DateTimeOffset.UtcNow;
        long exp = now.AddMinutes(minutesValid).ToUnixTimeSeconds();
        const string role = "owner";
        string payload = $"exp={exp}&role={role}&hwid={hwid}";
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        using (var rsa = RSA.Create())
        {
            rsa.FromXmlString(privXml);

            // Sign payload
            byte[] sig = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            string token = $"{B64UrlEncode(payloadBytes)}.{B64UrlEncode(sig)}";
            Console.WriteLine("TOKEN:");
            Console.WriteLine(token);

            File.WriteAllText("last_token.txt", token);
            Console.WriteLine("Saved to last_token.txt");
        }
    }
}
