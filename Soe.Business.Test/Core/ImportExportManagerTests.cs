using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoftOne.Soe.Shared.DTO;
using SoftOne.Soe.Business.Core.PaymentIO.Pg;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ImportExportManagerTests : TestBase
    {
        [TestMethod()]
        public void ImportStoreDataFromSftpTest()
        {
            ConfigurationSetupUtil.Init();
            bool active = false;
            List<int> actorcompanyIds = new List<int>();
            if (active)
            {
                using (CompEntities entities = new CompEntities())
                {
                    actorcompanyIds = entities.StaffingNeedsLocationGroup.Where(r => r.Company.State == (int)SoeEntityState.Active && r.State == (int)SoeEntityState.Active).Select(i => i.ActorCompanyId).Distinct().ToList();
                }
            }
            var m = new ImportExportManager(null);
            var result = m.ImportStoreDataFromSftp(actorcompanyIds);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ImportStoreDataFromFromAPITest()
        {
            ConfigurationSetupUtil.Init();

            List<int> actorcompanyIds = new List<int>();

            using (CompEntities entities = new CompEntities())
            {
                actorcompanyIds = entities.StaffingNeedsLocationGroup.Where(r => r.Company.State == (int)SoeEntityState.Active && r.State == (int)SoeEntityState.Active).Select(i => i.ActorCompanyId).Distinct().ToList();
            }

            var m = new ImportExportManager(null);
            var result = m.ImportStoreDataFromFromAPI(actorcompanyIds, out _);
            Assert.IsTrue(result.Success);
        }

        #region HRM
        [TestMethod()]
        public void GetTimeScheduleBlockIODTOsTest()
        {
            int actorCompanyId = 360772;
            int userId = 46378;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetTimeScheduleBlockIODTOs(actorCompanyId, null, new DateTime(2017, 10, 9), new DateTime(2017, 10, 15), new List<string>() { "248" });
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetTimeCodeTransactionsTest()
        {
            int actorCompanyId = 870002;
            int userId = 47441;
            var ex = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = ex.GetTimeCodeTransactions(actorCompanyId, new DateTime(2020, 10, 19), new DateTime(2020, 10, 21));
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void TryToSetAccountidOnStaffingneedsFrequencyTest()
        {
            ConfigurationSetupUtil.Init();
            List<int> actorcompanyIds;
            using (CompEntities entities = new CompEntities())
            {
                actorcompanyIds = entities.StaffingNeedsLocationGroup.Where(r => r.Company.State == (int)SoeEntityState.Active && r.State == (int)SoeEntityState.Active).Select(i => i.ActorCompanyId).Distinct().ToList();
            }
            ;
            var m = new ImportExportManager(null);
            var result = m.TryToSetAccountidOnStaffingneedsFrequency(actorcompanyIds);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void DeserializeStaffingneedsFrequencyIO()
        {
            var json = "[{#DateFrom#: #2022-02-03#,   #TimeFrom#: #2022-02-03 23:00:00#,   #DateTo#: #2022-02-04#,   #TimeTo#: #2022-02-04 00:00:00#,   #Amount#: 5588.958526008944,   #ParentExternalCode#: #2603#,   #FrequencyType#: #0#  }]";
            json = json.Replace('#', '"');
            var result = JsonConvert.DeserializeObject<List<StaffingNeedsFrequencyIODTO>>(json);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void HandleAxfoodExportTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 1292;
            int userId = 1389;
            var ex = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var message = ex.HandleAxfoodExport();
            Assert.IsTrue(message != null);
        }

        [TestMethod()]
        public void ImportEDWDataFromSftpTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 1292;
            int userId = 1389;
            var ex = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var message = ex.ImportEDWDataFromSftp();
            Assert.IsTrue(message != null);
        }

        #endregion

        #region Voucher

        [TestMethod()]
        public void ImportVoucherIOTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var vouchers = m.GetVoucherHeadIODTOs(actorCompanyId, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_MAXVALUE, null, null, 103, 15, 15);
            var first = vouchers[0];
            first.VoucherHeadId = 0;
            first.VoucherHeadIOId = 0;
            first.VoucherNr = null;
            first.BatchId = null;
            first.Text = "Testar från API";
            foreach (var row in first.Rows)
            {
                row.VoucherHeadIOId = 0;
                row.VoucherRowId = 0;
                row.VoucherRowIOId = 0;
                row.CreditAmount = Math.Abs(row.CreditAmount.GetValueOrDefault());
            }
            VoucherHeadIOItem voucherHeadIOItem = new VoucherHeadIOItem { Vouchers = new List<VoucherHeadIODTO>() };
            voucherHeadIOItem.Vouchers.Add(first);
            var result = m.ImportVoucherIO(voucherHeadIOItem, TermGroup_IOImportHeadType.Voucher, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, true, first.AccountYearId, first.VoucherSeriesId);
            Assert.IsTrue(result.Success);
        }

        #endregion

        #region CustomerInvoice
        [TestMethod()]
        public void SearchCustomerInvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.SearchCustomerInvoice(actorCompanyId, new CustomerInvoiceSearchIODTO { CustomerNr = "991", InvoiceDateFrom = new DateTime(2025, 1, 19), IncludeVoucherStatus = true, FullyPaid = false, IncludePreliminary = false }, 5000);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void SearchOrderInvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.SearchCustomerOrder(actorCompanyId, new CustomerOrderSearchIODTO { CustomerNr = "106", OrderDateFrom = new DateTime(2021, 1, 1) }, 5000);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void FilterCustomerInvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2084836;
            int userId = 72;
            var filter = new CustomerInvoiceFilterIODTO
            {
                IncludeRowAccountInfo = true,
                IncludeVoucher = true,
                InvoiceDateFrom = new DateTime(2023, 1, 23),
                InvoiceDateTo = new DateTime(2024, 1, 23),
                PageNrOfRecords = 10,
            };
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetCustomerInvoiceSmallIODTOs(actorCompanyId, SoeOriginType.CustomerInvoice, filter);
            //result = m.GetCustomerInvoiceSmallIODTOs(actorCompanyId, SoeOriginType.CustomerInvoice, filter);
            //result = m.GetCustomerInvoiceSmallIODTOs(actorCompanyId, SoeOriginType.CustomerInvoice, filter);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText("c:\\Temp\\FilterCustomerInvoice.json", json);
            Assert.IsTrue(result.Any());
        }

        [TestMethod()]
        public void ImportCustomerInvoiceHeadIO()
        {
            ConfigurationSetupUtil.Init();
            //update CustomerInvoiceHeadIO set Status = 2, invoiceId = null where CustomerInvoiceHeadIOId = 1576169
            //update CustomerInvoiceRowIO set Status = 2, InvoiceRowId = null where CustomerInvoiceHeadIOId = 1576169
            int actorCompanyId = 235096;
            int userId = 30094;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.ImportCustomerInvoiceHeadIO(new List<int>() { 2039840 }, actorCompanyId);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ImportCustomerHeadIOTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2365108;
            int userId = 97331;
            // 1764523
            //update CustomerInvoiceHeadIO set Status = 2 where CustomerInvoiceHeadIOId = 1739878
            //update CustomerInvoiceHeadIO set Status = 2, InvoiceId = null where CustomerInvoiceHeadIOId = 1739878
            //update CustomerInvoiceRowIO set Status = 2, InvoiceRowId = null where CustomerInvoiceHeadIOId = 1739878
            var ex = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            List<CustomerInvoiceHeadIO> ios = ex.GetCustomerInvoiceHeadIOResult(actorCompanyId, new List<int>() { 1305905 });
            var result = ex.ImportFromCustomerInvoiceHeadIO(ios, ios.First().ImportId.GetValueOrDefault(), actorCompanyId, TermGroup_IOType.WebAPI);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ImportCustomerHeadIODTOTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2365108;
            int userId = 97331;
            // 1764523
            //update CustomerInvoiceHeadIO set Status = 2 where CustomerInvoiceHeadIOId = 1739878
            //update CustomerInvoiceHeadIO set Status = 2, InvoiceId = null where CustomerInvoiceHeadIOId = 1739878
            //update CustomerInvoiceRowIO set Status = 2, InvoiceRowId = null where CustomerInvoiceHeadIOId = 1739878
            var ex = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            List<CustomerInvoiceIODTO> ios = ex.GetCustomerInvoiceHeadIOResult(actorCompanyId, new List<int>() { 1305905 }).ToDTOs(true).ToList();

            var customerInvoiceIOItem = new CustomerInvoiceIOItem();
            customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();

            customerInvoiceIOItem.CustomerInvoices.AddRange(ios);
            var result = ex.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, null, false);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SaveCustomerInvoiceIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var customerInvoiceHeadIODTOs = m.GetCustomerInvoiceIODTOs(actorCompanyId, SoeOriginType.Order, null, null, null, "4877", "4880", null);
            //var customerInvoiceHeadIODTOs = m.GetCustomerInvoiceIODTOs(actorCompanyId, SoeOriginType.Order, null, null, null, "4520", "4520", null);

            foreach (var invoice in customerInvoiceHeadIODTOs)
            {
                invoice.CustomerInvoiceNr = null;
                invoice.CustomerInvoiceHeadIOId = 0;
                invoice.InvoiceId = null;
                invoice.CustomerInvoiceNr = null;

                //invoice.InternalDescription = "api";
                invoice.VoucherNr = null;
                invoice.WorkingDescription = "En arbetsbeskring från API:et";
                invoice.InvoiceDate = DateTime.Today;
                invoice.DueDate = DateTime.Today.AddDays(30);
                //invoice.InvoiceFee = 20;
                //invoice.FreightAmount = 25;
                //var row = invoice.InvoiceRows[0];
                foreach (var row in invoice.InvoiceRows)
                {
                    row.AccountNr = null;
                    row.AccountDim2Nr = null;
                    row.AccountDim3Nr = null;
                    row.AccountDim4Nr = null;
                    row.AccountDim5Nr = null;
                    row.AccountDim6Nr = null;
                    row.VatAmount = null;
                    row.VatAmountCurrency = null;
                    row.VatCode = null;
                    row.VatCodeId = 0;
                    row.VatRate = null;
                    row.VatAccountnr = null;
                }
            }

            Task.Run(() =>
            {
                var m1 = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
                var customerInvoiceIOItem = new CustomerInvoiceIOItem();
                customerInvoiceIOItem.CustomerInvoices = new List<CustomerInvoiceIODTO>();
                customerInvoiceIOItem.CustomerInvoices.Add(customerInvoiceHeadIODTOs[0]);
                var result = m1.ImportCustomerInvoiceIO(customerInvoiceIOItem, TermGroup_IOImportHeadType.CustomerInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, null, true);
            }).ConfigureAwait(false);
            Task.Run(() =>
            {
                var m2 = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
                var customerInvoiceIOItem2 = new CustomerInvoiceIOItem();
                customerInvoiceIOItem2.CustomerInvoices = new List<CustomerInvoiceIODTO>();
                customerInvoiceIOItem2.CustomerInvoices.Add(customerInvoiceHeadIODTOs[1]);
                var result2 = m.ImportCustomerInvoiceIO(customerInvoiceIOItem2, TermGroup_IOImportHeadType.CustomerInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, null, true);
            }).ConfigureAwait(false);
            Task.Run(() =>
            {
                var m3 = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
                var customerInvoiceIOItem3 = new CustomerInvoiceIOItem();
                customerInvoiceIOItem3.CustomerInvoices = new List<CustomerInvoiceIODTO>();
                customerInvoiceIOItem3.CustomerInvoices.Add(customerInvoiceHeadIODTOs[2]);
                var result3 = m3.ImportCustomerInvoiceIO(customerInvoiceIOItem3, TermGroup_IOImportHeadType.CustomerInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, null, true);
            }).ConfigureAwait(false);
            Task.Run(() =>
            {
                var m4 = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
                var customerInvoiceIOItem4 = new CustomerInvoiceIOItem();
                customerInvoiceIOItem4.CustomerInvoices = new List<CustomerInvoiceIODTO>();
                customerInvoiceIOItem4.CustomerInvoices.Add(customerInvoiceHeadIODTOs[3]);
                var result4 = m4.ImportCustomerInvoiceIO(customerInvoiceIOItem4, TermGroup_IOImportHeadType.CustomerInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, null, true);
            }).ConfigureAwait(false);

            Thread.Sleep(200000);
            //Task.Delay(10000);
            //Assert.IsTrue(result.Success);
        }


        [TestMethod()]
        public void CreateInvoiceFromOrder()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.CreateInvoiceFromOrder(4042, true, actorCompanyId);
            Assert.IsTrue(result.Success);
        }
        [TestMethod()]
        public void GetCustomerInvoiceIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetCustomerInvoiceIODTOs(actorCompanyId, SoeOriginType.CustomerInvoice, null, null, null, "7459", "7459");
            Assert.IsTrue(result.Any());
        }

        [TestMethod()]
        public void UpdateCustomerOrderIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var customerInvoiceHeadIODTOs = m.GetCustomerInvoiceIODTOs(actorCompanyId, SoeOriginType.Order, null, null, null, "4520", "4520", null);
            var order = customerInvoiceHeadIODTOs[0];
            var updateOrder = new CustomerInvoiceOrderUpdateIODTO
            {
                InvoiceId = order.InvoiceId.GetValueOrDefault(),
                Number = order.CustomerInvoiceNr,
                ReferenceOur = "Vår ref",
                ReferenceYour = "Er ref",
                Rows = new List<CustomerInvoiceOrderRowUpdateIODTO>(),
            };
            updateOrder.Rows.Add(new CustomerInvoiceOrderRowUpdateIODTO
            {
                //InvoiceRowId = 141489,
                ProductNr = "999",
                Quantity = 1,
                Price = 500,
                //IsReady = true
            });
            var result = m.ImportUpdateCustomerInvoiceIO(updateOrder, actorCompanyId, (SoeOriginType)order.OriginType);
            Assert.IsTrue(result != null);
        }

        #endregion

        #region Customer
        [TestMethod()]
        public void SearchCustomer()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.SearchCustomer(actorCompanyId, new CustomerSearchIODTO { }, 99999, true);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetGetCustomerIO()
        {
            int actorCompanyId = 2084836;
            int userId = 95717;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var ios = m.GetCustomerIODTOs(actorCompanyId, "K00002898", "K00002898", null, null);
            Assert.IsTrue(ios != null);
        }

        [TestMethod()]
        public void ImportCustomerIO()
        {
            ConfigurationSetupUtil.Init();
            //update CustomerIO set Status = 2, ActorCustomerId = null where CustomerIOId = 1576169

            int actorCompanyId = 2084836;
            int userId = 95722;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.ImportCustomerIO(new List<int>() { 1273042 }, actorCompanyId);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetSaveCustomerIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2084836;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var ios = m.GetCustomerIODTOs(actorCompanyId, "", "", new List<int> { 2603667 }, null);
            var customer = ios[0];
            customer.CustomerId = 0;
            customer.IsPrivatePerson = true;
            customer.CustomerNr = "";
            customer.Email1 = "info22@hotell.se";
            customer.Email2 = "";
            customer.InvoiceLabel = "Tack för senast!";
            var customerIOItem = new CustomerIOItem { Customers = new List<CustomerIODTO>() };
            customerIOItem.Customers.Add(customer);
            var result = m.ImportCustomerIO(customerIOItem, TermGroup_IOImportHeadType.Customer, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, "", true);
            Assert.IsTrue(result.Success);
        }
        #endregion

        #region Payments

        [TestMethod()]
        public void SaveCustomerPaymentIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            List<PaymentRowImportIODTO> paymentIODTOs = new List<PaymentRowImportIODTO>
            {
                new PaymentRowImportIODTO
                {
                    Amount = 1000,
                    AmountCurrency = 1000,
                    CurrencyCode = "SEK",
                    InvoiceNr = "3400",
                    ChangeFullyPaid = false,
                    PayDate = new DateTime(2023, 05, 06),
                    PaymentNr = "0",
                    PaymentMethodCode = "Cash",
                    Type = 4
                },
                new PaymentRowImportIODTO
                {
                    Amount = 3953,
                    AmountCurrency = 3953,
                    CurrencyCode = "SEK",
                    InvoiceNr = "3400",
                    ChangeFullyPaid = true,
                    PayDate = new DateTime(2023, 05, 08),
                    PaymentNr = "0",
                    PaymentMethodCode = "Swish",
                    Type = 4
                }
            };
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.ImportPaymentFromIO(paymentIODTOs, actorCompanyId, TermGroup_IOType.WebAPI, false);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void SaveCustomerPaymentFromJson()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2365108;
            int userId = 97414;
            var fileContent = File.ReadAllText(@"C:\Temp\api\brobergsbetalningar.txt");
            var payments = JsonConvert.DeserializeObject<List<PaymentRowImportIODTO>>(fileContent);

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.ImportPaymentFromIO(payments, actorCompanyId, TermGroup_IOType.WebAPI, false);
            Assert.IsTrue(result != null);

        }

        [TestMethod()]
        public void GetCustomerPaymentIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetPaymentRowImportIODTOs(actorCompanyId, SoeOriginType.CustomerPayment, new PaymentSearchIODTO { PayDateFrom = CalendarUtility.DATETIME_DEFAULT, PayDateTo = DateTime.MaxValue, InvoiceId = 30186 });
            Assert.IsTrue(result != null);
        }


        #endregion

        #region Supplier

        [TestMethod()]
        public void GetSaveSupplierIO()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var ios = m.GetSupplierIODTOs(actorCompanyId, "", "", new List<int> { 85 }, null);
            var supplier = ios[0];
            supplier.SupplierId = null;
            supplier.SupplierNr = "1315";
            supplier.OrgNr = "888-131131";
            supplier.Name = "Api leverantör 131";
            var supplierIOItem = new SupplierIOItem { Suppliers = new List<SupplierIODTO>() };
            supplierIOItem.Suppliers.Add(supplier);
            var result = m.ImportSupplierIO(supplierIOItem, TermGroup_IOImportHeadType.Supplier, TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, actorCompanyId, true);
            Assert.IsTrue(result.Success);
        }

        #endregion

        #region SupplierInvoice

        [TestMethod()]
        public void FilterSupplierInvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            //var result = m.GetSupplierInvoiceIODTOs(actorCompanyId, new SupplierInvoiceFilterIODTO {SupplierNr = "4", InvoiceDateFrom = new DateTime(2023, 6, 14), InvoiceDateTo = new DateTime(2023, 6, 14), PageNrOfRecords = 5000 });
            var result = m.GetSupplierInvoiceIODTOs(actorCompanyId, new SupplierInvoiceFilterIODTO { Number = "465465465", InvoiceDateFrom = new DateTime(2023, 1, 14), PageNrOfRecords = 5000 });

            Assert.IsTrue(result.Any());
        }

        [TestMethod()]
        public void GetSupplierInvoice()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetSupplierInvoiceIODTOs(actorCompanyId, 37528);

            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetSupplierInvoiceImage()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetSupplierInvoiceImageIODTO(actorCompanyId, 37528);
            Assert.IsNotNull(result);
        }

        #endregion

        #region TimeScheduleInfo

        [TestMethod()]
        public void GetTimeScheduleInfoTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 6; //Martin & Servera
            int userId = 107;
            int roleId = 41;
            DateTime dateFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTo = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc);
            string employeeNrString = "111111136,111111129";
            var employeeNumbers = employeeNrString.Split(',').ToList();

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));
            var dto = m.GetTimeScheduleInfo(actorCompanyId, dateFrom, dateTo, employeeNumbers, false, true, true);
            var json = JsonConvert.SerializeObject(dto);
            Assert.IsTrue(json != null);
        }

        [TestMethod()]
        public void GetTimeScheduleInfoTestMyr()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 315146;
            int userId = 107534;
            int roleId = 2836;
            var employeeNumbers = new List<string>();
            DateTime dateFrom = DateTime.Today.AddDays(-60);
            DateTime dateTo = DateTime.Today;

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));
            var dto = m.GetTimeScheduleInfo(actorCompanyId, dateFrom, dateTo, employeeNumbers, true, false, true);
            var json = JsonConvert.SerializeObject(dto);
            Assert.IsTrue(json != null);
        }

        #endregion

        [TestMethod()]
        public void ImportPayrollReviewExcelTest()
        {
            int actorCompanyId = 291;
            int userId = 193;

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var ios = m.ConvertPayrollReviewExcelToDTOs(File.ReadAllBytes(@"c:\temp\pr.xlsx"), DateTime.Today, actorCompanyId);
            var excel = m.ConvertPayrollReviewDTOsToExcel(ios);
            File.WriteAllBytes(@"c:\temp\pr2.xlsx", excel);
            var ios2 = m.ConvertPayrollReviewExcelToDTOs(File.ReadAllBytes(@"c:\temp\pr2.xlsx"), DateTime.Today, actorCompanyId);
            Assert.IsTrue(ios2 != null);
        }

        [TestMethod()]
        public void GetTimeBalanceTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 291;
            int userId = 193;
            DateTime date = new DateTime(2020, 5, 1, 0, 0, 0, DateTimeKind.Utc);

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetTimeBalance(actorCompanyId, date, null, null, "");
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void CreateFakeTimeScheduleBlocksTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 1750;
            int userId = 1211;
            DateTime date = new DateTime(2020, 9, 28, 0, 0, 0, DateTimeKind.Utc);

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.CreateFakeTimeScheduleBlocks(actorCompanyId, new List<int>(5000), date, CalendarUtility.DATETIME_DEFAULT.AddHours(6), CalendarUtility.DATETIME_DEFAULT.AddHours(23), 2, 134);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetAccountHierarchyTreeTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 146132;
            int userId = 115686;

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var message = m.GetAccountHierarchyTree(actorCompanyId);
            Assert.IsTrue(message != null);
        }

        #region InvoiceProduct

        [TestMethod()]
        public void SearchProductTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.SearchInvoiceProduct(actorCompanyId, new InvoiceProductSearchIODTO { ModifiedSince = new DateTime(2021, 06, 10) }, 100);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetAndSaveProductTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            //var item = m.GetInvoiceProductIODTOs(actorCompanyId, "151515", "151515", null, null, false, null)?.FirstOrDefault();
            var item = m.GetInvoiceProductIODTOs(actorCompanyId, "0013", "0013", null, null, false, null)?.FirstOrDefault();

            item.ProductId = null;
            item.PurchasePrice = 200;
            item.Number = "0001311";
            item.Name = "JAMO S801PM KAIUTINPARI, MUSTA";
            item.Unit = "PAR";
            item.VatType = 1;
            item.Weight = 0;
            item.PurchasePrice = 50M;
            item.State = 0;
            item.PriceDTOs.Clear();
            item.PriceDTOs.Add(new InvoiceProductPriceIODTO
            {
                PriceListTypeId = 187,
                Price = 100,
                Quantity = 0,
                PriceListCode = "Stamkunder",
                //StartDate = new DateTime(1901, 1, 1),
                //StopDate = new DateTime(9999, 1, 1)
            });

            item.PriceDTOs.Add(new InvoiceProductPriceIODTO
            {
                PriceListTypeId = 187,
                Price = 75,
                Quantity = 50,
                PriceListCode = "Stamkunder",
                //StartDate = new DateTime(1901, 1, 1),
                //StopDate = new DateTime(9999, 1, 1)
            });

            var result = m.ImportFromInvoiceProductIO(new List<InvoiceProductIODTO>() { item }, actorCompanyId, false);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(item != null);
        }

        [TestMethod()]
        public void GetProductTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2830453;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetInvoiceProductIODTOs(actorCompanyId, "1986058", "1986058", null, null, false, null);
            Assert.IsTrue(result != null);
        }
        [TestMethod()]
        public void GetProductFilterTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2830453;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            List<InvoiceProductIODTO> result = null;
            for (var page = 1; page < 4; page++)
            {
                result = m.GetInvoiceProductIODTOs(actorCompanyId, new InvoiceProductFilterIODTO
                {
                    Number = "1986058",
                    //ProductGroupIds = new List<int> { 13 },
                    IncludePriceList = true,
                    PageNrOfRecords = 200,
                    PageNumber = page,
                });
                if (result == null)
                    break;
            }
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetProductUnitTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetProductUnits(actorCompanyId);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetPriceListsTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetPriceLists(actorCompanyId, true);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetProductStockBalance()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetProductStockbalance(new List<int>() { 1043, 620 }, "HL", actorCompanyId);
            if (result != null)
                result = m.GetProductStockbalance(new List<int>() { 1043, 620 }, "", actorCompanyId);
            Assert.IsTrue(result != null);
        }


        [TestMethod()]
        public void GetProductPriceTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2830453;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var pricesToFetch = new List<InvoiceProductPriceSearchIODTO>();
            //pricesToFetch.Add(new InvoiceProductPriceSearchIODTO { ProductId = 1043, Quantity = 1 });  //kundspecifkt pris
            //pricesToFetch.Add(new InvoiceProductPriceSearchIODTO { ProductId = 4909, Quantity = 1 }); //kundens prislist
            //pricesToFetch.Add(new InvoiceProductPriceSearchIODTO { ProductId = 7561, Quantity = 1 }); //standard
            pricesToFetch.Add(new InvoiceProductPriceSearchIODTO { ProductId = 4606397, Quantity = 1 }); //staffling
            //pricesToFetch.Add(new InvoiceProductPriceSearchIODTO { ProductId = 16205, Quantity = 25 }); //staffling

            var result = m.GetProductPrice(2831220, pricesToFetch, actorCompanyId);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetStockTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var result = m.GetStocks(actorCompanyId);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void SaftExport()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2533262; //7;
            int userId = 72;
            var m = new SAFTManager(GetParameterObject(actorCompanyId, userId));

            //var data = m.GetInvoiceBalances(new DateTime(2023, 2, 1), new DateTime(2023, 2, 28), 7, SoeOriginType.CustomerInvoice);
            var result = m.Export(new DateTime(2023, 12, 1), new DateTime(2023, 12, 31), actorCompanyId);

            File.WriteAllText("c:\\Temp\\Saft\\saft_output.xml", result.StringValue);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetAccountYearAndPeriod()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7; //7;
            int userId = 72;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var data = m.GetAccountYearIODTOs(actorCompanyId);

            Assert.IsTrue(false);
        }


        #endregion

        #region SaveEmployeeActiveSchedule

        [TestMethod]
        public void SaveEmployeeActiveScheduleIOs_ShouldSaveEmployeeActiveSchedules()
        {
            // Arrange
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 291;
            int userId = 83;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            var intervalStartDate = new DateTime(2025, 3, 31);
            var intervalStopDate = new DateTime(2024, 4, 30);
            var interval1Day1Block1StartTime = new DateTime(2024, 4, 1, 8, 0, 0);
            var interval1Day1Block1StopTime = new DateTime(2024, 4, 1, 13, 0, 0);
            var interval1Day1Block1ShiftTypeExternalCode = "FG";
            var interval1Day1Block1BreakStartTime = new DateTime(2024, 4, 1, 12, 0, 0);
            var interval1Day1Block1BreakStopTime = new DateTime(2024, 4, 1, 13, 0, 0);
            var interval1Day1Block2StartTime = new DateTime(2024, 4, 1, 13, 0, 0);
            var interval1Day1Block2StopTime = new DateTime(2024, 4, 1, 17, 0, 0);
            var interval1Day1Block2ShiftTypeExternalCode = "Kassa";
            var interval1Day2Block1StartTime = new DateTime(2024, 4, 2, 8, 0, 0);
            var interval1Day2Block1StopTime = new DateTime(2024, 4, 2, 13, 30, 0);
            var interval1Day2Block1ShiftTypeExternalCode = "Kassa";
            var interval1Day2Block1BreakStartTime = new DateTime(2024, 4, 2, 12, 30, 0);
            var interval1Day2Block1BreakStopTime = new DateTime(2024, 4, 2, 13, 30, 0);
            var interval1Day2Block2StartTime = new DateTime(2024, 4, 2, 13, 30, 0);
            var interval1Day2Block2StopTime = new DateTime(2024, 4, 2, 16, 30, 0);
            var interval1Day2Block2ShiftTypeExternalCode = "FG";

            List<EmployeeActiveScheduleIO> employeeActiveSchedules = new List<EmployeeActiveScheduleIO>();

            // Add test data for employeeActiveSchedules

            var day1 = new ActiveScheduleDay()
            {
                ActiveScheduleBlocks = new List<ActiveScheduleBlock>()
                {
                    new ActiveScheduleBlock()
                    {
                        StartTime = interval1Day1Block1StartTime,
                        StopTime = interval1Day1Block1StopTime,
                        ShiftTypeExternalCode = interval1Day1Block1ShiftTypeExternalCode
                    },
                    new ActiveScheduleBlock()
                    {
                        StartTime = interval1Day1Block2StartTime,
                        StopTime = interval1Day1Block2StopTime,
                        ShiftTypeExternalCode = interval1Day1Block2ShiftTypeExternalCode
                    }
                },
                ActiveSceduleBreakBlocks = new List<ActiveSceduleBreakBlock>()
                {
                    new ActiveSceduleBreakBlock()
                    {
                        StartTime = interval1Day1Block1BreakStartTime,
                        StopTime = interval1Day1Block1BreakStopTime
                    }
                },
                Date = new DateTime(2024, 4, 1),
                DayExternalCode = "DayExternalCode0401"
            };

            var day2 = new ActiveScheduleDay()
            {
                ActiveScheduleBlocks = new List<ActiveScheduleBlock>()
                {
                    new ActiveScheduleBlock()
                    {
                        StartTime = interval1Day2Block1StartTime,
                        StopTime = interval1Day2Block1StopTime,
                        ShiftTypeExternalCode = interval1Day2Block1ShiftTypeExternalCode
                    },
                    new ActiveScheduleBlock()
                    {
                        StartTime = interval1Day2Block2StartTime,
                        StopTime = interval1Day2Block2StopTime,
                        ShiftTypeExternalCode = interval1Day2Block2ShiftTypeExternalCode
                    }
                },
                ActiveSceduleBreakBlocks = new List<ActiveSceduleBreakBlock>()
                {
                    new ActiveSceduleBreakBlock()
                    {
                        StartTime = interval1Day2Block1BreakStartTime,
                        StopTime = interval1Day2Block1BreakStopTime
                    }
                },
                Date = new DateTime(2024, 4, 2),
                DayExternalCode = "DayExternalCode0402"
            };

            employeeActiveSchedules.Add(new EmployeeActiveScheduleIO()
            {
                EmployeeNr = "21",
                ActiveScheduleIntervals = new List<ActiveScheduleInterval>()
                {
                    new ActiveScheduleInterval()
                    {
                        StartDate = intervalStartDate,
                        StopDate = intervalStopDate,
                        ActiveScheduleDays = new List<ActiveScheduleDay>()
                        {
                            day1,
                            day2
                        }
                    }
                }
            });

            // Act
            ActionResult result = m.SaveEmployeeActiveScheduleIOs(actorCompanyId, employeeActiveSchedules);

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void SaveEmployeeActiveScheduleIOs_ShouldSaveEmployeeActiveSchedules_Json()
        {
            // Arrange
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 6;
            int userId = 107;
            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId));
            string json = File.ReadAllText(@"c:\temp\SaveEmployeeActiveSchedules.txt");
            var employeeActiveSchedules = JsonConvert.DeserializeObject<List<EmployeeActiveScheduleIO>>(json);

            // Act
            ActionResult result = m.SaveEmployeeActiveScheduleIOs(actorCompanyId, employeeActiveSchedules);
            // Assert
            Assert.IsNotNull(result);
        }

        #endregion

        [TestMethod()]
        public void GetTimeStampEntryIODTOsTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 291;

            var m = new ImportExportManager(GetParameterObject(291, 83, 66));
            var data = m.GetTimeStampEntryIODTOs(actorCompanyId, DateTime.Now.AddDays(-100), DateTime.Now, null);

            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetEmployeeInformationsTest()
        {
            //int actorCompanyId = 2160509; //Coop mitt - Fredriksberg (Matbutiken i Säffsen AB)
            //int userId = 121473; //api IDA
            //int roleId = 10469; //API_HR_READ
            int actorCompanyId = 2144208; //Coop väst
            int userId = 107337; //api.microweb
            int roleId = 10014; //API_HR_READ

            ImportExportManager iem = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));
            ConfigurationSetupUtil.Init();

            var model = GetModel(useFile: false);
            var result = iem.GetEmployeeInformations(actorCompanyId, model);
            Assert.IsTrue(result != null);

            FetchEmployeeInformation GetModel(bool useFile = true)
            {
                if (useFile)
                {
                    string json = File.ReadAllText(@"c:\temp\GetEmployeeInformations.txt");
                    return JsonConvert.DeserializeObject<FetchEmployeeInformation>(json);
                }

                return new FetchEmployeeInformation
                {
                    DateFrom = new DateTime(2025, 05, 17),
                    DateTo = new DateTime(2025, 06, 17),
                    LoadContactInformation = true,
                    LoadEmployments = true,
                    LoadEmploymentAccounts = true,
                    LoadEmploymentChanges = true,
                    SetInitialValuesOnEmployment = true,
                    LoadCalenderInfo = false,
                    LoadVacationInfo = true,
                    LoadVacationInfoHistory = true,
                    LoadHierarchyAccounts = true,
                    LoadPositions = true,
                    LoadReportSettings = true,
                    LoadExtraFields = true,
                    LoadSkills = true,
                    LoadUser = true,
                    LoadUserRoles = true,
                    LoadSocialSec = true,
                    EmployeesChangedOrAddedAfterUtc = new DateTime(2022, 06, 17),
                    AddVirtualHierarchyAccounts = true,
                    LoadExecutives = true,
                    LoadPayrollInformation = true,
                    LoadPayrollFormulaResult = true
                };
            }
        }

        [TestMethod()]
        public void GetEmployeesTest()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2144208; //Coop väst
            int userId = 107337; //api.microweb
            int roleId = 10014; //API_HR_READ
            DateTime? changedAfter = null;
            bool includeInactive = true;

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));
            var employeeIO = m.ExportEmployees(TermGroup_IOSource.Connect, TermGroup_IOType.WebAPI, changedAfter, actorCompanyId, includeInactive);

            Assert.IsTrue(employeeIO != null && !employeeIO.EmployeeIOs.IsNullOrEmpty());
        }

        [TestMethod()]
        public void GetHierachyAccounts()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 2144208; //Coop väst
            int userId = 107337; //api.microweb
            int roleId = 10014; //API_HR_READ
            int employeeId = 118222; //3150

            var m = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));
            var hierarchyDict = m.LoadHierarchyAccounts(
                actorCompanyId,
                new FetchEmployeeInformation
                {
                    DateFrom = DateTime.Now.AddYears(-1),
                    DateTo = DateTime.Now.AddYears(1),
                    LoadHierarchyAccounts = true,
                },
                true,
                employeeId.ObjToList(),
                out var _
                );

            Assert.IsTrue(!hierarchyDict.IsNullOrEmpty());
        }

        [TestMethod()]
        public void ImportGeneralLedgerIOTest()
        {
            int actorCompanyId = 7;
            var param = GetParameterObject(actorCompanyId);
            var filterparams = new GeneralLedgerParams() { FromDate = new DateTime(2024, 10, 1), ToDate = new DateTime(2024, 10, 31), IncludeTransactions = true };

            var iem = new ImportExportManager(param);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = iem.ImportGeneralLedgerIO(filterparams);
            sw.Stop();
            Debug.WriteLine(sw.Elapsed.ToString());
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void ImportGeneralLedgerIOTest2()
        {
            int actorCompanyId = 7;
            var param = GetParameterObject(actorCompanyId);
            var filterparams = new GeneralLedgerParams() { FromDate = new DateTime(2027, 10, 1), ToDate = new DateTime(2027, 10, 1), IncludeTransactions = false };

            var iem = new ImportExportManager(param);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = iem.ImportGeneralLedgerIO(filterparams);
            sw.Stop();
            Debug.WriteLine(sw.Elapsed.ToString());
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void TestLongTermAbsence()
        {
            ConfigurationSetupUtil.Init();
            int reportId = 170898;
            int actorCompanyId = 2149319;
            int userId = 96517;
            int roleId = 9689;
            ApiMatrixDataSelection apiMatrixDataSelection = new ApiMatrixDataSelection();

            apiMatrixDataSelection.ReportId = reportId;
            apiMatrixDataSelection.ReportUserSelectionId = 0;
            apiMatrixDataSelection.ApiMatrixDataSelectionDateRanges = new List<ApiMatrixDataSelectionDateRange>();
            apiMatrixDataSelection.ApiMatrixDataSelectionDateRanges.Add(new ApiMatrixDataSelectionDateRange()
            {
                TypeName = "string",
                Key = "string",
                SelectFrom = DateTime.Today.AddDays(-1),
                SelectTo = DateTime.Today.AddDays(-1),
            });
            apiMatrixDataSelection.ApiMatrixEmployeeSelections = new ApiMatrixEmployeeSelection()
            {
                TypeName = "string",
                EmployeeIds = new List<int>(),
                EmployeeNumbers = new List<string>() { "73890" },
                IncludeEnded = false,
                IncludeHidden = false,
                IncludeVacant = false
            };

            apiMatrixDataSelection.ApiMatrixDataSelectionIds = new List<ApiMatrixDataSelectionId>()
            {
               new ApiMatrixDataSelectionId()
               {
                   TypeName = "string",
                   Key = "numberOfDays",
                   Id = 1
               }
            };

            apiMatrixDataSelection.ApiMatrixColumnsSelection = new ApiMatrixColumnsSelection()
            {
                TypeName = "string",
                Key = "string",
                ApiMatrixColumnSelections = new List<ApiMatrixColumnSelection>()
                {
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.EmployeeNr).FirstCharToLowerCase()},
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.FirstName).FirstCharToLowerCase()},
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.LastName).FirstCharToLowerCase()},
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.SocialSec).FirstCharToLowerCase()},
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.StartDate).FirstCharToLowerCase()},
                    new ApiMatrixColumnSelection() { Field = EnumUtility.GetName<TermGroup_LongtermAbsenceMatrixColumns>(TermGroup_LongtermAbsenceMatrixColumns.StopDateInInterval).FirstCharToLowerCase()},
                }
            };

            var param = GetParameterObject(actorCompanyId, userId, roleId);
            ImportExportManager importExportManager = new ImportExportManager(param);
            importExportManager.GetLongTermAbsence(new LongTermAbsenceInput()
            {
                DateFrom = DateTime.Today.AddDays(-1),
                DateTo = DateTime.Today.AddDays(-1),
                EmployeeNrs = new List<string>() { "73139" },
                PayrollProductInputs = new List<LongTermAbsencePayrollProductInput>()
                {
                    new LongTermAbsencePayrollProductInput()
                    {
                 SysPayrollTypeLevel1 = TermGroup_SysPayrollType.SE_GrossSalary,
                 SysPayrollTypeLevel2 = TermGroup_SysPayrollType.SE_GrossSalary_Absence,
                 SysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick,
                    }
                },
            }, actorCompanyId);
        }

    }
}
