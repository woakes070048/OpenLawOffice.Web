// -----------------------------------------------------------------------
// <copyright file="AssetViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.ViewModels.Assets
{
    using System;
    using System.IO;
    using AutoMapper;
    using OpenLawOffice.Common.Models;
    using System.Collections.Generic;

    [MapMe]
    public class AssetViewModel : CoreViewModel
    {
        public Guid? Id { get; set; }

        public long? IdInt { get; set; }

        public DateTime Date { get; set; }
        public string Title { get; set; }

        public bool IsFinal { get; set; }
        public bool IsCourtFiled { get; set; }
        public bool IsAttorneyWorkProduct { get; set; }
        public bool IsPrivileged { get; set; }
        public bool IsDiscoverable { get; set; }
        public DateTime? ProvidedInDiscoveryAt { get; set; }

        public Account.UsersViewModel CheckedOutBy { get; set; }
        public DateTime? CheckedOutAt { get; set; }

        public List<TagViewModel> Tags { get; set; }

        public List<VersionViewModel> Versions { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Assets.Asset, AssetViewModel>()
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
                .ForMember(dst => dst.IdInt, opt => opt.MapFrom(src => src.IdInt))
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dst => dst.IsFinal, opt => opt.MapFrom(src => src.IsFinal))
                .ForMember(dst => dst.IsCourtFiled, opt => opt.MapFrom(src => src.IsCourtFiled))
                .ForMember(dst => dst.IsAttorneyWorkProduct, opt => opt.MapFrom(src => src.IsAttorneyWorkProduct))
                .ForMember(dst => dst.IsPrivileged, opt => opt.MapFrom(src => src.IsPrivileged))
                .ForMember(dst => dst.IsDiscoverable, opt => opt.MapFrom(src => src.IsDiscoverable))
                .ForMember(dst => dst.ProvidedInDiscoveryAt, opt => opt.MapFrom(src => src.ProvidedInDiscoveryAt))
                .ForMember(dst => dst.CheckedOutBy, opt => opt.ResolveUsing(db =>
                {
                    if (db.CheckedOutBy == null || !db.CheckedOutBy.PId.HasValue) return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = db.CheckedOutBy.PId.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.CheckedOutAt, opt => opt.MapFrom(src => src.CheckedOutAt))
                .ForMember(dst => dst.Tags, opt => opt.Ignore())
                .ForMember(dst => dst.Versions, opt => opt.Ignore());

            Mapper.CreateMap<AssetViewModel, Common.Models.Assets.Asset>()
                .ForMember(dst => dst.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dst => dst.Modified, opt => opt.MapFrom(src => src.Modified))
                .ForMember(dst => dst.Disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dst => dst.CreatedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.CreatedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.ModifiedBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CreatedBy == null || !x.CreatedBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.ModifiedBy.PId,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.DisabledBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.DisabledBy == null || !x.DisabledBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.DisabledBy.PId.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.IdInt, opt => opt.MapFrom(src => src.IdInt))
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dst => dst.IsFinal, opt => opt.MapFrom(src => src.IsFinal))
                .ForMember(dst => dst.IsCourtFiled, opt => opt.MapFrom(src => src.IsCourtFiled))
                .ForMember(dst => dst.IsAttorneyWorkProduct, opt => opt.MapFrom(src => src.IsAttorneyWorkProduct))
                .ForMember(dst => dst.IsPrivileged, opt => opt.MapFrom(src => src.IsPrivileged))
                .ForMember(dst => dst.IsDiscoverable, opt => opt.MapFrom(src => src.IsDiscoverable))
                .ForMember(dst => dst.ProvidedInDiscoveryAt, opt => opt.MapFrom(src => src.ProvidedInDiscoveryAt))
                .ForMember(dst => dst.CheckedOutBy, opt => opt.ResolveUsing(x =>
                {
                    if (x.CheckedOutBy == null || !x.CheckedOutBy.PId.HasValue)
                        return null;
                    return new ViewModels.Account.UsersViewModel()
                    {
                        PId = x.CheckedOutBy.PId.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.CheckedOutAt, opt => opt.MapFrom(src => src.CheckedOutAt));
        }
    }
}