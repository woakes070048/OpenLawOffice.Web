﻿// -----------------------------------------------------------------------
// <copyright file="InvoiceFeeViewModel.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.WebClient.ViewModels.Billing
{
    using System;
    using AutoMapper;
    using OpenLawOffice.Common.Models;
    using System.Collections.Generic;

    [MapMe]
    public class InvoiceFeeViewModel : CoreViewModel
    {
        public Guid? Id { get; set; }
        public InvoiceViewModel Invoice { get; set; }
        public FeeViewModel Fee { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Details { get; set; }

        public void BuildMappings()
        {
            Mapper.CreateMap<OpenLawOffice.Common.Models.Billing.InvoiceFee, InvoiceFeeViewModel>()
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
                .ForMember(dst => dst.Invoice, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Billing.InvoiceViewModel()
                    {
                        Id = db.Invoice.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Fee, opt => opt.ResolveUsing(db =>
                {
                    return new ViewModels.Billing.FeeViewModel()
                    {
                        Id = db.Fee.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
                .ForMember(dst => dst.Details, opt => opt.MapFrom(src => src.Details));

            Mapper.CreateMap<InvoiceFeeViewModel, OpenLawOffice.Common.Models.Billing.InvoiceFee>()
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
                .ForMember(dst => dst.Invoice, opt => opt.ResolveUsing(model =>
                {
                    if (model.Invoice == null) return null;
                    return new Common.Models.Billing.Invoice()
                    {
                        Id = model.Invoice.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Fee, opt => opt.ResolveUsing(model =>
                {
                    if (model.Fee == null) return null;
                    return new Common.Models.Billing.Fee()
                    {
                        Id = model.Fee.Id,
                        IsStub = true
                    };
                }))
                .ForMember(dst => dst.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dst => dst.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
                .ForMember(dst => dst.Details, opt => opt.MapFrom(src => src.Details));
        }
    }
}