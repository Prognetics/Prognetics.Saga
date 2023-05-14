using Prognetics.Saga.Core.Abstract;

namespace Prognetics.Saga.Parsers.Core.Abstract
{
    public interface IModelSourceFactory
    {
        IEnumerable<IModelSource> Build();
    }
}
