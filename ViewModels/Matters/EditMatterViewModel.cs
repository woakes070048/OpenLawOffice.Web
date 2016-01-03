using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Matters
{
    public class EditMatterViewModel
    {
        public MatterViewModel Matter { get; set; }

        public List<Contacts.ContactViewModel> EmployeeContactList;
        public List<Billing.BillingRateViewModel> BillingRateList;
        public List<Billing.BillingGroupViewModel> BillingGroupList;
        public List<MatterTypeViewModel> MatterTypeList;
        public List<CourtTypeViewModel> CourtTypes { get; set; }
        public List<CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }
        public List<CourtSittingInCityViewModel> CourtSittingInCities { get; set; }


        public int? MatterTypeId { get; set; }
        public int? DefaultBillingRateId { get; set; }
        public int? BillingGroupId { get; set; }
        public int? CourtTypeId { get; set; }
        public int? CourtGeographicalJurisdictionId { get; set; }
        public int? CourtSittingInCityId { get; set; }

        public EditMatterViewModel()
        {
            EmployeeContactList = new List<Contacts.ContactViewModel>();
            BillingRateList = new List<Billing.BillingRateViewModel>();
            BillingGroupList = new List<Billing.BillingGroupViewModel>();
            MatterTypeList = new List<MatterTypeViewModel>();
            CourtTypes = new List<CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<CourtGeographicalJurisdictionViewModel>();
            CourtSittingInCities = new List<CourtSittingInCityViewModel>();

            Matter = new MatterViewModel();
            Matter.MatterType = new MatterTypeViewModel();
            Matter.DefaultBillingRate = new Billing.BillingRateViewModel();
            Matter.BillingGroup = new Billing.BillingGroupViewModel();
            Matter.CourtType = new CourtTypeViewModel();
            Matter.CourtGeographicalJurisdiction = new CourtGeographicalJurisdictionViewModel();
            Matter.CourtSittingInCity = new CourtSittingInCityViewModel();
        }
    }
}