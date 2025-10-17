using System;
using System.Security.Cryptography;
using System.Text;

public class TokenVerificationResult
{
    public bool Valid { get; set; }
    public string Reason { get; set; }
    public long Exp { get; set; }
    public string Role { get; set; }
    public string Hwid { get; set; }
}

public static class TokenVerifier
{
    static byte[] B64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    static (long exp, string role, string hwid, string error) ParsePayload(string payload)
    {
        long exp = 0; string role = null; string hwid = null;
        var parts = payload.Split('&');
        foreach (var p in parts)
        {
            var kv = p.Split(new[] { '=' }, 2);
            if (kv.Length != 2) continue;
            var k = kv[0]; var v = kv[1];
            if (k == "exp") long.TryParse(v, out exp);
            else if (k == "role") role = v;
            else if (k == "hwid") hwid = v;
        }
        if (exp == 0) return (0, null, null, "Missing exp");
        if (string.IsNullOrEmpty(role)) return (0, null, null, "Missing role");
        return (exp, role, hwid, null);
    }

    public static TokenVerificationResult VerifyToken(string token, string publicKeyXml, string expectedRole, string currentHwid = null)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return new TokenVerificationResult { Valid = false, Reason = "Malformed token" };

            var payloadBytes = B64UrlDecode(parts[0]);
            var sigBytes = B64UrlDecode(parts[1]);

            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKeyXml);
                bool ok = rsa.VerifyData(payloadBytes, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                if (!ok) return new TokenVerificationResult { Valid = false, Reason = "Invalid signature" };
            }

            var payloadStr = Encoding.UTF8.GetString(payloadBytes);
            var (exp, role, hwid, err) = ParsePayload(payloadStr);
            if (err != null) return new TokenVerificationResult { Valid = false, Reason = err };

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now >= exp) return new TokenVerificationResult { Valid = false, Reason = "Token expired" };
            if (role != expectedRole) return new TokenVerificationResult { Valid = false, Reason = "Wrong role" };
            if (!string.IsNullOrEmpty(hwid) && !string.IsNullOrEmpty(currentHwid) && hwid != currentHwid)
                return new TokenVerificationResult { Valid = false, Reason = "HWID mismatch" };

            return new TokenVerificationResult { Valid = true, Exp = exp, Role = role, Hwid = hwid ?? "" };
        }
        catch (Exception ex)
        {
            return new TokenVerificationResult { Valid = false, Reason = "Exception: " + ex.Message };
        }
    }
}
