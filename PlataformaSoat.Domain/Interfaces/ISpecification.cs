using System.Linq.Expressions;

namespace PlataformaSoat.Domain.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    bool IsSatisfiedBy(T entity);
}
