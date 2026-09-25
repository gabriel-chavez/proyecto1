namespace PlataformaSoat.Application.Common.DTOs;

public class PaginationRequest
{
    private const int MaxPageSize = 50;
    public int Pagina { get; set; } = 1;
    private int _pageSize = 10;

    public int ElementosPorPagina
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
