using System.Linq.Expressions;
using PlataformaSoat.Domain.Interfaces;

namespace PlataformaSoat.Domain.Common;

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; }

    protected Specification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public bool IsSatisfiedBy(T entity)
    {
        return Criteria.Compile()(entity);
    }
}
