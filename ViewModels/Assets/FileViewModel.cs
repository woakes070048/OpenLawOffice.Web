// -----------------------------------------------------------------------
// <copyright file="FileViewModel.cs" company="Nodine Legal, LLC">
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
    public class FileViewModel : CoreViewModel
    {
        public Guid? Id { get; set; }

        public VersionViewModel Version { get; set; }

        public bool IsSource { get; set; }

        public string FilePath { get; set; }

        public long ContentLength { get; set; }

        public Common.Models.Assets.ContentType ContentType { get; set; }

        public string Extension { get; set; }

        // Future holders
        //public IDifferenceAlgorithm DifferenceAlgorithm { get; set; }

        //public ICompressionAlgorithm CompressionAlgorithm { get; set; }

        //public IEncryptionAlgorithm EncryptionAlgorithm { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<Common.Models.Assets.File, FileViewModel>()
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
                .ForMember(dst => dst.Version, opt => opt.ResolveUsing(db =>
                {
                    if (db.Version == null || !db.Version.Id.HasValue) return null;
                    return new ViewModels.Assets.VersionViewModel()
                    {
                        Id = db.Version.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsSource, opt => opt.MapFrom(src => src.IsSource))
                .ForMember(dst => dst.FilePath, opt => opt.Ignore())
                .ForMember(dst => dst.ContentLength, opt => opt.MapFrom(src => src.ContentLength))
                .ForMember(dst => dst.ContentType, opt => opt.MapFrom(src => src.ContentType))
                .ForMember(dst => dst.Extension, opt => opt.MapFrom(src => src.Extension));

            Mapper.CreateMap<FileViewModel, Common.Models.Assets.File>()
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
                .ForMember(dst => dst.Version, opt => opt.ResolveUsing(x =>
                {
                    if (x.Version == null || !x.Version.Id.HasValue)
                        return null;
                    return new ViewModels.Assets.VersionViewModel()
                    {
                        Id = x.Version.Id.Value,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.IsSource, opt => opt.MapFrom(src => src.IsSource))
                .ForMember(dst => dst.ContentLength, opt => opt.MapFrom(src => src.ContentLength))
                .ForMember(dst => dst.ContentType, opt => opt.MapFrom(src => src.ContentType))
                .ForMember(dst => dst.Extension, opt => opt.MapFrom(src => src.Extension));
        }
    }
}