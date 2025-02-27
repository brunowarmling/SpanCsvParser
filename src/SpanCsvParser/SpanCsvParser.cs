using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

/// Representa o serviço de parser de CSV.
/// </summary>
/// <typeparam name="TObjeto">Representa o tipo do objeto a ser feito parse.</typeparam>
public static class SpanCsvParser<TObjeto>
   where TObjeto : struct
{
    /// <summary>
    /// Callback para preencher o objeto com os valores da leitura do csv.
    /// </summary>
    /// <param name="campo">Dados do campo atual.</param>
    /// <param name="indiceColuna">Indice da coluna atual.</param>
    /// <param name="estado">Objeto com estado da linha atual.</param>
    public delegate void ReadCallback(ref ReadOnlySpan<byte> campo, ref int indiceColuna, ref TObjeto estado);

    public delegate ref TObjeto[] GetArrayCallBack();

    private static ReadOnlySpan<byte> Delimitador => ";"u8;

    private static ReadOnlySpan<byte> QuebraLinha => "\r\n"u8;

    /// <summary>
    /// Realiza o parse de um arquivo CSV para um array do tipo fornecido.
    /// </summary>
    /// <param name="getArrayCallBack">Método que irá retornar a referencia para o array.</param>
    /// <param name="arquivo">Arquivo csv a ser processado.</param>
    /// <param name="callback">Método que irá popular os campos no objeto.</param>
    /// <param name="numeroColunas">Indica o número de colunas que há no CSV.</param>
    /// <param name="cancellationToken">Representa o token de cancelamento.</param>
    /// <returns>Um <see cref="ValueTask"/> que representa a operação assincrona.</returns>
    public static async ValueTask<int> ParseAsync(GetArrayCallBack getArrayCallBack, Stream arquivo, ReadCallback callback, int numeroColunas, CancellationToken cancellationToken)
    {
        var arrRetorno = getArrayCallBack();
        return await ParseStream(arquivo, arrRetorno, callback, numeroColunas, cancellationToken);
    }

    /// <summary>
    /// Realiza o parse de um arquivo CSV para um array do tipo fornecido.
    /// </summary>
    /// <param name="arquivo">Arquivo csv a ser processado.</param>
    /// <param name="callback">Método que irá popular os campos no objeto.</param>
    /// <param name="numeroColunas">Indica o número de colunas que há no CSV.</param>
    /// <param name="cancellationToken">Representa o token de cancelamento.</param>
    /// <returns>Um <see cref="ValueTask"/> que representa a operação assincrona.</returns>
    public static async ValueTask<TObjeto[]> ParseAsync(Stream arquivo, ReadCallback callback, int numeroColunas, CancellationToken cancellationToken)
    {
        var arrRetorno = ArrayPool<TObjeto>.Shared.Rent(GetRentLength(arquivo.Length));
        try
        {
            var arrLength = await ParseStream(arquivo, arrRetorno, callback, numeroColunas, cancellationToken);
            var result = new TObjeto[arrLength];
            return result[..arrLength];
        }
        finally
        {
            ArrayPool<TObjeto>.Shared.Return(arrRetorno, true);
        }
    }

    public static int GetRentLength(long length)
    {
        if (length <= 1_000)
        {
            return 10;
        }
        else if (length <= 10_000)
        {
            return 100;
        }
        else if (length <= 100_000)
        {
            return 1000;
        }
        else if (length <= 500_000)
        {
            return 5000;
        }
        else
        {
            return (int)length / 100;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static async ValueTask<int> ParseStream(Stream arquivo, TObjeto[] items, ReadCallback callback, int numeroColunas, CancellationToken cancellationToken)
    {
        var position = 0;
        var currentLine = 0;
        var pipeReader = PipeReader.Create(arquivo);

        while (true)
        {
            var result = await pipeReader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            var sequencePosition = ParseLines(ref items, ref callback, ref numeroColunas, ref buffer, ref position, ref currentLine);
            pipeReader.AdvanceTo(sequencePosition, buffer.End);

            if (result.IsCompleted)
            {
                ParseLastLine(ref items, ref callback, ref numeroColunas, ref buffer, ref position);
                break;
            }
        }

        pipeReader.Complete();
        return position;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void ParseLastLine(ref TObjeto[] itemsArray, ref ReadCallback callback, ref int numeroColunas, ref ReadOnlySequence<byte> buffer, ref int position)
    {
        var reader = new SequenceReader<byte>(buffer);
        var unreadSpan = reader.UnreadSpan;
        if (unreadSpan.Length > 0)
        {
            itemsArray[position++] = ParseLine(ref unreadSpan, ref callback, ref numeroColunas);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static SequencePosition ParseLines(ref TObjeto[] itemsArray, ref ReadCallback callback, ref int numeroColunas, ref ReadOnlySequence<byte> buffer, ref int position, ref int currentLine)
    {
        var reader = new SequenceReader<byte>(buffer);

        while (!reader.End)
        {
            if (!reader.TryReadToAny(out ReadOnlySpan<byte> line, QuebraLinha, true))
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

            itemsArray[position++] = ParseLine(ref line, ref callback, ref numeroColunas);
        }

        return reader.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TObjeto ParseLine(ref ReadOnlySpan<byte> line, ref ReadCallback callback, ref int numeroColunas)
    {
        var commaCount = 0;
        var ur = default(TObjeto);
        while (commaCount <= numeroColunas)
        {
            var tabAt = line.IndexOf(Delimitador);
            if (commaCount == 0 && tabAt < 0 && numeroColunas > 1)
            {
                throw new InvalidOperationException("Arquivo não é um CSV válido.");
            }

            var newLineAt = line.IndexOf(QuebraLinha);

            var field = tabAt == -1 && newLineAt == -1 ? line[..^0] : commaCount == numeroColunas ? line[..newLineAt] : line[..tabAt];
            if (field.Length > 0)
            {
                callback(ref field, ref commaCount, ref ur);
            }

            line = line[(tabAt + 1)..];
            commaCount++;
        }

        return ur;
    }
}