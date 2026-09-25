namespace PlataformaSoat.Infrastructure.Repositories.DTOs
{
    public class ProcesarResultDto
    {
        public bool Procesado { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string CodigoError { get; set; } = string.Empty;
        public string DetalleError { get; set; } = string.Empty;
    }
}
