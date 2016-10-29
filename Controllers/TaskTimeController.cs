// -----------------------------------------------------------------------
// <copyright file="TaskTimeController.cs" company="Nodine Legal, LLC">
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

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using System.Web.Security;
using System.Data;

namespace OpenLawOffice.Web.Controllers
{
    [HandleError(View = "Errors/Index", Order = 10)]
    public class TaskTimeController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult SelectContactToAssign()
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

                task = Data.Tasks.Task.Get(long.Parse(Request["TaskId"]), conn, false);
                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;

            return View(modelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            long taskId;
            int contactId;
            Common.Models.Account.Users currentUser;
            ViewModels.Tasks.TaskTimeViewModel viewModel;
            Common.Models.Matters.Matter matter;
            Common.Models.Tasks.Task task;
            Common.Models.Contacts.Contact contact;
            Common.Models.Timing.TimeCategory timeCategory = null;
            List<Common.Models.Timing.TimeCategory> timeCategoryList;

            // Every TaskTime must be created from a task, so we should always know the TaskId
            taskId = long.Parse(Request["TaskId"]);
            contactId = int.Parse(Request["ContactId"]);

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                // Load task & contact
                currentUser = Data.Account.Users.Get(User.Identity.Name);
                task = Data.Tasks.Task.Get(taskId, conn, false);

                contact = Data.Contacts.Contact.Get(contactId, conn, false);

                timeCategory = Data.Timing.TimeCategory.Get(1, conn, false);

                if (timeCategory == null || !timeCategory.Id.HasValue)
                {
                    timeCategory = Data.Timing.TimeCategory.Create(new Common.Models.Timing.TimeCategory()
                    {
                        Title = "Standard"
                    }, currentUser);
                }

                viewModel = new ViewModels.Tasks.TaskTimeViewModel()
                {
                    Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(task),
                    Time = new ViewModels.Timing.TimeViewModel()
                    {
                        Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact),
                        TimeCategory = Mapper.Map<ViewModels.Timing.TimeCategoryViewModel>(timeCategory),
                        Start = DateTime.Now,
                        Billable = true
                    }
                };

                matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);

                timeCategoryList = Data.Timing.TimeCategory.List(conn, false);
                timeCategoryList.Insert(0, new Common.Models.Timing.TimeCategory()
                {
                    Id = 0,
                    Title = "Standard"
                });

                viewModel.Time.TimeCategory.Id = 0;
            }

            ViewBag.Task = task;
            ViewBag.Matter = matter;
            ViewBag.TimeCategoryList = timeCategoryList;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Tasks.TaskTimeViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Tasks.TaskTime taskTime;
            List<Common.Models.Timing.TimeCategory> timeCategoryList;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);
                    taskTime = Mapper.Map<Common.Models.Tasks.TaskTime>(viewModel);
                    taskTime.Time = Mapper.Map<Common.Models.Timing.Time>(viewModel.Time);

                    if (viewModel.Time.Stop.HasValue)
                    {
                        List<Common.Models.Timing.Time> conflicts = Data.Timing.Time.ListConflictingTimes(viewModel.Time.Start,
                            viewModel.Time.Stop.Value, viewModel.Time.Worker.Id.Value);

                        if (conflicts.Count > 0)
                        { // conflict found
                            long taskId;
                            int contactId;
                            string errorListString = "";
                            Common.Models.Tasks.Task task;
                            Common.Models.Contacts.Contact contact;
                            Common.Models.Matters.Matter matter;

                            taskId = long.Parse(Request["TaskId"]);
                            contactId = int.Parse(Request["ContactId"]);
                            task = Data.Tasks.Task.Get(taskId);
                            contact = Data.Contacts.Contact.Get(contactId);
                            matter = Data.Tasks.Task.GetRelatedMatter(taskId);

                            timeCategoryList = Data.Timing.TimeCategory.List(trans);
                            timeCategoryList.Insert(0, new Common.Models.Timing.TimeCategory()
                            {
                                Id = 0,
                                Title = "Standard"
                            });

                            viewModel.Task = Mapper.Map<ViewModels.Tasks.TaskViewModel>(task);
                            viewModel.Time.Worker = Mapper.Map<ViewModels.Contacts.ContactViewModel>(contact);
                    
                            ViewBag.Task = task;
                            ViewBag.Matter = matter;
                            ViewBag.TimeCategoryList = timeCategoryList;

                            foreach (Common.Models.Timing.Time time in conflicts)
                            {
                                time.Worker = Data.Contacts.Contact.Get(time.Worker.Id.Value);
                                errorListString += "<li>" + time.Worker.DisplayName + 
                                    "</a> worked from " + time.Start.ToString("M/d/yyyy h:mm tt");
                        
                                if (time.Stop.HasValue)
                                    errorListString += " to " + time.Stop.Value.ToString("M/d/yyyy h:mm tt") +
                                        " [<a href=\"/Timing/Edit/" + time.Id.Value.ToString() + "\">edit</a>]";
                                else
                                    errorListString += " to an unknown time " +
                                        "[<a href=\"/Timing/Edit/" + time.Id.Value.ToString() + "\">edit</a>]";

                                errorListString += "</li>";
                            }
                    
                            ViewBag.ErrorMessage = "Time conflicts with the following other time entries:<ul>" + errorListString + "</ul>";
                            return View(viewModel);
                        }
                    }

                    taskTime.Time = Data.Timing.Time.Create(taskTime.Time, currentUser);
                    taskTime = Data.Tasks.TaskTime.Create(taskTime, currentUser);

                    trans.Commit();

                    return RedirectToAction("Time", "Tasks", new { Id = Request["TaskId"] });
                }
                catch
                {
                    trans.Rollback();
                    return RedirectToAction("Create", "TaskTime", new { TaskId = Request["TaskId"], ContactId = Request["ContactId"] });
                }
            }
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult RelateTask()
        {
            return View();
        }
    }
}