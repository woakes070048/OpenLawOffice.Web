// -----------------------------------------------------------------------
// <copyright file="NotesController.cs" company="Nodine Legal, LLC">
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
    using AutoMapper.Internal;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class NotesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            ViewModels.Notes.NoteViewModel viewModel;
            Common.Models.Notes.Note model;
            Common.Models.Matters.Matter noteMatter;
            Common.Models.Tasks.Task noteTask;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Notes.Note.Get(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Notes.NoteViewModel>(model);

                noteMatter = Data.Notes.NoteMatter.GetRelatedMatter(id, conn, false);

                noteTask = Data.Notes.NoteTask.GetRelatedTask(id, conn, false);

                if (noteTask != null)
                { // Note belongs to a task
                    noteMatter = Data.Tasks.Task.GetRelatedMatter(noteTask.Id.Value, conn, false);
                    ViewBag.Task = noteTask;
                }

                viewModel.NoteNotifications = new List<ViewModels.Notes.NoteNotificationViewModel>();
                Data.Notes.NoteNotification.ListForNote(id, conn, false).ForEach(x =>
                {
                    x.Contact = Data.Contacts.Contact.Get(x.Contact.Id.Value, conn, false);
                    ViewModels.Notes.NoteNotificationViewModel vm = Mapper.Map<ViewModels.Notes.NoteNotificationViewModel>(x);
                    vm.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(x.Contact);
                    viewModel.NoteNotifications.Add(vm);
                });

                PopulateCoreDetails(viewModel, conn);
            }

            ViewBag.Matter = noteMatter;
            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult ClearNotification(Guid id, int? employeeId)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Notes.NoteNotification model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                currentUser = Data.Account.Users.Get(User.Identity.Name, conn, false);

                model = Data.Notes.NoteNotification.Get(id, conn, false);

                model = Data.Notes.NoteNotification.Clear(model, currentUser, conn, false);
            }

            return RedirectToAction("Index", "Home", new { Id = employeeId });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id)
        {
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            Common.Models.Notes.Note model;
            Common.Models.Matters.Matter matter;
            Common.Models.Tasks.Task task;
            ViewModels.Notes.NoteViewModel viewModel;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Notes.Note.Get(id, conn, false);
                matter = Data.Notes.Note.GetMatter(id, conn, false);
                task = Data.Notes.Note.GetTask(id, conn, false);

                viewModel = Mapper.Map<ViewModels.Notes.NoteViewModel>(model);

                if (task != null)
                {
                    matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
                    ViewBag.Task = task;
                }

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });

                List<Common.Models.Notes.NoteNotification> notesNotifications = 
                    Data.Notes.NoteNotification.ListForNote(id, conn, false);
                viewModel.NotifyContactIds = new string[notesNotifications.Count];
                for (int i = 0; i < notesNotifications.Count; i++)
                {
                    viewModel.NotifyContactIds[i] = notesNotifications[i].Contact.Id.Value.ToString();
                }
            }

            ViewBag.Matter = matter;
            ViewBag.EmployeeContactList = employeeContactList;

            return View(viewModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(Guid id, ViewModels.Notes.NoteViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Notes.Note model;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Notes.Note>(viewModel);
                    model.Body = new Ganss.XSS.HtmlSanitizer().Sanitize(model.Body);
                    model = Data.Notes.Note.Edit(trans, model, currentUser);

                    if (viewModel.NotifyContactIds != null)
                    {
                        viewModel.NotifyContactIds.Each(x =>
                        {
                            Data.Notes.NoteNotification.Create(trans, new Common.Models.Notes.NoteNotification()
                            {
                                Contact = new Common.Models.Contacts.Contact() { Id = int.Parse(x) },
                                Note = model,
                                Cleared = null
                            }, currentUser);
                        });
                    }

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
            List<ViewModels.Contacts.ContactViewModel> employeeContactList;
            Common.Models.Matters.Matter matter = null;
            Common.Models.Tasks.Task task = null;

            employeeContactList = new List<ViewModels.Contacts.ContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                if (Request["MatterId"] != null)
                {
                    matter = Data.Matters.Matter.Get(Guid.Parse(Request["MatterId"]), conn, false);
                }
                else if (Request["TaskId"] != null)
                {
                    task = Data.Tasks.Task.Get(long.Parse(Request["TaskId"]), conn, false);
                    matter = Data.Tasks.Task.GetRelatedMatter(task.Id.Value, conn, false);
                    ViewBag.Task = task;
                }

                Data.Contacts.Contact.ListEmployeesOnly(conn, false).ForEach(x =>
                {
                    employeeContactList.Add(Mapper.Map<ViewModels.Contacts.ContactViewModel>(x));
                });
            }

            ViewBag.Matter = matter;
            ViewBag.EmployeeContactList = employeeContactList;

            return View(new ViewModels.Notes.NoteViewModel() { Timestamp = DateTime.Now });
        }

        [ValidateInput(false)]
        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Create(ViewModels.Notes.NoteViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Notes.Note model;
            Guid matterid, eventid;
            long taskid;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    model = Mapper.Map<Common.Models.Notes.Note>(viewModel);

                    model.Body = new Ganss.XSS.HtmlSanitizer().Sanitize(model.Body);

                    model = Data.Notes.Note.Create(trans, model, currentUser);

                    if (viewModel.NotifyContactIds != null)
                    {
                        viewModel.NotifyContactIds.Each(x =>
                        {
                            Data.Notes.NoteNotification.Create(trans, new Common.Models.Notes.NoteNotification()
                            {
                                Contact = new Common.Models.Contacts.Contact() { Id = int.Parse(x) },
                                Note = model,
                                Cleared = null
                            }, currentUser);
                        });
                    }

                    if (Request["MatterId"] != null)
                    {
                        matterid = Guid.Parse(Request["MatterId"]);

                        Data.Notes.Note.RelateMatter(trans, model, matterid, currentUser);

                        trans.Commit();

                        return RedirectToAction("Details", "Matters", new { Id = matterid });
                    }
                    else if (Request["TaskId"] != null)
                    {
                        taskid = long.Parse(Request["TaskId"]);

                        Data.Notes.Note.RelateTask(trans, model, taskid, currentUser);

                        trans.Commit();

                        return RedirectToAction("Details", "Tasks", new { Id = taskid });
                    }
                    else if (Request["EventId"] != null)
                    {
                        eventid = Guid.Parse(Request["EventId"]);

                        Data.Notes.Note.RelateEvent(trans, model, eventid, currentUser);

                        trans.Commit();

                        return RedirectToAction("Details", "Events", new { Id = eventid });
                    }
                    else
                        throw new HttpRequestValidationException("Must specify a MatterId, TaskId or EventId");
                }
                catch
                {
                    trans.Rollback();

                    if (Request["MatterId"] != null)
                    {
                        matterid = Guid.Parse(Request["MatterId"]);
                        return RedirectToAction("Create", "Notes", new { MatterId = matterid });
                    }
                    else if (Request["TaskId"] != null)
                    {
                        taskid = long.Parse(Request["TaskId"]);
                        return RedirectToAction("Create", "Notes", new { TaskId = taskid });
                    }
                    else if (Request["EventId"] != null)
                    {
                        eventid = Guid.Parse(Request["EventId"]);
                        return RedirectToAction("Create", "Notes", new { EventId = eventid });
                    }
                    else
                        throw new HttpRequestValidationException("Must specify a MatterId, TaskId or EventId");
                }
            }
        }
    }
}