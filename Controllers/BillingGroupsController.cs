// -----------------------------------------------------------------------
// <copyright file="BillingGroupsController.cs" company="Nodine Legal, LLC">
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
    using System.Collections.Generic;
    using System.Web.Mvc;
    using AutoMapper;
    using System.Data;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class BillingGroupsController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Index()
        {
            List<ViewModels.Billing.BillingGroupViewModel> groups = new List<ViewModels.Billing.BillingGroupViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                Data.Billing.BillingGroup.List(conn, false).ForEach(x =>
                {
                    ViewModels.Billing.BillingGroupViewModel vm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(x);
                    vm.BillTo = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                        Data.Contacts.Contact.Get(x.BillTo.Id.Value, conn, false));
                    groups.Add(vm);
                });
            }

            return View(groups);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Invoices(int id)
        {
            ViewModels.Billing.BillingGroupViewModel bgvm;
            List<ViewModels.Billing.InvoiceViewModel> list = new List<ViewModels.Billing.InvoiceViewModel>();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                bgvm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(Data.Billing.BillingGroup.Get(id, conn, false));
                Data.Billing.BillingGroup.ListInvoicesForGroup(id, conn, false).ForEach(x =>
                {
                    list.Add(Mapper.Map<ViewModels.Billing.InvoiceViewModel>(x));
                });
            }

            ViewBag.BillingGroup = bgvm;
            return View(list);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Details(int id)
        {
            ViewModels.Billing.BillingGroupViewModel vm;
            Common.Models.Billing.BillingGroup group;
            List<Common.Models.Matters.Matter> matterMembers;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                group = Data.Billing.BillingGroup.Get(id, conn, false);
                group.BillTo = Data.Contacts.Contact.Get(group.BillTo.Id.Value, conn, false);
                matterMembers = Data.Billing.BillingGroup.ListMattersForGroup(id, conn, false);

                vm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(group);
                vm.BillTo = Mapper.Map<ViewModels.Contacts.ContactViewModel>(group.BillTo);
                vm.MatterMembers = new List<ViewModels.Matters.MatterViewModel>();
                matterMembers.ForEach(x =>
                {
                    vm.MatterMembers.Add(Mapper.Map<ViewModels.Matters.MatterViewModel>(x));
                });
                PopulateCoreDetails(vm, conn);
            }

            return View(vm);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult Create(ViewModels.Billing.BillingGroupViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Billing.BillingGroup model;

            model = Mapper.Map<Common.Models.Billing.BillingGroup>(viewModel);

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    Data.Billing.BillingGroup.Create(trans, model, currentUser);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult Edit(int id)
        {
            ViewModels.Billing.BillingGroupViewModel vm;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                vm = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(
                    Data.Billing.BillingGroup.Get(id, conn, false));
                vm.BillTo = Mapper.Map<ViewModels.Contacts.ContactViewModel>(
                    Data.Contacts.Contact.Get(vm.BillTo.Id.Value, conn, false));
            }

            return View(vm);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult Edit(int id, ViewModels.Billing.BillingGroupViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Billing.BillingGroup model;

            model = Mapper.Map<Common.Models.Billing.BillingGroup>(viewModel);

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);

                    Data.Billing.BillingGroup.Edit(trans, model, currentUser);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }

            return RedirectToAction("Index");
        }
    }
}