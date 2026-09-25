namespace PlataformaSoat.Application.Common.DTOs;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int ElementosPorPagina { get; set; }
    public int TotalElementos { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

    public PagedResponse(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalElementos = count;
        PaginaActual = pageNumber;
        ElementosPorPagina = pageSize;
        TotalPaginas = (int)Math.Ceiling(count / (double)pageSize);
    }
}
