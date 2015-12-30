using System.Collections.Generic;

namespace OpenLawOffice.Web.ViewModels.Matters
{
    public class CreateMatterViewModel
    {
        public MatterViewModel Matter { get; set; }

        public MatterContactViewModel LeadAttorney { get; set; }

        public List<CourtTypeViewModel> CourtTypes { get; set; }
        public List<CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }
        public List<CourtSittingInCityViewModel> CourtSittingInCities { get; set; }

        public Contacts.ContactViewModel Contact1 { get; set; }
        public string Role1 { get; set; }

        public Contacts.ContactViewModel Contact2 { get; set; }
        public string Role2  { get; set; }

        public Contacts.ContactViewModel Contact3 { get; set; }
        public string Role3 { get; set; }

        public Contacts.ContactViewModel Contact4 { get; set; }
        public string Role4 { get; set; }

        public Contacts.ContactViewModel Contact5 { get; set; }
        public string Role5 { get; set; }

        public Contacts.ContactViewModel Contact6 { get; set; }
        public string Role6 { get; set; }

        public CreateMatterViewModel()
        {
            CourtTypes = new List<CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<CourtGeographicalJurisdictionViewModel>();
            CourtSittingInCities = new List<CourtSittingInCityViewModel>();
        }
    }
}