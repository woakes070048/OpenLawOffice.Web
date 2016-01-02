using System.Collections.Generic;

namespace OpenLawOffice.Web.ViewModels.Matters
{
    public class CreateMatterViewModel
    {
        public MatterViewModel Matter { get; set; }
        
        public List<CourtTypeViewModel> CourtTypes { get; set; }
        public List<CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }
        public List<CourtSittingInCityViewModel> CourtSittingInCities { get; set; }

        public Matters.MatterContactViewModel Contact1 { get; set; }
        public Matters.MatterContactViewModel Contact2 { get; set; }
        public Matters.MatterContactViewModel Contact3 { get; set; }
        public Matters.MatterContactViewModel Contact4 { get; set; }
        public Matters.MatterContactViewModel Contact5 { get; set; }
        public Matters.MatterContactViewModel Contact6 { get; set; }
        public Matters.MatterContactViewModel Contact7 { get; set; }
        public Matters.MatterContactViewModel Contact8 { get; set; }
        public Matters.MatterContactViewModel Contact9 { get; set; }
        public Matters.MatterContactViewModel Contact10 { get; set; }

        public CreateMatterViewModel()
        {
            CourtTypes = new List<CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<CourtGeographicalJurisdictionViewModel>();
            CourtSittingInCities = new List<CourtSittingInCityViewModel>();

            Contact1 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact2 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact3 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact4 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact5 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact6 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact7 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact8 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact9 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
            Contact10 = new MatterContactViewModel() { Matter = new MatterViewModel(), Contact = new Contacts.ContactViewModel() };
        }
    }
}