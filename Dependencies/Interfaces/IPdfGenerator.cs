using Nml.Improve.Me.Dependencies;

namespace Bordeaux.actual.Dependencies.Interfaces
{
    public interface IPdfGenerator
    {
        PdfDocument GenerateFromHtml(string view, PdfOptions pdfOptions);
    }
}