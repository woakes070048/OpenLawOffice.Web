// -----------------------------------------------------------------------
// <copyright file="ResponsibleUsersController.cs" company="Nodine Legal, LLC">
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
    public class ResponsibleUsersController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            Common.Models.Matters.ResponsibleUser model;
            ViewModels.Matters.ResponsibleUserViewModel viewModel;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.ResponsibleUser.Get(id, conn, false);
                model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);
                model.User = Data.Account.Users.Get(model.User.PId.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.ResponsibleUserViewModel>(model);
                viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);
                viewModel.User = Mapper.Map<ViewModels.Account.UsersViewModel>(model.User);

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.MatterId = model.Matter.Id.Value;
            ViewBag.Matter = model.Matter.Title;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create(Guid id)
        {
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Matters.Matter matter;
            ViewModels.Matters.MatterViewModel matterViewModel;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);

                matterViewModel = Mapper.Map<ViewModels.Matters.MatterViewModel>(matter);

                userViewModelList = new List<ViewModels.Account.UsersViewModel>();

                Data.Account.Users.List(conn, false).ForEach(x =>
                {
                    userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                });
            }

            ViewBag.UserList = userViewModelList;

            ViewBag.MatterId = matter.Id.Value;
            ViewBag.Matter = matter.Title;

            return View(new ViewModels.Matters.ResponsibleUserViewModel() { Matter = matterViewModel });
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Matters.ResponsibleUserViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.ResponsibleUser model;
            Common.Models.Matters.ResponsibleUser currentResponsibleUser;
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Matters.Matter matter;
            ViewModels.Matters.MatterViewModel matterViewModel;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.ResponsibleUser>(viewModel);
                    model.Matter = new Common.Models.Matters.Matter() { Id = Guid.Parse(RouteData.Values["Id"].ToString()) };

                    // Is there already an entry for this user?
                    currentResponsibleUser = Data.Matters.ResponsibleUser.GetIgnoringDisable(trans,
                        Guid.Parse(RouteData.Values["Id"].ToString()), currentUser.PId.Value);

                    if (currentResponsibleUser != null)
                    { // Update
                        if (!currentResponsibleUser.Disabled.HasValue)
                        {
                            ModelState.AddModelError("User", "This user already has a responsibility.");

                            matter = Data.Matters.Matter.Get(trans, currentResponsibleUser.Matter.Id.Value);

                            matterViewModel = Mapper.Map<ViewModels.Matters.MatterViewModel>(matter);

                            userViewModelList = new List<ViewModels.Account.UsersViewModel>();

                            Data.Account.Users.List(trans).ForEach(x =>
                            {
                                userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                            });

                            ViewBag.UserList = userViewModelList;

                            ViewBag.MatterId = matter.Id.Value;
                            ViewBag.Matter = matter.Title;
                            return View(new ViewModels.Matters.ResponsibleUserViewModel() { Matter = matterViewModel });
                        }

                        // Remove disability
                        model = Data.Matters.ResponsibleUser.Enable(trans, model, currentUser);

                        // Update responsibility
                        model.Responsibility = model.Responsibility;
                        model = Data.Matters.ResponsibleUser.Edit(trans, model, currentUser);
                    }
                    else
                    { // Insert
                        model = Data.Matters.ResponsibleUser.Create(trans, model, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Matters", new { Id = model.Matter.Id.Value.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    return Create(viewModel.Matter.Id.Value);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Matters.ResponsibleUserViewModel viewModel;
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Matters.ResponsibleUser model;

            userViewModelList = new List<ViewModels.Account.UsersViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.ResponsibleUser.Get(id, conn, false);
                model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);
                model.User = Data.Account.Users.Get(model.User.PId.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.ResponsibleUserViewModel>(model);
                viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);
                viewModel.User = Mapper.Map<ViewModels.Account.UsersViewModel>(model.User);

                Data.Account.Users.List(conn, false).ForEach(x =>
                {
                    userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                });
            }

            ViewBag.UserList = userViewModelList;

            ViewBag.MatterId = model.Matter.Id.Value;
            ViewBag.Matter = model.Matter.Title;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Matters.ResponsibleUserViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.ResponsibleUser model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.ResponsibleUser>(viewModel);

                    model = Data.Matters.ResponsibleUser.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Matters", new { Id = model.Matter.Id.Value });
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id)
        {
            return Details(id);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id, ViewModels.Matters.ResponsibleUserViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.ResponsibleUser model;
            Guid matterId;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Data.Matters.ResponsibleUser.Get(trans, viewModel.Id.Value);
                    matterId = model.Matter.Id.Value;

                    model = Data.Matters.ResponsibleUser.Disable(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Matters", new { Id = matterId.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    return Delete(id);
                }
            }
        }
    }
}