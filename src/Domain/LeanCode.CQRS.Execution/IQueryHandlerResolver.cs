using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IQueryHandlerResolver
    {
        IQueryHandlerWrapper FindQueryHandler(Type queryType);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQueryHandlerWrapper
    {
        Task<object> ExecuteAsync(IQuery query);
    }
}
