// -----------------------------------------------------------------------
// <copyright file="MatterContactController.cs" company="Nodine Legal, LLC">
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
    public class MatterContactController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult SelectContactToAssign(Guid id)
        {
            Common.Models.Matters.Matter matter;
            List<ViewModels.Contacts.SelectableContactViewModel> modelList = new List<ViewModels.Contacts.SelectableContactViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Contacts.Contact.List(conn, false).ForEach(x =>
                {
                    modelList.Add(Mapper.Map<ViewModels.Contacts.SelectableContactViewModel>(x));
                });

                matter = Data.Matters.Matter.Get(id, conn, false);
            }

            ViewData["MatterId"] = matter.Id.Value;
            ViewData["Matter"] = matter.Title;

            return View(modelList);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(int id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Matters.MatterContactViewModel vm;
            Guid matterId = Guid.Empty;

            if (Request["MatterId"] == null)
                return View("InvalidRequest");

            if (!Guid.TryParse(Request["MatterId"], out matterId))
                return View("InvalidRequest");

            vm = new ViewModels.Matters.MatterContactViewModel();
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                vm.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                    Data.Matters.Matter.Get(matterId, conn, false));
                vm.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                    Data.Contacts.Contact.Get(id, conn, false));
                matter = Data.Matters.Matter.Get(matterId, conn, false);
            }

            ViewData["MatterId"] = matter.Id.Value;
            ViewData["Matter"] = matter.Title;

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(ViewModels.Matters.MatterContactViewModel model)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.MatterContact matterContact;

            // We need to reset the Id of the model as it is picking up the id from the route,
            // which is incorrect
            model.Id = null;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    matterContact = Data.Matters.MatterContact.Get(model.Matter.Id.Value, model.Contact.Id.Value);

                    if (matterContact == null)
                    { // Create
                        matterContact = Mapper.Map<Common.Models.Matters.MatterContact>(model);
                        matterContact = Data.Matters.MatterContact.Create(matterContact, currentUser);
                    }
                    else
                    { // Enable
                        matterContact = Mapper.Map<Common.Models.Matters.MatterContact>(model);
                        matterContact = Data.Matters.MatterContact.Enable(matterContact, currentUser);
                    }

                    if (model.Role == "Lead Attorney")
                    {
                        Common.Models.Matters.Matter matter = Data.Matters.Matter.Get(model.Matter.Id.Value);
                        matter.LeadAttorney = Mapper.Map<Common.Models.Contacts.Contact>(model.Contact);
                        Data.Matters.Matter.Edit(matter, currentUser);
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    return RedirectToAction("AssignContact", "MatterContact", 
                        new { Id = model.Contact.Id, MatterId = model.Matter.Id });
                }
            }

            return RedirectToAction("Contacts", "Matters",
                new { id = matterContact.Matter.Id.Value.ToString() });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Matters.MatterContactViewModel viewModel;
            Common.Models.Matters.MatterContact model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.MatterContact.Get(id, conn, false);
                model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);
                model.Contact = Data.Contacts.Contact.Get(model.Contact.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.MatterContactViewModel>(model);
                viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);
                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);

                if (model.AttorneyFor != null && model.AttorneyFor.Id.HasValue)
                    viewModel.AttorneyFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.AttorneyFor.Id.Value, conn, false));

                if (model.SupportStaffFor != null && model.SupportStaffFor.Id.HasValue)
                    viewModel.SupportStaffFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.SupportStaffFor.Id.Value, conn, false));

                if (model.ThirdPartyPayorFor != null && model.ThirdPartyPayorFor.Id.HasValue)
                    viewModel.ThirdPartyPayorFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.ThirdPartyPayorFor.Id.Value, conn, false));
            }

            ViewData["MatterId"] = model.Matter.Id.Value;
            ViewData["Matter"] = model.Matter.Title;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id, ViewModels.Matters.MatterContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.MatterContact model, modelCurrent;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    modelCurrent = Data.Matters.MatterContact.Get(id);

                    model = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel);

                    model.Matter = modelCurrent.Matter;
                    model.Contact = modelCurrent.Contact;

                    model = Data.Matters.MatterContact.Edit(model, currentUser);

                    if (model.Role == "Lead Attorney")
                    {
                        model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value);
                        model.Matter.LeadAttorney = model.Contact;
                        Data.Matters.Matter.Edit(model.Matter, currentUser);
                    }

                    trans.Commit();

                    return RedirectToAction("Contacts", "Matters",
                        new { id = model.Matter.Id.Value.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            ViewModels.Matters.MatterContactViewModel viewModel;
            Common.Models.Matters.MatterContact model;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                model = Data.Matters.MatterContact.Get(id, conn, false);
                model.Matter = Data.Matters.Matter.Get(model.Matter.Id.Value, conn, false);
                model.Contact = Data.Contacts.Contact.Get(model.Contact.Id.Value, conn, false);

                viewModel = Mapper.Map<ViewModels.Matters.MatterContactViewModel>(model);
                viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(model.Matter);
                viewModel.Contact = Mapper.Map<ViewModels.Contacts.ContactViewModel>(model.Contact);

                if (model.AttorneyFor != null && model.AttorneyFor.Id.HasValue)
                    viewModel.AttorneyFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.AttorneyFor.Id.Value, conn, false));

                if (model.SupportStaffFor != null && model.SupportStaffFor.Id.HasValue)
                    viewModel.SupportStaffFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.SupportStaffFor.Id.Value, conn, false));

                if (model.ThirdPartyPayorFor != null && model.ThirdPartyPayorFor.Id.HasValue)
                    viewModel.ThirdPartyPayorFor = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(model.ThirdPartyPayorFor.Id.Value, conn, false));

                PopulateCoreDetails(viewModel, conn);
            }

            ViewData["MatterId"] = model.Matter.Id.Value;
            ViewData["Matter"] = model.Matter.Title;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id)
        {
            return Details(id);
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult Delete(int id, ViewModels.Matters.MatterContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.MatterContact model;
            Guid matterId;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(User.Identity.Name);

                    model = Data.Matters.MatterContact.Get(viewModel.Id.Value);
                    matterId = model.Matter.Id.Value;

                    model = Data.Matters.MatterContact.Disable(model, currentUser);

                    if (model.Role == "Lead Attorney")
                    {
                        Common.Models.Matters.Matter matter = Data.Matters.Matter.Get(model.Matter.Id.Value);
                        matter.LeadAttorney = null;
                        Data.Matters.Matter.Edit(matter, currentUser);
                    }

                    trans.Commit();
                    return RedirectToAction("Contacts", "Matters",
                        new { id = matterId.ToString() });
                }
                catch
                {
                    trans.Rollback();
                    return Edit(id);
                }
            }
        }
    }
}