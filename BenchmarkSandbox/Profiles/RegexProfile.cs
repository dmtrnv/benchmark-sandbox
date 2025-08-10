using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BenchmarkSandbox.Profiles;

/*
Benchmark summary:

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4652)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


| Method              | Mean          | Error        | StdDev       | Median        | Ratio    | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------- |--------------:|-------------:|-------------:|--------------:|---------:|--------:|-----:|-------:|----------:|------------:|
| StringContains      |      26.07 ns |     0.538 ns |     0.699 ns |      26.20 ns |     1.00 |    0.04 |    1 |      - |         - |          NA |
| GeneratedRegex      |      56.51 ns |     1.125 ns |     0.997 ns |      56.73 ns |     2.17 |    0.07 |    2 |      - |         - |          NA |
| CompiledRegex       |      66.20 ns |     0.907 ns |     0.848 ns |      66.45 ns |     2.54 |    0.08 |    3 |      - |         - |          NA |
| Regex               |     139.93 ns |     2.788 ns |     6.787 ns |     140.78 ns |     5.37 |    0.30 |    4 |      - |         - |          NA |
| StringContainsN100  |   2,113.37 ns |    47.096 ns |   132.064 ns |   2,139.94 ns |    81.11 |    5.50 |    5 | 0.0019 |      40 B |          NA |
| GeneratedRegexN100  |   5,689.46 ns |   186.399 ns |   528.783 ns |   5,850.31 ns |   218.35 |   21.04 |    6 |      - |      40 B |          NA |
| CompiledRegexN100   |   6,850.12 ns |    46.939 ns |    39.196 ns |   6,850.51 ns |   262.90 |    7.25 |    7 |      - |      40 B |          NA |
| RegexN100           |  13,974.89 ns |   328.206 ns |   925.711 ns |  14,239.00 ns |   536.34 |   38.20 |    8 |      - |      40 B |          NA |
| StringContainsN1000 |  21,361.63 ns |   257.409 ns |   228.187 ns |  21,422.73 ns |   819.83 |   23.72 |    9 |      - |      40 B |          NA |
| GeneratedRegexN1000 |  57,826.06 ns | 1,141.036 ns | 2,621.720 ns |  58,374.00 ns | 2,219.28 |  116.48 |   10 |      - |      40 B |          NA |
| CompiledRegexN1000  |  68,107.08 ns |   777.308 ns |   727.094 ns |  68,323.17 ns | 2,613.85 |   75.63 |   11 |      - |      40 B |          NA |
| RegexN1000          | 145,449.45 ns | 1,215.644 ns | 1,077.637 ns | 145,672.64 ns | 5,582.13 |  156.08 |   12 |      - |      40 B |          NA |
 
*/

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public partial class RegexProfile
{
    private const string StringUnderTest = "application/some-type+json";
    private const string Pattern = "application.*json";
    
    private static readonly Regex CompiledPattern = new (Pattern, RegexOptions.Compiled);
    
    [GeneratedRegex(Pattern)]
    private static partial Regex GeneratedPattern();

    [Benchmark(Baseline = true)]
    public bool StringContains()
    {
        return StringUnderTest.Contains("application") && StringUnderTest.Contains("json");
    }

    [Benchmark]
    public bool Regex()
    {
        return System.Text.RegularExpressions.Regex.IsMatch(StringUnderTest, Pattern);
    }
    
    [Benchmark]
    public bool GeneratedRegex()
    {
        return GeneratedPattern().IsMatch(StringUnderTest);
    }
    
    [Benchmark]
    public bool CompiledRegex()
    {
        return CompiledPattern.IsMatch(StringUnderTest);
    }
    
    [Benchmark]
    public bool StringContainsN100()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 100))
        {
            result = StringUnderTest.Contains("application") && StringUnderTest.Contains("json");
        }

        return result;
    }

    [Benchmark]
    public bool RegexN100()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 100))
        {
            result = System.Text.RegularExpressions.Regex.IsMatch(StringUnderTest, Pattern);
        }

        return result;
    }
    
    [Benchmark]
    public bool GeneratedRegexN100()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 100))
        {
            result = GeneratedPattern().IsMatch(StringUnderTest);
        }

        return result;
    }
    
    [Benchmark]
    public bool CompiledRegexN100()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 100))
        {
            result = CompiledPattern.IsMatch(StringUnderTest);
        }

        return result;
    }
    
    [Benchmark]
    public bool StringContainsN1000()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            result = StringUnderTest.Contains("application") && StringUnderTest.Contains("json");
        }

        return result;
    }

    [Benchmark]
    public bool RegexN1000()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            result = System.Text.RegularExpressions.Regex.IsMatch(StringUnderTest, Pattern);
        }

        return result;
    }
    
    [Benchmark]
    public bool GeneratedRegexN1000()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            result = GeneratedPattern().IsMatch(StringUnderTest);
        }

        return result;
    }
    
    [Benchmark]
    public bool CompiledRegexN1000()
    {
        var result = false;
        
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            result = CompiledPattern.IsMatch(StringUnderTest);
        }

        return result;
    }
}