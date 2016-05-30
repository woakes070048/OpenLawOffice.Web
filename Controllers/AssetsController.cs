// -----------------------------------------------------------------------
// <copyright file="AssetsController.cs" company="Nodine Legal, LLC">
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
    public class AssetsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            ViewModels.Assets.AssetViewModel viewModel;
            Common.Models.Assets.Asset model;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Assets.Asset.Get(id, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Assets.AssetViewModel>(model);

                if (model.CheckedOutBy != null && model.CheckedOutBy.PId.HasValue)
                {
                    model.CheckedOutBy = Data.Account.Users.Get(model.CheckedOutBy.PId.Value, conn, false);
                    viewModel.CheckedOutBy = Mapper.Map<ViewModels.Account.UsersViewModel>(model.CheckedOutBy);
                }

                viewModel.Versions = new List<ViewModels.Assets.VersionViewModel>();
                viewModel.Tags = new List<ViewModels.Assets.TagViewModel>();

                Data.Assets.Tag.ListForAsset(model.Id.Value, conn, false).ForEach(tag =>
                {
                    viewModel.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                });

                Data.Assets.Version.ListForAsset(model.Id.Value, conn, false).ForEach(version =>
                {
                    ViewModels.Assets.VersionViewModel versionVM = Mapper.Map<ViewModels.Assets.VersionViewModel>(version);
                    versionVM.Files = new List<ViewModels.Assets.FileViewModel>();
                    viewModel.Versions.Add(versionVM);
                    Data.Assets.File.ListForVersion(version.Id.Value, conn, false).ForEach(file =>
                    {
                        ViewModels.Assets.FileViewModel fileVM = Mapper.Map<ViewModels.Assets.FileViewModel>(file);
                        versionVM.Files.Add(fileVM);
                    });
                });

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create(Guid id)
        {
            ViewModels.Assets.CreateAssetViewModel viewModel = new ViewModels.Assets.CreateAssetViewModel();
            Common.Models.Matters.Matter matter;
            List<Common.Models.Assets.Tag> list;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);
                list = Data.Assets.Tag.List(conn, false);

                viewModel.PossibleTags = new List<ViewModels.Assets.TagViewModel>();
                Data.Assets.Tag.ListOrderedByFrequency(conn, false).ForEach(x =>
                {
                    viewModel.PossibleTags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(x));
                });
            }
            
            ViewBag.TagJson = Newtonsoft.Json.Linq.JArray.FromObject(list).ToString();
            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(Guid id, ViewModels.Assets.CreateAssetViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;

            // FS layout
            // Asset/Version/File
            Common.Models.Assets.Asset asset;
            Common.Models.Assets.Version version;
            List<Common.Models.Assets.File> files = new List<Common.Models.Assets.File>();

            Common.FileSystem.Asset fsAsset;
            Common.FileSystem.Version fsVersion;
            List<Common.FileSystem.File> fsFiles = new List<Common.FileSystem.File>();

            asset = Mapper.Map<Common.Models.Assets.Asset>(viewModel.Asset);
            if (!asset.Id.HasValue) asset.Id = Guid.NewGuid();
            fsAsset = new Common.FileSystem.Asset(asset);

            if(!System.IO.Directory.Exists(fsAsset.Path))
                System.IO.Directory.CreateDirectory(fsAsset.Path);

            version = new Common.Models.Assets.Version()
            {
                Asset = asset,
                Id = Guid.NewGuid(),
                SequenceNumber = 1,
                ChangeDetails = "Initial version."
            };
            fsVersion = new Common.FileSystem.Version(fsAsset, version);

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

                files.Add(file);
                fsFiles.Add(fsFile);

                vmFile.SaveAs(fsFile.Path);
            });

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);
                    Common.Models.Matters.Matter matter = Data.Matters.Matter.Get(trans, id);

                    asset = Data.Assets.Asset.Create(trans, asset, currentUser);

                    Data.Assets.AssetMatter.Create(trans, new Common.Models.Assets.AssetMatter()
                    {
                        Asset = asset,
                        Matter = matter
                    }, currentUser);

                    version = Data.Assets.Version.Create(trans, version, currentUser);

                    files.ForEach(file =>
                    {
                        Data.Assets.File.Create(trans, file, currentUser);
                    });

                    while (string.Join("", viewModel.Tags).Contains(","))
                    {
                        List<string> bldr = new List<string>();
                        foreach (string str in viewModel.Tags)
                        {
                            bldr.AddRange(str.Split(','));
                        }
                        bldr.ForEach(x => x = x.Trim());
                        viewModel.Tags = bldr.ToArray();
                    }

                    foreach (string tag in viewModel.Tags)
                    {
                        if (!string.IsNullOrEmpty(tag))
                        {
                            Common.Models.Assets.Tag mTag = new Common.Models.Assets.Tag() { Name = tag };
                            Data.Assets.Tag.Create(trans, mTag, currentUser);

                            Data.Assets.AssetTag.Create(trans, new Common.Models.Assets.AssetTag()
                            { Asset = asset, Tag = mTag }, currentUser);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    System.IO.Directory.Delete(fsAsset.Path, true);
                }
            }

            return RedirectToAction("Details", new { Id = asset.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            ViewModels.Assets.AssetViewModel viewModel;
            Common.Models.Assets.Asset model;
            Common.Models.Matters.Matter matter;
            List<Common.Models.Assets.Tag> list;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Assets.Asset.Get(id, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Assets.AssetViewModel>(model);
                viewModel.Tags = new List<ViewModels.Assets.TagViewModel>();
                list = Data.Assets.Tag.List(conn, false);

                Data.Assets.Tag.ListForAsset(model.Id.Value, conn, false).ForEach(tag =>
                {
                    viewModel.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                });

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = matter;
            ViewBag.TagJson = Newtonsoft.Json.Linq.JArray.FromObject(list).ToString();

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Assets.AssetViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;

            // FS layout
            // Asset/Version/File
            Common.Models.Assets.Asset asset;

            asset = Mapper.Map<Common.Models.Assets.Asset>(viewModel);

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);
                    Common.Models.Matters.Matter matter = Data.Matters.Matter.Get(trans, id);

                    Data.Assets.Asset.Edit(trans, asset, currentUser);

                    viewModel.Tags = new List<ViewModels.Assets.TagViewModel>();
                    Request.Params["Tags"].Split(',').ToList().ForEach(x =>
                    {
                        viewModel.Tags.Add(new ViewModels.Assets.TagViewModel()
                        {
                            Name = x.Trim()
                        });
                    });

                    // Disassociate all asset_asset_tags for this asset
                    Data.Assets.AssetTag.DeleteAllForAsset(trans, asset.Id.Value);

                    foreach (ViewModels.Assets.TagViewModel tag in viewModel.Tags)
                    {
                        if (tag != null)
                        {
                            Common.Models.Assets.Tag mTag = Mapper.Map<Common.Models.Assets.Tag>(tag);
                            Data.Assets.Tag.Create(trans, mTag, currentUser);

                            Data.Assets.AssetTag.Create(trans, new Common.Models.Assets.AssetTag()
                            { Asset = asset, Tag = mTag }, currentUser);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                }
            }

            return RedirectToAction("Details", new { Id = asset.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Checkout(Guid id) //AssetId
        {
            ViewModels.Assets.AssetViewModel viewModel;
            Common.Models.Assets.Asset model;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Assets.Asset.Get(id, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Assets.AssetViewModel>(model);

                if (model.CheckedOutBy != null && model.CheckedOutBy.PId.HasValue)
                {
                    model.CheckedOutBy = Data.Account.Users.Get(model.CheckedOutBy.PId.Value, conn, false);
                    viewModel.CheckedOutBy = Mapper.Map<ViewModels.Account.UsersViewModel>(model.CheckedOutBy);
                }

                viewModel.Tags = new List<ViewModels.Assets.TagViewModel>();

                Data.Assets.Tag.ListForAsset(model.Id.Value, conn, false).ForEach(tag =>
                {
                    viewModel.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                });

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Checkout(ViewModels.Assets.AssetViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            
            Common.Models.Assets.Asset asset;

            asset = Mapper.Map<Common.Models.Assets.Asset>(viewModel);

            using (Data.Transaction trans = Data.Transaction.Create())
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    Data.Assets.Asset.Checkout(trans, viewModel.Id.Value, currentUser);

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

            return RedirectToAction("Details", new { Id = asset.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Checkin(Guid id) //AssetId
        {
            ViewModels.Assets.CheckinAssetViewModel viewModel;
            Common.Models.Assets.Asset asset;
            Common.Models.Matters.Matter matter;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                asset = Data.Assets.Asset.Get(id, conn, false);
                matter = Data.Assets.Asset.GetRelatedMatter(id, conn, false);

                viewModel = new ViewModels.Assets.CheckinAssetViewModel();
                viewModel.Asset = Mapper.Map<ViewModels.Assets.AssetViewModel>(asset);

                if (asset.CheckedOutBy != null && asset.CheckedOutBy.PId.HasValue)
                {
                    asset.CheckedOutBy = Data.Account.Users.Get(asset.CheckedOutBy.PId.Value, conn, false);
                    viewModel.Asset.CheckedOutBy = Mapper.Map<ViewModels.Account.UsersViewModel>(asset.CheckedOutBy);
                }

                viewModel.Tags = new List<ViewModels.Assets.TagViewModel>();

                Data.Assets.Tag.ListForAsset(asset.Id.Value, conn, false).ForEach(tag =>
                {
                    viewModel.Tags.Add(Mapper.Map<ViewModels.Assets.TagViewModel>(tag));
                });
            }

            viewModel.Files = null;

            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Checkin(Guid id, ViewModels.Assets.CheckinAssetViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;

            // FS layout
            // Asset/Version/File
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

                    // current asset
                    asset = Data.Assets.Asset.Get(trans, id);

                    if (asset.CheckedOutBy == null ||
                        asset.CheckedOutBy.PId.Value == currentUser.PId.Value)
                    {
                        // If files submitted -> new version
                        if (viewModel.Files.Count > 0)
                        {
                            // For some reason, the files input is posting with 1 even when there is none
                            // it is always 1 and always null
                            if (viewModel.Files.Count > 1 ||
                                viewModel.Files[0] != null)
                            {
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
                                        IsSource = isSource,
                                        Extension = System.IO.Path.GetExtension(vmFile.FileName)
                                    };
                                    Common.FileSystem.File fsFile = new Common.FileSystem.File(fsAsset, fsVersion, file);

                                    Data.Assets.File.Create(trans, file, currentUser);

                                    fsFiles.Add(fsFile);

                                    vmFile.SaveAs(fsFile.Path);
                                });
                            }
                        }

                        // Update asset if needed
                        if (viewModel.Asset.IsCourtFiled != asset.IsCourtFiled ||
                            viewModel.Asset.IsFinal != asset.IsFinal)
                        {
                            asset.IsFinal = viewModel.Asset.IsFinal;
                            asset.IsCourtFiled = viewModel.Asset.IsCourtFiled;
                            asset.CheckedOutAt = null;
                            asset.CheckedOutBy = null;
                            asset = Data.Assets.Asset.Edit(trans, asset, currentUser);
                        }
                        else
                        {
                            // No asset update done -> Checkin
                            Data.Assets.Asset.Checkin(trans, id, currentUser);
                        }
                    }
                    else
                    {   // Wrong user trying to checkin
                        return RedirectToAction("Checkin", new { Id = id });
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();

                    if (fsVersion != null)
                        System.IO.Directory.Delete(fsVersion.Path, true);

                    throw ex;
                }
            }

            return RedirectToAction("Details", new { Id = id });
        }
    }
}