namespace CsvParser.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class CsvParser
    {
        public static async ValueTask<List<Ur>> ParseAsync(Stream stream, CancellationToken cancellationToken)
        {
                Memory<byte> buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, cancellationToken);
                string csv = Encoding.UTF8.GetString(buffer.Span);
                var linhas = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                var urs = new List<Ur>();

                foreach (var linha in linhas.Skip(1))
                {
                    var colunas = linha.Split(";");
                    var dataAtualizacao = colunas[0];
                    var titularUr = colunas[1];
                    var credenciadora = colunas[2];
                    var arranjoPagamento = colunas[3];
                    var valorTotal = colunas[4];
                    var prioridade = colunas[5];
                    var regraDivisao = colunas[6];
                    var valorSolicitado = colunas[7];
                    var valorConstituido = colunas[8];
                    var dataLiquidacao = colunas[9];

                    if (!DateOnly.TryParse(dataAtualizacao, out DateOnly dataAtualizacaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(dataAtualizacao)}");
                    }

                    if (!decimal.TryParse(valorTotal, out decimal valorTotalConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(valorTotal)}");
                    }

                    if (!long.TryParse(prioridade, out long prioridadeConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(prioridade)}");
                    }

                    if (!long.TryParse(regraDivisao, out long regraDivisaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(regraDivisao)}");
                    }

                    if (!decimal.TryParse(valorSolicitado, out decimal valorSolicitadoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(valorSolicitado)}");
                    }

                    if (!decimal.TryParse(valorConstituido, out decimal valorConstituidoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(valorConstituido)}");
                    }

                    if (!DateOnly.TryParse(dataLiquidacao, out DateOnly dataLiquidacaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(dataLiquidacao)}");
                    }

                    urs.Add(new Ur
                    {
                        DataAtualizacao = dataAtualizacaoConvertido,
                        TitularUr = titularUr,
                        Credenciadora = credenciadora,
                        ArranjoPagamento = arranjoPagamento,
                        ValorTotal = valorTotalConvertido,
                        Prioridade = prioridadeConvertido,
                        RegraDivisao = regraDivisaoConvertido,
                        ValorSolicitado = valorSolicitadoConvertido,
                        ValorConstituido = valorConstituidoConvertido,
                        DataLiquidacao = dataLiquidacaoConvertido,
                    });
                }

                return urs;
        }
    }
}
