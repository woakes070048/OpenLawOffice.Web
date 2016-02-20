// -----------------------------------------------------------------------
// <copyright file="TaskAssignedContactsController.cs" company="Nodine Legal, LLC">
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
    public class TaskAssignedContactsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult SelectContactToAssign(long id)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Tasks.Task task;
            List<ViewModels.Contacts.SelectableContactViewModel> modelList;

            modelList = new List<ViewModels.Contacts.SelectableContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    modelList.Add(Mapper.Map<ViewModels.Contacts.SelectableContactViewModel>(x));
                });

                task = Data.Tasks.Task.Get(id, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(modelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(int id)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Tasks.Task task;
            long taskId = 0;
            ViewModels.Tasks.TaskAssignedContactViewModel vm;

            if (Request["TaskId"] == null)
                return View("InvalidRequest");

            if (!long.TryParse(Request["TaskId"], out taskId))
                return View("InvalidRequest");

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                task = Data.Tasks.Task.Get(taskId, conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);

                vm = new ViewModels.Tasks.TaskAssignedContactViewModel();
                vm.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(task);
                vm.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                    Data.Contacts.Contact.Get(id, conn, false));
                vm.AssignmentType = ViewModels.AssignmentTypeViewModel.Delegated;
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(ViewModels.Tasks.TaskAssignedContactViewModel model)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskAssignedContact taskContact;

            // We need to reset the Id of the model as it is picking up the id from the route,
            // which is incorrect
            model.Id = null;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    taskContact = Data.Tasks.TaskAssignedContact.Get(trans, model.Task.Id.Value, model.Contact.Id.Value);

                    if (taskContact == null)
                    { // Create
                        taskContact = Mapper.Map<Common.Models.Tasks.TaskAssignedContact>(model);
                        taskContact = Data.Tasks.TaskAssignedContact.Create(trans, taskContact, currentUser);
                    }
                    else
                    { // Enable
                        Guid id = taskContact.Id.Value;
                        taskContact = Mapper.Map<Common.Models.Tasks.TaskAssignedContact>(model);
                        taskContact.Id = id;
                        taskContact = Data.Tasks.TaskAssignedContact.Enable(trans, taskContact, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("Contacts", "Tasks",
                        new { id = taskContact.Task.Id.Value.ToString() });
                }
                catch(Exception e)
                {
                    trans.Rollback();
                    return AssignContact(model.Contact.Id.Value);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            Common.Models.Matters.Matter matter;
            Common.Models.Tasks.TaskAssignedContact model;
            ViewModels.Tasks.TaskAssignedContactViewModel viewModel;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.TaskAssignedContact.Get(id, conn, false);
                model.Task = Data.Tasks.Task.Get(model.Task.Id.Value, conn, false);
                model.Contact = Data.Contacts.Contact.Get(model.Contact.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskAssignedContactViewModel>(model);
                viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Task);
                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);

                matter = Data.Tasks.Task.GetRelatedMatter(model.Task.Id.Value, conn, false);
            }

            ViewBag.Task = model.Task;
            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Tasks.TaskAssignedContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskAssignedContact currentModel;
            Common.Models.Tasks.TaskAssignedContact model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    currentModel = Data.Tasks.TaskAssignedContact.Get(trans, id);

                    model = Mapper.Map<Common.Models.Tasks.TaskAssignedContact>(viewModel);
                    model.Contact = currentModel.Contact;
                    model.Task = currentModel.Task;

                    model = Data.Tasks.TaskAssignedContact.Edit(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Contacts", "Tasks",
                        new { id = model.Task.Id.Value.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Tasks.TaskAssignedContactViewModel viewModel;
            Common.Models.Tasks.TaskAssignedContact model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Tasks.TaskAssignedContact.Get(id, conn, false);
                model.Task = Data.Tasks.Task.Get(model.Task.Id.Value, conn, false);
                model.Contact = Data.Contacts.Contact.Get(model.Contact.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Tasks.TaskAssignedContactViewModel>(model);
                viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(model.Task);
                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);

                PopulateCoreDetails(viewModel, conn);

                matter = Data.Tasks.Task.GetRelatedMatter(model.Task.Id.Value, conn, false);
            }

            ViewBag.Task = model.Task;
            ViewBag.Matter = matter;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(Guid id)
        {
            return Details(id);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(Guid id, ViewModels.Tasks.TaskAssignedContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskAssignedContact currentModel;
            Common.Models.Tasks.TaskAssignedContact model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    currentModel = Data.Tasks.TaskAssignedContact.Get(trans, id);

                    model = Mapper.Map<Common.Models.Tasks.TaskAssignedContact>(viewModel);
                    model.Contact = currentModel.Contact;
                    model.Task = currentModel.Task;

                    model = Data.Tasks.TaskAssignedContact.Disable(trans, model, currentUser);

                    trans.Commit();

                    return RedirectToAction("Contacts", "Tasks",
                        new { id = model.Task.Id.Value.ToString() });
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