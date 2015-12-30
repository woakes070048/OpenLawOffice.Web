// -----------------------------------------------------------------------
// <copyright file="MatterViewModel.cs" company="Nodine Legal, LLC">
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
    using System.Collections.Generic;

    public class MatterIndexViewModel
    {
        public List<MatterViewModel> Matters { get; set; }
        public CourtTypeViewModel SelectedCourtType { get; set; }
        public List<CourtTypeViewModel> CourtTypes { get; set; }
        public CourtGeographicalJurisdictionViewModel SelectedCourtGeographicalJurisdiction { get; set; }
        public List<CourtGeographicalJurisdictionViewModel> CourtGeographicalJurisdictions { get; set; }

        public MatterIndexViewModel()
        {
            Matters = new List<MatterViewModel>();
            CourtTypes = new List<CourtTypeViewModel>();
            CourtGeographicalJurisdictions = new List<CourtGeographicalJurisdictionViewModel>();
        }
    }
}