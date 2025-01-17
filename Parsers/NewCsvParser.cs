namespace CsvParser.Parsers
{
    using FluentResults;
    using System;
    using System.Buffers;
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class NewCsvParser
    {
        private static ReadOnlySpan<byte> Comma => ";"u8;

        private static ReadOnlySpan<byte> NewLine => "\r\n"u8;

        public static async ValueTask<Result<UrNew[]>> ParseAsync(Stream arquivo, CancellationToken cancellationToken)
        {
            var urs = ArrayPool<UrNew>.Shared.Rent(25000);
            try
            {
                var arrLength = await ParseStream(arquivo, urs, cancellationToken);
                return Result.Ok(urs[..arrLength]);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
            finally
            {
                ArrayPool<UrNew>.Shared.Return(urs);
            }
        }

        private static async ValueTask<int> ParseStream(Stream arquivo, UrNew[] items, CancellationToken cancellationToken)
        {
            var position = 0;
            var currentLine = 0;
            var pipeReader = PipeReader.Create(arquivo);

            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                var sequencePosition = ParseLines(items, ref buffer, ref position, ref currentLine);
                pipeReader.AdvanceTo(sequencePosition, buffer.End);

                if (result.IsCompleted)
                {
                    sequencePosition = ParseLastLine(items, ref buffer, ref position);
                    pipeReader.AdvanceTo(sequencePosition, buffer.End);
                    break;
                }
            }

            await pipeReader.CompleteAsync();
            return position - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SequencePosition ParseLastLine(UrNew[] itemsArray, ref ReadOnlySequence<byte> buffer, ref int position)
        {
            var reader = new SequenceReader<byte>(buffer);
            var unreadSpan = reader.UnreadSpan;
            itemsArray[position++] = ParseLine(ref unreadSpan);
            return reader.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SequencePosition ParseLines(UrNew[] itemsArray, ref ReadOnlySequence<byte> buffer, ref int position, ref int currentLine)
        {
            var reader = new SequenceReader<byte>(buffer);

            while (!reader.End)
            {
                if (!reader.TryReadToAny(out ReadOnlySpan<byte> line, NewLine, true))
                {
                    break;
                }

                if (line.Length == 0)
                {
                    continue;
                }

                currentLine++;
                if (currentLine == 1)
                {
                    continue;
                }

                itemsArray[position++] = ParseLine(ref line);
            }

            return reader.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UrNew ParseLine(ref ReadOnlySpan<byte> line)
        {
            var commaCount = 0;
            var ur = new UrNew();
            while (commaCount <= 9)
            {
                var tabAt = line.IndexOf(Comma);
                var newLineAt = line.IndexOf(NewLine);
                var field = Encoding.UTF8.GetString(tabAt == -1 && newLineAt == -1 ? line[..^0] : commaCount == 9 ? line[..newLineAt] : line[..tabAt]);

                if (commaCount == 0)
                {
                    if (!DateOnly.TryParse(field, out DateOnly dataAtualizacaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataAtualizacao)}");
                    }

                    ur.DataAtualizacao = dataAtualizacaoConvertido;
                }
                else if (commaCount == 1)
                {
                    ur.TitularUr = field;
                }
                else if (commaCount == 2)
                {
                    ur.Credenciadora = field;
                }
                else if (commaCount == 3)
                {
                    ur.ArranjoPagamento = field;
                }
                else if (commaCount == 4)
                {
                    if (!decimal.TryParse(field, out decimal vTotalConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorTotal)}");
                    }

                    ur.ValorTotal = vTotalConvertido;
                }
                else if (commaCount == 5)
                {
                    if (!long.TryParse(field, out long prioridadeConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.Prioridade)}");
                    }

                    ur.Prioridade = prioridadeConvertido;
                }
                else if (commaCount == 6)
                {
                    if (!long.TryParse(field, out long regraDivisaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.RegraDivisao)}");
                    }

                    ur.RegraDivisao = regraDivisaoConvertido;
                }
                else if (commaCount == 7)
                {
                    if (!decimal.TryParse(field, out decimal valorSolicitadoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorSolicitado)}");
                    }

                    ur.ValorSolicitado = valorSolicitadoConvertido;
                }
                else if (commaCount == 8)
                {
                    if (!decimal.TryParse(field, out decimal valorConstituidoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorConstituido)}");
                    }

                    ur.ValorConstituido = valorConstituidoConvertido;
                }
                else if (commaCount == 9)
                {
                    if (!DateOnly.TryParse(field, out DateOnly dataLiquidacaoConvertido))
                    {
                        throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataLiquidacao)}");
                    }

                    ur.DataLiquidacao = dataLiquidacaoConvertido;
                }

                line = line[(tabAt + 1)..];
                commaCount++;
            }

            return ur;
        }
    }
}
