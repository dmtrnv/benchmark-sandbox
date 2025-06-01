using System.Security.Cryptography;

namespace BenchmarkSandbox.Utils;

public static class Encryptor
{
    public static byte[] EncryptAes(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        
        var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);
        
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        
        cryptoStream.Write(data);
        cryptoStream.FlushFinalBlock();
        
        return memoryStream.ToArray();
    }
    
    public static byte[] DecryptAes(byte[] data, byte[] key)
    {
        using var memoryStream = new MemoryStream(data);

        var iv = new byte[16];
        memoryStream.ReadExactly(iv, 0, iv.Length);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        
        var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var resultStream = new MemoryStream();
        
        cryptoStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    public static byte[] EncryptRsa(byte[] data, RSAParameters key)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(key);
        
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);   
    }
    
    public static byte[] DecryptRsa(byte[] data, RSAParameters key)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(key);
        
        return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);   
    }
    
    public static byte[] EncryptDes(byte[] data, byte[] key, byte[] iv)
    {
        using var des = DES.Create();
        des.Key = key;
        des.IV = iv;

        using var encryptor = des.CreateEncryptor();
        
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }
    
    public static byte[] DecryptDes(byte[] data, byte[] key, byte[] iv)
    {
        using var des = DES.Create();
        des.Key = key;
        des.IV = iv;

        using var decryptor = des.CreateDecryptor();
        
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
    
    public static byte[] EncryptTripleDes(byte[] dataToEncrypt, byte[] key, byte[] iv)
    {
        using var tripleDes = TripleDES.Create();
        tripleDes.Key = key;
        tripleDes.IV = iv;

        using var encryptor = tripleDes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }
    
    public static byte[] DecryptTripleDes(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using var tripleDes = TripleDES.Create();
        tripleDes.Key = key;
        tripleDes.IV = iv;

        using var decryptor = tripleDes.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedData);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var resultStream = new MemoryStream();

        cryptoStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}