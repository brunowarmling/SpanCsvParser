namespace CsvParser.Parsers
{
    using FluentResults;
    using System;
    using System.Buffers;
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class SpanCsvParser<TRecod>
        where TRecod : struct
    {
        public delegate void ReadCallback(ref ReadOnlySpan<byte> field, ref int commaIndex, ref TRecod state);

        private static ReadOnlySpan<byte> Comma => ";"u8;

        private static ReadOnlySpan<byte> NewLine => "\r\n"u8;

        public static async ValueTask<Result<TRecod[]>> ParseAsync(Stream arquivo, ReadCallback callback, CancellationToken cancellationToken)
        {
            var urs = ArrayPool<TRecod>.Shared.Rent(25000);
            try
            {
                var arrLength = await ParseStream(arquivo, urs, callback, cancellationToken);
                return Result.Ok(urs[..arrLength]);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
            finally
            {
                ArrayPool<TRecod>.Shared.Return(urs);
            }
        }

        private static async ValueTask<int> ParseStream(Stream arquivo, TRecod[] items, ReadCallback callback, CancellationToken cancellationToken)
        {
            var position = 0;
            var currentLine = 0;
            var pipeReader = PipeReader.Create(arquivo);

            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                var sequencePosition = ParseLines(items, ref callback, ref buffer, ref position, ref currentLine);
                pipeReader.AdvanceTo(sequencePosition, buffer.End);

                if (result.IsCompleted)
                {
                    sequencePosition = ParseLastLine(items, ref callback, ref buffer, ref position);
                    pipeReader.AdvanceTo(sequencePosition, buffer.End);
                    break;
                }
            }

            await pipeReader.CompleteAsync();
            return position - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SequencePosition ParseLastLine(TRecod[] itemsArray, ref ReadCallback callback, ref ReadOnlySequence<byte> buffer, ref int position)
        {
            var reader = new SequenceReader<byte>(buffer);
            var unreadSpan = reader.UnreadSpan;
            itemsArray[position++] = ParseLine(ref unreadSpan, ref callback);
            return reader.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SequencePosition ParseLines(TRecod[] itemsArray, ref ReadCallback callback, ref ReadOnlySequence<byte> buffer, ref int position, ref int currentLine)
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

                itemsArray[position++] = ParseLine(ref line, ref callback);
            }

            return reader.Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TRecod ParseLine(ref ReadOnlySpan<byte> line, ref ReadCallback callback)
        {
            var commaCount = 0;
            var ur = new TRecod();
            while (commaCount <= 9)
            {
                var tabAt = line.IndexOf(Comma);
                var newLineAt = line.IndexOf(NewLine);

                var field = tabAt == -1 && newLineAt == -1 ? line[..^0] : commaCount == 9 ? line[..newLineAt] : line[..tabAt];
                callback(ref field, ref commaCount, ref ur);

                line = line[(tabAt + 1)..];
                commaCount++;
            }

            return ur;
        }
    }
}
