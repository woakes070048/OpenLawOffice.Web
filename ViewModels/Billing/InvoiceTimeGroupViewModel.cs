
namespace OpenLawOffice.Web.ViewModels.Billing
{
    using System.Collections.Generic;

    public class InvoiceTimeGroupViewModel
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public List<InvoiceTimeViewModel> Times { get; set; }

        public InvoiceTimeGroupViewModel()
        {
            Id = -1;
            GroupName = null;
            Times = new List<InvoiceTimeViewModel>();
        }
    }
}