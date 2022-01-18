using MongoDB.Bson;

namespace WorkerAcoes.Documents;

public class AcaoDocument
{
    public ObjectId _id { get; set; }
    public string? HistLancamento { get; set; }
    public string? Codigo { get; set; }
    public double? Valor { get; set; }
    public string? DataReferencia { get; set; }
    public string? CodCorretora { get; set; }
    public string? NomeCorretora { get; set; }
}