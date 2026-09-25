using PlataformaSoat.Domain.Core;

namespace PlataformaSoat.Domain.ValueObjects;

public class Moneda : BaseValueObject
{
    public string Codigo { get; }
    public string Nombre { get; }

    public Moneda(string codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
        yield return Nombre;
    }
}
