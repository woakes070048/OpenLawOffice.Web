// -----------------------------------------------------------------------
// <copyright file="AssetTagsController.cs" company="Nodine Legal, LLC">
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

namespace OpenLawOffice.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AutoMapper;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class AssetTagsController : BaseController
    {

        [Authorize(Roles = "Login, User")]
        public ActionResult JsonList(string q)
        {
            List<Common.Models.Assets.Tag> list;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                list = Data.Assets.Tag.List(q, conn, false);
            }

            list.Add(new Common.Models.Assets.Tag() { Name = "a1", Id = 1 });
            list.Add(new Common.Models.Assets.Tag() { Name = "b2", Id = 2 });
            list.Add(new Common.Models.Assets.Tag() { Name = "c3", Id = 3 });

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}