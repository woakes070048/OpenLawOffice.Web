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
    public class VersionsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(Guid id) // AssetId
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Assets.Asset asset;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                asset = Data.Assets.Asset.Get(id, conn, false);
                if (asset.CheckedOutBy != null)
                    asset.CheckedOutBy = Data.Account.Users.Get(asset.CheckedOutBy.PId.Value, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(asset.Id.Value, conn, false);
            }
            
            ViewBag.Matter = matter;
            ViewBag.Asset = asset;

            return View(new ViewModels.Assets.CreateVersionViewModel()
            {
                IsCourtFiled = asset.IsCourtFiled,
                IsFinal = asset.IsFinal
            });
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(Guid id, ViewModels.Assets.CreateVersionViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;

            // FS layout
            // Asset/Version/File
            Guid assetId = Guid.Empty;
            Common.Models.Assets.Asset asset;
            Common.Models.Assets.Version version;
            List<Common.Models.Assets.File> files = new List<Common.Models.Assets.File>();

            Common.FileSystem.Asset fsAsset;
            Common.FileSystem.Version fsVersion = null;
            List<Common.FileSystem.File> fsFiles = new List<Common.FileSystem.File>();
            
            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);
                    asset = Data.Assets.Asset.Get(trans, id);
                    assetId = asset.Id.Value;
                    if (asset.CheckedOutBy != null && asset.CheckedOutBy.PId.HasValue)
                    {
                        if (asset.CheckedOutBy.PId.Value != currentUser.PId.Value)
                            throw new Exception("Asset is checked out by another user.");
                    }

                    fsAsset = new Common.FileSystem.Asset(asset);

                    version = Mapper.Map<Common.Models.Assets.Version>(viewModel.Version);
                    version.Asset = asset;
                    version.Id = Guid.NewGuid();
                    version.SequenceNumber = Data.Assets.Asset.GetNextVersionSequenceNumber(trans, version.Asset.Id.Value);
                    fsVersion = new Common.FileSystem.Version(fsAsset, version);

                    version = Data.Assets.Version.Create(trans, version, currentUser);

                    if (!System.IO.Directory.Exists(fsVersion.Path))
                        System.IO.Directory.CreateDirectory(fsVersion.Path);

                    viewModel.Files.ForEach(vmFile =>
                    {
                        bool isSource = false;
                        if (viewModel.SourceFiles != null &&
                            viewModel.SourceFiles.Select(x => x == vmFile.FileName).Count() > 0)
                            isSource = true;
                        Common.Models.Assets.File file = new Common.Models.Assets.File()
                        {
                            Version = version,
                            Id = Guid.NewGuid(),
                            ContentLength = vmFile.ContentLength,
                            ContentType = vmFile.ContentType.ToContentType(),
                            //ContentType = (Common.Models.Assets.ContentType)Enum.Parse(typeof(Common.Models.Assets.ContentType), vmFile.ContentType.Substring(vmFile.ContentType.LastIndexOf('/') + 1), true),
                            IsSource = isSource,
                            Extension = System.IO.Path.GetExtension(vmFile.FileName)
                        };
                        Common.FileSystem.File fsFile = new Common.FileSystem.File(fsAsset, fsVersion, file);

                        Data.Assets.File.Create(trans, file, currentUser);

                        fsFiles.Add(fsFile);

                        vmFile.SaveAs(fsFile.Path);
                    });

                    if (viewModel.IsCourtFiled != asset.IsCourtFiled ||
                        viewModel.IsFinal != asset.IsFinal)
                    {
                        asset.IsFinal = viewModel.IsFinal;
                        asset.IsCourtFiled = viewModel.IsCourtFiled;
                        asset = Data.Assets.Asset.Edit(trans, asset, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("Details", "Assets", new { Id = asset.Id });
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    if (fsVersion != null)
                        System.IO.Directory.Delete(fsVersion.Path, true);

                    throw ex;
                }
            }

        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id) //VersionId
        {
            Common.Models.Assets.Version version;
            ViewModels.Assets.VersionViewModel viewModel;
            Common.Models.Assets.Asset asset;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                version = Data.Assets.Version.Get(id, conn, false);
                asset = Data.Assets.Asset.Get(version.Asset.Id.Value, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(asset.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Assets.VersionViewModel>(version);

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = matter;
            ViewBag.Asset = asset;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Assets.VersionViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            
            Common.Models.Assets.Version version;
            Guid assetId = Guid.Empty;

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    // Current
                    version = Data.Assets.Version.Get(trans, id);
                    assetId = version.Asset.Id.Value;

                    // Update
                    version = Mapper.Map<Common.Models.Assets.Version>(viewModel);

                    version = Data.Assets.Version.Edit(trans, version, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", "Assets", new { Id = assetId });
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    throw ex;
                }
            }

        }

    }
}