using Nml.Improve.Me.Dependencies;

namespace Bordeaux.actual.Dependencies.Interfaces
{
    public class InReviewApplicationViewModel
        : ApplicationViewModel
    {
        public string InReviewMessage { get; set; }
        public Review InReviewInformation { get; set; }
    }
}