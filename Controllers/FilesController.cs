// -----------------------------------------------------------------------
// <copyright file="VersionsController.cs" company="Nodine Legal, LLC">
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
    using System.Configuration;
    using System.Web.Profile;
    using System.Web.Security;
    using System.Data;
    using System.Linq;
    using Common.Models.Assets;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class FilesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id) //FileId
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Assets.Asset asset;
            Common.Models.Assets.Version version;
            Common.Models.Assets.File file;
            ViewModels.Assets.FileViewModel viewModel;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                file = Data.Assets.File.Get(id, conn, false);
                version = Data.Assets.Version.Get(file.Version.Id.Value, conn, false);
                asset = Data.Assets.Asset.Get(version.Asset.Id.Value, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(asset.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Assets.FileViewModel>(file);

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = matter;
            ViewBag.Asset = asset;
            ViewBag.Version = version;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(ViewModels.Assets.FileViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Assets.File file;

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);
                    file = Data.Assets.File.Get(trans, viewModel.Id.Value);
                    Common.Models.Assets.Version version = Data.Assets.Version.Get(trans, file.Version.Id.Value);

                    
                    file = Mapper.Map<Common.Models.Assets.File>(viewModel);

                    file = Data.Assets.File.Edit(trans, file, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", "Assets", new { Id = version.Asset.Id.Value });
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    throw ex;
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Download(Guid id) //FileId
        {
            Common.Models.Assets.Asset asset;
            Common.Models.Assets.Version version;
            Common.Models.Assets.File file;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                file = Data.Assets.File.Get(id, conn, false);
                version = Data.Assets.Version.Get(file.Version.Id.Value, conn, false);
                asset = Data.Assets.Asset.Get(version.Asset.Id.Value, conn, false);
            }

            Common.FileSystem.Asset fsAsset = new Common.FileSystem.Asset(asset);
            Common.FileSystem.Version fsVersion = new Common.FileSystem.Version(fsAsset, version);
            Common.FileSystem.File fsFile = new Common.FileSystem.File(fsAsset, fsVersion, file);

            return File(fsFile.Path, file.ContentType.ToValue(), asset.Title + file.Extension);
        }
    }
}