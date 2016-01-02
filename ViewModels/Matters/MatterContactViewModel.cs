// -----------------------------------------------------------------------
// <copyright file="MatterContactViewModel.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.Web.ViewModels.Matters
{
    using AutoMapper;
    using OpenLawOffice.Common.Models;

    [MapMe]
    public class MatterContactViewModel : CoreViewModel
    {
        public int? Id { get; set; }

        public MatterViewModel Matter { get; set; }

        public Contacts.ContactViewModel Contact { get; set; }

        public bool IsClient { get; set; }

        public bool IsClientContact { get; set; }

        public bool IsAppointed { get; set; }

        public bool IsParty { get; set; }

        public string PartyTitle { get; set; }

        public bool IsJudge { get; set; }

        public bool IsWitness { get; set; }

        public bool IsAttorney { get; set; }

        public Contacts.ContactViewModel AttorneyFor { get; set; }

        public bool IsLeadAttorney { get; set; }

        public bool IsSupportStaff { get; set; }

        public Contacts.ContactViewModel SupportStaffFor { get; set; }

        public bool IsThirdPartyPayor { get; set; }

        public Contacts.ContactViewModel ThirdPartyPayorFor { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<OpenLawOffice.Common.Models.Matters.MatterContact, MatterContactViewModel>()
                .ForMember(dst => dst.IsStub, opt => opt.UseValue(false))
                .ForMember(dst => dst.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dst => dst.Modified, opt => opt.MapFrom(src => src.Modified))
                .ForMember(dst => dst.Disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.CreatedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.ModifiedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(db =>
                {
                    if (db.DisabledBy == null || !db.DisabledBy.PId.HasValue) return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.DisabledBy.PId.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Matter, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Matters.MatterViewModel()
                    {
                        Id = db.Matter.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Contact, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.Contact.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsClient, opt => opt.MapFrom(src => src.IsClient))
                .ForMember(dst => dst.IsClientContact, opt => opt.MapFrom(src => src.IsClientContact))
                .ForMember(dst => dst.IsAppointed, opt => opt.MapFrom(src => src.IsAppointed))
                .ForMember(dst => dst.IsParty, opt => opt.MapFrom(src => src.IsParty))
                .ForMember(dst => dst.PartyTitle, opt => opt.MapFrom(src => src.PartyTitle))
                .ForMember(dst => dst.IsJudge, opt => opt.MapFrom(src => src.IsJudge))
                .ForMember(dst => dst.IsWitness, opt => opt.MapFrom(src => src.IsWitness))
                .ForMember(dst => dst.IsAttorney, opt => opt.MapFrom(src => src.IsAttorney))
                .ForMember(dst => dst.AttorneyFor, opt => opt.ResolveUsing(db =>
                {
                    if (db.AttorneyFor == null || !db.AttorneyFor.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.AttorneyFor.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsLeadAttorney, opt => opt.MapFrom(src => src.IsLeadAttorney))
                .ForMember(dst => dst.IsSupportStaff, opt => opt.MapFrom(src => src.IsSupportStaff))
                .ForMember(dst => dst.SupportStaffFor, opt => opt.ResolveUsing(db =>
                {
                    if (db.SupportStaffFor == null || !db.SupportStaffFor.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.SupportStaffFor.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsThirdPartyPayor, opt => opt.MapFrom(src => src.IsThirdPartyPayor))
                .ForMember(dst => dst.ThirdPartyPayorFor, opt => opt.ResolveUsing(db =>
                {
                    if (db.ThirdPartyPayorFor == null || !db.ThirdPartyPayorFor.Id.HasValue) return null;
                    return new ViewModels.Contacts.ContactViewModel()
                    {
                        Id = db.ThirdPartyPayorFor.Id,
                        IsStub = true
                    };
                }));

            Mapper.CreateMap<MatterContactViewModel, OpenLawOffice.Common.Models.Matters.MatterContact>()
                .ForMember(dst => dst.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dst => dst.Modified, opt => opt.MapFrom(src => src.Modified))
                .ForMember(dst => dst.Disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.CreatedBy.PId
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.ModifiedBy.PId
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.DisabledBy == null || !x.DisabledBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.DisabledBy.PId.Value
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Matter, opt => opt.ResolveUsing(model =>
                {
                    if (model.Matter == null) return null;
                    return new Common.Models.Matters.Matter()
                    {
                        Id = model.Matter.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Contact, opt => opt.ResolveUsing(model =>
                {
                    if (model.Contact == null) return null;
                    return new Common.Models.Contacts.Contact()
                    {
                        Id = model.Contact.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsClient, opt => opt.MapFrom(src => src.IsClient))
                .ForMember(dst => dst.IsClientContact, opt => opt.MapFrom(src => src.IsClientContact))
                .ForMember(dst => dst.IsAppointed, opt => opt.MapFrom(src => src.IsAppointed))
                .ForMember(dst => dst.IsParty, opt => opt.MapFrom(src => src.IsParty))
                .ForMember(dst => dst.PartyTitle, opt => opt.MapFrom(src => src.PartyTitle))
                .ForMember(dst => dst.IsJudge, opt => opt.MapFrom(src => src.IsJudge))
                .ForMember(dst => dst.IsWitness, opt => opt.MapFrom(src => src.IsWitness))
                .ForMember(dst => dst.IsAttorney, opt => opt.MapFrom(src => src.IsAttorney))
                .ForMember(dst => dst.AttorneyFor, opt => opt.ResolveUsing(model =>
                {
                    if (model.AttorneyFor == null) return null;
                    return new Common.Models.Contacts.Contact()
                    {
                        Id = model.AttorneyFor.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsLeadAttorney, opt => opt.MapFrom(src => src.IsLeadAttorney))
                .ForMember(dst => dst.IsSupportStaff, opt => opt.MapFrom(src => src.IsSupportStaff))
                .ForMember(dst => dst.SupportStaffFor, opt => opt.ResolveUsing(model =>
                {
                    if (model.SupportStaffFor == null) return null;
                    return new Common.Models.Contacts.Contact()
                    {
                        Id = model.SupportStaffFor.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsThirdPartyPayor, opt => opt.MapFrom(src => src.IsThirdPartyPayor))
                .ForMember(dst => dst.ThirdPartyPayorFor, opt => opt.ResolveUsing(model =>
                {
                    if (model.ThirdPartyPayorFor == null) return null;
                    return new Common.Models.Contacts.Contact()
                    {
                        Id = model.ThirdPartyPayorFor.Id,
                        IsStub = true
                    };
                }));
        }
    }
}