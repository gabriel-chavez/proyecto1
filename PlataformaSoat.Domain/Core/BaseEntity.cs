namespace PlataformaSoat.Domain.Core;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
