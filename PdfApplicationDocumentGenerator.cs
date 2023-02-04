using System;
using System.Linq;
using Bordeaux.actual.Dependencies.Interfaces;
using Nml.Improve.Me.Dependencies;
using static System.Net.Mime.MediaTypeNames;

namespace Nml.Improve.Me
{
    public class PdfApplicationDocumentGenerator : IApplicationDocumentGenerator
	{
		private readonly IDataContext _dataContext;
        private readonly IPathProvider _templatePathProvider;
        private readonly IViewGenerator _view_Generator;
        private readonly IConfiguration _configuration;
		private readonly ILogger<PdfApplicationDocumentGenerator> _logger;
		private readonly IPdfGenerator _pdfGenerator;

		public PdfApplicationDocumentGenerator(
			IDataContext dataContext,
			IPathProvider templatePathProvider,
			IViewGenerator viewGenerator,
			IConfiguration configuration,
			IPdfGenerator pdfGenerator,
			ILogger<PdfApplicationDocumentGenerator> logger)
		    {
			    if (dataContext == null)
                {
                    _logger.LogWarning("Data context is null here");
                    throw new ArgumentNullException(nameof(dataContext));
                }				
			    _dataContext = dataContext;
			    _templatePathProvider = templatePathProvider
                                        ?? throw new ArgumentNullException("templatePathProvider");
                _view_Generator = viewGenerator;
			    _configuration = configuration;
			    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
			    _pdfGenerator = pdfGenerator;
		    }
		
		public byte[] Generate(Guid applicationId, string baseUri)
		{
            Dependencies.Application application = _dataContext.Applications.Single(app => app.Id == applicationId);

			if (application != null)
			{
                string view = GetView(application,baseUri);
                PdfOptions pdfOptions = new PdfOptions
                {
					PageNumbers = PageNumbers.Numeric,
					HeaderOptions = new HeaderOptions
					{
						HeaderRepeat = HeaderRepeat.FirstPageOnly,
						HeaderHtml = PdfConstants.Header
					}
				};
				PdfDocument pdf = _pdfGenerator.GenerateFromHtml(view, pdfOptions);
				return pdf.ToBytes();
			}
			else
			{
				_logger.LogWarning(
					$"No application found for id '{applicationId}'");
				return null;
            }
        }
        public string GetView(Dependencies.Application application, string baseUri)
        {
            if (baseUri.EndsWith("/"))
                baseUri = baseUri.Substring(0, baseUri.Length - 1);

            ApplicationViewModel vm = new ApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };

            if (application.State == ApplicationState.Activated ||
                application.State == ApplicationState.InReview)
            {
                vm.LegalEntity = application.IsLegalEntity ? application.LegalEntity : null;
                vm.PortfolioFunds = application.Products.SelectMany(p => p.Funds);
                vm.PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds)
                                                    .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                                                    .Sum();
            }

            string path;
            switch (application.State)
            {
                case ApplicationState.Pending:
                    path = _templatePathProvider.Get("PendingApplication");
                    break;
                case ApplicationState.Activated:
                    path = _templatePathProvider.Get("ActivatedApplication");
                    break;
                case ApplicationState.InReview:
                    path = _templatePathProvider.Get("InReviewApplication");
                    string inReviewMessage = GetInReviewMessage(application.CurrentReview.Reason);
                    vm = new InReviewApplicationViewModel
                    {
                        ReferenceNumber = application.ReferenceNumber,
                        State = application.State.ToDescription(),
                        FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                        AppliedOn = application.Date,
                        SupportEmail = _configuration.SupportEmail,
                        Signature = _configuration.Signature,
                        InReviewMessage = inReviewMessage,
                        InReviewInformation = application.CurrentReview
                    };
                    break;
                default:
                    _logger.LogWarning($"The application is in state '{application.State}' and no valid document can be generated for it.");
                    return null;
            }
            return _view_Generator.GenerateFromPath($"{baseUri}{path}", vm);
        }

        private string GetInReviewMessage(string reason)
        {
            if (reason.Contains("address"))
                return "Your application has been placed in review pending outstanding address verification for FICA purposes.";
            if (reason.Contains("bank"))
                return "Your application has been placed in review pending outstanding bank account verification.";
            return "Your application has been placed in review because of suspicious account behaviour. Please contact support ASAP.";
        }
    }
}
