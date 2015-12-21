// -----------------------------------------------------------------------
// <copyright file="TaskResponsibleUsersController.cs" company="Nodine Legal, LLC">
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
    public class TaskResponsibleUsersController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Tasks.TaskResponsibleUserViewModel viewModel;
            Common.Models.Tasks.TaskResponsibleUser model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.TaskResponsibleUser.Get(id, conn, false);
                model.Task = Data.Tasks.Task.Get(model.Task.Id.Value, conn, false);
                model.User = Data.Account.Users.Get(model.User.PId.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskResponsibleUserViewModel>(model);
                viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Task);
                viewModel.User = Mapper.Map<ViewModels.Account.UsersViewModel>(model.User);

                PopulateCoreDetails(viewModel, conn);

                matter = Data.Tasks.Task.GetRelatedMatter(model.Task.Id.Value, conn, false);
            }

            ViewBag.Task = model.Task.Title;
            ViewBag.TaskId = model.Task.Id;
            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create(long id)
        {
            Common.Models.Matters.Matter matter;
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Tasks.Task task;
            ViewModels.Tasks.TaskViewModel taskViewModel;

            userViewModelList = new List<ViewModels.Account.UsersViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                task = Data.Tasks.Task.Get(id, conn, false);

                taskViewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(task);

                Data.Account.Users.List(conn, false).ForEach(x =>
                {
                    userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                });

                ViewBag.UserList = userViewModelList;

                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
            }

            ViewBag.Task = task.Title;
            ViewBag.TaskId = task.Id;
            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(new ViewModels.Tasks.TaskResponsibleUserViewModel() { Task = taskViewModel });
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Tasks.TaskResponsibleUserViewModel viewModel)
        {
            Common.Models.Tasks.TaskResponsibleUser model;
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskResponsibleUser currentResponsibleUser;
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Tasks.Task task;
            ViewModels.Tasks.TaskViewModel taskViewModel;

            model = Mapper.Map<Common.Models.Tasks.TaskResponsibleUser>(viewModel);


            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    // Is there already an entry for this user?
                    currentResponsibleUser = Data.Tasks.TaskResponsibleUser.GetIgnoringDisable(trans,
                        long.Parse(RouteData.Values["Id"].ToString()), currentUser.PId.Value);

                    if (currentResponsibleUser != null)
                    { // Update
                        if (!currentResponsibleUser.Disabled.HasValue)
                        {
                            ModelState.AddModelError("User", "This user already has a responsibility.");

                            userViewModelList = new List<ViewModels.Account.UsersViewModel>();

                            task = Data.Tasks.Task.Get(trans, currentResponsibleUser.Task.Id.Value);

                            taskViewModel = Mapper.Map<ViewModels.Tasks.TaskViewModel>(task);

                            Data.Account.Users.List(trans).ForEach(x =>
                            {
                                userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                            });

                            ViewBag.UserList = userViewModelList;

                            return View(new ViewModels.Tasks.TaskResponsibleUserViewModel() { Task = taskViewModel });
                        }

                        model.Id = currentResponsibleUser.Id;
                        model.Responsibility = model.Responsibility;

                        // Remove disability
                        model = Data.Tasks.TaskResponsibleUser.Enable(trans, model, currentUser);

                        // Update responsibility
                        model = Data.Tasks.TaskResponsibleUser.Edit(trans, model, currentUser);
                    }
                    else
                    { // Insert
                        model = Data.Tasks.TaskResponsibleUser.Create(trans, model, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Tasks", new { Id = model.Task.Id.Value.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    return Create(viewModel.Task.Id.Value);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Tasks.TaskResponsibleUserViewModel viewModel;
            List<ViewModels.Account.UsersViewModel> userViewModelList;
            Common.Models.Tasks.TaskResponsibleUser model;

            userViewModelList = new List<ViewModels.Account.UsersViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.TaskResponsibleUser.Get(id, conn, false);
                model.Task = Data.Tasks.Task.Get(model.Task.Id.Value, conn, false);
                model.User = Data.Account.Users.Get(model.User.PId.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskResponsibleUserViewModel>(model);
                viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Task);
                viewModel.User = Mapper.Map<ViewModels.Account.UsersViewModel>(model.User);

                Data.Account.Users.List(conn, false).ForEach(x =>
                {
                    userViewModelList.Add(Mapper.Map<ViewModels.Account.UsersViewModel>(x));
                });

                ViewBag.UserList = userViewModelList;

                matter = Data.Tasks.Task.GetRelatedMatter(model.Task.Id.Value, conn, false);
            }

            ViewBag.Task = model.Task.Title;
            ViewBag.TaskId = model.Task.Id;
            ViewBag.Matter = matter.Title;
            ViewBag.MatterId = matter.Id;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Tasks.TaskResponsibleUserViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskResponsibleUser model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Tasks.TaskResponsibleUser>(viewModel);

                    model = Data.Tasks.TaskResponsibleUser.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Tasks", new { Id = model.Task.Id.Value });
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(Guid id)
        {
            return Details(id);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(Guid id, ViewModels.Tasks.TaskResponsibleUserViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskResponsibleUser model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Data.Tasks.TaskResponsibleUser.Get(trans, id);

                    model = Data.Tasks.TaskResponsibleUser.Disable(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("ResponsibleUsers", "Tasks", new { Id = model.Task.Id.Value });
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