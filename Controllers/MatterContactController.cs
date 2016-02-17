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
    using System.Web.Mvc;
    using AutoMapper;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class MatterContactController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(Guid id)
        {
            Common.Models.Matters.Matter matter;
            ViewModels.Matters.CreateMatterContactViewModel vm;

            vm = new ViewModels.Matters.CreateMatterContactViewModel();
            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                matter = Data.Matters.Matter.Get(id, conn, false);
                vm.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(matter);
            }
            
            return View(vm);
        }

        private void MatterContactAssignment(
            ViewModels.Matters.MatterContactViewModel viewModel, 
            Common.Models.Matters.Matter matter,
            Common.Models.Account.Users currentUser, 
            Data.Transaction trans)
        {
            if (viewModel != null && viewModel.Contact != null
                && viewModel.Contact.Id.HasValue)
            {
                Common.Models.Matters.MatterContact mcOld;

                mcOld = Data.Matters.MatterContact.Get(matter.Id.Value, viewModel.Contact.Id.Value);

                if (mcOld == null)
                { // Create
                    Common.Models.Matters.MatterContact mcNew;
                    mcNew = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel);
                    mcNew.Matter = matter;
                    mcNew = Data.Matters.MatterContact.Create(trans, mcNew, currentUser);
                }
                else
                { // Enable
                    Common.Models.Matters.MatterContact mcNew;
                    mcNew = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel);
                    mcNew.Matter = matter;
                    mcNew.Id = mcOld.Id;
                    mcNew = Data.Matters.MatterContact.Enable(trans, mcNew, currentUser);
                    mcNew = Data.Matters.MatterContact.Edit(trans, mcNew, currentUser);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Login, User")]
        public ActionResult AssignContact(ViewModels.Matters.CreateMatterContactViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter matter;

            if (viewModel.Matter == null)
                viewModel.Matter = new ViewModels.Matters.MatterViewModel();

            if (!viewModel.Matter.Id.HasValue)
                viewModel.Matter.Id = Guid.Parse((string)RouteData.Values["Id"]);

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    matter = Data.Matters.Matter.Get(trans, viewModel.Matter.Id.Value);

                    MatterContactAssignment(viewModel.Contact1, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact2, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact3, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact4, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact5, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact6, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact7, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact8, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact9, matter, currentUser, trans);
                    MatterContactAssignment(viewModel.Contact10, matter, currentUser, trans);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Details", "Matters",
                new { id = viewModel.Matter.Id });
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

                    modelCurrent = Data.Matters.MatterContact.Get(trans, id);
                    modelCurrent.Contact = Data.Contacts.Contact.Get(trans, modelCurrent.Contact.Id.Value);

                    model = Mapper.Map<Common.Models.Matters.MatterContact>(viewModel);

                    model.Matter = modelCurrent.Matter;
                    model.Contact = modelCurrent.Contact;

                    model = Data.Matters.MatterContact.Edit(model, currentUser);

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
                    model.Contact = Data.Contacts.Contact.Get(trans, model.Contact.Id.Value);
                    matterId = model.Matter.Id.Value;

                    model = Data.Matters.MatterContact.Disable(model, currentUser);

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