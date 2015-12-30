using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Matters
{
    public class EditMatterViewModel
    {
        public MatterViewModel Matter { get; set; }

        public Matters.MatterContactViewModel LeadAttorney { get; set; }

        public List<CourtTypeViewModel> CourtTypes { get; set; }
        public List<CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }
        public List<CourtSittingInCityViewModel> CourtSittingInCities { get; set; }


        public int? MatterTypeId { get; set; }
        public int? LeadAttorneyId { get; set; }
        public int? DefaultBillingRateId { get; set; }
        public int? BillingGroupId { get; set; }
        public int? CourtTypeId { get; set; }
        public int? CourtGeographicalJurisdictionId { get; set; }
        public int? CourtSittingInCityId { get; set; }

        public EditMatterViewModel()
        {
            CourtTypes = new List<CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<CourtGeographicalJurisdictionViewModel>();
            CourtSittingInCities = new List<CourtSittingInCityViewModel>();

            Matter = new MatterViewModel();
            Matter.LeadAttorney = new Contacts.ContactViewModel();
            Matter.MatterType = new MatterTypeViewModel();
            Matter.DefaultBillingRate = new Billing.BillingRateViewModel();
            Matter.BillingGroup = new Billing.BillingGroupViewModel();
            Matter.CourtType = new CourtTypeViewModel();
            Matter.CourtGeographicalJurisdiction = new CourtGeographicalJurisdictionViewModel();
            Matter.CourtSittingInCity = new CourtSittingInCityViewModel();
        }
    }
}