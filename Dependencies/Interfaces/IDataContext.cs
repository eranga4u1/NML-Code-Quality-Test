using System.Linq;
using Nml.Improve.Me.Dependencies;

namespace Bordeaux.actual.Dependencies.Interfaces
{
    public interface IDataContext
    {
        IQueryable<Application> Applications { get; set; }
    }
}