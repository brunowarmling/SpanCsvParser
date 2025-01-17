using CsvHelper.Configuration.Attributes;

[Delimiter(";")]
[CultureInfo("pt-Br")]
public class Ur
{
    [Index(0)]
    required public DateOnly? DataAtualizacao { get; init; }

    [Index(1)]
    required public string? TitularUr { get; init; }

    [Index(2)]
    required public string? Credenciadora { get; init; }

    [Index(3)]
    required public string? ArranjoPagamento { get; init; }

    [Index(4)]
    required public decimal? ValorTotal { get; init; }

    [Index(5)]
    required public long? Prioridade { get; init; }

    [Index(6)]
    required public long? RegraDivisao { get; init; }

    [Index(7)]
    required public decimal? ValorSolicitado { get; init; }

    [Index(8)]
    required public decimal? ValorConstituido { get; init; }

    [Index(9)]
    required public DateOnly? DataLiquidacao { get; init; }
}