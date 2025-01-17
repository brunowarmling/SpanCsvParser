using BenchmarkDotNet.Attributes;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;

[MemoryDiagnoser]
public class CsvParserBenchmark
{
    private const string FileName = "Resources\\urs_teste.csv";

    [Benchmark(Baseline = true)]
    public async ValueTask<Result<List<Ur>>> ParseAsync()
    {
        await using Stream fileStream = File.OpenRead(FileName);
        fileStream.Position = 0;
        return await CsvParser.Parsers.CsvParser.ParseAsync(fileStream, CancellationToken.None);
    }

    [Benchmark]
    public async ValueTask<Result<(UrNew[], int)>> SpanCsvParser()
    {
        await using Stream fileStream = File.OpenRead(FileName);
        fileStream.Position = 0;
        return await SpanCsvParser<UrNew>.ParseAsync(fileStream, AccessStageCsvHelper.PreencherCamposV1, AccessStageCsvHelper.QuantidadeColunasV1, CancellationToken.None);
    }
    [Benchmark]
    public async ValueTask<Result<Ur[]>> CsvHelper()
    {
        await using Stream fileStream = File.OpenRead(FileName);
        fileStream.Position = 0;

        var config = CsvConfiguration.FromAttributes<Ur>();
        using var reader = new StreamReader(FileName);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<Ur>().ToArray();
    }
}