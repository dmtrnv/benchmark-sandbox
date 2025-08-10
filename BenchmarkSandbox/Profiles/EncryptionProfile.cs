using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkSandbox.Utils;

namespace BenchmarkSandbox.Profiles;

/*
Benchmark summary:
 
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


| Method    | DataSize | Mean         | Error       | StdDev      | Ratio | RatioSD | Rank | Gen0      | Gen1      | Gen2      | Allocated | Alloc Ratio |
|---------- |--------- |-------------:|------------:|------------:|------:|--------:|-----:|----------:|----------:|----------:|----------:|------------:|
| Rsa       | 524288   |           NA |          NA |          NA |     ? |       ? |    ? |        NA |        NA |        NA |        NA |           ? |
| Aes       | 524288   |     763.4 us |     6.86 us |     6.42 us |  1.00 |    0.01 |    1 | 1004.8828 |  998.0469 |  998.0469 |   3.68 MB |        1.00 |
| Des       | 524288   |  23,798.1 us |    52.21 us |    48.84 us | 31.17 |    0.26 |    2 |  218.7500 |  218.7500 |  218.7500 |      1 MB |        0.27 |
| TripleDes | 524288   |  23,879.0 us |   101.54 us |    90.01 us | 31.28 |    0.28 |    2 |  968.7500 |  968.7500 |  968.7500 |   3.68 MB |        1.00 |
|           |          |              |             |             |       |         |      |           |           |           |           |             |
| Rsa       | 1572864  |           NA |          NA |          NA |     ? |       ? |    ? |        NA |        NA |        NA |        NA |           ? |
| Aes       | 1572864  |   3,066.7 us |    63.92 us |   188.46 us |  1.00 |    0.09 |    1 |  714.8438 |  710.9375 |  710.9375 |  12.42 MB |        1.00 |
| Des       | 1572864  |  70,356.4 us |   239.89 us |   200.32 us | 23.03 |    1.42 |    2 |  125.0000 |  125.0000 |  125.0000 |      3 MB |        0.24 |
| TripleDes | 1572864  |  73,104.8 us |   362.34 us |   338.93 us | 23.93 |    1.48 |    3 | 1000.0000 | 1000.0000 | 1000.0000 |  12.42 MB |        1.00 |
|           |          |              |             |             |       |         |      |           |           |           |           |             |
| Rsa       | 3670016  |           NA |          NA |          NA |     ? |       ? |    ? |        NA |        NA |        NA |        NA |           ? |
| Aes       | 3670016  |   7,622.7 us |   146.41 us |   174.29 us |  1.00 |    0.03 |    1 |  968.7500 |  968.7500 |  968.7500 |  27.42 MB |        1.00 |
| Des       | 3670016  | 164,412.8 us | 1,925.02 us | 1,800.66 us | 21.58 |    0.54 |    2 |         - |         - |         - |      7 MB |        0.26 |
| TripleDes | 3670016  | 178,541.6 us | 2,725.64 us | 2,549.57 us | 23.43 |    0.62 |    3 |  666.6667 |  666.6667 |  666.6667 |  27.42 MB |        1.00 |
|           |          |              |             |             |       |         |      |           |           |           |           |             |
| Rsa       | 10485760 |           NA |          NA |          NA |     ? |       ? |    ? |        NA |        NA |        NA |        NA |           ? |
| Aes       | 10485760 |  21,563.9 us |   429.84 us |   558.92 us |  1.00 |    0.04 |    1 |  812.5000 |  812.5000 |  812.5000 |  89.92 MB |        1.00 |
| Des       | 10485760 | 490,805.9 us | 6,170.73 us | 5,772.10 us | 22.78 |    0.64 |    2 |         - |         - |         - |     20 MB |        0.22 |
| TripleDes | 10485760 | 508,743.1 us | 4,265.08 us | 3,780.88 us | 23.61 |    0.63 |    2 |         - |         - |         - |  89.92 MB |        1.00 |

*/

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EncryptionProfile
{
    private const int AesKeySize = 32;
    
    private byte[] _data = [];
    private byte[] _aesKey = [];
    private RSAParameters _rsaPublicKey = default;
    private RSAParameters _rsaPrivateKey = default;
    private byte[] _desKey = [];
    private byte[] _desIv = [];
    private byte[] _tripleDesKey = [];
    private byte[] _tripleDesIv = [];
    
    [ParamsSource(nameof(DataSizes))] 
    public int DataSize { get; set; }
    public static IEnumerable<int> DataSizes =>
    [
        524288,     // 0.5 MB
        1572864,    // 1.5 MB
        3670016,    // 3.5 MB
        10485760    // 10 MB
    ];
    
    [GlobalSetup]
    public void Setup()
    {
        using var rng = RandomNumberGenerator.Create();
        
        _data = new byte[DataSize];
        rng.GetBytes(_data);
        
        _aesKey = new byte[AesKeySize];
        rng.GetBytes(_aesKey);
        
        GenerateRsaKeyPair(out _rsaPublicKey, out _rsaPrivateKey);
        
        using var des = DES.Create();
        des.GenerateKey();
        des.GenerateIV();
        _desKey = des.Key;
        _desIv = des.IV;
        
        using var tripleDes = TripleDES.Create();
        tripleDes.GenerateKey();
        tripleDes.GenerateIV();
        _tripleDesKey = tripleDes.Key;
        _tripleDesIv = tripleDes.IV;
    }
    
    [Benchmark(Baseline = true)]
    public byte[] Aes()
    {
        var encrypted = Encryptor.EncryptAes(_data, _aesKey);
        var decrypted = Encryptor.DecryptAes(encrypted, _aesKey);
    
        return decrypted;
    }
    
    [Benchmark]
    public byte[] Rsa()
    {
        var encrypted = Encryptor.EncryptRsa(_data, _rsaPublicKey);
        var decrypted = Encryptor.DecryptRsa(encrypted, _rsaPrivateKey);
    
        return decrypted;
    }
    
    [Benchmark]
    public byte[] Des()
    {
        var encrypted = Encryptor.EncryptDes(_data, _desKey, _desIv);
        var decrypted = Encryptor.DecryptDes(encrypted, _desKey, _desIv);
    
        return decrypted;
    }
    
    [Benchmark]
    public byte[] TripleDes()
    {
        var encrypted = Encryptor.EncryptTripleDes(_data, _tripleDesKey, _tripleDesIv);
        var decrypted = Encryptor.DecryptTripleDes(encrypted, _tripleDesKey, _tripleDesIv);
    
        return decrypted;
    }
    
    #region private

    private static void GenerateRsaKeyPair(out RSAParameters publicKey, out RSAParameters privateKey)
    {
        using var rsa = RSA.Create(2048);
        publicKey = rsa.ExportParameters(false);
        privateKey = rsa.ExportParameters(true); 
    }

    #endregion
}