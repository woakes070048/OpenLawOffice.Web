// -----------------------------------------------------------------------
// <copyright file="AssetTagViewModel.cs" company="Nodine Legal, LLC">
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
    public class AssetTagViewModel : CoreViewModel
    {
        public Guid? Id { get; set; }
        public AssetViewModel Asset { get; set; }
        public TagViewModel Tag { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Assets.AssetTag, AssetTagViewModel>()
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
                .ForMember(dst => dst.Asset, opt => opt.ResolveUsing(db =>
                {
                    if (db.Asset == null || !db.Asset.Id.HasValue) return null;
                    return new ViewModels.Assets.AssetViewModel()
                    {
                        Id = db.Asset.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Tag, opt => opt.ResolveUsing(db =>
                {
                    if (db.Tag == null || !db.Tag.Id.HasValue) return null;
                    return new ViewModels.Assets.TagViewModel()
                    {
                        Id = db.Tag.Id.Value,
                        IsStub = true
                    };
                }));

            Mapper.CreateMap<AssetTagViewModel, Common.Models.Assets.AssetTag>()
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
                .ForMember(dst => dst.Asset, opt => opt.ResolveUsing(x =>
                {
                    if (x.Asset == null || !x.Asset.Id.HasValue)
                        return null;
                    return new ViewModels.Assets.AssetViewModel()
                    {
                        Id = x.Asset.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Tag, opt => opt.ResolveUsing(x =>
                {
                    if (x.Tag == null || !x.Tag.Id.HasValue)
                        return null;
                    return new ViewModels.Assets.TagViewModel()
                    {
                        Id = x.Tag.Id.Value,
                        IsStub = true
                    };
                }));
        }
    }
}