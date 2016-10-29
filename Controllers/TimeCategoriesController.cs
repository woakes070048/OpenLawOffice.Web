// -----------------------------------------------------------------------
// <copyright file="TimeCategoryController.cs" company="Nodine Legal, LLC">
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
    using System.Web.Mvc;
    using AutoMapper;
    using System.Collections.Generic;
    using System.Data;
    using System;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class TimeCategoriesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Timing.TimeCategoryViewModel> vmList = new List<ViewModels.Timing.TimeCategoryViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Timing.TimeCategory.List(conn, false).ForEach(x =>
                {
                    vmList.Add(Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(x));
                });
            }

            return View(vmList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            ViewModels.Timing.TimeCategoryViewModel viewModel;
            Common.Models.Timing.TimeCategory model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Timing.TimeCategory.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(model);

                PopulateCoreDetails(viewModel, conn);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Timing.TimeCategoryViewModel viewModel;
            Common.Models.Timing.TimeCategory model;

            model = Data.Timing.TimeCategory.Get(id);

            viewModel = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Timing.TimeCategoryViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Timing.TimeCategory model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Timing.TimeCategory>(viewModel);

                    model = Data.Timing.TimeCategory.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Timing.TimeCategoryViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Timing.TimeCategory model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Timing.TimeCategory>(viewModel);

                    model = Data.Timing.TimeCategory.Create(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Index");
                }
                catch(Exception ex)
                {
                    trans.Rollback();
                    return Create();
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id)
        {
            ViewModels.Timing.TimeCategoryViewModel viewModel;
            Common.Models.Timing.TimeCategory model;

            model = Data.Timing.TimeCategory.Get(id);

            viewModel = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id, ViewModels.Timing.TimeCategoryViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Timing.TimeCategory model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Timing.TimeCategory>(viewModel);

                    model = Data.Timing.TimeCategory.Disable(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Index");
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