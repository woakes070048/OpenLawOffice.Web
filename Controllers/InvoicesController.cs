// -----------------------------------------------------------------------
// <copyright file="InvoicesController.cs" company="Nodine Legal, LLC">
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
    using System.Linq;

    [HandleError(View = "Errors/Index", Order = 10)]
    public class InvoicesController : BaseController
    {
        [Authorize(Roles = "Login, User")]
        public ActionResult Details(Guid id)
        {
            Common.Models.Billing.Invoice invoice = Data.Billing.Invoice.Get(id);

            if (invoice.BillingGroup != null && invoice.BillingGroup.Id.HasValue)
                return RedirectToAction("GroupDetails", new { Id = id });

            return RedirectToAction("MatterDetails", new { Id = id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult MatterEdit(Guid id)
        {
            Common.Models.Billing.BillingRate billingRate = null;
            Common.Models.Matters.Matter matter = null;
            Common.Models.Billing.Invoice invoice = null;
            ViewModels.Billing.InvoiceViewModel viewModel = new ViewModels.Billing.InvoiceViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                invoice = Data.Billing.Invoice.Get(id, conn, false);
                viewModel = Mapper.Map<ViewModels.Billing.InvoiceViewModel>(invoice);

                if (invoice.Matter != null)
                {
                    matter = Data.Matters.Matter.Get(invoice.Matter.Id.Value, conn, false);
                    viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(matter);
                    if (matter.DefaultBillingRate != null && matter.DefaultBillingRate.Id.HasValue)
                        billingRate = Data.Billing.BillingRate.Get(matter.DefaultBillingRate.Id.Value, conn, false);
                }

                invoice.BillTo = Data.Contacts.Contact.Get(invoice.BillTo.Id.Value, conn, false);

                Data.Billing.Invoice.ListInvoiceExpensesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceExpenseViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceExpenseViewModel>(x);
                    vm.Expense = Mapper.Map<ViewModels.Billing.ExpenseViewModel>(
                        Data.Billing.Expense.Get(vm.Expense.Id.Value, conn, false));
                    viewModel.Expenses.Add(vm);
                });

                Data.Billing.Invoice.ListInvoiceFeesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceFeeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceFeeViewModel>(x);
                    vm.Fee = Mapper.Map<ViewModels.Billing.FeeViewModel>(
                        Data.Billing.Fee.Get(vm.Fee.Id.Value, conn, false));
                    viewModel.Fees.Add(vm);
                });
                
                Data.Billing.Invoice.ListInvoiceTimesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceTimeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceTimeViewModel>(x);
                    vm.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(
                        Data.Timing.Time.Get(vm.Time.Id.Value, conn, false));

                    ViewModels.Billing.InvoiceTimeGroupViewModel timeGroup;
                    if (vm.Time.TimeCategory == null || !vm.Time.TimeCategory.Id.HasValue)
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == 0);
                    else
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == vm.Time.TimeCategory.Id);
                    if (timeGroup == null || timeGroup.Id == -1)
                    {
                        Common.Models.Timing.TimeCategory tc = Data.Timing.TimeCategory.Get(vm.Time.TimeCategory.Id.Value, conn, false);
                        timeGroup = new ViewModels.Billing.InvoiceTimeGroupViewModel()
                        {
                            Id = tc.Id.Value,
                            GroupName = tc.Title,
                            Times = new List<ViewModels.Billing.InvoiceTimeViewModel>()
                        };
                        timeGroup.Times.Add(vm);
                        viewModel.TimeGroups.Add(timeGroup);
                    }
                    else
                        timeGroup.Times.Add(vm);
                });
            }

            ViewData["MatterTitle"] = matter.Title;
            ViewData["CaseNumber"] = matter.CaseNumber;
            ViewData["FirmName"] = Common.Settings.Manager.Instance.System.BillingFirmName;
            ViewData["FirmAddress"] = Common.Settings.Manager.Instance.System.BillingFirmAddress;
            ViewData["FirmCity"] = Common.Settings.Manager.Instance.System.BillingFirmCity;
            ViewData["FirmState"] = Common.Settings.Manager.Instance.System.BillingFirmState;
            ViewData["FirmZip"] = Common.Settings.Manager.Instance.System.BillingFirmZip;
            ViewData["FirmPhone"] = Common.Settings.Manager.Instance.System.BillingFirmPhone;
            ViewData["FirmWeb"] = Common.Settings.Manager.Instance.System.BillingFirmWeb;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult MatterEdit(Guid id, ViewModels.Billing.InvoiceViewModel viewModel)
        {
            // Update Invoice
            // Loop Expenses
            // Loop Fees
            // Loop Times
            // Redirect to invoice viewing
            Common.Models.Account.Users currentUser;
            Common.Models.Matters.Matter matter;
            DateTime? start = null, stop = null;
            List<Common.Models.Billing.InvoiceExpense> invoiceExpenseList;
            List<Common.Models.Billing.InvoiceFee> invoiceFeeList;
            List<Common.Models.Billing.InvoiceTime> invoiceTimeList;
            Common.Models.Billing.Invoice invoice = null;

            if (!string.IsNullOrEmpty(Request["StartDate"]))
                start = DateTime.Parse(Request["StartDate"]);
            if (!string.IsNullOrEmpty(Request["StopDate"]))
                stop = DateTime.Parse(Request["StopDate"]);

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    invoice = Mapper.Map<Common.Models.Billing.Invoice>(viewModel);
                    ViewModels.Billing.InvoiceViewModel savedInvoice;
                    ViewModels.Billing.InvoiceExpenseViewModel ievm;
                    ViewModels.Billing.InvoiceFeeViewModel ifvm;
                    ViewModels.Billing.InvoiceTimeViewModel itvm;
                    
                    // Validation
                    for (int i = 0; i < viewModel.TimeGroups.Count; i++)
                    {
                        if (viewModel.TimeGroups[i].Times.Count > 0)
                        {
                            Common.Models.Timing.TimeCategory zTc;
                            Common.Models.Timing.Time zItem = Data.Timing.Time.Get(viewModel.TimeGroups[i].Times[0].Time.Id.Value);

                            if (zItem.TimeCategory == null || !zItem.TimeCategory.Id.HasValue || zItem.TimeCategory.Id.Value < 1)
                            {
                                viewModel.TimeGroups[i].GroupName = "Standard";
                                viewModel.TimeGroups[i].Id = 0;
                            }
                            else{
                                zTc = Data.Timing.TimeCategory.Get(zItem.TimeCategory.Id.Value);
                                viewModel.TimeGroups[i].GroupName = zTc.Title;
                                viewModel.TimeGroups[i].Id = zTc.Id.Value;
                            }
                        }
                    }

                    savedInvoice = Mapper.Map<ViewModels.Billing.InvoiceViewModel>(Data.Billing.Invoice.Get(trans, id));
                    savedInvoice.Expenses = new List<ViewModels.Billing.InvoiceExpenseViewModel>();
                    Data.Billing.Invoice.ListInvoiceExpensesForInvoice(trans, id).ForEach(x =>
                    {
                        savedInvoice.Expenses.Add(Mapper.Map<ViewModels.Billing.InvoiceExpenseViewModel>(x));
                    });
                    savedInvoice.Fees = new List<ViewModels.Billing.InvoiceFeeViewModel>();
                    Data.Billing.Invoice.ListInvoiceFeesForInvoice(trans, id).ForEach(x =>
                    {
                        savedInvoice.Fees.Add(Mapper.Map<ViewModels.Billing.InvoiceFeeViewModel>(x));
                    });

                    Data.Billing.Invoice.ListInvoiceTimesForInvoice(trans, id).ForEach(x =>
                    {
                        ViewModels.Billing.InvoiceTimeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceTimeViewModel>(x);
                        vm.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(Data.Timing.Time.Get(trans, vm.Time.Id.Value));
                                                
                        ViewModels.Billing.InvoiceTimeGroupViewModel timeGroup;
                        if (vm.Time.TimeCategory == null || !vm.Time.TimeCategory.Id.HasValue)
                            timeGroup = savedInvoice.TimeGroups.SingleOrDefault(y => y.Id == 0);
                        else
                            timeGroup = savedInvoice.TimeGroups.SingleOrDefault(y => y.Id == vm.Time.TimeCategory.Id);
                        if (timeGroup == null || timeGroup.Id == -1)
                        {
                            Common.Models.Timing.TimeCategory tc = Data.Timing.TimeCategory.Get(trans, vm.Time.TimeCategory.Id.Value);
                            timeGroup = new ViewModels.Billing.InvoiceTimeGroupViewModel()
                            {
                                Id = tc.Id.Value,
                                GroupName = tc.Title,
                                Times = new List<ViewModels.Billing.InvoiceTimeViewModel>()
                            };
                            timeGroup.Times.Add(vm);
                            savedInvoice.TimeGroups.Add(timeGroup);
                        }
                        else
                            timeGroup.Times.Add(vm);
                    });
                    
                    // Validation
                    for (int i = 0; i < viewModel.Expenses.Count; i++)
                    {
                        ievm = savedInvoice.Expenses.Single(x => x.Expense.Id.Value == viewModel.Expenses[i].Expense.Id.Value);
                        viewModel.Expenses[i].Expense = ievm.Expense;
                        if (string.IsNullOrEmpty(viewModel.Expenses[i].Details))
                            ModelState.AddModelError(string.Format("Expenses[{0}].Details", i), "Required");
                    };
                    for (int i = 0; i < viewModel.Fees.Count; i++)
                    {
                        ifvm = savedInvoice.Fees.Single(x => x.Fee.Id.Value == viewModel.Fees[i].Fee.Id.Value);
                        viewModel.Fees[i].Fee = ifvm.Fee;
                        if (string.IsNullOrEmpty(viewModel.Fees[i].Details))
                            ModelState.AddModelError(string.Format("Fees[{0}].Details", i), "Required");
                    };
                    for (int i = 0; i < viewModel.TimeGroups.Count; i++)
                    {
                        for (int j = 0; j < viewModel.TimeGroups[i].Times.Count; j++)
                        {
                            itvm = savedInvoice.TimeGroups[i].Times.Single(x => x.Time.Id.Value == viewModel.TimeGroups[i].Times[j].Time.Id);
                            viewModel.TimeGroups[i].Times[j].Time = itvm.Time;
                            if (string.IsNullOrEmpty(viewModel.TimeGroups[i].Times[j].Details))
                                ModelState.AddModelError(string.Format("TimeGroups[{0}].Times[{1}].Details", i, j), "Required");
                        };
                    }

                    if (!ModelState.IsValid)
                    {
                        // Errors - do nothing, but tell user and show again for fixing
                        matter = Data.Matters.Matter.Get(trans, id);
                        ViewData["MatterTitle"] = matter.Title;
                        ViewData["CaseNumber"] = matter.CaseNumber;
                        ViewData["FirmName"] = Common.Settings.Manager.Instance.System.BillingFirmName;
                        ViewData["FirmAddress"] = Common.Settings.Manager.Instance.System.BillingFirmAddress;
                        ViewData["FirmCity"] = Common.Settings.Manager.Instance.System.BillingFirmCity;
                        ViewData["FirmState"] = Common.Settings.Manager.Instance.System.BillingFirmState;
                        ViewData["FirmZip"] = Common.Settings.Manager.Instance.System.BillingFirmZip;
                        ViewData["FirmPhone"] = Common.Settings.Manager.Instance.System.BillingFirmPhone;
                        ViewData["FirmWeb"] = Common.Settings.Manager.Instance.System.BillingFirmWeb;
                        return View(viewModel);
                    }

                    decimal subtotal = 0;
                    invoiceExpenseList = new List<Common.Models.Billing.InvoiceExpense>();
                    invoiceFeeList = new List<Common.Models.Billing.InvoiceFee>();
                    invoiceTimeList = new List<Common.Models.Billing.InvoiceTime>();

                    viewModel.Expenses.ForEach(vm =>
                    {
                        Common.Models.Billing.InvoiceExpense mod = new Common.Models.Billing.InvoiceExpense()
                        {
                            Id = vm.Id,
                            Invoice = invoice,
                            Expense = new Common.Models.Billing.Expense()
                            {
                                Id = vm.Expense.Id
                            },
                            Amount = vm.Amount,
                            Details = vm.Details
                        };
                        subtotal += mod.Amount;
                        Data.Billing.InvoiceExpense.Edit(trans, mod, currentUser);
                    });

                    viewModel.Fees.ForEach(vm =>
                    {
                        Common.Models.Billing.InvoiceFee mod = new Common.Models.Billing.InvoiceFee()
                        {
                            Id = vm.Id,
                            Invoice = invoice,
                            Fee = new Common.Models.Billing.Fee()
                            {
                                Id = vm.Fee.Id
                            },
                            Amount = vm.Amount,
                            Details = vm.Details
                        };
                        subtotal += mod.Amount;
                        Data.Billing.InvoiceFee.Edit(trans, mod, currentUser);
                    });

                    viewModel.TimeGroups.ForEach(tg =>
                    {
                        tg.Times.ForEach(vm =>
                        {
                            Common.Models.Billing.InvoiceTime mod = new Common.Models.Billing.InvoiceTime()
                            {
                                Id = vm.Id,
                                Invoice = invoice,
                                Time = new Common.Models.Timing.Time()
                                {
                                    Id = vm.Time.Id,
                                    TimeCategory = new Common.Models.Timing.TimeCategory()
                                    {
                                        Id = tg.Id,
                                        Title = tg.GroupName
                                    }
                                },
                                Duration = vm.Duration,
                                PricePerHour = vm.PricePerHour,
                                Details = vm.Details
                            };
                            subtotal += ((decimal)mod.Duration.TotalHours * mod.PricePerHour);
                            Data.Billing.InvoiceTime.Edit(trans, mod, currentUser);
                        });
                    });

                    invoice.Subtotal = subtotal;
                    invoice.Total = invoice.Subtotal + invoice.TaxAmount;

                    Data.Billing.Invoice.Edit(trans, invoice, currentUser);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Details", "Invoices", new { id = invoice.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult MatterDetails(Guid id)
        {
            Common.Models.Billing.Invoice invoice = null;
            ViewModels.Billing.InvoiceViewModel viewModel = new ViewModels.Billing.InvoiceViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                invoice = Data.Billing.Invoice.Get(id, conn, false);
                viewModel = Mapper.Map<ViewModels.Billing.InvoiceViewModel>(invoice);

                if (invoice.Matter != null)
                    viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Matters.Matter.Get(invoice.Matter.Id.Value, conn, false));

                Data.Billing.Invoice.ListInvoiceExpensesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceExpenseViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceExpenseViewModel>(x);
                    vm.Expense = Mapper.Map<ViewModels.Billing.ExpenseViewModel>(
                        Data.Billing.Expense.Get(vm.Expense.Id.Value, conn, false));
                    viewModel.Expenses.Add(vm);
                });

                Data.Billing.Invoice.ListInvoiceFeesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceFeeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceFeeViewModel>(x);
                    vm.Fee = Mapper.Map<ViewModels.Billing.FeeViewModel>(
                        Data.Billing.Fee.Get(vm.Fee.Id.Value, conn, false));
                    viewModel.Fees.Add(vm);
                });

                Data.Billing.Invoice.ListInvoiceTimesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceTimeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceTimeViewModel>(x);
                    vm.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(
                        Data.Timing.Time.Get(vm.Time.Id.Value, conn, false));

                    ViewModels.Billing.InvoiceTimeGroupViewModel timeGroup;
                    if (vm.Time.TimeCategory == null || !vm.Time.TimeCategory.Id.HasValue)
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == 0);
                    else
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == vm.Time.TimeCategory.Id);
                    if (timeGroup == null || timeGroup.Id == -1)
                    {
                        Common.Models.Timing.TimeCategory tc = Data.Timing.TimeCategory.Get(vm.Time.TimeCategory.Id.Value, conn, false);
                        timeGroup = new ViewModels.Billing.InvoiceTimeGroupViewModel()
                        {
                            Id = tc.Id.Value,
                            GroupName = tc.Title,
                            Times = new List<ViewModels.Billing.InvoiceTimeViewModel>()
                        };
                        timeGroup.Times.Add(vm);
                        viewModel.TimeGroups.Add(timeGroup);
                    }
                    else
                        timeGroup.Times.Add(vm);
                });
            }

            ViewData["FirmName"] = Common.Settings.Manager.Instance.System.BillingFirmName;
            ViewData["FirmAddress"] = Common.Settings.Manager.Instance.System.BillingFirmAddress;
            ViewData["FirmCity"] = Common.Settings.Manager.Instance.System.BillingFirmCity;
            ViewData["FirmState"] = Common.Settings.Manager.Instance.System.BillingFirmState;
            ViewData["FirmZip"] = Common.Settings.Manager.Instance.System.BillingFirmZip;
            ViewData["FirmPhone"] = Common.Settings.Manager.Instance.System.BillingFirmPhone;
            ViewData["FirmWeb"] = Common.Settings.Manager.Instance.System.BillingFirmWeb;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult MatterDelete(Guid id)
        {
            Common.Models.Billing.Invoice invoice = null;
            ViewModels.Billing.InvoiceViewModel viewModel = new ViewModels.Billing.InvoiceViewModel();

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                invoice = Data.Billing.Invoice.Get(id, conn, false);
                viewModel = Mapper.Map<ViewModels.Billing.InvoiceViewModel>(invoice);

                if (invoice.Matter != null)
                    viewModel.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(
                        Data.Matters.Matter.Get(invoice.Matter.Id.Value, conn, false));

                Data.Billing.Invoice.ListInvoiceExpensesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceExpenseViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceExpenseViewModel>(x);
                    vm.Expense = Mapper.Map<ViewModels.Billing.ExpenseViewModel>(
                        Data.Billing.Expense.Get(vm.Expense.Id.Value, conn, false));
                    viewModel.Expenses.Add(vm);
                });

                Data.Billing.Invoice.ListInvoiceFeesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceFeeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceFeeViewModel>(x);
                    vm.Fee = Mapper.Map<ViewModels.Billing.FeeViewModel>(
                        Data.Billing.Fee.Get(vm.Fee.Id.Value, conn, false));
                    viewModel.Fees.Add(vm);
                });

                Data.Billing.Invoice.ListInvoiceTimesForInvoice(invoice.Id.Value, conn, false).ForEach(x =>
                {
                    ViewModels.Billing.InvoiceTimeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceTimeViewModel>(x);
                    vm.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(
                        Data.Timing.Time.Get(vm.Time.Id.Value, conn, false));

                    ViewModels.Billing.InvoiceTimeGroupViewModel timeGroup;
                    if (x.Time.TimeCategory == null || !x.Time.TimeCategory.Id.HasValue)
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == 0);
                    else
                        timeGroup = viewModel.TimeGroups.SingleOrDefault(y => y.Id == vm.Time.TimeCategory.Id);
                    if (timeGroup == null || timeGroup.Id == -1)
                    {
                        Common.Models.Timing.TimeCategory tc = Data.Timing.TimeCategory.Get(vm.Time.TimeCategory.Id.Value, conn, false);
                        timeGroup = new ViewModels.Billing.InvoiceTimeGroupViewModel()
                        {
                            Id = tc.Id.Value,
                            GroupName = tc.Title,
                            Times = new List<ViewModels.Billing.InvoiceTimeViewModel>()
                        };
                        timeGroup.Times.Add(vm);
                        viewModel.TimeGroups.Add(timeGroup);
                    }
                    else
                        timeGroup.Times.Add(vm);
                });
            }

            ViewData["FirmName"] = Common.Settings.Manager.Instance.System.BillingFirmName;
            ViewData["FirmAddress"] = Common.Settings.Manager.Instance.System.BillingFirmAddress;
            ViewData["FirmCity"] = Common.Settings.Manager.Instance.System.BillingFirmCity;
            ViewData["FirmState"] = Common.Settings.Manager.Instance.System.BillingFirmState;
            ViewData["FirmZip"] = Common.Settings.Manager.Instance.System.BillingFirmZip;
            ViewData["FirmPhone"] = Common.Settings.Manager.Instance.System.BillingFirmPhone;
            ViewData["FirmWeb"] = Common.Settings.Manager.Instance.System.BillingFirmWeb;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        [HttpPost]
        public ActionResult MatterDelete(Guid id, ViewModels.Billing.InvoiceViewModel viewModel)
        {
            Common.Models.Account.Users currentUser;
            Common.Models.Billing.Invoice invoice = null;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    invoice = Mapper.Map<Common.Models.Billing.Invoice>(viewModel);
                    invoice = Data.Billing.Invoice.Get(trans, invoice.Id.Value);
                    currentUser = Data.Account.Users.Get(trans, User.Identity.Name);
                    Data.Billing.Invoice.Delete(trans, invoice, currentUser);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Invoices", "Matters", new { id = invoice.Matter.Id });
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult MatterPrint(Guid id)
        {
            return MatterDetails(id);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult GroupDetails(Guid id)
        {
            Common.Models.Billing.BillingGroup billingGroup;
            ViewModels.Billing.GroupInvoiceViewModel viewModel = new ViewModels.Billing.GroupInvoiceViewModel();
            List<Common.Models.Matters.Matter> mattersList;
            Common.Models.Billing.Invoice invoice = null;

            using (IDbConnection conn = Data.Database.Instance.GetConnection())
            {
                invoice = Data.Billing.Invoice.Get(id, conn, false);
                billingGroup = Data.Billing.BillingGroup.Get(invoice.BillingGroup.Id.Value, conn, false);

                viewModel = new ViewModels.Billing.GroupInvoiceViewModel()
                {
                    Id = invoice.Id,
                    BillTo = new ViewModels.Contacts.ContactViewModel() { Id = invoice.BillTo.Id },
                    Date = invoice.Date,
                    Due = invoice.Due,
                    Subtotal = invoice.Subtotal,
                    TaxAmount = invoice.TaxAmount,
                    Total = invoice.Total,
                    ExternalInvoiceId = invoice.ExternalInvoiceId,
                    BillTo_NameLine1 = invoice.BillTo_NameLine1,
                    BillTo_NameLine2 = invoice.BillTo_NameLine2,
                    BillTo_AddressLine1 = invoice.BillTo_AddressLine1,
                    BillTo_AddressLine2 = invoice.BillTo_AddressLine2,
                    BillTo_City = invoice.BillTo_City,
                    BillTo_State = invoice.BillTo_State,
                    BillTo_Zip = invoice.BillTo_Zip,
                    BillingGroup = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(billingGroup)
                };

                mattersList = Data.Billing.BillingGroup.ListMattersForGroup(billingGroup.Id.Value, conn, false);

                for (int i = 0; i < mattersList.Count; i++)
                {
                    ViewModels.Billing.GroupInvoiceItemViewModel giivm = new ViewModels.Billing.GroupInvoiceItemViewModel();
                    giivm.Matter = Mapper.Map<ViewModels.Matters.MatterViewModel>(mattersList[i]);

                    Data.Billing.InvoiceExpense.ListForMatterAndInvoice(id, mattersList[i].Id.Value, conn, false).ForEach(x =>
                    {
                        ViewModels.Billing.InvoiceExpenseViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceExpenseViewModel>(x);
                        vm.Expense = Mapper.Map<ViewModels.Billing.ExpenseViewModel>(
                            Data.Billing.Expense.Get(vm.Expense.Id.Value, conn, false));
                        giivm.ExpensesSum += vm.Amount;
                        giivm.Expenses.Add(vm);
                    });

                    Data.Billing.InvoiceFee.ListForMatterAndInvoice(id, mattersList[i].Id.Value, conn, false).ForEach(x =>
                    {
                        ViewModels.Billing.InvoiceFeeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceFeeViewModel>(x);
                        vm.Fee = Mapper.Map<ViewModels.Billing.FeeViewModel>(
                            Data.Billing.Fee.Get(vm.Fee.Id.Value, conn, false));
                        giivm.FeesSum += vm.Amount;
                        giivm.Fees.Add(vm);
                    });

                    Data.Billing.InvoiceTime.ListForMatterAndInvoice(id, mattersList[i].Id.Value, conn, false).ForEach(x =>
                    {
                        ViewModels.Billing.InvoiceTimeViewModel vm = Mapper.Map<ViewModels.Billing.InvoiceTimeViewModel>(x);
                        vm.Time = Mapper.Map<ViewModels.Timing.TimeViewModel>(
                            Data.Timing.Time.Get(vm.Time.Id.Value, conn, false));
                        
                        ViewModels.Billing.InvoiceTimeGroupViewModel timeGroup;
                        if (vm.Time.TimeCategory == null || !vm.Time.TimeCategory.Id.HasValue)
                            timeGroup = giivm.TimeGroups.SingleOrDefault(y => y.Id == 0);
                        else
                            timeGroup = giivm.TimeGroups.SingleOrDefault(y => y.Id == vm.Time.TimeCategory.Id);
                        if (timeGroup == null || timeGroup.Id == -1)
                        {
                            Common.Models.Timing.TimeCategory tc = Data.Timing.TimeCategory.Get(vm.Time.TimeCategory.Id.Value, conn, false);
                            timeGroup = new ViewModels.Billing.InvoiceTimeGroupViewModel()
                            {
                                Id = tc.Id.Value,
                                GroupName = tc.Title,
                                Times = new List<ViewModels.Billing.InvoiceTimeViewModel>()
                            };
                            timeGroup.Times.Add(vm);
                            giivm.TimeGroups.Add(timeGroup);
                        }
                        else
                            timeGroup.Times.Add(vm);

                        giivm.TimeSum = giivm.TimeSum.Add(vm.Duration);
                        giivm.TimeSumMoney += vm.PricePerHour * (decimal)vm.Duration.TotalHours;
                    });

                    if (((giivm.TimeGroups.Count > 0) && (giivm.TimeGroups.Count(x => x.Times.Count > 0) > 0)) ||
                        (giivm.Expenses.Count > 0) ||
                        (giivm.Fees.Count > 0))
                        viewModel.Matters.Add(giivm);
                }
            }

            ViewBag.Invoice = Mapper.Map<ViewModels.Billing.InvoiceViewModel>(invoice);
            ViewBag.BillingGroup = Mapper.Map<ViewModels.Billing.BillingGroupViewModel>(billingGroup);
            ViewData["FirmName"] = Common.Settings.Manager.Instance.System.BillingFirmName;
            ViewData["FirmAddress"] = Common.Settings.Manager.Instance.System.BillingFirmAddress;
            ViewData["FirmCity"] = Common.Settings.Manager.Instance.System.BillingFirmCity;
            ViewData["FirmState"] = Common.Settings.Manager.Instance.System.BillingFirmState;
            ViewData["FirmZip"] = Common.Settings.Manager.Instance.System.BillingFirmZip;
            ViewData["FirmPhone"] = Common.Settings.Manager.Instance.System.BillingFirmPhone;
            ViewData["FirmWeb"] = Common.Settings.Manager.Instance.System.BillingFirmWeb;

            return View(viewModel);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult GroupPrint_Full(Guid id)
        {
            return GroupDetails(id);
        }

        [Authorize(Roles = "Login, User")]
        public ActionResult GroupPrint_Summary(Guid id)
        {
            return GroupDetails(id);
        }
    }
}
