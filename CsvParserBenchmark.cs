using BenchmarkDotNet.Attributes;
using CsvHelper;
using CsvHelper.Configuration;
using CsvParser.Parsers;
using FluentResults;
using System.Text;

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
    public async ValueTask<Result<UrNew[]>> SpanCsvParser()
    {
        await using Stream fileStream = File.OpenRead(FileName);
        fileStream.Position = 0;
        return await SpanCsvParser<UrNew>.ParseAsync(fileStream, Callback, CancellationToken.None);

        static void Callback(ref ReadOnlySpan<byte> field, ref int commaIndex, ref UrNew state)
        {
            var campoAtual = Encoding.UTF8.GetString(field);

            if (commaIndex == 0)
            {
                if (!DateOnly.TryParse(campoAtual, out DateOnly dataAtualizacaoConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataAtualizacao)}");
                }

                state.DataAtualizacao = dataAtualizacaoConvertido;
            }
            else if (commaIndex == 1)
            {
                state.TitularUr = campoAtual;
            }
            else if (commaIndex == 2)
            {
                state.Credenciadora = campoAtual;
            }
            else if (commaIndex == 3)
            {
                state.ArranjoPagamento = campoAtual;
            }
            else if (commaIndex == 4)
            {
                if (!decimal.TryParse(campoAtual, out decimal vTotalConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorTotal)}");
                }

                state.ValorTotal = vTotalConvertido;
            }
            else if (commaIndex == 5)
            {
                if (!long.TryParse(campoAtual, out long prioridadeConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.Prioridade)}");
                }

                state.Prioridade = prioridadeConvertido;
            }
            else if (commaIndex == 6)
            {
                if (!long.TryParse(campoAtual, out long regraDivisaoConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.RegraDivisao)}");
                }

                state.RegraDivisao = regraDivisaoConvertido;
            }
            else if (commaIndex == 7)
            {
                if (!decimal.TryParse(campoAtual, out decimal valorSolicitadoConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorSolicitado)}");
                }

                state.ValorSolicitado = valorSolicitadoConvertido;
            }
            else if (commaIndex == 8)
            {
                if (!decimal.TryParse(campoAtual, out decimal valorConstituidoConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorConstituido)}");
                }

                state.ValorConstituido = valorConstituidoConvertido;
            }
            else if (commaIndex == 9)
            {
                if (!DateOnly.TryParse(campoAtual, out DateOnly dataLiquidacaoConvertido))
                {
                    throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataLiquidacao)}");
                }

                state.DataLiquidacao = dataLiquidacaoConvertido;
            }
        }

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