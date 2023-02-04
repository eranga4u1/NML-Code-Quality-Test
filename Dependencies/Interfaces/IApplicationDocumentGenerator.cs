using System;

namespace Bordeaux.actual.Dependencies.Interfaces
{
    public interface IApplicationDocumentGenerator
    {
        byte[] Generate(Guid applicationId, string baseUri);
    }
}