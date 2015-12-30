// -----------------------------------------------------------------------
// <copyright file="CourtTypesController.cs" company="Nodine Legal, LLC">
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

    [HandleError(View = "Errors/Index", Order = 10)]
    public class CourtTypesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Matters.CourtTypeViewModel> vmList = new List<ViewModels.Matters.CourtTypeViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Matters.CourtType.List(conn, false).ForEach(x =>
                {
                    vmList.Add(Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(x));
                });
            }

            return View(vmList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            ViewModels.Matters.CourtTypeViewModel viewModel;
            Common.Models.Matters.CourtType model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.CourtType.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(model);

                PopulateCoreDetails(viewModel, conn);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Matters.CourtTypeViewModel viewModel;
            Common.Models.Matters.CourtType model;

            model = Data.Matters.CourtType.Get(id);

            viewModel = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Matters.CourtTypeViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.CourtType model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.CourtType>(viewModel);

                    model = Data.Matters.CourtType.Edit(trans, model, currentUser);

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
        public ActionResult Create(ViewModels.Matters.CourtTypeViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.CourtType model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.CourtType>(viewModel);

                    model = Data.Matters.CourtType.Create(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    return Create();
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id)
        {
            ViewModels.Matters.CourtTypeViewModel viewModel;
            Common.Models.Matters.CourtType model;

            model = Data.Matters.CourtType.Get(id);

            viewModel = Mapper.Map<ViewModels.Matters.CourtTypeViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id, ViewModels.Matters.CourtTypeViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.CourtType model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Matters.CourtType>(viewModel);

                    model = Data.Matters.CourtType.Disable(trans, model, currentUser);

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