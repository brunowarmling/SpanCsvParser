using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CsvHelper;
using CsvHelper.Configuration;
using System.Buffers;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class CsvParserBenchmark
{
    private const string Folder = "Resources";
    private const string FileName1Linha = "1_linha.csv";
    private const string FileName50Linhas = "50_linhas.csv";
    private const string FileName500Linhas = "500_linhas.csv";
    private const string FileName5000Linhas = "5000_linhas.csv";

    private UrNew[] arr;

    ref UrNew[] GetRefValue()
    {
        // Simulating a reference return
        return ref arr;
    }

    [Params(FileName1Linha, FileName50Linhas, FileName500Linhas, FileName5000Linhas)]
    public static string FileName { get; set; }

    [Benchmark(Baseline = true)]
    public async ValueTask<List<Ur>> ParseAsync()
    {
        await using Stream fileStream = File.OpenRead(Folder + "\\" + FileName);
        fileStream.Position = 0;
        return await CsvParser.Parsers.CsvParser.ParseAsync(fileStream, CancellationToken.None);
    }

    [Benchmark]
    public async ValueTask<UrNew[]> SpanCsvParser()
    {
        await using Stream fileStream = File.OpenRead(Folder + "\\" + FileName);
        fileStream.Position = 0;
        return await SpanCsvParser<UrNew>.ParseAsync(fileStream, AccessStageCsvHelper.PreencherCamposV1, AccessStageCsvHelper.QuantidadeColunasV1, CancellationToken.None);
    }

    [Benchmark]
    public async ValueTask<int> SpanWithRef()
    {
        await using Stream fileStream = File.OpenRead(Folder + "\\" + FileName);
        fileStream.Position = 0;

        this.arr = ArrayPool<UrNew>.Shared.Rent(SpanCsvParser<UrNew>.GetRentLength(fileStream.Length));
        try
        {
            return await SpanCsvParser<UrNew>.ParseAsync(this.GetRefValue, fileStream, AccessStageCsvHelper.PreencherCamposV1, AccessStageCsvHelper.QuantidadeColunasV1, CancellationToken.None);
        }
        finally
        {
            ArrayPool<UrNew>.Shared.Return(this.arr, true);
        }
    }

    [Benchmark]
    public async ValueTask<Ur[]> CsvHelper()
    {
        var config = CsvConfiguration.FromAttributes<Ur>();
        using var reader = new StreamReader(Folder + "\\" + FileName);
        using var csv = new CsvReader(reader, config);
        return [.. csv.GetRecords<Ur>()];
    }
}