namespace Bordeaux.actual.Dependencies.Interfaces
{
    public interface IViewGenerator
    {
        string GenerateFromPath(string url, object viewModel);
    }
}