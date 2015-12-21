// -----------------------------------------------------------------------
// <copyright file="TaskTemplatesController.cs" company="Nodine Legal, LLC">
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
    using System.Web;
    using System.Web.Mvc;
    using AutoMapper;
    using System.Collections.Generic;
    using System.IO;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class TaskTemplatesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Tasks.TaskTemplateViewModel> vmList =
                new List<ViewModels.Tasks.TaskTemplateViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Tasks.TaskTemplate.List(conn, false).ForEach(x =>
                {
                    vmList.Add(Mapper.Map<ViewModels.Tasks.TaskTemplateViewModel>(x));
                });
            }

            return View(vmList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            ViewModels.Tasks.TaskTemplateViewModel viewModel;
            Common.Models.Tasks.TaskTemplate model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.TaskTemplate.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskTemplateViewModel>(model);

                PopulateCoreDetails(viewModel, conn);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Tasks.TaskTemplateViewModel viewModel;
            Common.Models.Tasks.TaskTemplate model;

            model = Data.Tasks.TaskTemplate.Get(id);

            viewModel = Mapper.Map<ViewModels.Tasks.TaskTemplateViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Tasks.TaskTemplateViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskTemplate model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.TaskTemplate>(viewModel);

                    model = Data.Tasks.TaskTemplate.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = id });
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
        public ActionResult Create(ViewModels.Tasks.TaskTemplateViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskTemplate model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.TaskTemplate>(viewModel);

                    model = Data.Tasks.TaskTemplate.Create(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Details", new { Id = model.Id });
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
            ViewModels.Tasks.TaskTemplateViewModel viewModel;
            Common.Models.Tasks.TaskTemplate model;

            model = Data.Tasks.TaskTemplate.Get(id);

            viewModel = Mapper.Map<ViewModels.Tasks.TaskTemplateViewModel>(model);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id, ViewModels.Tasks.TaskTemplateViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskTemplate model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.TaskTemplate>(viewModel);

                    model = Data.Tasks.TaskTemplate.Disable(trans, model, currentUser);

                    // Don't delete the file - in theory a system admin could enable this again
                    // deleting file would leave a form in the DB without a corresponding file

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