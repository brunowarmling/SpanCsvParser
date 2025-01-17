using System;
using System.Text;

/// <summary>
/// Representa o helper para realizar parse do csv da access stage.
/// </summary>
public static class AccessStageCsvHelper
{
    /// <summary>
    /// Representa a quantidade de colunas processadas no csv 1.0 da access stage.
    /// </summary>
    public const int QuantidadeColunasV1 = 9;

    /// <summary>
    /// Preenche o objeto de <see cref="UrNew"/> com as informações do csv.
    /// </summary>
    /// <param name="campo">O campo que está sendo lido do csv.</param>
    /// <param name="indiceColuna">O índice da coluna atual.</param>
    /// <param name="objeto">O objeto <see cref="Ur"/> que irá receber o campo.</param>
    public static void PreencherCamposV1(ref ReadOnlySpan<byte> campo, ref int indiceColuna, ref UrNew objeto)
    {
        var campoAtual = Encoding.UTF8.GetString(campo);

        if (indiceColuna == 0)
        {
            if (!DateOnly.TryParse(campoAtual, out DateOnly dataAtualizacaoConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataAtualizacao)}");
            }

            objeto.DataAtualizacao = dataAtualizacaoConvertido;
        }
        else if (indiceColuna == 1)
        {
            objeto.TitularUr = campoAtual;
        }
        else if (indiceColuna == 2)
        {
            objeto.Credenciadora = campoAtual;
        }
        else if (indiceColuna == 3)
        {
            objeto.ArranjoPagamento = campoAtual;
        }
        else if (indiceColuna == 4)
        {
            if (!decimal.TryParse(campoAtual, out decimal vTotalConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorTotal)}");
            }

            objeto.ValorTotal = vTotalConvertido;
        }
        else if (indiceColuna == 5)
        {
            if (!long.TryParse(campoAtual, out long prioridadeConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.Prioridade)}");
            }

            objeto.Prioridade = prioridadeConvertido;
        }
        else if (indiceColuna == 6)
        {
            if (!long.TryParse(campoAtual, out long regraDivisaoConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.RegraDivisao)}");
            }

            objeto.RegraDivisao = regraDivisaoConvertido;
        }
        else if (indiceColuna == 7)
        {
            if (!decimal.TryParse(campoAtual, out decimal valorSolicitadoConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorSolicitado)}");
            }

            objeto.ValorSolicitado = valorSolicitadoConvertido;
        }
        else if (indiceColuna == 8)
        {
            if (!decimal.TryParse(campoAtual, out decimal valorConstituidoConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.ValorConstituido)}");
            }

            objeto.ValorConstituido = valorConstituidoConvertido;
        }
        else if (indiceColuna == 9)
        {
            if (!DateOnly.TryParse(campoAtual, out DateOnly dataLiquidacaoConvertido))
            {
                throw new InvalidOperationException($"Não é possível converter {nameof(Ur.DataLiquidacao)}");
            }

            objeto.DataLiquidacao = dataLiquidacaoConvertido;
        }
    }
}