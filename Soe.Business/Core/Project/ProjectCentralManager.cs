using Org.BouncyCastle.Asn1.X509;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class ProjectCentralManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool? HasExpenseRows = null;

        #endregion

        #region Ctor

        public ProjectCentralManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public List<ProjectCentralStatusDTO> GetProjectCentralStatusList(int actorCompanyId, int projectId, DateTime? dateFrom, DateTime? dateTo, bool includeChildProjects)
        {
            var dtos = new List<ProjectCentralStatusDTO>();
            if (!ProjectManager.HasRightToViewProject(projectId))
            {
                return null;
            }
            #region Get data

            bool incomeMaterialInvoicedAdded = false;
            bool incomePersonellInvoicedAdded = false;
            bool billableMinutesInvoicedAdded = false;
            bool costMaterialAdded = false;
            bool costPersonellAdded = false;
            bool costExpenseAdded = false;
            bool costOverheadAdded = false;
            bool budgetRowIncomeInvoicedAdded = false;

            decimal billableMinutesInvoiced = 0;
            decimal billableMinutesNotInvoiced = 0;
            decimal incomePersonnelInvoiced = 0;
            decimal incomePersonnelNotInvoiced = 0;
            decimal incomeInvoiced = 0;
            decimal incomeMaterialInvoiced = 0;
            decimal incomeMaterialNotInvoiced = 0;
            decimal incomeNotInvoiced = 0;

            decimal costPersonnel = 0;
            decimal costMaterial = 0;
            decimal costExpense = 0;
            decimal costOverhead = 0;

            //int totalWorkingMinutes = 0;
            //decimal totalWorkingHours = 0;

            int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
            bool overheadCostAsFixedAmount = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, 0, actorCompanyId, 0);
            bool overheadCostAsAmountPerHour = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, 0, actorCompanyId, 0);
            int fixedPriceProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, actorCompanyId, 0);
            int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, actorCompanyId, 0);
            int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, actorCompanyId, 0);
            int? attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);

            // Get main project
            List<Project> allProjects = GetProjectsForProjectCentralStatus(new List<int>() { projectId }, actorCompanyId);

            //Get budget for project - For now using highest id
            #region Budget

            BudgetRow budgetRow = null;
            BudgetRow budgetRow2 = null;
            BudgetHead budgetHead = null;
            Project proj = allProjects.FirstOrDefault(p => p.ProjectId == projectId);

            if (proj != null)
            {
                if (!proj.BudgetHead.IsLoaded)
                    proj.BudgetHead.Load();

                if (proj.BudgetHead != null && proj.BudgetHead.Count > 0)
                {
                    budgetHead = proj.BudgetHead.Where(x => x.State == (int)SoeEntityState.Active).OrderByDescending(h => h.BudgetHeadId).FirstOrDefault();

                    if (!budgetHead.BudgetRow.IsLoaded)
                        budgetHead.BudgetRow.Load();
                }
            }

            #endregion

            if (allProjects != null && allProjects.Count > 0)
            {

                #region Child projects

                // Get child projects recursively
                if (includeChildProjects)
                {
                    List<Project> currentLevelProjects = new List<Project>();
                    currentLevelProjects.AddRange(allProjects);

                    // Loop until no further child projects are found
                    while (true)
                    {
                        List<Project> childProjects = GetProjectsForProjectCentralStatus(currentLevelProjects.FirstOrDefault().ChildProjects.Select(p => p.ProjectId).ToList(), actorCompanyId);
                        if (childProjects.Count == 0)
                            break;

                        // Add found projects to current level projects to be used for next level search
                        currentLevelProjects.Clear();
                        currentLevelProjects.AddRange(childProjects);

                        // Add found projects to all projects list
                        allProjects.AddRange(childProjects);
                    }
                }

                #endregion

                foreach (var project in allProjects)
                {
                    #region Load relatet objects

                    /*.Include("Invoice.Origin")
                    .Include("ChildProjects")
                    .Include("TimeCodeTransaction.TimeCode")
                    .Include("TimeCodeTransaction.TimePayrollTransaction")*/

                    #region Invoice and Origin

                    if (!project.Invoice.IsLoaded)
                        project.Invoice.Load();

                    foreach (Invoice inv in project.Invoice)
                    {
                        if (!inv.OriginReference.IsLoaded)
                            inv.OriginReference.Load();
                    }

                    #endregion

                    #region Child projects

                    if (!project.ChildProjects.IsLoaded)
                        project.ChildProjects.Load();

                    #endregion

                    #region TimeCodeTransaction

                    if (!project.TimeCodeTransaction.IsLoaded)
                        project.TimeCodeTransaction.Load();

                    foreach (TimeCodeTransaction tct in project.TimeCodeTransaction)
                    {
                        if (!tct.TimeCodeReference.IsLoaded)
                            tct.TimeCodeReference.Load();

                        if (!tct.TimePayrollTransaction.IsLoaded)
                            tct.TimePayrollTransaction.Load();
                    }

                    #endregion

                    #endregion

                    #region Order and invoice rows

                    var rowsForProject = new List<CustomerInvoiceRow>();
                    var invoicesForProject = project.Invoice.OfType<CustomerInvoice>().Where(i => i.State == (int)SoeEntityState.Active && i.Origin.Status != (int)SoeOriginStatus.Cancel).ToList();

                    foreach (var invoice in invoicesForProject)
                    {
                        #region CustomerInvoice


                        if (!invoice.CustomerInvoiceRow.IsLoaded)
                            invoice.CustomerInvoiceRow.Load();

                        if (!invoice.OriginReference.IsLoaded)
                            invoice.OriginReference.Load();

                        // Loop through active product rows only (skip freightamount, invoicefee and centrounding)
                        var rowsForInvoice = invoice.CustomerInvoiceRow.Where(r => (r.Type == (int)SoeInvoiceRowType.ProductRow || r.Type == (int)SoeInvoiceRowType.BaseProductRow)
                            && !r.IsFreightAmountRow && !r.IsInvoiceFeeRow && !r.IsCentRoundingRow && r.State == (int)SoeEntityState.Active).ToList();

                        int isFixedPriceOrder = 0;
                        int isFixedPriceKeepPricesOrder = 0;

                        //Check if fixedpriceorder
                        if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                        {
                            if (fixedPriceProductId != 0)
                                isFixedPriceOrder = invoice.ActiveCustomerInvoiceRows.Count(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);

                            if (fixedPriceKeepPricesProductId != 0)
                                isFixedPriceKeepPricesOrder = invoice.ActiveCustomerInvoiceRows.Count(r => r.ProductId == fixedPriceKeepPricesProductId && fixedPriceKeepPricesProductId != 0);
                        }

                        //bool invoiceIsCredit = invoice.IsCredit;                        

                        rowsForProject.AddRange(rowsForInvoice);
                        foreach (CustomerInvoiceRow row in rowsForInvoice)
                        {
                            // Check date interval condition
                            // Check for each row, not on the invoice head
                            if (dateFrom.HasValue || dateTo.HasValue)
                            {
                                // Do not include rows without dates
                                if (!row.Created.HasValue)
                                    continue;

                                DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;
                                if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate))
                                    continue;
                            }

                            #region CustomerInvoiceRow

                            decimal purchasePrice = row.PurchasePriceCurrency * (row.Quantity.HasValue ? row.Quantity.Value : 0);

                            if (invoice.IsCredit)
                                purchasePrice = Decimal.Negate(purchasePrice);

                            if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                            {
                                if (!row.ProductReference.IsLoaded)
                                    row.ProductReference.Load();

                                if (row.TargetRowId == null) // Order rows not transferred to invoice
                                {
                                    if (row.Product != null)
                                    {
                                        if (((InvoiceProduct)row.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing)
                                            continue;

                                        if (((InvoiceProduct)row.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
                                        {
                                            if (row.AttestStateId == defaultStatusTransferredOrderToInvoice)
                                                continue; //The lift row is lifted/transferred to invoice                                         

                                            ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced);

                                            decimal rowSumAmount = Math.Abs(row.SumAmountCurrency);

                                            if (rowSumAmount != 0)
                                            {
                                                if (inDto != null)
                                                {
                                                    inDto.Value += rowSumAmount;
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, rowSumAmount, 0, isVisible: false));
                                                    incomeMaterialNotInvoiced += rowSumAmount;
                                                }
                                            }
                                        }
                                    }

                                    //if (row.Product != null && ((InvoiceProduct)row.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing)

                                    if (isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0)
                                    {
                                        if (row.Product != null && row.Product.ProductId != 0 && (row.Product.ProductId == fixedPriceProductId || row.Product.ProductId == fixedPriceKeepPricesProductId))
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", "", row.SumAmountCurrency, 0));
                                        }
                                    }

                                    if (!row.IsTimeProjectRow)
                                    {
                                        if (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0)) //if not fixed price
                                        {
                                            ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced);

                                            if (row.SumAmountCurrency != 0)
                                            {
                                                if (inDto != null)
                                                {
                                                    inDto.Value += row.SumAmountCurrency;
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency, 0, isVisible: false));
                                                    incomeMaterialNotInvoiced += row.SumAmountCurrency;
                                                }
                                            }
                                        }

                                        if (budgetHead != null && !costMaterialAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                            costMaterialAdded = true;

                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial);

                                            if (dto != null)
                                            {
                                                dto.Value += purchasePrice;
                                            }
                                            else
                                            {
                                                string customerName = String.Empty;

                                                if (!invoice.ActorReference.IsLoaded)
                                                    invoice.ActorReference.Load();

                                                if (invoice.Actor != null)
                                                {
                                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                                        invoice.Actor.CustomerReference.Load();

                                                    if (invoice.Actor.Customer != null)
                                                        customerName = invoice.Actor.Customer.Name;
                                                }

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice, 0));
                                            }

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;

                                        }
                                    }
                                    else //TimeProjectRow
                                    {
                                        if (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0)) //if not fixed price
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced);

                                            if (row.SumAmountCurrency != 0)
                                            {
                                                if (dto != null)
                                                {
                                                    dto.Value += row.SumAmountCurrency;
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency, 0, isVisible: false));
                                                }
                                                incomePersonnelNotInvoiced += row.SumAmountCurrency;
                                            }
                                        }
                                    }
                                }// End of Order rows not transferred to invoice                                    
                                /*else // Order rows transferred to invoice
                                {
                                    //Handle when not fixedprice order and lift
                                    if (row.Product != null && ((((InvoiceProduct)row.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift || ((InvoiceProduct)row.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing)) && isFixedPriceOrder == 0 && isFixedPriceKeepPricesOrder == 0)
                                    {
                                        //CHECK IF THE LIFT IS TRANSFERRED TO INVOICE, ADJUST THE LIFT AMOUNT FROM THE ORDER (NOT INVOICED SUM)                                        
                                        if (row.AttestStateId == defaultStatusTransferredOrderToInvoice)
                                        {
                                            ProjectCentralStatusDTO inDto = dtos.Where(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced).FirstOrDefault();

                                            Decimal liftRowAmountCurrency = row.SumAmountCurrency;

                                            if (liftRowAmountCurrency != 0)
                                            {
                                                if (inDto != null)
                                                {
                                                    inDto.Value += liftRowAmountCurrency;
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, liftRowAmountCurrency, 0, isVisible: false));
                                                    incomeMaterialNotInvoiced += liftRowAmountCurrency;
                                                }
                                            }
                                        }
                                    }

                                }*/
                            }
                            else if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Invoice)
                            {
                                if (invoice.Status == (int)SoeOriginStatus.Draft) //Invoice not invoiced! --- Preliminary invoices counts as not invoiced ---
                                {
                                    incomeNotInvoiced += row.SumAmountCurrency;

                                    if (!row.ProductReference.IsLoaded)
                                        row.ProductReference.Load();

                                    if (!row.IsTimeProjectRow)
                                    {
                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced);

                                        if (inDto != null)
                                        {
                                            inDto.Value += row.SumAmountCurrency;
                                        }
                                        else
                                        {
                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency));
                                            incomeMaterialNotInvoiced += row.SumAmountCurrency;
                                        }

                                        if (budgetHead != null && !costMaterialAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                        else
                                        {

                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial);

                                            if (dto != null)
                                            {
                                                dto.Value += purchasePrice;
                                            }
                                            else
                                            {
                                                string customerName = String.Empty;

                                                if (!invoice.ActorReference.IsLoaded)
                                                    invoice.ActorReference.Load();

                                                if (invoice.Actor != null)
                                                {
                                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                                        invoice.Actor.CustomerReference.Load();

                                                    if (invoice.Actor.Customer != null)
                                                        customerName = invoice.Actor.Customer.Name;
                                                }

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice)); //, budgetRow != null ? budgetRow.TotalAmount : 0, isVisible: budgetHead != null
                                            }

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                    }
                                    else //TimeProjectRow
                                    {
                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced);

                                        if (dto != null)
                                        {
                                            dto.Value += row.SumAmountCurrency;
                                        }
                                        else
                                        {
                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency));
                                        }

                                        incomePersonnelNotInvoiced += row.SumAmountCurrency;

                                    }
                                }//End of invoice not invoiced
                                else //Invoice invoiced!
                                {
                                    //Invoice invoiced but product is guaranteeamount (Add to NOT invoiced!)
                                    //if (productGuaranteeId != 0 && row.ProductId == productGuaranteeId)
                                    //{                                        
                                    //    ProjectCentralStatusDTO inDto = dtos.Where(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced).FirstOrDefault();

                                    //    //decimal rowGuaranteeAmount = Decimal.Negate(row.SumAmountCurrency);
                                    //    decimal rowGuaranteeAmount = row.SumAmountCurrency;

                                    //    if (inDto != null)
                                    //    {
                                    //        inDto.Value += rowGuaranteeAmount;
                                    //    }
                                    //    else
                                    //    {
                                    //        string customerName = String.Empty;

                                    //        if (!invoice.ActorReference.IsLoaded)
                                    //            invoice.ActorReference.Load();

                                    //        if (invoice.Actor != null)
                                    //        {
                                    //            if (!invoice.Actor.CustomerReference.IsLoaded)
                                    //                invoice.Actor.CustomerReference.Load();

                                    //            if (invoice.Actor.Customer != null)
                                    //                customerName = invoice.Actor.Customer.Name;
                                    //        }

                                    //        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, rowGuaranteeAmount));
                                    //        incomeMaterialNotInvoiced += row.SumAmountCurrency;
                                    //    }                                    
                                    //}

                                    incomeInvoiced += row.SumAmountCurrency;

                                    if (!row.ProductReference.IsLoaded)
                                        row.ProductReference.Load();

                                    decimal budgetTotalAmount = 0;
                                    if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                        budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                        budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                    }

                                    if (!row.IsTimeProjectRow)
                                    {
                                        if (budgetHead != null && !incomeMaterialInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                                        {
                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency,
                                                budgetTotalAmount, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome));

                                            budgetRowIncomeInvoicedAdded = true;
                                            incomeMaterialInvoicedAdded = true;
                                            incomeMaterialInvoiced += row.SumAmountCurrency;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced);

                                            if (dto != null)
                                            {
                                                dto.Value += row.SumAmountCurrency;
                                            }
                                            else
                                            {
                                                string customerName = String.Empty;

                                                if (!invoice.ActorReference.IsLoaded)
                                                    invoice.ActorReference.Load();

                                                if (invoice.Actor != null)
                                                {
                                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                                        invoice.Actor.CustomerReference.Load();

                                                    if (invoice.Actor.Customer != null)
                                                        customerName = invoice.Actor.Customer.Name;
                                                }

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency, value2: row.MarginalIncome, date: invoice.InvoiceDate));
                                            }

                                            incomeMaterialInvoicedAdded = true;
                                            incomeMaterialInvoiced += row.SumAmountCurrency;
                                        }

                                        if (budgetHead != null && !costMaterialAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome)); //, budgetRow != null ? budgetRow.TotalAmount : 0, isVisible: budgetHead != null

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial);
                                            if (purchasePrice != 0) //Dont add zero costs
                                            {
                                                if (dto != null)
                                                {
                                                    dto.Value += purchasePrice;
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, purchasePrice, value2: row.MarginalIncome)); //, budgetRow != null ? budgetRow.TotalAmount : 0, isVisible: budgetHead != null
                                                }
                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                        }
                                    }
                                    else //TimeProject
                                    {
                                        if (budgetHead != null && !incomePersonellInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                                        {
                                            //budgetRow = budgetHead.BudgetRow.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal).FirstOrDefault();

                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency,
                                                budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: row.MarginalIncome));

                                            budgetRowIncomeInvoicedAdded = true;
                                            incomePersonellInvoicedAdded = true;
                                            incomePersonnelInvoiced += row.SumAmountCurrency;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced);

                                            if (dto != null)
                                            {
                                                dto.Value += row.SumAmountCurrency;
                                            }
                                            else
                                            {
                                                string customerName = String.Empty;

                                                if (!invoice.ActorReference.IsLoaded)
                                                    invoice.ActorReference.Load();

                                                if (invoice.Actor != null)
                                                {
                                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                                        invoice.Actor.CustomerReference.Load();

                                                    if (invoice.Actor.Customer != null)
                                                        customerName = invoice.Actor.Customer.Name;
                                                }

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNr + " " + customerName, row.SumAmountCurrency, value2: row.MarginalIncome));
                                            }

                                            incomePersonellInvoicedAdded = true;
                                            incomePersonnelInvoiced += row.SumAmountCurrency;
                                        }
                                    }
                                } //End of Invoice invoiced!
                            }

                            #endregion
                        }

                        #endregion
                    }

                    #endregion

                    #region TimeInvoiceTransactionsProjectView

                    var transactionItemsForProject = GetTransactionsForProjectCentralStatus(projectId);
                    foreach (var transactionItem in transactionItemsForProject.Where(t => t.TimeCodeTransactionType == (int)TimeCodeTransactionType.TimeProject && t.CustomerInvoiceRowId.HasValue))
                    {
                        #region TimeInvoiceTransaction

                        // Check date interval condition
                        if ((dateFrom.HasValue || dateTo.HasValue) && transactionItem.Date.HasValue)
                        {
                            DateTime transDate = transactionItem.Date.Value;
                            if ((dateFrom.HasValue && dateFrom.Value.Date > transDate) || (dateTo.HasValue && dateTo.Value.Date < transDate))
                                continue;
                        }

                        // Transaction is billed if it has a connected CustomerInvocieRow on an invoice that is not draft
                        var billedInvoiceRow = (from r in rowsForProject
                                                where r.CustomerInvoice != null &&
                                                r.CustomerInvoiceRowId == transactionItem.CustomerInvoiceRowId &&
                                                r.AttestStateId == defaultStatusTransferredOrderToInvoice
                                                /*r.CustomerInvoice.Origin != null &&
                                                r.CustomerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                (r.CustomerInvoice.Origin.Status == (int)SoeOriginStatus.Origin || r.CustomerInvoice.Origin.Status == (int)SoeOriginStatus.Voucher)*/
                                                select r).FirstOrDefault();

                        Invoice invoice = InvoiceManager.GetInvoice(transactionItem.InvoiceId, loadOrigin: true);

                        decimal quantity = transactionItem.InvoiceQuantity;

                        if (invoice.IsCredit)
                            quantity = decimal.Negate(quantity);

                        bool isBilled = transactionItem.Exported || billedInvoiceRow != null;
                        if (isBilled)
                        {
                            if (budgetHead != null && !billableMinutesInvoicedAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);
                                billableMinutesInvoicedAdded = true;

                                string customerName = String.Empty;

                                if (!invoice.ActorReference.IsLoaded)
                                    invoice.ActorReference.Load();

                                if (invoice.Actor != null)
                                {
                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                        invoice.Actor.CustomerReference.Load();

                                    if (invoice.Actor.Customer != null)
                                        customerName = invoice.Actor.Customer.Name;
                                }

                                dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)invoice.Origin.Type, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, quantity, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                                billableMinutesInvoiced += quantity;
                            }
                            else
                            {
                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesInvoiced && d.Description == transactionItem.EmployeeName);

                                if (dto != null)
                                {
                                    dto.Value += quantity;
                                }
                                else
                                {
                                    string customerName = String.Empty;

                                    if (!invoice.ActorReference.IsLoaded)
                                        invoice.ActorReference.Load();

                                    if (invoice.Actor != null)
                                    {
                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                            invoice.Actor.CustomerReference.Load();

                                        if (invoice.Actor.Customer != null)
                                            customerName = invoice.Actor.Customer.Name;
                                    }

                                    dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)invoice.Origin.Type, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, quantity));
                                }

                                billableMinutesInvoiced += quantity;
                                billableMinutesInvoicedAdded = true;
                            }
                        }
                        else
                        {
                            /*if (budgetHead != null)
                                budgetRow = budgetHead.BudgetRow.Where(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesNotInvoiced).FirstOrDefault();*/
                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == transactionItem.EmployeeName);

                            if (dto != null)
                            {
                                dto.Value += quantity;
                            }
                            else
                            {
                                string customerName = String.Empty;

                                if (!invoice.ActorReference.IsLoaded)
                                    invoice.ActorReference.Load();

                                if (invoice.Actor != null)
                                {
                                    if (!invoice.Actor.CustomerReference.IsLoaded)
                                        invoice.Actor.CustomerReference.Load();

                                    if (invoice.Actor.Customer != null)
                                        customerName = invoice.Actor.Customer.Name;
                                }

                                dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)invoice.Origin.Type, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, quantity));
                            }

                            billableMinutesNotInvoiced += quantity;
                        }

                        #endregion
                    }

                    #endregion

                    #region TimeCodeTransactions

                    foreach (var timeCodeTrans in project.TimeCodeTransaction.Where(t => t.Type == (int)TimeCodeTransactionType.TimeProject && t.State == (int)SoeEntityState.Active))
                    {
                        if (timeCodeTrans.SupplierInvoiceId.HasValue && timeCodeTrans.AmountCurrency.HasValue && timeCodeTrans.InvoiceQuantity.HasValue)
                        {
                            bool doNotCharge = (timeCodeTrans.DoNotChargeProject != null && (bool)timeCodeTrans.DoNotChargeProject);

                            if (!timeCodeTrans.SupplierInvoiceReference.IsLoaded)
                                timeCodeTrans.SupplierInvoiceReference.Load();

                            if (timeCodeTrans.SupplierInvoice != null && timeCodeTrans.SupplierInvoice.Created != null && timeCodeTrans.SupplierInvoice.Created.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoice.InvoiceDate.Value < (DateTime)dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoice.InvoiceDate.Value > (DateTime)dateTo)
                                    continue;
                            }

                            if (!timeCodeTrans.TimeCodeReference.IsLoaded)
                                timeCodeTrans.TimeCodeReference.Load();

                            if (timeCodeTrans.TimeCode is TimeCodeMaterial)
                            {
                                decimal costMat = 0;
                                decimal costMatVal2 = 0;

                                if (budgetHead != null && !costMaterialAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                    costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                    costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                    string supplierName = String.Empty;

                                    if (timeCodeTrans.SupplierInvoice != null)
                                    {
                                        if (!timeCodeTrans.SupplierInvoice.ActorReference.IsLoaded)
                                            timeCodeTrans.SupplierInvoice.ActorReference.Load();

                                        if (timeCodeTrans.SupplierInvoice.Actor != null)
                                        {
                                            if (!timeCodeTrans.SupplierInvoice.Actor.SupplierReference.IsLoaded)
                                                timeCodeTrans.SupplierInvoice.Actor.SupplierReference.Load();

                                            if (timeCodeTrans.SupplierInvoice.Actor.Supplier != null)
                                                supplierName = timeCodeTrans.SupplierInvoice.Actor.Supplier.Name;
                                        }
                                    }

                                    if (timeCodeTrans.SupplierInvoice.State == (int)SoeEntityState.Active)
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCode.Name, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoice != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.SupplierInvoice.InvoiceNr + " " + supplierName : String.Empty, costMat, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, null, false, costMatVal2));

                                        costMaterial += costMat;
                                        costMaterialAdded = true;
                                    }
                                }
                                else
                                {

                                    costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                    costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.Description == timeCodeTrans.TimeCode.Name);

                                    if (dto != null)
                                    {
                                        dto.Value += costMat;
                                        dto.Value2 += costMatVal2;
                                    }
                                    else
                                    {
                                        string supplierName = String.Empty;

                                        if (timeCodeTrans.SupplierInvoice != null)
                                        {
                                            if (!timeCodeTrans.SupplierInvoice.ActorReference.IsLoaded)
                                                timeCodeTrans.SupplierInvoice.ActorReference.Load();

                                            if (timeCodeTrans.SupplierInvoice.Actor != null)
                                            {
                                                if (!timeCodeTrans.SupplierInvoice.Actor.SupplierReference.IsLoaded)
                                                    timeCodeTrans.SupplierInvoice.Actor.SupplierReference.Load();

                                                if (timeCodeTrans.SupplierInvoice.Actor.Supplier != null)
                                                    supplierName = timeCodeTrans.SupplierInvoice.Actor.Supplier.Name;
                                            }
                                        }

                                        if (timeCodeTrans.SupplierInvoice.State == (int)SoeEntityState.Active)
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCode.Name, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoice != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.SupplierInvoice.InvoiceNr + " " + supplierName : String.Empty, costMat, 0, "", "", null, false, costMatVal2));
                                        }
                                    }

                                    costMaterial += costMat;
                                    costMaterialAdded = true;
                                }
                            }
                            else
                            {
                                decimal costExp = 0;
                                decimal costExpVal2 = 0;

                                if (budgetHead != null && !costExpenseAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);

                                    costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                    costExpVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                    string supplierName = String.Empty;

                                    if (timeCodeTrans.SupplierInvoice != null)
                                    {
                                        if (!timeCodeTrans.SupplierInvoice.ActorReference.IsLoaded)
                                            timeCodeTrans.SupplierInvoice.ActorReference.Load();

                                        if (timeCodeTrans.SupplierInvoice.Actor != null)
                                        {
                                            if (!timeCodeTrans.SupplierInvoice.Actor.SupplierReference.IsLoaded)
                                                timeCodeTrans.SupplierInvoice.Actor.SupplierReference.Load();

                                            if (timeCodeTrans.SupplierInvoice.Actor.Supplier != null)
                                                supplierName = timeCodeTrans.SupplierInvoice.Actor.Supplier.Name;
                                        }
                                    }

                                    if (timeCodeTrans.SupplierInvoice.State == (int)SoeEntityState.Active)
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCode.Name, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoice != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.SupplierInvoice.InvoiceNr + " " + supplierName : String.Empty, costExp, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, null, false, costExpVal2));

                                        costExpense += costExp;
                                        costExpenseAdded = true;
                                    }
                                }
                                else
                                {
                                    costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                    costExpVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostExpense && d.Description == timeCodeTrans.TimeCode.Name);

                                    if (dto != null)
                                    {
                                        dto.Value += costExp;
                                        dto.Value2 += costExpVal2;
                                    }
                                    else
                                    {
                                        string supplierName = String.Empty;

                                        if (timeCodeTrans.SupplierInvoice != null)
                                        {
                                            if (!timeCodeTrans.SupplierInvoice.ActorReference.IsLoaded)
                                                timeCodeTrans.SupplierInvoice.ActorReference.Load();

                                            if (timeCodeTrans.SupplierInvoice.Actor != null)
                                            {
                                                if (!timeCodeTrans.SupplierInvoice.Actor.SupplierReference.IsLoaded)
                                                    timeCodeTrans.SupplierInvoice.Actor.SupplierReference.Load();

                                                if (timeCodeTrans.SupplierInvoice.Actor.Supplier != null)
                                                    supplierName = timeCodeTrans.SupplierInvoice.Actor.Supplier.Name;
                                            }
                                        }

                                        if (timeCodeTrans.SupplierInvoice.State == (int)SoeEntityState.Active)
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCode.Name, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoice != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.SupplierInvoice.InvoiceNr + " " + supplierName : String.Empty, costExp, 0, "", "", null, false, costExpVal2));
                                        }
                                    }

                                    costExpense += costExp;
                                    costExpenseAdded = true;
                                }
                            }
                        }
                        else
                        {
                            if (timeCodeTrans.TimeCode is TimeCodeWork && timeCodeTrans.TimePayrollTransaction.Count > 0)
                            {
                                TimePayrollTransaction tpTrans = timeCodeTrans.TimePayrollTransaction.FirstOrDefault();

                                if (!tpTrans.EmployeeReference.IsLoaded)
                                    tpTrans.EmployeeReference.Load();

                                if (tpTrans.Employee == null)
                                    continue;

                                if (!tpTrans.Employee.ContactPersonReference.IsLoaded)
                                    tpTrans.Employee.ContactPersonReference.Load();

                                if (!timeCodeTrans.ProjectInvoiceDayReference.IsLoaded)
                                    timeCodeTrans.ProjectInvoiceDayReference.Load();

                                if (timeCodeTrans.ProjectInvoiceDay == null)
                                    continue;

                                if (dateFrom != null && timeCodeTrans.ProjectInvoiceDay.Date < dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.ProjectInvoiceDay.Date > dateTo)
                                    continue;

                                if (!timeCodeTrans.ProjectInvoiceDay.ProjectInvoiceWeekReference.IsLoaded)
                                    timeCodeTrans.ProjectInvoiceDay.ProjectInvoiceWeekReference.Load();

                                if (timeCodeTrans.ProjectInvoiceDay.ProjectInvoiceWeek == null || timeCodeTrans.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId == 0)
                                    continue;

                                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(timeCodeTrans.ProjectInvoiceDay.ProjectInvoiceWeek.RecordId, true);

                                if (invoice == null)
                                    continue;

                                if (!timeCodeTrans.TimeCodeReference.IsLoaded)
                                    timeCodeTrans.TimeCodeReference.Load();

                                bool useCalculatedCost = true;
                                decimal invoiceProductCost = 0;
                                if (timeCodeTrans.TimeCode != null)
                                {
                                    TimeCode timeCode = TimeCodeManager.GetTimeCodeWithInvoiceProduct(timeCodeTrans.TimeCode.TimeCodeId, actorCompanyId);
                                    TimeCodeInvoiceProduct timeCodeInvoiceProduct = timeCode.TimeCodeInvoiceProduct.FirstOrDefault();

                                    if (timeCodeInvoiceProduct != null)
                                    {

                                        if ((timeCodeInvoiceProduct.InvoiceProduct != null && !timeCodeInvoiceProduct.InvoiceProduct.UseCalculatedCost.HasValue) ||
                                            (timeCodeInvoiceProduct.InvoiceProduct != null && timeCodeInvoiceProduct.InvoiceProduct.UseCalculatedCost.HasValue && timeCodeInvoiceProduct.InvoiceProduct.UseCalculatedCost.Value == false))
                                        {
                                            useCalculatedCost = false;
                                            invoiceProductCost = timeCodeInvoiceProduct.InvoiceProduct.PurchasePrice;
                                        }
                                    }
                                }

                                // Personnel
                                string employeeName = tpTrans.Employee.ContactPerson != null ? tpTrans.Employee.ContactPerson.FirstName + " " + tpTrans.Employee.ContactPerson.LastName : tpTrans.Employee.Name;

                                string employeeText = string.Empty;
                                if (employeeName != "")
                                    employeeText = ", " + employeeName;


                                string info = string.Empty;
                                decimal quantity = tpTrans.Quantity / 60;

                                if (invoice.IsCredit)
                                    quantity = decimal.Negate(quantity);

                                decimal price = useCalculatedCost ? EmployeeManager.GetEmployeeCalculatedCost(tpTrans.Employee, timeCodeTrans.ProjectInvoiceDay.Date, project.ProjectId) : invoiceProductCost;
                                decimal cost = quantity * price;
                                costPersonnel += cost;

                                if (Math.Abs(quantity) > 0 && price == 0)
                                {
                                    // Warn if no cost is specified on employee
                                    info = string.Format(GetText(116, (int)TermGroup.ProjectCentral, "Det finns anställda som saknar {0} vilket kan göra denna siffra missvisande!"), GetText(65, (int)TermGroup.EmployeeUserEdit).ToLower());
                                }

                                if (budgetHead != null && !costPersonellAdded)
                                {
                                    #region cost personell

                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);

                                    string customerName = String.Empty;

                                    if (!invoice.ActorReference.IsLoaded)
                                        invoice.ActorReference.Load();

                                    if (invoice.Actor != null)
                                    {
                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                            invoice.Actor.CustomerReference.Load();

                                        if (invoice.Actor.Customer != null)
                                            customerName = invoice.Actor.Customer.Name;
                                    }

                                    //workinghours is set in value2
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, employeeName, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName + employeeText, cost, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, value2: quantity));

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                else
                                {
                                    #region cost personell

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.Description == employeeName);

                                    if (dto != null)
                                    {
                                        dto.Value += cost;
                                        dto.Value2 += quantity; //workinghours is set in value2
                                    }
                                    else
                                    {
                                        string customerName = String.Empty;

                                        if (!invoice.ActorReference.IsLoaded)
                                            invoice.ActorReference.Load();

                                        if (invoice.Actor != null)
                                        {
                                            if (!invoice.Actor.CustomerReference.IsLoaded)
                                                invoice.Actor.CustomerReference.Load();

                                            if (invoice.Actor.Customer != null)
                                                customerName = invoice.Actor.Customer.Name;
                                        }

                                        //workinghours is set in value2
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, employeeName, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName + employeeText, cost, info: info, value2: quantity));
                                    }

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }

                                if (overheadCostAsAmountPerHour)
                                {
                                    if (budgetHead != null && !costOverheadAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                                        BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                        if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                        {

                                            string customerName = String.Empty;

                                            if (!invoice.ActorReference.IsLoaded)
                                                invoice.ActorReference.Load();

                                            if (invoice.Actor != null)
                                            {
                                                if (!invoice.Actor.CustomerReference.IsLoaded)
                                                    invoice.Actor.CustomerReference.Load();

                                                if (invoice.Actor.Customer != null)
                                                    customerName = invoice.Actor.Customer.Name;
                                            }

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info));

                                            costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                            costOverheadAdded = true;
                                        }

                                    }
                                    else
                                    {
                                        if (budgetHead != null)
                                        {
                                            ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.OverheadCost);
                                            BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                            if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                            {
                                                if (ohdto != null)
                                                {
                                                    ohdto.Value += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                }
                                                else
                                                {
                                                    string customerName = String.Empty;

                                                    if (!invoice.ActorReference.IsLoaded)
                                                        invoice.ActorReference.Load();

                                                    if (invoice.Actor != null)
                                                    {
                                                        if (!invoice.Actor.CustomerReference.IsLoaded)
                                                            invoice.Actor.CustomerReference.Load();

                                                        if (invoice.Actor.Customer != null)
                                                            customerName = invoice.Actor.Customer.Name;
                                                    }

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)invoice.Origin.Type, invoice.InvoiceId, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNr + " " + customerName, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, info: info));
                                                }

                                                costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                costOverheadAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region If specific rows not exists, add budgetrow or zerorow anyway 
            //Income not invoiced
            ProjectCentralStatusDTO inDtoNotInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeNotInvoiced);
            if (inDtoNotInvoiced == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
            }

            //Income invoiced
            ProjectCentralStatusDTO inDtoInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeInvoiced);
            if (inDtoInvoiced == null)
            {
                if (budgetHead != null)
                {
                    decimal budgetIncomeInvoicedTotalAmount = 0;
                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                    budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                    if (budgetRow != null && budgetRow2 != null)
                    {
                        budgetIncomeInvoicedTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, 0, budgetRow != null ? budgetIncomeInvoicedTotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                    }
                    else
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                    }
                }
                else
                {
                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                }
            }

            //Cost material            
            ProjectCentralStatusDTO inDtoCostMaterial = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsMaterial);
            if (inDtoCostMaterial == null)
            {
                if (budgetHead != null)
                {
                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                    if (budgetRow != null)
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                    }
                    else
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                    }
                }
                else
                {
                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                }
            }

            //Cost personell
            ProjectCentralStatusDTO inDtoCostPersonell = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsPersonell);
            if (inDtoCostPersonell == null)
            {
                if (budgetHead != null)
                {
                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                    if (budgetRow != null)
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                    }
                    else
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                    }
                }
                else
                {
                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0));
                }
            }

            //Cost overhead
            if ((overheadCostAsFixedAmount || overheadCostAsAmountPerHour) && !costOverheadAdded)
            {
                if (budgetHead != null)
                {
                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                    if (budgetRow != null && budgetRow.TotalAmount != 0)
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, String.Empty, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                    }
                }
            }

            //Cost expense
            if (!costExpenseAdded)
            {
                if (budgetHead != null)
                {
                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);
                    if (budgetRow != null && budgetRow.TotalAmount != 0)
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, String.Empty, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), GetText(115, (int)TermGroup.ProjectCentral, "Utlägg") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty));
                    }
                }
            }


            #endregion


            #region Calculate diffs

            foreach (ProjectCentralStatusDTO dto in dtos.Where(r => r.Budget != 0))
            {
                dto.Diff = dtos.Where(r => r.Type == dto.Type).Sum(r => r.Value) - dto.Budget;
            }

            #endregion

            return dtos;
        }

        public IEnumerable<ProjectCentralStatusDTO> GetProjectCentralStatus(int actorCompanyId, int projectId, DateTime? dateFrom, DateTime? dateTo, bool includeChildProjects, bool loadDetails)
        {
            if (!ProjectManager.HasRightToViewProject(projectId))
            {
                return null;
            }
            return GetProjectCentralStatus_v4(actorCompanyId, projectId, dateFrom, dateTo, includeChildProjects, false, loadDetails);
        }

        public IEnumerable<ProjectCentralStatusDTO> GetProjectCentralStatus_v3(int actorCompanyId, int projectId, DateTime? dateFrom, DateTime? dateTo, bool includeChildProjects, bool loadDetails)
        {

            var dtos = new List<ProjectCentralStatusDTO>();

            #region Prereq

            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            int ROTDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, this.ActorCompanyId, 0);
            int RUTDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, this.ActorCompanyId, 0);
            int Green15DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green20DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green50DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, this.ActorCompanyId, 0);
            bool getPurchasePriceFromInvoiceProduct = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.GetPurchasePriceFromInvoiceProduct, this.UserId, this.ActorCompanyId, 0, true);

            #endregion

            #region Get data

            bool incomeMaterialInvoicedAdded = false;
            bool incomePersonellInvoicedAdded = false;
            bool billableMinutesInvoicedAdded = false;
            bool costMaterialAdded = false;
            bool costPersonellAdded = false;
            bool costExpenseAdded = false;
            bool costOverheadAdded = false;
            bool budgetRowIncomeInvoicedAdded = false;

            decimal budgetBillableMinutesIB = 0;
            decimal budgetIncomeIB = 0;
            decimal budgetCostMaterialIB = 0;
            decimal budgetCostPersonellIB = 0;
            decimal budgetExpenseIB = 0;
            decimal budgetOverheadIB = 0;
            decimal budgetBillableMinutes = 0;
            decimal budgetIncome = 0;
            decimal budgetCostMaterial = 0;
            decimal budgetCostPersonell = 0;
            decimal budgetExpense = 0;
            decimal budgetOverhead = 0;

            decimal billableMinutesInvoiced = 0;
            decimal billableMinutesNotInvoiced = 0;
            decimal incomePersonnelInvoiced = 0;
            decimal incomePersonnelNotInvoiced = 0;
            decimal incomeInvoiced = 0;
            decimal incomeMaterialInvoiced = 0;
            decimal incomeMaterialNotInvoiced = 0;

            decimal costPersonnel = 0;
            decimal costMaterial = 0;
            decimal costExpense = 0;
            decimal costOverhead = 0;

            int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
            bool overheadCostAsFixedAmount = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, 0, actorCompanyId, 0);
            bool overheadCostAsAmountPerHour = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, 0, actorCompanyId, 0);
            int fixedPriceProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, actorCompanyId, 0);
            int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, actorCompanyId, 0);

            // Get main project
            List<Project> allProjects = GetProjectsForProjectCentralStatus(new List<int>() { projectId }, actorCompanyId);

            if (includeChildProjects)
            {
                List<Project> currentLevelProjects = new List<Project>();
                currentLevelProjects.AddRange(allProjects);

                // Loop until no further child projects are found
                while (true)
                {
                    List<Project> childProjects = GetProjectsForProjectCentralStatus(currentLevelProjects.FirstOrDefault().ChildProjects.Select(p => p.ProjectId).ToList(), actorCompanyId);
                    if (childProjects.Count == 0)
                        break;

                    // Add found projects to current level projects to be used for next level search
                    currentLevelProjects.Clear();
                    currentLevelProjects.AddRange(childProjects);

                    // Add found projects to all projects list
                    allProjects.AddRange(childProjects);
                }
            }

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var overviewRows = entitiesReadOnly.GetProjectOverview(projectId, includeChildProjects, false, base.ActorCompanyId).ToList();

            var rowsToIgnore = new List<int>();
            rowsToIgnore.AddRange(overviewRows.Where(i => i.TargetRowId != null).Select(i => (int)i.TargetRowId));

            var projectInvoiceRows = overviewRows.GroupBy(p => p.ProjectId).ToList();

            #endregion

            foreach (var project in allProjects)
            {
                #region Budget

                BudgetRow budgetRow = null;
                BudgetRow budgetRow2 = null;
                BudgetHead budgetHead = null;

                if (!project.BudgetHead.IsLoaded)
                    project.BudgetHead.Load();

                if (project.BudgetHead != null && project.BudgetHead.Count > 0)
                {
                    budgetHead = project.BudgetHead.OrderByDescending(h => h.BudgetHeadId).FirstOrDefault();

                    if (!budgetHead.BudgetRow.IsLoaded)
                        budgetHead.BudgetRow.Load();
                    if (loadDetails)
                    {
                        foreach (BudgetRow row in budgetHead.BudgetRow)
                        {
                            if (row.TimeCodeId != null && row.TimeCodeId > 0)
                            {
                                row.TimeCodeReference.Load();
                            }
                        }
                    }
                }

                #endregion

                #region budgetIB

                //budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeTotalIB) : null;
                var ibRowPersonell = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotalIB) : null;
                var ibRowMaterial = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotalIB) : null;
                budgetIncomeIB += ibRowPersonell != null ? ibRowPersonell.TotalAmount : 0;
                budgetIncomeIB += ibRowMaterial != null ? ibRowMaterial.TotalAmount : 0;


                var budgetRowMaterial = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal) : null;
                var budgetRowPersonell = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal) : null;
                var budgetTotal = (budgetRowMaterial != null ? budgetRowMaterial.TotalAmount : 0) + (budgetRowPersonell != null ? budgetRowPersonell.TotalAmount : 0);

                ProjectCentralStatusDTO incomeInvoicedDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced);

                if (incomeInvoicedDto == null)
                {
                    budgetIncome = budgetTotal;

                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetIncomeIB,
                                                budgetTotal, budgetRowMaterial != null && budgetRowMaterial.Modified != null ? ((DateTime)budgetRowMaterial.Modified).ToString() : String.Empty, budgetRowMaterial != null && budgetRowMaterial.ModifiedBy != null ? budgetRowMaterial.ModifiedBy : String.Empty, isEditable: false));
                    budgetRowIncomeInvoicedAdded = true;
                }
                else
                {
                    budgetIncome += budgetTotal;
                    incomeInvoicedDto.Value += (ibRowPersonell != null ? ibRowPersonell.TotalAmount : 0) + (ibRowMaterial != null ? ibRowMaterial.TotalAmount : 0);
                    incomeInvoicedDto.Budget += budgetTotal;
                }

                budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterialIB) : null;
                var budgetRowCostMaterial = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TimeCodeId.IsNullOrEmpty()) : null;

                ProjectCentralStatusDTO costPersonellDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostMaterial);

                if (costPersonellDto == null)
                {
                    budgetCostMaterialIB = budgetRow != null ? budgetRow.TotalAmount : 0;

                    if (loadDetails)
                    {
                        budgetCostMaterial = budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                    }
                    else
                    {
                        budgetCostMaterial = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial ? r.TotalAmount : 0) : 0;
                    }


                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0,
                    budgetCostMaterial, budgetRowCostMaterial != null && budgetRowCostMaterial.Modified != null ? ((DateTime)budgetRowCostMaterial.Modified).ToString() : String.Empty, budgetRowCostMaterial != null && budgetRowCostMaterial.ModifiedBy != null ? budgetRowCostMaterial.ModifiedBy : String.Empty, isEditable: false));
                    costMaterialAdded = true;
                }
                else
                {
                    budgetCostMaterialIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetCostMaterial += budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                    costPersonellDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                    costPersonellDto.Budget += budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                }

                budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonellIB) : null;
                var budgetRowBillableMinutesIB = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB) : null;
                var budgetRowCostPersonell = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell) : null;
                var budgetTRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal) : null;
                if (budgetTRow == null)
                    budgetTRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced) : null;

                ProjectCentralStatusDTO costPersDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostPersonell);

                if (costPersDto == null)
                {
                    //Billableminutesib???
                    budgetCostPersonellIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetBillableMinutesIB = budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;

                    if (loadDetails)
                    {
                        budgetCostPersonell = budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;
                        budgetBillableMinutes = budgetTRow != null ? budgetTRow.TotalAmount : 0;
                    }
                    else
                    {
                        budgetCostPersonell = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell ? r.TotalAmount : 0) : 0;

                        budgetBillableMinutes = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && !r.TimeCodeId.IsNullOrEmpty() ? r.TotalQuantity : 0) : 0;
                        //TODO: Remove budgetTRow (and the type) instead use TotalQuantity on the personell cost row.
                        if (budgetTRow != null)
                        {
                            budgetBillableMinutes += budgetTRow.TotalAmount;
                        }
                    }

                    //workinghours is set in value2
                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0,
                    budgetCostPersonell, budgetRowCostPersonell != null && budgetRowCostPersonell.Modified != null ? ((DateTime)budgetRowCostPersonell.Modified).ToString() : String.Empty, budgetRowCostPersonell != null && budgetRowCostPersonell.ModifiedBy != null ? budgetRowCostPersonell.ModifiedBy : String.Empty, budgetTime: budgetBillableMinutes, value2: budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0, isEditable: false));
                    costPersonellAdded = true;
                }
                else
                {
                    budgetCostMaterialIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetCostMaterial += budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;
                    budgetBillableMinutes += budgetTRow != null ? budgetTRow.TotalAmount : 0;
                    budgetBillableMinutesIB += budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;

                    costPersDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                    costPersDto.Budget += budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;
                    costPersDto.BudgetTime += budgetTRow != null ? budgetTRow.TotalAmount : 0;
                    costPersDto.Value2 += budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;
                }

                budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostIB) : null;
                var budgetRowOverhead = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost) : null;

                ProjectCentralStatusDTO overheadCDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.OverheadCost);

                if (overheadCDto == null)
                {
                    budgetOverheadIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetOverhead = budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;

                    if (budgetOverheadIB == 0)
                    {
                        budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;
                        budgetOverheadIB = budgetBillableMinutesIB * (budgetRow?.TotalAmount ?? 0);
                    }

                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetOverheadIB, budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0, budgetRowOverhead != null && budgetRowOverhead.Modified != null ? ((DateTime)budgetRowOverhead.Modified).ToString() : String.Empty, budgetRowOverhead != null && budgetRowOverhead.ModifiedBy != null ? budgetRowOverhead.ModifiedBy : String.Empty, value2: budgetOverheadIB, isEditable: false));
                    costOverheadAdded = true;
                }
                else
                {
                    budgetOverheadIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetOverhead += budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;

                    overheadCDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                    overheadCDto.Budget += budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;
                }

                budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpenseIB) : null;
                var budgetRowCostExpense = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense) : null;

                ProjectCentralStatusDTO cExpenseDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostExpense);

                if (cExpenseDto == null)
                {
                    budgetExpenseIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetExpense = budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;

                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0, budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0, budgetRowCostExpense != null && budgetRowCostExpense.Modified != null ? ((DateTime)budgetRowCostExpense.Modified).ToString() : String.Empty, budgetRowCostExpense != null && budgetRowCostExpense.ModifiedBy != null ? budgetRowCostExpense.ModifiedBy : String.Empty, isEditable: false));
                    costExpenseAdded = true;
                }
                else
                {
                    budgetExpenseIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                    budgetExpense += budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;

                    cExpenseDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                    cExpenseDto.Budget += budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;
                }

                if (loadDetails && budgetHead != null && budgetHead.BudgetRow != null && budgetHead.State == (int)SoeEntityState.Active)
                {
                    var timeCodeBudgets = budgetHead.BudgetRow.Where(b => !b.TimeCodeId.IsNullOrEmpty() && b.TimeCodeId > 0 && (b.Type == (int)ProjectCentralBudgetRowType.CostMaterial || b.Type == (int)ProjectCentralBudgetRowType.CostPersonell));
                    foreach (var row in timeCodeBudgets)
                    {
                        if (row.Type == (int)ProjectCentralBudgetRowType.CostMaterial && row.TimeCode != null)
                        {
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                                row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, costTypeName: row.TimeCode.Name.HasValue() ? row.TimeCode.Name : "", isEditable: false));
                        }
                        else if (row.Type == (int)ProjectCentralBudgetRowType.CostPersonell && row.TimeCode != null)
                        {
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                                row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, budgetTime: row.TotalQuantity, costTypeName: row.TimeCode.Name.HasValue() ? row.TimeCode.Name : "", isEditable: false));
                        }
                    }
                }

                #endregion

                // Get group
                var group = projectInvoiceRows.FirstOrDefault(g => g.Key == project.ProjectId);
                if (group == null)
                    continue;

                #region Order and invoice rows

                var invoicesForProject = group.GroupBy(i => i.InvoiceId);

                foreach (var invoiceGroup in invoicesForProject)
                {
                    // Get an "invoice" to use
                    var invoice = invoiceGroup.FirstOrDefault();

                    #region CustomerInvoice

                    int isFixedPriceOrder = 0;
                    int isFixedPriceKeepPricesOrder = 0;

                    //Check if fixedpriceorder
                    if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                    {

                        if (fixedPriceProductId != 0)
                            isFixedPriceOrder = invoiceGroup.Count(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);

                        if (fixedPriceKeepPricesProductId != 0)
                            isFixedPriceKeepPricesOrder = invoiceGroup.Count(r => r.ProductId == fixedPriceKeepPricesProductId && fixedPriceKeepPricesProductId != 0);
                    }

                    if (invoiceGroup.Count() == 1 && !invoice.ProductId.HasValue)
                    {
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, 0, 0, isVisible: false, actorName: invoice.CustomerName));
                        continue;
                    }

                    foreach (var row in invoiceGroup.Where(r => r.ProductId == null || (r.ProductId.Value != ROTDeductionProductId && r.ProductId.Value != RUTDeductionProductId && r.ProductId.Value != Green15DeductionProductId && r.ProductId.Value != Green20DeductionProductId && r.ProductId.Value != Green50DeductionProductId)))
                    {
                        //Check date interval condition
                        //Check for each row, not on the invoice head
                        if (dateFrom.HasValue || dateTo.HasValue)
                        {
                            // Do not include rows without dates
                            if (!row.Created.HasValue)
                                continue;

                            DateTime invRowCreatedDate;
                            if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order || invoice.OriginStatus == (int)SoeOriginStatus.Draft)
                                invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;
                            else
                                invRowCreatedDate = invoice.InvoiceDate.Value;

                            if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                continue;
                        }

                        #region CustomerInvoiceRow

                        decimal purchasePrice = row.PurchaseAmount.HasValue ? row.PurchaseAmount.Value : 0;

                        if (invoice.BillingType == (int)TermGroup_BillingType.Credit)
                            purchasePrice = Decimal.Negate(purchasePrice);

                        if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                        {
                            if (row.TargetRowId == null) // Order rows not transferred to invoice
                            {
                                if (row.ProductId != null && (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift))
                                    continue;

                                if ((isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) && (row.ProductId.HasValue && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", row.SalesAmount.Value, 0));

                                if (!row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                {
                                    if (invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) || (row.ProductId != 0 && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))) //if not fixed price
                                    {
                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                        {
                                            if (inDto != null)
                                            {
                                                inDto.Value += row.SalesAmount.Value;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.Value, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                            }
                                        }
                                    }

                                    if (row.ProductId.HasValue && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                    {
                                        if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                        {
                                            if (budgetHead != null && !costPersonellAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                if (budgetTimeRow == null)
                                                    budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                costPersonellAdded = true;
                                                costPersonnel += purchasePrice;
                                            }
                                            else
                                            {
                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (dto != null)
                                                {
                                                    dto.Value += purchasePrice;
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));
                                                }

                                                costPersonellAdded = true;
                                                costPersonnel += purchasePrice;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (budgetHead != null && !costMaterialAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                            if (dto != null)
                                            {
                                                dto.Value += purchasePrice;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                            }

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;

                                        }
                                    }
                                }
                                else //TimeProjectRow
                                {
                                    // Deleted?
                                }
                            }// End of Order rows not transferred to invoice                                    
                            else // Order rows transferred to invoice
                            {
                                // Check fixed price
                                if ((isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) && (row.ProductId.HasValue && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", row.SalesAmount.Value, 0));

                                if (!row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                {
                                    if (row.TargetRowId != null)
                                    {
                                        var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);
                                        if (targetRow != null)
                                        {
                                            if (dateTo.HasValue)
                                            {
                                                DateTime invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;
                                                if (dateTo.Value.Date < invRowCreatedDate.Date)
                                                {
                                                    // Handle transfered row as not invoiced
                                                    if (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) || (row.ProductId.HasValue && row.ProductId.Value != 0 && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice))) //if not fixed price
                                                    {
                                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                        {
                                                            if (inDto != null)
                                                            {
                                                                inDto.Value += row.SalesAmount.Value;
                                                            }
                                                            else
                                                            {
                                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed)
                                                    {
                                                        // Subtract lift from order sum
                                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                        {
                                                            if (inDto != null)
                                                            {
                                                                inDto.Value += row.SalesAmount.Value;
                                                            }
                                                            else
                                                            {
                                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: row.MaterialCode, actorName: invoice.CustomerName));
                                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed)
                                            {
                                                // Subtract lift from order sum
                                                ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                {
                                                    if (inDto != null)
                                                    {
                                                        inDto.Value += row.SalesAmount.Value;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                        incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // Calculate material costs from transfered order rows
                                    if (row.TargetRowId != null)
                                    {
                                        var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);

                                        if (targetRow != null && dateTo.HasValue)
                                        {
                                            if (!row.Created.HasValue)
                                                continue;

                                            DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                            //Order
                                            if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                continue;
                                        }
                                    }
                                    else
                                    {
                                        // Do not include rows without dates
                                        if (!row.Created.HasValue)
                                            continue;

                                        DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                        //Orderrow
                                        if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                            continue;
                                    }

                                    if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                    {
                                        if (budgetHead != null && !costPersonellAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                            var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                            if (budgetTimeRow == null)
                                                budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                            costPersonellAdded = true;
                                            costPersonnel += purchasePrice;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                            if (dto != null)
                                            {
                                                dto.Value += purchasePrice;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                            }

                                            costPersonellAdded = true;
                                            costPersonnel += purchasePrice;

                                        }
                                    }
                                    else
                                    {
                                        if (budgetHead != null && !costMaterialAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, row.MaterialCode, actorName: invoice.CustomerName));

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;
                                        }
                                        else
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                            if (dto != null)
                                            {
                                                dto.Value += purchasePrice;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                            }

                                            costMaterialAdded = true;
                                            costMaterial += purchasePrice;

                                        }
                                    }
                                }
                            }
                        }
                        else if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Invoice)
                        {
                            if (invoice.OriginStatus == (int)SoeOriginStatus.Draft) //Invoice not invoiced! --- Preliminary invoices counts as not invoiced ---
                            {
                                if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                {
                                    ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                    if (inDto != null)
                                    {
                                        inDto.Value += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                        incomeMaterialNotInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }

                                    if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                    {
                                        if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                        {
                                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                            {
                                                if (budgetHead != null && !costPersonellAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                    if (budgetTimeRow == null)
                                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                    }

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (budgetHead != null && !costMaterialAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                            else
                                            {
                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (dto != null)
                                                {
                                                    dto.Value += purchasePrice;
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                }

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                        }
                                    }
                                }
                            }//End of invoice not invoiced
                            else
                            {
                                //Invoice invoiced!
                                incomeInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;

                                decimal budgetTotalAmount = 0;

                                if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                    budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                    budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                }

                                if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                {
                                    if (budgetHead != null && !incomeMaterialInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0,
                                            budgetTotalAmount, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));

                                        budgetRowIncomeInvoicedAdded = true;
                                        incomeMaterialInvoicedAdded = true;
                                        incomeMaterialInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }
                                    else
                                    {
                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                        if (dto != null)
                                        {
                                            dto.Value += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                            dto.Value2 += row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0;
                                        }
                                        else
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                        }

                                        incomeMaterialInvoicedAdded = true;
                                        incomeMaterialInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }

                                    if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                    {
                                        if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                        {
                                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                            {
                                                if (budgetHead != null && !costPersonellAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                    if (budgetTimeRow == null)
                                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                    }

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (budgetHead != null && !costMaterialAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice,
                                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                            else
                                            {
                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));
                                                if (purchasePrice != 0) //Dont add zero costs
                                                {
                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                                    }
                                                    costMaterialAdded = true;
                                                    costMaterial += purchasePrice;
                                                }
                                            }
                                        }
                                    }
                                }
                            } //End of Invoice invoiced!
                        }

                        #endregion
                    }

                    #endregion

                    #region ExpenseRowTransactionView

                    if (!HasExpenseRows.HasValue)
                    {
                        HasExpenseRows = ExpenseManager.HasExpenseRows(actorCompanyId);
                    }

                    if (HasExpenseRows.GetValueOrDefault())
                    {
                        var expenseRows = ExpenseManager.GetExpenseRowsForGrid(invoiceGroup.Key, actorCompanyId, base.UserId, base.RoleId);
                        foreach (var expenseRow in expenseRows.Where(e => (dateFrom == null || e.From >= dateFrom.Value) && (dateTo == null || e.From <= dateTo.Value)))
                        {
                            var typeName = GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber + " - " + expenseRow.EmployeeName;
                            if (budgetHead != null && !costExpenseAdded)
                            {
                                budgetRow = budgetHead.BudgetRow?.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);

                                var dto = CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, (SoeOriginType)invoice.OriginType, invoiceGroup.Key, expenseRow.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), typeName, expenseRow.Amount,
                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: expenseRow.Quantity, budgetTime: budgetRow != null ? budgetRow.TotalAmount : 0, costTypeName: loadDetails ? expenseRow.TimeCodeName : "");

                                if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                    dto.Value2 = expenseRow.Quantity;

                                dtos.Add(dto);

                                costExpense += expenseRow.Amount;
                                costExpenseAdded = true;
                            }
                            else
                            {
                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoiceGroup.Key && d.Type == ProjectCentralBudgetRowType.CostExpense && d.TypeName == typeName && d.CostTypeName == (loadDetails ? expenseRow.TimeCodeName : ""));
                                if (dto != null)
                                {
                                    dto.Value += expenseRow.Amount;
                                    if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                        dto.Value2 += expenseRow.Quantity;
                                }
                                else
                                {
                                    dto = CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, (SoeOriginType)invoice.OriginType, invoiceGroup.Key, expenseRow.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), typeName, expenseRow.Amount, costTypeName: loadDetails ? expenseRow.TimeCodeName : "");
                                    if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                        dto.Value2 = expenseRow.Quantity;
                                    dtos.Add(dto);
                                }

                                costExpense += expenseRow.Amount;
                                costExpenseAdded = true;
                            }
                        }
                    }

                    #endregion
                }

                #endregion

                #region TimeInvoiceTransactionsProjectView

                var transactionItemsForProject = GetTransactionsForProjectCentralStatus(project.ProjectId);
                foreach (var transactionItem in transactionItemsForProject.Where(t => t.TimeCodeTransactionType == (int)TimeCodeTransactionType.TimeProject && t.CustomerInvoiceRowId.HasValue))
                {
                    #region TimeInvoiceTransaction

                    //// Check date interval condition
                    if ((dateFrom.HasValue || dateTo.HasValue) && transactionItem.Date.HasValue)
                    {
                        DateTime transDate = transactionItem.Date.Value;
                        if ((dateFrom.HasValue && dateFrom.Value.Date > transDate) || (dateTo.HasValue && dateTo.Value.Date < transDate))
                            continue;
                    }

                    // Transaction is billed if it has a connected CustomerInvocieRow on an invoice that is not draft
                    var invoiceGroup = invoicesForProject.FirstOrDefault(p => p.Key == transactionItem.InvoiceId);

                    if (invoiceGroup == null)
                        continue;

                    var customerInvoiceRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == transactionItem.CustomerInvoiceRowId);

                    if (customerInvoiceRow == null)
                        continue;

                    decimal quantity = transactionItem.InvoiceQuantity;

                    if (customerInvoiceRow.BillingType == (int)TermGroup_BillingType.Credit)
                        quantity = decimal.Negate(quantity);

                    bool isBilled = false;
                    if (transactionItem.Exported)
                    {
                        isBilled = true;
                    }
                    else
                    {
                        if (customerInvoiceRow.AttestStateId.HasValue && customerInvoiceRow.AttestStateId == defaultStatusTransferredOrderToInvoice && customerInvoiceRow.TargetRowId.HasValue)
                        {
                            var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == customerInvoiceRow.TargetRowId);

                            if (targetRow != null)
                            {
                                if (dateTo.HasValue)
                                {
                                    DateTime invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;
                                    isBilled = (dateTo.Value.Date >= invRowCreatedDate.Date);
                                }
                                else
                                {
                                    isBilled = true;
                                }
                            }
                        }
                    }

                    #region back to income from transactions (old: removed since incomeinvoiced should be set from customerinvoicerow)
                    if (customerInvoiceRow.OriginType == (int)SoeOriginType.Order)
                    {
                        var isFixedPriceOrder = false;
                        if (fixedPriceProductId != 0)
                            isFixedPriceOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);

                        var isFixedPriceKeepPricesOrder = false;
                        if (fixedPriceKeepPricesProductId != 0)
                            isFixedPriceKeepPricesOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceKeepPricesProductId);

                        if (isBilled)
                        {
                            var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == customerInvoiceRow.TargetRowId);
                            if (targetRow == null)
                                targetRow = customerInvoiceRow;

                            var amount = 0m;
                            if (targetRow.SalesAmount.HasValue && targetRow.SalesAmount.Value != 0 && targetRow.Quantity.HasValue && targetRow.Quantity != 0 && transactionItem.InvoiceQuantity != 0)
                                amount = (targetRow.SalesAmount.Value / (targetRow.Quantity.HasValue ? (decimal)targetRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);

                            if (targetRow.OriginStatus == (int)SoeOriginStatus.Draft)
                            {
                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == targetRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                if (dto != null)
                                {
                                    dto.Value += amount;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + ", " + targetRow.CustomerName, amount, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                }

                                incomePersonnelNotInvoiced += amount;
                            }
                            else
                            {
                                decimal budgetTotalAmount = 0;
                                if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                    budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                    budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                }

                                if (budgetHead != null && !incomePersonellInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                                {
                                    // Check date interval condition
                                    if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                        continue;

                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + targetRow.InvoiceNumber + ", " + targetRow.CustomerName, amount,
                                        budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: targetRow.MarginalIncome.HasValue ? targetRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: targetRow.InvoiceDate));

                                    budgetRowIncomeInvoicedAdded = true;
                                    incomePersonellInvoicedAdded = true;
                                    incomePersonnelInvoiced += amount;
                                }
                                else
                                {
                                    // Check date interval condition
                                    if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                        continue;

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == targetRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += amount;
                                    }
                                    else
                                    {
                                        // Check date interval condition
                                        if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                            continue;


                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + targetRow.InvoiceNumber + ", " + targetRow.CustomerName, amount, value2: targetRow.MarginalIncome.HasValue ? targetRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: targetRow.InvoiceDate));
                                    }

                                    incomePersonellInvoicedAdded = true;
                                    incomePersonnelInvoiced += amount;
                                }
                            }
                        }
                        else
                        {
                            if (!isFixedPriceOrder && !isFixedPriceKeepPricesOrder)
                            {
                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                if (customerInvoiceRow.SalesAmount.HasValue && customerInvoiceRow.SalesAmount.Value != 0)
                                {
                                    if (dto != null)
                                    {
                                        dto.Value += (customerInvoiceRow.SalesAmount.Value / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, (customerInvoiceRow.SalesAmount.Value / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60), 0, isVisible: false, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                    }
                                    incomePersonnelNotInvoiced += customerInvoiceRow.SalesAmount.Value;
                                }
                            }
                        }
                    }
                    else if (customerInvoiceRow.OriginType == (int)SoeOriginType.CustomerInvoice)
                    {
                        var amount = ((customerInvoiceRow.SalesAmount.HasValue ? customerInvoiceRow.SalesAmount.Value : 0) / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);
                        if (customerInvoiceRow.OriginStatus == (int)SoeOriginStatus.Draft)
                        {
                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                            if (dto != null)
                            {
                                dto.Value += amount;
                            }
                            else
                            {
                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                            }

                            incomePersonnelNotInvoiced += amount;
                        }
                        else
                        {
                            decimal budgetTotalAmount = 0;
                            if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                            }

                            if (budgetHead != null && !incomePersonellInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                            {
                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount,
                                    budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: customerInvoiceRow.InvoiceDate));

                                budgetRowIncomeInvoicedAdded = true;
                                incomePersonellInvoicedAdded = true;
                                incomePersonnelInvoiced += amount;
                            }
                            else
                            {
                                // Check date interval condition
                                if ((dateFrom.HasValue && dateFrom.Value.Date > customerInvoiceRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < customerInvoiceRow.InvoiceDate))
                                    continue;

                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                if (dto != null)
                                {
                                    dto.Value += amount;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: customerInvoiceRow.InvoiceDate));
                                }

                                incomePersonellInvoicedAdded = true;
                                incomePersonnelInvoiced += amount;
                            }
                        }
                    }
                    #endregion

                    if (isBilled)
                    {
                        if (budgetHead != null && !billableMinutesInvoicedAdded)
                        {
                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);
                            billableMinutesInvoicedAdded = true;

                            dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                            billableMinutesInvoiced += quantity;
                        }
                        else
                        {
                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesInvoiced && d.Description == transactionItem.EmployeeName && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                            if (dto != null)
                            {
                                dto.Value += quantity;
                            }
                            else
                            {
                                dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                            }

                            billableMinutesInvoiced += quantity;
                            billableMinutesInvoicedAdded = true;
                        }
                    }
                    else
                    {
                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == transactionItem.EmployeeName && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                        if (dto != null)
                        {
                            dto.Value += quantity;
                        }
                        else
                        {
                            dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                        }

                        billableMinutesNotInvoiced += quantity;
                    }

                    #endregion
                }

                #endregion

                #region TimeCodeTransactions

                var timeCodeTransactions = entitiesReadOnly.GetTimeCodeTransactionsForProjectOverview(project.ProjectId, useProjectTimeBlock, false);

                foreach (var timeCodeTrans in timeCodeTransactions)
                {
                    if (timeCodeTrans.SupplierInvoiceId.HasValue && timeCodeTrans.AmountCurrency.HasValue && timeCodeTrans.InvoiceQuantity.HasValue)
                    {
                        bool doNotCharge = (timeCodeTrans.DoNotChargeProject != null && (bool)timeCodeTrans.DoNotChargeProject);
                        DateTime? date = timeCodeTrans.SupplierInvoiceDate;

                        if (timeCodeTrans.SupplierInvoiceCreated.HasValue)
                        {
                            if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < (DateTime)dateFrom)
                                continue;

                            if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > (DateTime)dateTo)
                                continue;
                        }

                        if (timeCodeTrans.TimeCodeMaterialId.HasValue)
                        {
                            decimal costMat = 0;
                            decimal costMatVal2 = 0;

                            if (budgetHead != null && !costMaterialAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costMat, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, null, false, costMatVal2, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.Name, date: date));

                                costMaterial += costMat;
                                costMaterialAdded = true;
                            }
                            else
                            {

                                costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.Description == timeCodeTrans.TimeCodeName);

                                if (dto != null)
                                {
                                    dto.Value += costMat;
                                    dto.Value2 += costMatVal2;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costMat, 0, "", "", null, false, costMatVal2, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.Name, date: date));
                                }

                                costMaterial += costMat;
                                costMaterialAdded = true;
                            }
                        }
                        else
                        {
                            decimal costExp = 0;

                            if (budgetHead != null && !costExpenseAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);

                                costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costExp, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.TimeCodeName, date: date));

                                costExpense += costExp;
                                costExpenseAdded = true;
                            }
                            else
                            {
                                costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostExpense && d.Description == timeCodeTrans.TimeCodeName);

                                if (dto != null)
                                {
                                    dto.Value += costExp;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costExp, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.TimeCodeName, date: date));
                                }

                                costExpense += costExp;
                                costExpenseAdded = true;
                            }
                        }
                    }
                    else
                    {
                        if (!useProjectTimeBlock)
                        {
                            #region TimePayrollTransaction - when useProjectTimeBlock is false

                            if (timeCodeTrans.TimeCodeWorkId.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < (DateTime)dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > (DateTime)dateTo)
                                    continue;

                                decimal invoiceProductCost = 0;
                                if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                    invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                                else
                                    invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                                // Personnel
                                string employeeText = string.Empty;
                                if (timeCodeTrans.Name != "")
                                    employeeText = ", " + timeCodeTrans.Name;

                                string info = string.Empty;
                                decimal quantity = (timeCodeTrans.TransactionQuantity.HasValue && timeCodeTrans.TransactionQuantity.Value > 0 ? timeCodeTrans.TransactionQuantity.Value / 60 : 0);

                                if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                    quantity = decimal.Negate(quantity);

                                decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = actorCompanyId }, timeCodeTrans.SupplierInvoiceDate.Value.Date, project.ProjectId) : invoiceProductCost;
                                decimal cost = quantity * price;
                                costPersonnel += cost;

                                if (Math.Abs(quantity) > 0 && price == 0)
                                {
                                    // Warn if no cost is specified on employee
                                    info = string.Format(GetText(116, (int)TermGroup.ProjectCentral, "Det finns anställda som saknar {0} vilket kan göra denna siffra missvisande!"), GetText(65, (int)TermGroup.EmployeeUserEdit).ToLower());
                                }

                                if (budgetHead != null && !costPersonellAdded)
                                {
                                    #region cost personell

                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                    if (budgetTimeRow == null)
                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                    //workinghours is set in value2
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName + employeeText, cost, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, value2: quantity, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                else
                                {
                                    #region cost personell

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.Description == timeCodeTrans.Name && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += cost;
                                        dto.Value2 += quantity; //workinghours is set in value2
                                    }
                                    else
                                    {
                                        //workinghours is set in value2
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName + employeeText, cost, info: info, value2: quantity, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    }

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }

                                if (overheadCostAsAmountPerHour)
                                {
                                    if (budgetHead != null && !costOverheadAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                                        BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                        if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                        {

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + " " + timeCodeTrans.CustomerName, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                            costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                            costOverheadAdded = true;
                                        }

                                    }
                                    else
                                    {
                                        if (budgetHead != null)
                                        {
                                            ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                            BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                            if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                            {
                                                if (ohdto != null)
                                                {
                                                    ohdto.Value += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + " " + timeCodeTrans.CustomerName, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                                }

                                                costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                costOverheadAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region ProjectTimeBlock - when useProjectTimeBlock is true

                            if (timeCodeTrans.TimeCodeWorkId.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > dateTo)
                                    continue;

                                decimal invoiceProductCost = 0;
                                if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                    invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                                else
                                    invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                                // Personnel
                                string employeeText = string.Empty;
                                if (timeCodeTrans.Name != "")
                                    employeeText = ", " + timeCodeTrans.Name;

                                string info = string.Empty;
                                decimal quantity = Convert.ToDecimal(CalendarUtility.TimeSpanToMinutes(timeCodeTrans.Stop.Value, timeCodeTrans.Start.Value)) / 60;

                                if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                    quantity = decimal.Negate(quantity);

                                decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = actorCompanyId }, timeCodeTrans.SupplierInvoiceDate.Value.Date, project.ProjectId) : invoiceProductCost;
                                decimal cost = quantity * price;
                                costPersonnel += cost;

                                if (Math.Abs(quantity) > 0 && price == 0)
                                {
                                    // Warn if no cost is specified on employee
                                    info = string.Format(GetText(116, (int)TermGroup.ProjectCentral, "Det finns anställda som saknar {0} vilket kan göra denna siffra missvisande!"), GetText(65, (int)TermGroup.EmployeeUserEdit).ToLower());
                                }

                                if (budgetHead != null && !costPersonellAdded)
                                {
                                    #region cost personell

                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);

                                    //workinghours is set in value2
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName + employeeText, cost, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, value2: quantity, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                else
                                {
                                    #region cost personell

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.Description == timeCodeTrans.Name && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += cost;
                                        dto.Value2 += quantity; //workinghours is set in value2
                                    }
                                    else
                                    {
                                        //workinghours is set in value2
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName + employeeText, cost, info: info, value2: quantity, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    }

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }

                                if (overheadCostAsAmountPerHour)
                                {
                                    if (budgetHead != null && !costOverheadAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                                        BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                        if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + " " + timeCodeTrans.CustomerName, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                            costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                            costOverheadAdded = true;
                                        }

                                    }
                                    else
                                    {
                                        if (budgetHead != null)
                                        {
                                            ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                            BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                            if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                            {
                                                if (ohdto != null)
                                                {
                                                    ohdto.Value += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + " " + timeCodeTrans.Name, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                                }

                                                costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                costOverheadAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                #endregion

            }

            #region If specific rows not exists, add budgetrow or zerorow anyway 
            //Income not invoiced
            ProjectCentralStatusDTO inDtoNotInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeNotInvoiced);
            if (inDtoNotInvoiced == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0, isEditable: false));
            }

            //Income invoiced
            ProjectCentralStatusDTO inDtoInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeInvoiced);
            if (inDtoInvoiced == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetIncomeIB, budgetIncome, isEditable: false));
            }

            //Cost material            
            ProjectCentralStatusDTO inDtoCostMaterial = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsMaterial);
            if (inDtoCostMaterial == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetCostMaterialIB, budgetCostMaterial, isEditable: false));
            }

            //Cost personell
            ProjectCentralStatusDTO inDtoCostPersonell = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsPersonell);
            if (inDtoCostPersonell == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetCostPersonellIB, budgetCostPersonell, value2: budgetBillableMinutesIB, budgetTime: budgetBillableMinutes, isEditable: false));
            }

            //Cost overhead
            if ((overheadCostAsFixedAmount || overheadCostAsAmountPerHour) && !costOverheadAdded)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, String.Empty, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", budgetOverheadIB, budgetOverhead, isEditable: false));
            }

            //Cost expense
            if (!costExpenseAdded)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, String.Empty, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), GetText(115, (int)TermGroup.ProjectCentral, "Utlägg") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", budgetExpenseIB, budgetExpense, isEditable: false));
            }


            #endregion

            #region Calculate diffs
            if (loadDetails)
            {
                foreach (ProjectCentralStatusDTO dto in dtos.Where(r => r.Budget != 0 || r.BudgetTime != 0))
                {
                    dto.Diff = dtos.Where(r => r.Type == dto.Type && r.CostTypeName == dto.CostTypeName).Sum(r => r.Value) - dto.Budget;
                    //dto.Diff2 = 
                    dto.Diff2 = dtos.Where(r => r.Type == dto.Type && r.CostTypeName == dto.CostTypeName).Sum(r => r.Value2) - (dto.BudgetTime / 60);
                }
            }
            else
            {
                foreach (ProjectCentralStatusDTO dto in dtos.Where(r => r.Budget != 0 || r.BudgetTime != 0))
                {
                    dto.Diff = dtos.Where(r => r.Type == dto.Type).Sum(r => r.Value) - dto.Budget;
                    dto.Diff2 = dtos.Where(r => r.Type == dto.Type).Sum(r => r.Value2) - (dto.BudgetTime / 60);
                }
            }


            #endregion

            return dtos.OrderBy(d => d.GroupRowType);
        }

        public List<ProjectCentralStatusDTO> GetProjectCentralStatus_v4(int actorCompanyId, int projectId, DateTime? dateFrom, DateTime? dateTo, bool includeChildProjects, bool excludeInternalOrders, bool loadDetails)
        {

            var dtos = new List<ProjectCentralStatusDTO>();

            #region Prereq

            bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            int ROTDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, this.ActorCompanyId, 0);
            int ROT50DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, this.ActorCompanyId, 0);
            int RUTDeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, this.ActorCompanyId, 0);
            int Green15DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green20DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green50DeductionProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, this.ActorCompanyId, 0);
            bool getPurchasePriceFromInvoiceProduct = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.GetPurchasePriceFromInvoiceProduct, this.UserId, this.ActorCompanyId, 0, true);
            bool useDateIntervalInIncomeNotInvoiced = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseDateIntervalInIncomeNotInvoiced, this.UserId, this.ActorCompanyId, 0);

            #endregion

            #region Get data

            bool incomeMaterialInvoicedAdded = false;
            bool billableMinutesInvoicedAdded = false;
            bool costMaterialAdded = false;
            bool costPersonellAdded = false;
            bool costExpenseAdded = false;
            bool costOverheadAdded = false;
            bool budgetRowIncomeInvoicedAdded = false;

            decimal budgetBillableMinutesIB = 0;
            decimal budgetIncomeIB = 0;
            decimal budgetCostMaterialIB = 0;
            decimal budgetCostPersonellIB = 0;
            decimal budgetExpenseIB = 0;
            decimal budgetOverheadIB = 0;
            decimal budgetBillableMinutes = 0;
            decimal budgetIncome = 0;
            decimal budgetCostMaterial = 0;
            decimal budgetCostPersonell = 0;
            decimal budgetExpense = 0;
            decimal budgetOverhead = 0;
            decimal budgetOverheadPerHour = 0;

            decimal billableMinutesInvoiced = 0;
            decimal billableMinutesNotInvoiced = 0;
            decimal incomeInvoiced = 0;
            decimal incomeMaterialInvoiced = 0;
            decimal incomeMaterialNotInvoiced = 0;

            decimal costPersonnel = 0;
            decimal costMaterial = 0;
            decimal costExpense = 0;
            decimal costOverhead = 0;

            int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
            bool overheadCostAsFixedAmount = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, 0, actorCompanyId, 0);
            bool overheadCostAsAmountPerHour = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, 0, actorCompanyId, 0);
            int fixedPriceProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, actorCompanyId, 0);
            int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, actorCompanyId, 0);

            // Get all projects
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<Project> allProjects = GetProjectsForProjectCentralStatus(entitiesReadOnly, includeChildProjects ? ProjectManager.GetProjectIdsFromMain(entitiesReadOnly, ActorCompanyId, projectId) : new List<int>() { projectId }, actorCompanyId);

            /*
            // Get main project
            List<Project> allProjects = GetProjectsForProjectCentralStatus(new List<int>() { projectId }, actorCompanyId);

            if (includeChildProjects)
            {
                List<Project> currentLevelProjects = new List<Project>();
                currentLevelProjects.AddRange(allProjects);

                // Loop until no further child projects are found
                while (true)
                {
                    List<Project> childProjects = GetProjectsForProjectCentralStatus(currentLevelProjects.FirstOrDefault().ChildProjects.Select(p => p.ProjectId).ToList(), actorCompanyId);
                    if (childProjects.Count == 0)
                        break;

                    // Add found projects to current level projects to be used for next level search
                    currentLevelProjects.Clear();
                    currentLevelProjects.AddRange(childProjects);

                    // Add found projects to all projects list
                    allProjects.AddRange(childProjects);
                }
            }*/

            var overviewRows = entitiesReadOnly.GetProjectOverview(projectId, includeChildProjects, excludeInternalOrders, base.ActorCompanyId).ToList();

            var rowsToIgnore = new List<int>();
            rowsToIgnore.AddRange(overviewRows.Where(i => i.TargetRowId != null).Select(i => (int)i.TargetRowId));

            var projectInvoiceRows = overviewRows.GroupBy(p => p.ProjectId).ToList();

            #endregion

            foreach (var project in allProjects)
            {
                BudgetRow budgetRow = null;
                BudgetRow budgetRow2 = null;
                BudgetHead budgetHead = null;

                bool hasFromDate = dateFrom.HasValue;
                bool isNewBudget = ProjectBudgetManager.HasExtendedProjectBudget(project.ProjectId);
                if (isNewBudget)
                {
                    dtos = GetProjectCentralBudget(project.ProjectId, dtos, hasFromDate, loadDetails, ref budgetIncomeIB, ref budgetIncome, ref budgetRowIncomeInvoicedAdded, ref budgetCostMaterialIB, ref budgetCostMaterial, ref costMaterialAdded, ref budgetCostPersonellIB, ref budgetBillableMinutesIB, ref budgetCostPersonell, ref budgetBillableMinutes, ref costPersonellAdded,
                        ref budgetOverheadIB, ref budgetOverhead, ref budgetOverheadPerHour, ref costOverheadAdded, ref budgetExpenseIB, ref budgetExpense, ref costExpenseAdded);
                }
                else
                {
                    #region Budget

                    // Use normal budget

                    if (!project.BudgetHead.IsLoaded)
                        project.BudgetHead.Load();

                    if (project.BudgetHead != null && project.BudgetHead.Count > 0)
                    {
                        budgetHead = project.BudgetHead.OrderByDescending(h => h.BudgetHeadId).FirstOrDefault();

                        if (!budgetHead.BudgetRow.IsLoaded)
                            budgetHead.BudgetRow.Load();
                        if (loadDetails)
                        {
                            foreach (BudgetRow row in budgetHead.BudgetRow)
                            {
                                if (row.TimeCodeId != null && row.TimeCodeId > 0)
                                {
                                    row.TimeCodeReference.Load();
                                }
                            }
                        }
                    }

                    #endregion

                    #region budgetIB

                    //budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeTotalIB) : null;
                    var ibRowPersonell = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotalIB) : null;
                    var ibRowMaterial = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotalIB) : null;
                    budgetIncomeIB += ibRowPersonell != null ? ibRowPersonell.TotalAmount : 0;
                    budgetIncomeIB += ibRowMaterial != null ? ibRowMaterial.TotalAmount : 0;


                    var budgetRowIncomeMaterial = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal) : null;
                    var budgetRowIncomePersonell = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal) : null;
                    var budgetTotal = (budgetRowIncomeMaterial != null ? budgetRowIncomeMaterial.TotalAmount : 0) + (budgetRowIncomePersonell != null ? budgetRowIncomePersonell.TotalAmount : 0);

                    ProjectCentralStatusDTO incomeInvoicedDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced);

                    if (incomeInvoicedDto == null)
                    {
                        budgetIncome = budgetTotal;

                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetIncomeIB,
                                                    budgetTotal, budgetRowIncomeMaterial != null && budgetRowIncomeMaterial.Modified != null ? ((DateTime)budgetRowIncomeMaterial.Modified).ToString() : String.Empty, budgetRowIncomeMaterial != null && budgetRowIncomeMaterial.ModifiedBy != null ? budgetRowIncomeMaterial.ModifiedBy : String.Empty, isEditable: false));
                        budgetRowIncomeInvoicedAdded = true;
                    }
                    else
                    {
                        budgetIncome += budgetTotal;
                        incomeInvoicedDto.Value += (ibRowPersonell != null ? ibRowPersonell.TotalAmount : 0) + (ibRowMaterial != null ? ibRowMaterial.TotalAmount : 0);
                        incomeInvoicedDto.Budget += budgetTotal;
                    }

                    budgetRow = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterialIB) : null;
                    var budgetRowCostMaterial = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TimeCodeId.IsNullOrEmpty()) : null;

                    ProjectCentralStatusDTO costPersonellDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostMaterial);

                    if (costPersonellDto == null)
                    {
                        budgetCostMaterialIB = budgetRow != null ? budgetRow.TotalAmount : 0;

                        if (loadDetails)
                        {
                            budgetCostMaterial = budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                        }
                        else
                        {
                            budgetCostMaterial = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial ? r.TotalAmount : 0) : 0;
                        }


                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0,
                        budgetCostMaterial, budgetRowCostMaterial != null && budgetRowCostMaterial.Modified != null ? ((DateTime)budgetRowCostMaterial.Modified).ToString() : String.Empty, budgetRowCostMaterial != null && budgetRowCostMaterial.ModifiedBy != null ? budgetRowCostMaterial.ModifiedBy : String.Empty, isEditable: false));
                        costMaterialAdded = true;
                    }
                    else
                    {
                        budgetCostMaterialIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                        costPersonellDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;

                        if (loadDetails)
                        {
                            budgetCostMaterial += budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                            costPersonellDto.Budget += budgetRowCostMaterial != null ? budgetRowCostMaterial.TotalAmount : 0;
                        }
                        else
                        {
                            var sumCostMaterial = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial ? r.TotalAmount : 0) : 0;

                            budgetCostMaterial += sumCostMaterial;
                            costPersonellDto.Budget += sumCostMaterial;
                        }


                    }

                    budgetRow = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonellIB) : null;
                    var budgetRowBillableMinutesIB = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB) : null;
                    var budgetRowCostPersonell = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell) : null;
                    var budgetTRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal) : null;
                    if (budgetTRow == null)
                        budgetTRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced) : null;

                    ProjectCentralStatusDTO costPersDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostPersonell);

                    if (costPersDto == null)
                    {
                        //Billableminutesib???
                        budgetCostPersonellIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetBillableMinutesIB = budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;

                        if (loadDetails)
                        {
                            budgetCostPersonell = budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;
                            budgetBillableMinutes = budgetTRow != null ? budgetTRow.TotalAmount : 0;
                        }
                        else
                        {
                            budgetCostPersonell = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell ? r.TotalAmount : 0) : 0;

                            budgetBillableMinutes = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && !r.TimeCodeId.IsNullOrEmpty() ? r.TotalQuantity : 0) : 0;
                            //TODO: Remove budgetTRow (and the type) instead use TotalQuantity on the personell cost row.
                            if (budgetTRow != null)
                            {
                                budgetBillableMinutes += budgetTRow.TotalAmount;
                            }
                        }

                        //workinghours is set in value2
                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0,
                        budgetCostPersonell, budgetRowCostPersonell != null && budgetRowCostPersonell.Modified != null ? ((DateTime)budgetRowCostPersonell.Modified).ToString() : String.Empty, budgetRowCostPersonell != null && budgetRowCostPersonell.ModifiedBy != null ? budgetRowCostPersonell.ModifiedBy : String.Empty, budgetTime: budgetBillableMinutes, value2: budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0, isEditable: false));
                        costPersonellAdded = true;
                    }
                    else
                    {
                        budgetCostMaterialIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetBillableMinutesIB += budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;
                        costPersDto.Value2 += budgetRowBillableMinutesIB != null ? budgetRowBillableMinutesIB.TotalAmount / 60 : 0;

                        if (loadDetails)
                        {
                            budgetBillableMinutes += budgetTRow != null ? budgetTRow.TotalAmount : 0;
                            budgetCostMaterial += budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;

                            costPersDto.Budget += budgetRowCostPersonell != null ? budgetRowCostPersonell.TotalAmount : 0;
                            costPersDto.BudgetTime += budgetTRow != null ? budgetTRow.TotalAmount : 0;
                        }
                        else
                        {
                            var sumCostPersonell = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell ? r.TotalAmount : 0) : 0;
                            var sumBillableMinutes = budgetHead != null ? budgetHead.BudgetRow.Sum(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && !r.TimeCodeId.IsNullOrEmpty() ? r.TotalQuantity : 0) : 0;

                            budgetCostMaterial += sumCostPersonell;
                            budgetBillableMinutes += sumBillableMinutes;

                            if (budgetTRow != null)
                            {
                                budgetBillableMinutes += budgetTRow.TotalAmount;
                            }

                            costPersDto.Budget += sumCostPersonell;
                            costPersDto.BudgetTime += sumBillableMinutes;
                        }



                    }

                    budgetRow = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostIB) : null;
                    var budgetRowOverhead = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost) : null;

                    ProjectCentralStatusDTO overheadCDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.OverheadCost);

                    if (overheadCDto == null)
                    {
                        budgetOverheadIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetOverhead = budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;

                        if (budgetOverheadIB == 0)
                        {
                            budgetRow = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;
                            budgetOverheadIB = budgetBillableMinutesIB * (budgetRow?.TotalAmount ?? 0);
                        }

                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetOverheadIB, budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0, budgetRowOverhead != null && budgetRowOverhead.Modified != null ? ((DateTime)budgetRowOverhead.Modified).ToString() : String.Empty, budgetRowOverhead != null && budgetRowOverhead.ModifiedBy != null ? budgetRowOverhead.ModifiedBy : String.Empty, value2: budgetOverheadIB, isEditable: false));
                        costOverheadAdded = true;
                    }
                    else
                    {
                        budgetOverheadIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetOverhead += budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;

                        overheadCDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                        overheadCDto.Budget += budgetRowOverhead != null ? budgetRowOverhead.TotalAmount : 0;
                    }

                    budgetRow = budgetHead != null && !hasFromDate ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpenseIB) : null;
                    var budgetRowCostExpense = budgetHead != null ? budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense) : null;

                    ProjectCentralStatusDTO cExpenseDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostExpense);

                    if (cExpenseDto == null)
                    {
                        budgetExpenseIB = budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetExpense = budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;

                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRow != null ? budgetRow.TotalAmount : 0, budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0, budgetRowCostExpense != null && budgetRowCostExpense.Modified != null ? ((DateTime)budgetRowCostExpense.Modified).ToString() : String.Empty, budgetRowCostExpense != null && budgetRowCostExpense.ModifiedBy != null ? budgetRowCostExpense.ModifiedBy : String.Empty, isEditable: false));
                        costExpenseAdded = true;
                    }
                    else
                    {
                        budgetExpenseIB += budgetRow != null ? budgetRow.TotalAmount : 0;
                        budgetExpense += budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;

                        cExpenseDto.Value += budgetRow != null ? budgetRow.TotalAmount : 0;
                        cExpenseDto.Budget += budgetRowCostExpense != null ? budgetRowCostExpense.TotalAmount : 0;
                    }

                    // Reworked to sum all rows containing TimeCodeId that is missing from code above when not using "loadDetails"
                    if (loadDetails && budgetHead != null && budgetHead.BudgetRow != null && budgetHead.State == (int)SoeEntityState.Active)
                    {
                        var timeCodeBudgets = budgetHead.BudgetRow.Where(b => !b.TimeCodeId.IsNullOrEmpty() && b.TimeCodeId > 0 && (b.Type == (int)ProjectCentralBudgetRowType.CostMaterial || b.Type == (int)ProjectCentralBudgetRowType.CostPersonell));
                        foreach (var row in timeCodeBudgets)
                        {
                            if (row.Type == (int)ProjectCentralBudgetRowType.CostMaterial && row.TimeCode != null)
                            {
                                var costMaterialTimeCodeDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (row.TimeCode.Name.HasValue() ? row.TimeCode.Name : ""));
                                if (costMaterialTimeCodeDto == null)
                                {
                                    budgetCostMaterial += row != null ? row.TotalAmount : 0;
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                                        row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, costTypeName: row.TimeCode.Name.HasValue() ? row.TimeCode.Name : "", isEditable: false));
                                }
                                else
                                {
                                    budgetCostMaterial += row != null ? row.TotalAmount : 0;
                                    costMaterialTimeCodeDto.Budget += row != null ? row.TotalAmount : 0;
                                }
                            }
                            else if (row.Type == (int)ProjectCentralBudgetRowType.CostPersonell && row.TimeCode != null)
                            {
                                var costPersonellTimeCodeDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (row.TimeCode.Name.HasValue() ? row.TimeCode.Name : ""));
                                if (costPersonellTimeCodeDto == null)
                                {
                                    budgetCostPersonell += row != null ? row.TotalAmount : 0;
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                                        row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, budgetTime: row.TotalQuantity, costTypeName: row.TimeCode.Name.HasValue() ? row.TimeCode.Name : "", isEditable: false));
                                }
                                else
                                {
                                    budgetCostPersonell += row != null ? row.TotalAmount : 0;
                                    costPersonellTimeCodeDto.Budget += row != null ? row.TotalAmount : 0;

                                    budgetBillableMinutes += row != null ? row.TotalQuantity : 0;
                                    costPersonellTimeCodeDto.BudgetTime += row != null ? row.TotalQuantity : 0;
                                }
                            }
                        }
                    }

                    #endregion
                }

                // Get group
                var group = projectInvoiceRows.FirstOrDefault(g => g.Key == project.ProjectId);

                if (group != null)
                {
                    #region Order and invoice rows 
                    var invoicesForProject = group.GroupBy(i => i.InvoiceId).ToList();

                    foreach (var invoiceGroup in invoicesForProject)
                    {
                        // Get an "invoice" to use
                        var invoice = invoiceGroup.FirstOrDefault();

                        // Check fixed price - add fixed row to handle it correctly in reports even when there is no fixed price row
                        if (invoice.FixedPriceOrder)
                        {
                            var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", fixedPriceRowsExist ? 0 : budgetIncomeIB, 0));
                        }

                        #region CustomerInvoice

                        int isFixedPriceOrder = 0;
                        int isFixedPriceKeepPricesOrder = 0;

                        //Check if fixedpriceorder
                        if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                        {
                            if (fixedPriceProductId != 0)
                                isFixedPriceOrder = invoiceGroup.Count(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);
                            if (fixedPriceKeepPricesProductId != 0)
                                isFixedPriceKeepPricesOrder = invoiceGroup.Count(r => r.ProductId == fixedPriceKeepPricesProductId && fixedPriceKeepPricesProductId != 0);
                        }

                        if (invoiceGroup.Count() == 1 && !invoice.ProductId.HasValue)
                        {
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, 0, 0, isVisible: false, actorName: invoice.CustomerName));
                            continue;
                        }

                        foreach (var row in invoiceGroup.Where(r => r.ProductId == null || (r.ProductId.Value != ROTDeductionProductId && r.ProductId.Value != ROT50DeductionProductId && r.ProductId.Value != RUTDeductionProductId && r.ProductId.Value != Green15DeductionProductId && r.ProductId.Value != Green20DeductionProductId && r.ProductId.Value != Green50DeductionProductId)).ToList())
                        {
                            if (useDateIntervalInIncomeNotInvoiced && row.IsTimeProjectRow.HasValue && row.IsTimeProjectRow.Value)
                                continue;

                            //Check date interval condition
                            //Check for each row, not on the invoice head
                            if (dateFrom.HasValue || dateTo.HasValue)
                            {
                                // Do not include rows without dates
                                if (!row.Created.HasValue)
                                    continue;

                                DateTime invRowCreatedDate;
                                if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order || invoice.OriginStatus == (int)SoeOriginStatus.Draft)
                                {
                                    invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                    if (row.TargetRowId != null)
                                    {
                                        var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);
                                        if (targetRow != null && targetRow.InvoiceDate.HasValue)
                                        {
                                            invRowCreatedDate = targetRow.InvoiceDate.Value;

                                            if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date && row.CalculationType != (int)TermGroup_InvoiceProductCalculationType.Clearing && row.CalculationType != (int)TermGroup_InvoiceProductCalculationType.Lift)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                if (useDateIntervalInIncomeNotInvoiced)
                                                {
                                                    invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                                    if (row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                        continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (useDateIntervalInIncomeNotInvoiced && row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                continue;
                                        }
                                    }
                                    else
                                    {
                                        if (useDateIntervalInIncomeNotInvoiced && row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                            continue;
                                    }
                                }
                                else
                                {
                                    invRowCreatedDate = invoice.InvoiceDate.Value;

                                    if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date)
                                        continue;
                                }
                            }

                            #region CustomerInvoiceRow

                            decimal purchasePrice = row.PurchaseAmount.HasValue ? row.PurchaseAmount.Value : 0;

                            if (invoice.BillingType == (int)TermGroup_BillingType.Credit)
                                purchasePrice = Decimal.Negate(purchasePrice);

                            if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                            {
                                if (row.TargetRowId == null) // Order rows not transferred to invoice
                                {
                                    if (row.ProductId != null && (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift))
                                        continue;

                                    if ((isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) && (row.ProductId.HasValue && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))
                                    {
                                        var fixedDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                        if (fixedDto != null)
                                        {
                                            fixedDto.Value += row.SalesAmount.Value;
                                        }
                                        else
                                        {
                                            var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", fixedPriceRowsExist ? row.SalesAmount.Value : row.SalesAmount.Value + budgetIncomeIB, 0));
                                        }
                                    }

                                    if (invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) || (row.ProductId != 0 && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))) //if not fixed price
                                    {
                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                        {
                                            if (inDto != null)
                                            {
                                                inDto.Value += row.SalesAmount.Value;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.Value, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                            }
                                        }
                                    }

                                    if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                    {
                                        DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value.Date : row.Created.Value.Date;
                                        if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date)
                                            continue;

                                        if (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date)
                                            continue;

                                        if (row.ProductId.HasValue && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                        {
                                            if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                            {
                                                if (budgetHead != null && !costPersonellAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                    if (budgetTimeRow == null)
                                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                        dto.Value2 += row.Quantity.HasValue ? row.Quantity.Value : 0;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));
                                                    }

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;

                                                }

                                                // Billable minutes not invoice
                                                if (row.Quantity.HasValue)
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat") && d.CostTypeName == (loadDetails ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += row.Quantity.Value * 60;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.Quantity.Value * 60, costTypeName: loadDetails ? row.MaterialCode : ""));
                                                    }

                                                    billableMinutesNotInvoiced += row.Quantity.Value * 60;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (budgetHead != null && !costMaterialAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                            else
                                            {
                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (dto != null)
                                                {
                                                    dto.Value += purchasePrice;
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                }

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;

                                            }
                                        }
                                    }
                                }// End of Order rows not transferred to invoice                                    
                                else // Order rows transferred to invoice
                                {
                                    // Check fixed price
                                    if ((isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) && (row.ProductId.HasValue && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))
                                    {
                                        var fixedDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                        if (fixedDto != null)
                                        {
                                            fixedDto.Value += row.SalesAmount.Value;
                                        }
                                        else
                                        {
                                            var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", fixedPriceRowsExist ? row.SalesAmount.Value : row.SalesAmount.Value + budgetIncomeIB, 0));
                                        }
                                    }

                                    if (row.TargetRowId != null)
                                    {
                                        var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);
                                        if (targetRow != null)
                                        {
                                            if (dateTo.HasValue)
                                            {
                                                DateTime invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;
                                                if (dateTo.Value.Date < invRowCreatedDate.Date)
                                                {
                                                    // Handle transfered row as not invoiced
                                                    if (!(isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) || (row.ProductId.HasValue && row.ProductId.Value != 0 && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice))) //if not fixed price
                                                    {
                                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                        {
                                                            if (inDto != null)
                                                            {
                                                                inDto.Value += row.SalesAmount.Value;
                                                            }
                                                            else
                                                            {
                                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                            }
                                                        }
                                                    }

                                                    // Billable minutes not invoice
                                                    if (row.Quantity.HasValue && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service && (!row.IsTimeProjectRow.HasValue || !row.IsTimeProjectRow.Value))
                                                    {
                                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat") && d.CostTypeName == (loadDetails ? row.MaterialCode : ""));

                                                        if (dto != null)
                                                        {
                                                            dto.Value += row.Quantity.Value * 60;
                                                        }
                                                        else
                                                        {
                                                            dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.Quantity.Value * 60, costTypeName: loadDetails ? row.MaterialCode : ""));
                                                        }

                                                        billableMinutesNotInvoiced += row.Quantity.Value * 60;
                                                    }
                                                }
                                                else
                                                {
                                                    if (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed)
                                                    {
                                                        // Subtract lift from order sum
                                                        ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                        {
                                                            if (inDto != null)
                                                            {
                                                                inDto.Value += row.SalesAmount.Value;
                                                            }
                                                            else
                                                            {
                                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: row.MaterialCode, actorName: invoice.CustomerName));
                                                                incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice && invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed)
                                            {
                                                // Subtract lift from order sum
                                                ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                                {
                                                    if (inDto != null)
                                                    {
                                                        inDto.Value += row.SalesAmount.Value;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, 0, isVisible: false, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                        incomeMaterialNotInvoiced += row.SalesAmount.Value;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                    {

                                        // Calculate material costs from transfered order rows
                                        if (row.TargetRowId != null)
                                        {
                                            var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);

                                            if (targetRow != null && dateTo.HasValue)
                                            {
                                                if (!row.Created.HasValue)
                                                    continue;

                                                DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                                //Order
                                                if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                    continue;
                                            }
                                        }
                                        else
                                        {
                                            // Do not include rows without dates
                                            if (!row.Created.HasValue)
                                                continue;

                                            DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                            //Orderrow
                                            if ((dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date) || (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                continue;
                                        }

                                        if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                        {
                                            if (!row.IsTimeProjectRow.HasValue || !row.IsTimeProjectRow.Value)
                                            {
                                                if (budgetHead != null && !costPersonellAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                    if (budgetTimeRow == null)
                                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                        dto.Value2 += row.Quantity.HasValue ? row.Quantity.Value : 0;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                    }

                                                    costPersonellAdded = true;
                                                    costPersonnel += purchasePrice;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (budgetHead != null && !costMaterialAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                    budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, row.MaterialCode, actorName: invoice.CustomerName));

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;
                                            }
                                            else
                                            {
                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                if (dto != null)
                                                {
                                                    dto.Value += purchasePrice;
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                }

                                                costMaterialAdded = true;
                                                costMaterial += purchasePrice;

                                            }
                                        }
                                    }
                                }
                            }
                            else if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Invoice)
                            {
                                if (!dtos.Any(d => d.AssociatedId == row.InvoiceId && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal))
                                {
                                    var parentInvoice = overviewRows.FirstOrDefault(r => r.TargetRowId == row.CustomerInvoiceRowId);
                                    if (parentInvoice != null)
                                    {
                                        if (overviewRows.Any(r => r.InvoiceId == parentInvoice.InvoiceId && r.ProductId.HasValue && ((fixedPriceProductId != 0 && r.ProductId.Value == fixedPriceProductId) || (fixedPriceKeepPricesProductId != 0 && r.ProductId.Value == fixedPriceKeepPricesProductId))))
                                        {
                                            var fixedDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                            if (fixedDto != null)
                                            {
                                                fixedDto.Value += rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value) ? 0 : row.SalesAmount.Value;
                                            }
                                            else
                                            {
                                                var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value) ? 0 : (fixedPriceRowsExist ? row.SalesAmount.Value : row.SalesAmount.Value + budgetIncomeIB), 0));
                                            }
                                        }
                                    }
                                }

                                if (invoice.OriginStatus == (int)SoeOriginStatus.Draft) //Invoice not invoiced! --- Preliminary invoices counts as not invoiced ---
                                {
                                    ProjectCentralStatusDTO inDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                    if (inDto != null)
                                    {
                                        inDto.Value += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                        incomeMaterialNotInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }

                                    if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                    {
                                        if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                        {
                                            DateTime invRowCreatedDate = row.Date?.Date ?? row.Created?.Date ?? DateTime.Today;
                                            if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date)
                                                continue;

                                            if (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date)
                                                continue;

                                            if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                            {
                                                if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow && invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                                {
                                                    if (budgetHead != null && !costPersonellAdded)
                                                    {
                                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                        var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                        if (budgetTimeRow == null)
                                                            budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                            budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                        costPersonellAdded = true;
                                                        costPersonnel += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (dto != null)
                                                        {
                                                            dto.Value += purchasePrice;
                                                        }
                                                        else
                                                        {
                                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                        }

                                                        costPersonellAdded = true;
                                                        costPersonnel += purchasePrice;

                                                    }

                                                    // Billable minutes not invoice
                                                    if (row.Quantity.HasValue)
                                                    {
                                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat") && d.CostTypeName == (loadDetails ? row.MaterialCode : ""));

                                                        if (dto != null)
                                                        {
                                                            dto.Value += row.Quantity.Value * 60;
                                                        }
                                                        else
                                                        {
                                                            dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.Quantity.Value * 60, costTypeName: loadDetails ? row.MaterialCode : ""));
                                                        }

                                                        billableMinutesNotInvoiced += row.Quantity.Value * 60;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (budgetHead != null && !costMaterialAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                    costMaterialAdded = true;
                                                    costMaterial += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                    if (dto != null)
                                                    {
                                                        dto.Value += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, purchasePrice, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                    }

                                                    costMaterialAdded = true;
                                                    costMaterial += purchasePrice;
                                                }
                                            }
                                        }
                                    }
                                }//End of invoice not invoiced
                                else
                                {
                                    //Invoice invoiced!
                                    incomeInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;

                                    decimal budgetTotalAmount = 0;

                                    if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                        budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                        budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                    }

                                    if (budgetHead != null && !incomeMaterialInvoicedAdded && !budgetRowIncomeInvoicedAdded)
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0,
                                            budgetTotalAmount, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));

                                        budgetRowIncomeInvoicedAdded = true;
                                        incomeMaterialInvoicedAdded = true;
                                        incomeMaterialInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }
                                    else
                                    {
                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                        if (dto != null)
                                        {
                                            dto.Value += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                            dto.Value2 += row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0;
                                        }
                                        else
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + invoice.InvoiceNumber + ", " + invoice.CustomerName, row.SalesAmount.HasValue ? row.SalesAmount.Value : 0, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                        }

                                        incomeMaterialInvoicedAdded = true;
                                        incomeMaterialInvoiced += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                    }

                                    if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                    {
                                        if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                        {
                                            DateTime invRowCreatedDate = row.InvoiceDate.HasValue ? row.InvoiceDate.Value.Date : row.Created.Value.Date;
                                            if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date)
                                                continue;

                                            if (dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date)
                                                continue;

                                            if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                            {
                                                if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow && invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                                {
                                                    if (budgetHead != null && !costPersonellAdded)
                                                    {
                                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                        var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                                        if (budgetTimeRow == null)
                                                            budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice,
                                                            budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                        costPersonellAdded = true;
                                                        costPersonnel += purchasePrice;
                                                    }
                                                    else
                                                    {
                                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));

                                                        if (dto != null)
                                                        {
                                                            dto.Value += purchasePrice;
                                                            dto.Value2 += row.Quantity.HasValue ? row.Quantity.Value : 0;
                                                        }
                                                        else
                                                        {
                                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice, 0, value2: row.Quantity.HasValue ? row.Quantity.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));
                                                        }

                                                        costPersonellAdded = true;
                                                        costPersonnel += purchasePrice;

                                                    }

                                                    //billable minutes invoiced
                                                    if (row.Quantity.HasValue)
                                                    {
                                                        if (budgetHead != null && !billableMinutesInvoicedAdded)
                                                        {
                                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);
                                                            billableMinutesInvoicedAdded = true;

                                                            dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)row.OriginType, invoice.InvoiceId, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + row.InvoiceNumber + ", " + row.CustomerName, row.Quantity.Value, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails ? row.MaterialCode : ""));
                                                            billableMinutesInvoiced += row.Quantity.Value;
                                                        }
                                                        else
                                                        {
                                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesInvoiced && d.Description == GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat") && d.CostTypeName == (loadDetails ? row.MaterialCode : ""));

                                                            if (dto != null)
                                                            {
                                                                dto.Value += row.Quantity.Value;
                                                            }
                                                            else
                                                            {
                                                                dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)row.OriginType, invoice.InvoiceId, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + row.InvoiceNumber + ", " + row.CustomerName, row.Quantity.Value, costTypeName: loadDetails ? row.MaterialCode : ""));
                                                            }

                                                            billableMinutesInvoiced += row.Quantity.Value;
                                                            billableMinutesInvoicedAdded = true;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (budgetHead != null && !costMaterialAdded)
                                                {
                                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice,
                                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", actorName: invoice.CustomerName));

                                                    costMaterialAdded = true;
                                                    costMaterial += purchasePrice;
                                                }
                                                else
                                                {
                                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == (loadDetails && row.MaterialCode != null ? row.MaterialCode : ""));
                                                    if (purchasePrice != 0) //Dont add zero costs
                                                    {
                                                        if (dto != null)
                                                        {
                                                            dto.Value += purchasePrice;
                                                        }
                                                        else
                                                        {
                                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + ", " + invoice.InvoiceNumber + " " + invoice.CustomerName, purchasePrice, value2: row.MarginalIncome.HasValue ? row.MarginalIncome.Value : 0, costTypeName: loadDetails && row.MaterialCode != null ? row.MaterialCode : "", date: invoice.InvoiceDate, actorName: invoice.CustomerName));
                                                        }
                                                        costMaterialAdded = true;
                                                        costMaterial += purchasePrice;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                } //End of Invoice invoiced!
                            }

                            #endregion
                        }

                        #endregion

                        #region ExpenseRowTransactionView

                        if (!HasExpenseRows.HasValue)
                        {
                            HasExpenseRows = ExpenseManager.HasExpenseRows(actorCompanyId);
                        }

                        if (HasExpenseRows.GetValueOrDefault())
                        {
                            var expenseRows = ExpenseManager.GetExpenseRowsForProjectOverview(actorCompanyId, base.UserId, base.RoleId, invoiceGroup.Key);
                            foreach (var expenseRow in expenseRows.Where(e => (dateFrom == null || e.From >= dateFrom.Value) && (dateTo == null || e.From <= dateTo.Value)))
                            {
                                var typeName = GetText(130, (int)TermGroup.ProjectCentral, "Order") + ", " + invoice.InvoiceNumber;
                                if (budgetHead != null && !costExpenseAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow?.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);

                                    var dto = CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, (SoeOriginType)invoice.OriginType, invoiceGroup.Key, expenseRow.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), typeName, (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount),
                                        budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, value2: expenseRow.Quantity, budgetTime: budgetRow != null ? budgetRow.TotalAmount : 0, costTypeName: loadDetails ? expenseRow.TimeCodeName : "", employeeId: expenseRow.EmployeeId, employeeName: expenseRow.EmployeeName);

                                    if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                        dto.Value2 = expenseRow.Quantity;

                                    dtos.Add(dto);

                                    costExpense += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                                    costExpenseAdded = true;
                                }
                                else
                                {
                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == invoiceGroup.Key && d.Type == ProjectCentralBudgetRowType.CostExpense && d.EmployeeId == expenseRow.EmployeeId && d.TypeName == typeName && d.CostTypeName == (loadDetails ? expenseRow.TimeCodeName : ""));
                                    if (dto != null)
                                    {
                                        dto.Value += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                                        if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                            dto.Value2 += expenseRow.Quantity;
                                    }
                                    else
                                    {
                                        dto = CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, (SoeOriginType)invoice.OriginType, invoiceGroup.Key, expenseRow.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), typeName, (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount), costTypeName: loadDetails ? expenseRow.TimeCodeName : "", employeeId: expenseRow.EmployeeId, employeeName: expenseRow.EmployeeName);
                                        if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                            dto.Value2 = expenseRow.Quantity;
                                        dtos.Add(dto);
                                    }

                                    costExpense += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                                    costExpenseAdded = true;
                                }
                            }
                        }

                        #endregion
                    }

                    #endregion

                    #region TimeInvoiceTransactionsProjectView

                    var transactionItemsForProject = GetTransactionsForProjectCentralStatus(project.ProjectId).Where(t => t.TimeCodeTransactionType == (int)TimeCodeTransactionType.TimeProject && t.CustomerInvoiceRowId.HasValue).ToList();

                    foreach (var transactionGroup in transactionItemsForProject.GroupBy(t => t.CustomerInvoiceRowId))
                    {
                        bool isBilled = false;
                        bool targetRowMissing = false;
                        decimal transactionQuantity = 0;

                        var customerInvoiceRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == transactionGroup.Key);

                        if (customerInvoiceRow == null)
                            continue;

                        var invoiceGroup = invoicesForProject.FirstOrDefault(p => p.Key == customerInvoiceRow.InvoiceId);

                        if (invoiceGroup == null)
                            continue;

                        GetProjectOverview_Result targetRow = null;
                        if (customerInvoiceRow.AttestStateId.HasValue && customerInvoiceRow.AttestStateId == defaultStatusTransferredOrderToInvoice && customerInvoiceRow.TargetRowId.HasValue)
                        {
                            // Check from invoice
                            targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == customerInvoiceRow.TargetRowId);

                            if (targetRow != null && targetRow.OriginStatus != (int)SoeOriginStatus.Draft)
                            {
                                DateTime invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;

                                if (dateFrom.HasValue && dateFrom.Value.Date.IsAfter(invRowCreatedDate.Date))
                                    continue;

                                if (dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                {
                                    // Revert to order row
                                    targetRow = customerInvoiceRow;

                                    invRowCreatedDate = targetRow.Date.HasValue ? targetRow.Date.Value.Date : targetRow.Created.Value.Date;

                                    if (dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                        continue;

                                    isBilled = false;
                                }
                                else
                                {
                                    isBilled = true;
                                }
                            }
                            else
                            {
                                if (targetRow != null && targetRow.OriginStatus == (int)SoeOriginStatus.Draft)
                                    targetRow = customerInvoiceRow;
                                else
                                    targetRowMissing = true;

                                if (!targetRowMissing)
                                {
                                    DateTime invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;

                                    if (dateFrom.HasValue && dateFrom.Value.Date.IsAfter(invRowCreatedDate.Date))
                                        continue;

                                    if (dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                    {
                                        // Revert to order row
                                        targetRow = customerInvoiceRow;

                                        invRowCreatedDate = targetRow.Date.HasValue ? targetRow.Date.Value.Date : targetRow.Created.Value.Date;

                                        if (dateFrom.HasValue && dateFrom.Value.Date.IsAfter(invRowCreatedDate.Date) || dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                            continue;
                                    }
                                }
                                else
                                {
                                    DateTime invRowCreatedDate = customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value.Date : customerInvoiceRow.Created.Value.Date;

                                    // Check from order row
                                    if (dateFrom.HasValue && dateFrom.Value.Date.IsAfter(invRowCreatedDate.Date) || dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                        continue;
                                }
                            }
                        }
                        else
                        {

                            DateTime invRowCreatedDate = customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value.Date : customerInvoiceRow.Created.Value.Date;

                            // Check from order row
                            if (dateFrom.HasValue && dateFrom.Value.Date.IsAfter(invRowCreatedDate.Date) || dateTo.HasValue && dateTo.Value.Date.IsBefore(invRowCreatedDate.Date))
                                continue;
                        }

                        var isFixedPriceOrder = false;
                        if (fixedPriceProductId != 0)
                            isFixedPriceOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);

                        var isFixedPriceKeepPricesOrder = false;
                        if (fixedPriceKeepPricesProductId != 0)
                            isFixedPriceKeepPricesOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceKeepPricesProductId);

                        foreach (var transactionItem in transactionGroup)
                        {
                            decimal quantity = transactionItem.InvoiceQuantity;

                            if (customerInvoiceRow.BillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            transactionQuantity += quantity;

                            if (transactionItem.Exported && targetRow != null)
                                isBilled = true;

                            #region back to income from transactions (old: removed since incomeinvoiced should be set from customerinvoicerow) and then back to income from transactions again to be able to sort out income based on added timerows
                            if (useDateIntervalInIncomeNotInvoiced)
                            {
                                if (customerInvoiceRow.OriginType == (int)SoeOriginType.Order)
                                {
                                    if (isBilled)
                                    {
                                        var amount = 0m;
                                        if (targetRow.SalesAmount.HasValue && targetRow.SalesAmount.Value != 0 && targetRow.Quantity.HasValue && targetRow.Quantity != 0 && transactionItem.InvoiceQuantity != 0)
                                            amount = (targetRow.SalesAmount.Value / (targetRow.Quantity.HasValue ? (decimal)targetRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);

                                        if (targetRow.OriginStatus == (int)SoeOriginStatus.Draft)
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == targetRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                            if (dto != null)
                                            {
                                                dto.Value += amount;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + ", " + targetRow.CustomerName, amount, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                            }
                                        }
                                        else
                                        {
                                            decimal budgetTotalAmount = 0;
                                            if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                            {
                                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                                budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                                budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                            }

                                            if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                            {
                                                // Check date interval condition
                                                /*if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                                    continue;*/

                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + targetRow.InvoiceNumber + ", " + targetRow.CustomerName, amount,
                                                    budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: targetRow.MarginalIncome.HasValue ? targetRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: targetRow.InvoiceDate));

                                                budgetRowIncomeInvoicedAdded = true;
                                            }
                                            else
                                            {
                                                // Check date interval condition
                                                /*if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                                    continue;*/

                                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == targetRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                                if (dto != null)
                                                {
                                                    dto.Value += amount;
                                                }
                                                else
                                                {
                                                    // Check date interval condition
                                                    /*if ((dateFrom.HasValue && dateFrom.Value.Date > targetRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < targetRow.InvoiceDate))
                                                        continue;*/


                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)targetRow.OriginType, targetRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + targetRow.InvoiceNumber + ", " + targetRow.CustomerName, amount, value2: targetRow.MarginalIncome.HasValue ? targetRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: targetRow.InvoiceDate));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!isFixedPriceOrder && !isFixedPriceKeepPricesOrder && !targetRowMissing)
                                        {
                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                            if (customerInvoiceRow.SalesAmount.HasValue && customerInvoiceRow.SalesAmount.Value != 0)
                                            {
                                                if (dto != null)
                                                {
                                                    dto.Value += (customerInvoiceRow.SalesAmount.Value / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(147, (int)TermGroup.ProjectCentral, "Arbete"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, (customerInvoiceRow.SalesAmount.Value / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60), 0, isVisible: false, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (customerInvoiceRow.OriginType == (int)SoeOriginType.CustomerInvoice)
                                {
                                    var amount = ((customerInvoiceRow.SalesAmount.HasValue ? customerInvoiceRow.SalesAmount.Value : 0) / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);
                                    if (customerInvoiceRow.OriginStatus == (int)SoeOriginStatus.Draft)
                                    {
                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                        if (dto != null)
                                        {
                                            dto.Value += amount;
                                        }
                                        else
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                        }
                                    }
                                    else
                                    {
                                        decimal budgetTotalAmount = 0;
                                        if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                        {
                                            budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                            budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                            budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                        }

                                        if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount,
                                                budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: customerInvoiceRow.InvoiceDate));

                                            budgetRowIncomeInvoicedAdded = true;
                                        }
                                        else
                                        {
                                            // Check date interval condition
                                            /*if ((dateFrom.HasValue && dateFrom.Value.Date > customerInvoiceRow.InvoiceDate) || (dateTo.HasValue && dateTo.Value.Date < customerInvoiceRow.InvoiceDate))
                                                continue;*/

                                            ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                            if (dto != null)
                                            {
                                                dto.Value += amount;
                                            }
                                            else
                                            {
                                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: loadDetails ? transactionItem.TimeCodeName : "", date: customerInvoiceRow.InvoiceDate));
                                            }
                                        }
                                    }
                                }

                                // Add rows for total
                            }
                            #endregion

                            if (isBilled)
                            {
                                if (budgetHead != null && !billableMinutesInvoicedAdded)
                                {
                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);
                                    billableMinutesInvoicedAdded = true;

                                    dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                    billableMinutesInvoiced += quantity;
                                }
                                else
                                {
                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesInvoiced && d.Description == transactionItem.EmployeeName && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += quantity;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(103, (int)TermGroup.ProjectCentral, "Debiterbara timmar, fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                    }

                                    billableMinutesInvoiced += quantity;
                                    billableMinutesInvoicedAdded = true;
                                }
                            }
                            else
                            {
                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == transactionItem.InvoiceId && d.Type == ProjectCentralBudgetRowType.BillableMinutesNotInvoiced && d.Description == transactionItem.EmployeeName && d.CostTypeName == (loadDetails ? transactionItem.TimeCodeName : ""));

                                if (dto != null)
                                {
                                    dto.Value += quantity;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType.Time, "Tid", ProjectCentralBudgetRowType.BillableMinutesNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, transactionItem.InvoiceId, transactionItem.EmployeeName, GetText(104, (int)TermGroup.ProjectCentral, "Debiterbara timmar, ej fakturerat"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, quantity, costTypeName: loadDetails ? transactionItem.TimeCodeName : ""));
                                }

                                billableMinutesNotInvoiced += quantity;
                            }
                        }

                        // Add rests
                        var rowQuantity = customerInvoiceRow.Quantity.GetValueOrDefault();
                        var restQuantity = rowQuantity - (transactionQuantity / 60);
                        if (useDateIntervalInIncomeNotInvoiced && restQuantity != 0 && !dateFrom.HasValue && !dateTo.HasValue)
                        {
                            // Add rows for total
                            var salesAmount = customerInvoiceRow.SalesAmount.GetValueOrDefault();
                            var amount = rowQuantity != 0 ? (salesAmount / rowQuantity) * restQuantity : salesAmount * restQuantity;
                            if (amount != 0)
                            {
                                if ((customerInvoiceRow.OriginType == (int)SoeOriginType.Order && isBilled && targetRow.OriginStatus != (int)SoeOriginStatus.Draft) || (customerInvoiceRow.OriginType == (int)SoeOriginType.CustomerInvoice && targetRow.OriginStatus != (int)SoeOriginStatus.Draft))
                                {
                                    decimal budgetTotalAmount = 0;
                                    if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                        budgetRow2 = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                        budgetTotalAmount = budgetRow.TotalAmount + budgetRow2.TotalAmount;
                                    }

                                    if (budgetHead != null && !budgetRowIncomeInvoicedAdded)
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + (targetRow != null ? targetRow.InvoiceNumber : customerInvoiceRow.InvoiceNumber) + ", " + customerInvoiceRow.CustomerName, amount,
                                            budgetTotalAmount, budgetRow2 != null && budgetRow2.Modified != null ? ((DateTime)budgetRow2.Modified).ToString() : String.Empty, budgetRow2 != null && budgetRow2.ModifiedBy != null ? budgetRow2.ModifiedBy : String.Empty, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: "", date: customerInvoiceRow.InvoiceDate));

                                        budgetRowIncomeInvoicedAdded = true;
                                    }
                                    else
                                    {
                                        ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (targetRow != null ? targetRow.InvoiceId : customerInvoiceRow.InvoiceId) && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced && d.CostTypeName == "");

                                        if (dto != null)
                                        {
                                            dto.Value += amount;
                                        }
                                        else
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(131, (int)TermGroup.ProjectCentral, "Faktura") + " " + (targetRow != null ? targetRow.InvoiceNumber : customerInvoiceRow.InvoiceNumber) + ", " + customerInvoiceRow.CustomerName, amount, value2: customerInvoiceRow.MarginalIncome.HasValue ? customerInvoiceRow.MarginalIncome.Value : 0, costTypeName: "", date: customerInvoiceRow.InvoiceDate));
                                        }
                                    }
                                }
                                else
                                {
                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == customerInvoiceRow.InvoiceId && d.Type == ProjectCentralBudgetRowType.IncomeNotInvoiced && d.CostTypeName == "");
                                    if (dto != null)
                                    {
                                        dto.Value += amount;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.IncomeNotInvoiced, (SoeOriginType)customerInvoiceRow.OriginType, customerInvoiceRow.InvoiceId, "", "", GetText(152, (int)TermGroup.ProjectCentral, "Preliminär faktura") + " " + customerInvoiceRow.InvoiceNumber + ", " + customerInvoiceRow.CustomerName, amount, costTypeName: ""));
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }

                #region TimeCodeTransactions
                var timeCodeTransactions = entitiesReadOnly.GetTimeCodeTransactionsForProjectOverview(project.ProjectId, useProjectTimeBlock, excludeInternalOrders).ToList();

                foreach (var timeCodeTrans in timeCodeTransactions)
                {
                    if (timeCodeTrans.SupplierInvoiceId.HasValue && timeCodeTrans.AmountCurrency.HasValue && timeCodeTrans.InvoiceQuantity.HasValue)
                    {
                        bool doNotCharge = (timeCodeTrans.DoNotChargeProject != null && (bool)timeCodeTrans.DoNotChargeProject);
                        DateTime? date = timeCodeTrans.SupplierInvoiceDate;

                        if (timeCodeTrans.SupplierInvoiceCreated.HasValue)
                            if (timeCodeTrans.SupplierInvoiceCreated.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < (DateTime)dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > (DateTime)dateTo)
                                    continue;
                            }

                        if (!dtos.Any(d => d.AssociatedId == timeCodeTrans.SupplierInvoiceId.Value && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal) && group != null)
                        {
                            // Check for order and fixed price
                            var fixedAdded = false;
                            if (timeCodeTrans.CustomerInvoiceId.HasValue && timeCodeTrans.CustomerInvoiceId.Value > 0)
                            {
                                var mappedOrder = group.Any(i => i.InvoiceId == timeCodeTrans.CustomerInvoiceId.Value && i.RegistrationType == (int)OrderInvoiceRegistrationType.Order && (i.FixedPriceOrder || (i.ProductId == fixedPriceProductId && fixedPriceProductId != 0) || (i.ProductId == fixedPriceKeepPricesProductId && fixedPriceKeepPricesProductId != 0)));
                                if (mappedOrder)
                                {
                                    var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId.Value, "", "", "", fixedPriceRowsExist ? 0 : budgetIncomeIB, 0));
                                    fixedAdded = true;
                                }
                            }

                            if (!fixedAdded && timeCodeTrans.OrderNr.HasValue && timeCodeTrans.OrderNr.Value > 0)
                            {
                                var mappedOrder = group.Any(i => i.InvoiceNumber == timeCodeTrans.OrderNr.Value.ToString() && i.RegistrationType == (int)OrderInvoiceRegistrationType.Order && (i.FixedPriceOrder || (i.ProductId == fixedPriceProductId && fixedPriceProductId != 0) || (i.ProductId == fixedPriceKeepPricesProductId && fixedPriceKeepPricesProductId != 0)));
                                if (mappedOrder)
                                {
                                    var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId.Value, "", "", "", fixedPriceRowsExist ? 0 : budgetIncomeIB, 0));
                                }
                            }
                        }

                        if (timeCodeTrans.TimeCodeMaterialId.HasValue)
                        {
                            decimal costMat = 0;
                            decimal costMatVal2 = 0;

                            if (budgetHead != null && !costMaterialAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);

                                costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costMat, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, null, false, costMatVal2, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.Name, date: date));

                                costMaterial += costMat;
                                costMaterialAdded = true;
                            }
                            else
                            {

                                costMat += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);
                                costMatVal2 += (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.Description == timeCodeTrans.TimeCodeName);

                                if (dto != null)
                                {
                                    dto.Value += costMat;
                                    dto.Value2 += costMatVal2;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(110, (int)TermGroup.ProjectCentral, "Materialkostnad"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costMat, 0, "", "", null, false, costMatVal2, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.Name, date: date));
                                }

                                costMaterial += costMat;
                                costMaterialAdded = true;
                            }
                        }
                        else
                        {
                            decimal costExp = 0;

                            if (budgetHead != null && !costExpenseAdded)
                            {
                                budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);

                                costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costExp, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.TimeCodeName, date: date));

                                costExpense += costExp;
                                costExpenseAdded = true;
                            }
                            else
                            {
                                costExp += doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                                ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == (timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0) && d.Type == ProjectCentralBudgetRowType.CostExpense && d.Description == timeCodeTrans.TimeCodeName);

                                if (dto != null)
                                {
                                    dto.Value += costExp;
                                }
                                else
                                {
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.SupplierInvoice, timeCodeTrans.SupplierInvoiceId != null ? (int)timeCodeTrans.SupplierInvoiceId : 0, timeCodeTrans.TimeCodeName, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), timeCodeTrans.SupplierInvoiceId != null ? GetText(132, (int)TermGroup.ProjectCentral, "Leverantörsfaktura") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.Name : String.Empty, costExp, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", actorName: timeCodeTrans.TimeCodeName, date: date));
                                }

                                costExpense += costExp;
                                costExpenseAdded = true;
                            }
                        }
                    }
                    else
                    {
                        if (!useProjectTimeBlock)
                        {
                            #region TimePayrollTransaction - when useProjectTimeBlock is false

                            if (timeCodeTrans.TimeCodeWorkId.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < (DateTime)dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > (DateTime)dateTo)
                                    continue;

                                decimal invoiceProductCost = 0;
                                if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                    invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                                else
                                    invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                                // Personnel
                                /*string employeeText = string.Empty;
                                if (timeCodeTrans.Name != "")
                                    employeeText = ", " + timeCodeTrans.Name;*/

                                string info = string.Empty;
                                decimal quantity = (timeCodeTrans.TransactionQuantity.HasValue && timeCodeTrans.TransactionQuantity.Value > 0 ? timeCodeTrans.TransactionQuantity.Value / 60 : 0);

                                if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                    quantity = decimal.Negate(quantity);

                                decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? GetCalculatedCost(timeCodeTrans.EmployeeId, actorCompanyId, timeCodeTrans.SupplierInvoiceDate.Value.Date, project.ProjectId) : invoiceProductCost;
                                decimal cost = quantity * price;
                                costPersonnel += cost;

                                if (Math.Abs(quantity) > 0 && price == 0)
                                {
                                    // Warn if no cost is specified on employee
                                    info = string.Format(GetText(116, (int)TermGroup.ProjectCentral, "Det finns anställda som saknar {0} vilket kan göra denna siffra missvisande!"), GetText(65, (int)TermGroup.EmployeeUserEdit).ToLower());
                                }

                                if (budgetHead != null && !costPersonellAdded)
                                {
                                    #region cost personell

                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);
                                    if (budgetTimeRow == null)
                                        budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced);

                                    //workinghours is set in value2
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName, cost, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, value2: quantity, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", employeeId: timeCodeTrans.EmployeeId, employeeName: timeCodeTrans.Name));

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                else
                                {
                                    #region cost personell

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.EmployeeId == timeCodeTrans.EmployeeId && d.Description == timeCodeTrans.Name && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += cost;
                                        dto.Value2 += quantity; //workinghours is set in value2
                                    }
                                    else
                                    {
                                        //workinghours is set in value2
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName, cost, info: info, value2: quantity, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", employeeId: timeCodeTrans.EmployeeId, employeeName: timeCodeTrans.Name));
                                    }

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                if (isNewBudget && budgetOverheadPerHour > 0)
                                {
                                    ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    if (ohdto != null)
                                    {
                                        ohdto.Value += budgetOverheadPerHour * quantity;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetOverheadPerHour * quantity, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    }

                                    costOverhead += budgetOverheadPerHour * quantity;
                                    costOverheadAdded = true;
                                }
                                else if (overheadCostAsAmountPerHour)
                                {
                                    if (budgetHead != null && !costOverheadAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                                        BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                        if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                        {

                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                            costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                            costOverheadAdded = true;
                                        }

                                    }
                                    else
                                    {
                                        if (budgetHead != null)
                                        {
                                            ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                            BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                            if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                            {
                                                if (ohdto != null)
                                                {
                                                    ohdto.Value += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                                }

                                                costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                costOverheadAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region ProjectTimeBlock - when useProjectTimeBlock is true

                            if (timeCodeTrans.TimeCodeWorkId.HasValue)
                            {
                                if (dateFrom != null && timeCodeTrans.SupplierInvoiceDate.Value.Date < dateFrom)
                                    continue;

                                if (dateTo != null && timeCodeTrans.SupplierInvoiceDate.Value.Date > dateTo)
                                    continue;

                                decimal invoiceProductCost = 0;
                                if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                    invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                                else
                                    invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                                // Personnel
                                /*string employeeText = string.Empty;
                                if (timeCodeTrans.Name != "")
                                    employeeText = ", " + timeCodeTrans.Name;*/

                                string info = string.Empty;
                                decimal quantity = Convert.ToDecimal(CalendarUtility.TimeSpanToMinutes(timeCodeTrans.Stop.Value, timeCodeTrans.Start.Value)) / 60;

                                if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                    quantity = decimal.Negate(quantity);

                                decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? GetCalculatedCost(timeCodeTrans.EmployeeId, actorCompanyId, timeCodeTrans.SupplierInvoiceDate.Value.Date, project.ProjectId) : invoiceProductCost;
                                decimal cost = quantity * price;
                                costPersonnel += cost;

                                if (Math.Abs(quantity) > 0 && price == 0)
                                {
                                    // Warn if no cost is specified on employee
                                    info = string.Format(GetText(116, (int)TermGroup.ProjectCentral, "Det finns anställda som saknar {0} vilket kan göra denna siffra missvisande!"), GetText(65, (int)TermGroup.EmployeeUserEdit).ToLower());
                                }

                                if (budgetHead != null && !costPersonellAdded)
                                {
                                    #region cost personell

                                    budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    var budgetTimeRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesTotal);

                                    //workinghours is set in value2
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName, cost, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, value2: quantity, budgetTime: budgetTimeRow != null ? budgetTimeRow.TotalAmount : 0, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", employeeId: timeCodeTrans.EmployeeId, employeeName: timeCodeTrans.Name));

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }
                                else
                                {
                                    #region cost personell

                                    ProjectCentralStatusDTO dto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.EmployeeId == timeCodeTrans.EmployeeId && d.Description == timeCodeTrans.Name && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                    if (dto != null)
                                    {
                                        dto.Value += cost;
                                        dto.Value2 += quantity; //workinghours is set in value2
                                    }
                                    else
                                    {
                                        //workinghours is set in value2
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, timeCodeTrans.Name, GetText(109, (int)TermGroup.ProjectCentral, "Personalkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number + ", " + timeCodeTrans.CustomerName, cost, info: info, value2: quantity, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : "", employeeId: timeCodeTrans.EmployeeId, employeeName: timeCodeTrans.Name));
                                    }

                                    costPersonnel += cost;
                                    costPersonellAdded = true;

                                    #endregion
                                }

                                if (isNewBudget && budgetOverheadPerHour > 0)
                                {
                                    ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    if (ohdto != null)
                                    {
                                        ohdto.Value += budgetOverheadPerHour * quantity;
                                    }
                                    else
                                    {
                                        dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetOverheadPerHour * quantity, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                    }

                                    costOverhead += budgetOverheadPerHour * quantity;
                                    costOverheadAdded = true;
                                }
                                else if (overheadCostAsAmountPerHour)
                                {
                                    if (budgetHead != null && !costOverheadAdded)
                                    {
                                        budgetRow = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
                                        BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                        if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                        {
                                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, budgetRow != null ? budgetRow.TotalAmount : 0, budgetRow != null && budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : String.Empty, budgetRow != null && budgetRow.ModifiedBy != null ? budgetRow.ModifiedBy : String.Empty, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));

                                            costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                            costOverheadAdded = true;
                                        }

                                    }
                                    else
                                    {
                                        if (budgetHead != null)
                                        {
                                            ProjectCentralStatusDTO ohdto = dtos.FirstOrDefault(d => d.AssociatedId == timeCodeTrans.CustomerInvoice && d.Type == ProjectCentralBudgetRowType.OverheadCost && d.CostTypeName == (loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                            BudgetRow budgetRowPerHour = budgetHead.BudgetRow.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

                                            if (budgetRowPerHour != null && budgetRowPerHour.TotalAmount * quantity != 0)
                                            {
                                                if (ohdto != null)
                                                {
                                                    ohdto.Value += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                }
                                                else
                                                {
                                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, (SoeOriginType)timeCodeTrans.OriginType, timeCodeTrans.CustomerInvoice, "", GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(130, (int)TermGroup.ProjectCentral, "Order") + " " + timeCodeTrans.Number, budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0, info: info, costTypeName: loadDetails ? timeCodeTrans.TimeCodeName : ""));
                                                }

                                                costOverhead += (budgetRowPerHour != null ? (budgetRowPerHour.TotalAmount * quantity) : 0);
                                                costOverheadAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                #endregion
            }

            #region If specific rows not exists, add budgetrow or zerorow anyway 
            //Income not invoiced
            ProjectCentralStatusDTO inDtoNotInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeNotInvoiced);
            if (inDtoNotInvoiced == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeNotInvoiced, GetText(156, (int)TermGroup.ProjectCentral, "Intäkter ofakturerat"), ProjectCentralBudgetRowType.Separator, SoeOriginType.None, 0, String.Empty, String.Empty, String.Empty, 0, 0, "", "", "", false, 0, isEditable: false));
            }

            //Income invoiced
            ProjectCentralStatusDTO inDtoInvoiced = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.IncomeInvoiced);
            if (inDtoInvoiced == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetIncomeIB, budgetIncome, isEditable: false));
            }

            //Cost material            
            ProjectCentralStatusDTO inDtoCostMaterial = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsMaterial);
            if (inDtoCostMaterial == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetCostMaterialIB, budgetCostMaterial, isEditable: false));
            }

            //Cost personell
            ProjectCentralStatusDTO inDtoCostPersonell = dtos.FirstOrDefault(d => d.GroupRowType == ProjectCentralHeaderGroupType.CostsPersonell);
            if (inDtoCostPersonell == null)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, String.Empty, string.Empty, string.Empty, budgetCostPersonellIB, budgetCostPersonell, value2: budgetBillableMinutesIB, budgetTime: budgetBillableMinutes, isEditable: false));
            }

            //Cost overhead
            if ((overheadCostAsFixedAmount || overheadCostAsAmountPerHour) && !costOverheadAdded)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, String.Empty, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", budgetOverheadIB, budgetOverhead, isEditable: false));
            }

            //Cost expense
            if (!costExpenseAdded)
            {
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, String.Empty, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), GetText(115, (int)TermGroup.ProjectCentral, "Utlägg") + "(" + GetText(150, (int)TermGroup.ProjectCentral, "totalbelopp") + ")", budgetExpenseIB, budgetExpense, isEditable: false));
            }


            #endregion

            #region Calculate diffs
            if (loadDetails)
            {
                foreach (ProjectCentralStatusDTO dto in dtos.Where(r => r.Budget != 0 || r.BudgetTime != 0))
                {
                    dto.Diff = dtos.Where(r => r.Type == dto.Type && r.CostTypeName == dto.CostTypeName).Sum(r => r.Value) - dto.Budget;
                    //dto.Diff2 = 
                    dto.Diff2 = dtos.Where(r => r.Type == dto.Type && r.CostTypeName == dto.CostTypeName).Sum(r => r.Value2) - (dto.BudgetTime / 60);
                }
            }
            else
            {
                foreach (ProjectCentralStatusDTO dto in dtos.Where(r => r.Budget != 0 || r.BudgetTime != 0))
                {
                    dto.Diff = dtos.Where(r => r.Type == dto.Type).Sum(r => r.Value) - dto.Budget;
                    dto.Diff2 = dtos.Where(r => r.Type == dto.Type).Sum(r => r.Value2) - (dto.BudgetTime / 60);
                }
            }


            #endregion

            return dtos.OrderBy(d => d.GroupRowType).ToList();
        }

        #region Budget

        private List<ProjectCentralStatusDTO> GetProjectCentralBudget(int projectId, List<ProjectCentralStatusDTO> dtos, bool hasFromDate, bool loadDetails, ref decimal budgetIncomeIB, ref decimal budgetIncome, ref bool budgetRowIncomeInvoicedAdded, ref decimal budgetCostMaterialIB, ref decimal budgetCostMaterial, ref bool costMaterialAdded,
            ref decimal budgetCostPersonellIB, ref decimal budgetBillableMinutesIB, ref decimal budgetCostPersonell, ref decimal budgetBillableMinutes, ref bool costPersonellAdded, ref decimal budgetOverheadIB, ref decimal budgetOverhead, ref decimal budgetOverHeadPerHour, ref bool costOverheadAdded,
            ref decimal budgetExpenseIB, ref decimal budgetExpense, ref bool costExpenseAdded)
        {
            var budgetHead = ProjectBudgetManager.GetLatestProjectBudgetHeadIncludingRows(projectId, DistributionCodeBudgetType.ProjectBudgetForecast, true);
            if (budgetHead == null)
                budgetHead = ProjectBudgetManager.GetLatestProjectBudgetHeadIncludingRows(projectId, DistributionCodeBudgetType.ProjectBudgetExtended, true);

            var budgetHeadIB = hasFromDate ? null : ProjectBudgetManager.GetLatestProjectBudgetHeadIncludingRows(projectId, DistributionCodeBudgetType.ProjectBudgetIB, true);


            var ibRowPersonellAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal).Sum(r => r.TotalAmount) : 0;
            var ibRowMaterialAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal).Sum(r => r.TotalAmount) : 0;
            budgetIncomeIB += ibRowPersonellAmount;
            budgetIncomeIB += ibRowMaterialAmount;

            var budgetRowMaterial = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            // Unused for now, but could be used to show last modified on budget line
            //var budgetRowPersonell = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            var budgetRowMaterialAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal).Sum(r => r.TotalAmount) : 0;
            var budgetRowPersonellAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal).Sum(r => r.TotalAmount) : 0;
            var budgetTotal = budgetRowMaterialAmount + budgetRowPersonellAmount;

            ProjectCentralStatusDTO incomeInvoicedDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.IncomeInvoiced);
            if (incomeInvoicedDto == null)
            {
                budgetIncome = budgetTotal;

                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.IncomeInvoiced, GetText(155, (int)TermGroup.ProjectCentral, "Intäkter fakturerat"), ProjectCentralBudgetRowType.IncomeInvoiced, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetIncomeIB,
                                            budgetTotal, budgetRowMaterial != null && budgetRowMaterial.Modified != null ? ((DateTime)budgetRowMaterial.Modified).ToString() : String.Empty, budgetRowMaterial != null && budgetRowMaterial.ModifiedBy != null ? budgetRowMaterial.ModifiedBy : String.Empty, isEditable: false));
                budgetRowIncomeInvoicedAdded = true;
            }
            else
            {
                budgetIncome += budgetTotal;
                incomeInvoicedDto.Value += (ibRowPersonellAmount + ibRowMaterialAmount);
                incomeInvoicedDto.Budget += budgetTotal;
            }

            var budgetRowCostMaterialIBAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial).Sum(r => r.TotalAmount) : 0;
            var budgetRowCostMaterial = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TimeCodeId == 0 && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            var budgetRowCostMaterialAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && (!loadDetails || r.TimeCodeId == 0)).Sum(r => r.TotalAmount) : 0;

            ProjectCentralStatusDTO costMaterialDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostMaterial);

            if (costMaterialDto == null)
            {
                budgetCostMaterialIB = budgetRowCostMaterialIBAmount;
                budgetCostMaterial = budgetRowCostMaterialAmount;

                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRowCostMaterialIBAmount,
                budgetCostMaterial, budgetRowCostMaterial != null && budgetRowCostMaterial.Modified != null ? ((DateTime)budgetRowCostMaterial.Modified).ToString() : String.Empty, budgetRowCostMaterial != null && budgetRowCostMaterial.ModifiedBy != null ? budgetRowCostMaterial.ModifiedBy : String.Empty, isEditable: false));
                costMaterialAdded = true;
            }
            else
            {
                budgetCostMaterialIB += budgetRowCostMaterialIBAmount;
                budgetCostMaterial += budgetRowCostMaterialAmount;
                costMaterialDto.Value += budgetRowCostMaterialIBAmount;
                costMaterialDto.Budget += budgetRowCostMaterialAmount;
            }

            var budgetRowCostPersonellIBAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalAmount) : 0;
            var budgetRowBillableMinutesIBQuantity = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalQuantity) : 0;
            var budgetRowCostPersonell = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TimeCodeId == 0 && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            var budgetRowCostPersonellAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && (!loadDetails || r.TimeCodeId == 0)).Sum(r => r.TotalAmount) : 0;
            var budgetRowBillableMinutesQuantity = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && (!loadDetails || r.TimeCodeId == 0)).Sum(r => r.TotalQuantity) : 0;

            ProjectCentralStatusDTO costPersDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostPersonell);
            if (costPersDto == null)
            {
                budgetCostPersonellIB = budgetRowCostPersonellIBAmount;
                budgetBillableMinutesIB = budgetRowBillableMinutesIBQuantity != 0 ? budgetRowBillableMinutesIBQuantity / 60 : 0;

                budgetCostPersonell = budgetRowCostPersonellAmount;
                budgetBillableMinutes = budgetRowBillableMinutesQuantity != 0 ? budgetRowBillableMinutesQuantity : 0;

                //workinghours is set in value2
                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetCostPersonellIB,
                budgetCostPersonell, budgetRowCostPersonell != null && budgetRowCostPersonell.Modified != null ? ((DateTime)budgetRowCostPersonell.Modified).ToString() : String.Empty, budgetRowCostPersonell != null && budgetRowCostPersonell.ModifiedBy != null ? budgetRowCostPersonell.ModifiedBy : String.Empty, budgetTime: budgetBillableMinutes, value2: budgetBillableMinutesIB, isEditable: false));
                costPersonellAdded = true;
            }
            else
            {
                budgetCostPersonellIB += budgetRowCostPersonellIBAmount;
                budgetBillableMinutesIB += budgetRowBillableMinutesIBQuantity != 0 ? budgetRowBillableMinutesIBQuantity / 60 : 0;

                budgetCostPersonell += budgetRowCostPersonellAmount;
                budgetBillableMinutes += budgetRowBillableMinutesQuantity != 0 ? budgetRowBillableMinutesQuantity : 0;

                costPersDto.Value += budgetRowCostPersonellIBAmount;
                costPersDto.Budget += budgetRowCostPersonellAmount;
                costPersDto.BudgetTime += budgetRowBillableMinutesQuantity != 0 ? budgetRowBillableMinutesQuantity : 0;
                costPersDto.Value2 += budgetRowBillableMinutesIBQuantity != 0 ? budgetRowBillableMinutesIBQuantity / 60 : 0;
            }

            var budgetRowCostExpenseIBAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense).Sum(r => r.TotalAmount) : 0;
            var budgetRowCostExpense = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            var budgetRowCostExpenseAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense).Sum(r => r.TotalAmount) : 0;

            ProjectCentralStatusDTO cExpenseDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostExpense);

            if (cExpenseDto == null)
            {
                budgetExpenseIB = budgetRowCostExpenseIBAmount;
                budgetExpense = budgetRowCostExpenseAmount;

                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsExpense, GetText(115, (int)TermGroup.ProjectCentral, "Utlägg"), ProjectCentralBudgetRowType.CostExpense, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetRowCostExpenseIBAmount, budgetRowCostExpenseAmount, budgetRowCostExpense != null && budgetRowCostExpense.Modified != null ? ((DateTime)budgetRowCostExpense.Modified).ToString() : String.Empty, budgetRowCostExpense != null && budgetRowCostExpense.ModifiedBy != null ? budgetRowCostExpense.ModifiedBy : String.Empty, isEditable: false));
                costExpenseAdded = true;
            }
            else
            {
                budgetExpenseIB += budgetRowCostExpenseIBAmount;
                budgetExpense += budgetRowCostExpenseAmount;

                cExpenseDto.Value += budgetRowCostExpenseIBAmount;
                cExpenseDto.Budget += budgetRowCostExpenseAmount;
            }

            if (loadDetails && budgetHead != null && budgetHead.Rows != null)
            {
                var timeCodeBudgets = budgetHead.Rows.Where(b => b.TimeCodeId > 0 && (b.Type == (int)ProjectCentralBudgetRowType.CostMaterial || b.Type == (int)ProjectCentralBudgetRowType.CostPersonell));
                foreach (var row in timeCodeBudgets)
                {
                    if (row.Type == (int)ProjectCentralBudgetRowType.CostMaterial && !String.IsNullOrEmpty(row.TypeCodeName))
                    {
                        budgetCostMaterial += row.TotalAmount;

                        ProjectCentralStatusDTO costMatDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostMaterial && d.CostTypeName == row.TypeCodeName);
                        if (costMatDto == null)
                        {
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsMaterial, GetText(160, (int)TermGroup.ProjectCentral, "Kostnader material"), ProjectCentralBudgetRowType.CostMaterial, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                                row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, costTypeName: row.TypeCodeName, isEditable: false));
                        }
                        else
                        {
                            costMatDto.Budget += row.TotalAmount;
                        }
                    }
                    else if (row.Type == (int)ProjectCentralBudgetRowType.CostPersonell && !String.IsNullOrEmpty(row.TypeCodeName))
                    {
                        budgetCostPersonell += row.TotalAmount;
                        budgetBillableMinutes += row.TotalQuantity != 0 ? row.TotalQuantity : 0;

                        ProjectCentralStatusDTO costPersonellDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.CostPersonell && d.CostTypeName == row.TypeCodeName);
                        if (costPersonellDto == null)
                        {
                            dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsPersonell, GetText(161, (int)TermGroup.ProjectCentral, "Kostnader personal"), ProjectCentralBudgetRowType.CostPersonell, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), 0,
                            row != null ? row.TotalAmount : 0, row != null && row.Modified != null ? ((DateTime)row.Modified).ToString() : String.Empty, row != null && row.ModifiedBy != null ? row.ModifiedBy : String.Empty, budgetTime: row.TotalQuantity != 0 ? row.TotalQuantity : 0, costTypeName: row.TypeCodeName, isEditable: false));
                        }
                        else
                        {
                            costPersonellDto.Budget += row.TotalAmount;
                            costPersonellDto.BudgetTime += row.TotalQuantity != 0 ? row.TotalQuantity : 0;
                        }
                    }
                }
            }

            var budgetOverheadIBAmount = budgetHeadIB != null ? budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost).Sum(r => r.TotalAmount) : 0;
            var budgetRowOverhead = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost && r.Created.HasValue).OrderByDescending(r => r.Created.Value).FirstOrDefault() : null;
            var budgetOverheadAmount = budgetHead != null ? budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost).Sum(r => r.TotalAmount) : 0;

            var budgetOverheadIBPerHourRow = budgetHeadIB != null ? budgetHeadIB.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;
            var budgetOverheadPerHourRow = budgetHead != null ? budgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour) : null;

            // Set per row cost to be calculated
            budgetOverHeadPerHour = budgetOverheadPerHourRow != null ? budgetOverheadPerHourRow.TotalAmount : 0;

            budgetOverheadIBAmount += budgetBillableMinutesIB != 0 ? budgetBillableMinutesIB * (budgetOverheadIBPerHourRow?.TotalAmount ?? 0) : 0;
            budgetOverheadAmount += budgetBillableMinutes != 0 ? (budgetBillableMinutes / 60) * (budgetOverheadPerHourRow?.TotalAmount ?? 0) : 0;

            ProjectCentralStatusDTO overheadCDto = dtos.FirstOrDefault(d => d.OriginType == SoeOriginType.None && d.AssociatedId == 0 && d.Type == ProjectCentralBudgetRowType.OverheadCost);
            if (overheadCDto == null)
            {
                budgetOverheadIB = budgetOverheadIBAmount;
                budgetOverhead = budgetOverheadAmount;

                dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.CostsOverhead, GetText(148, (int)TermGroup.ProjectCentral, "Overheadkostnad"), ProjectCentralBudgetRowType.OverheadCost, SoeOriginType.None, 0, "", "", GetText(167, (int)TermGroup.ProjectCentral, "Budget/Ingående balans"), budgetOverheadIB, budgetOverheadAmount, budgetRowOverhead != null && budgetRowOverhead.Modified != null ? ((DateTime)budgetRowOverhead.Modified).ToString() : String.Empty, budgetRowOverhead != null && budgetRowOverhead.ModifiedBy != null ? budgetRowOverhead.ModifiedBy : String.Empty, value2: budgetOverheadIB, isEditable: false));
                costOverheadAdded = true;
            }
            else
            {
                budgetOverheadIB += budgetOverheadIBAmount;
                budgetOverhead += budgetOverheadAmount;

                overheadCDto.Value += budgetOverheadIBAmount;
                overheadCDto.Budget += budgetOverheadAmount;
            }

            return dtos;
        }

        #endregion

        #region Get data

        Dictionary<int, List<EmployeeCalculatedCostDTO>> EmployeeCalculatedCosts;
        public decimal GetCalculatedCost(int employeeId, int actorCompanyId, DateTime date, int projectId)
        {
            if (EmployeeCalculatedCosts == null)
                EmployeeCalculatedCosts = new Dictionary<int, List<EmployeeCalculatedCostDTO>>();

            List<EmployeeCalculatedCostDTO> calculatedCosts = null;
            if (EmployeeCalculatedCosts.Any(d => d.Key == employeeId))
            {
                calculatedCosts = EmployeeCalculatedCosts[employeeId];
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                calculatedCosts = EmployeeManager.GetEmployeeCalculatedCostDTOs(entitiesReadOnly, employeeId, actorCompanyId, true);
                if (calculatedCosts.Count > 0)
                    EmployeeCalculatedCosts.Add(employeeId, calculatedCosts);
            }

            var currentCost = calculatedCosts.Where(x => x.fromDate <= date && x.ProjectId == null).OrderByDescending(x => x.fromDate).FirstOrDefault();
            var currentProjectCost = calculatedCosts.Where(x => x.ProjectId == projectId && x.fromDate <= date).OrderByDescending(x => x.fromDate).FirstOrDefault();
            if (currentProjectCost != null)
                currentCost = currentProjectCost;

            return currentCost?.CalculatedCostPerHour ?? 0;
        }

        public List<Project> GetProjectsForProjectCentralStatus(List<int> projectIds, int actorCompanyId, bool getAll = false)
        {
            // Get projects with all neccessary relations
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetProjectsForProjectCentralStatus(entities, projectIds, actorCompanyId, getAll);
        }

        public List<Project> GetProjectsForProjectCentralStatus(CompEntities entities, List<int> projectIds, int actorCompanyId, bool getAll = false)
        {
            // Get projects with all neccessary relations
            IQueryable<Project> projQuery = (from p in entities.Project
                             .Include("ChildProjects")
                                                 /*.Include("Invoice.Origin")
                                                 .Include("TimeCodeTransaction.TimeCode")
                                                 .Include("TimeCodeTransaction.TimePayrollTransaction")*/
                                             where p.ActorCompanyId == actorCompanyId
                                             select p);

            if (projectIds != null && projectIds.Any())
                return projectIds.Count < 1000 ? projQuery.Where(x => projectIds.Contains(x.ProjectId)).ToList() :
                                            projQuery.ToList().Where(x => projectIds.Contains(x.ProjectId)).ToList();
            else if (getAll)
                return projQuery.ToList();
            else
                return new List<Project>();
        }

        public bool ValidateExcludeInternalOrders(int projectId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return entitiesReadOnly.Invoice.OfType<CustomerInvoice>().Any(i => i.ProjectId == projectId && (i.Origin.Type != (int)SoeOriginType.Order || i.OrderType != (int)TermGroup_OrderType.Internal));
        }

        private List<TimeInvoiceTransactionsProjectView> GetTransactionsForProjectCentralStatus(int projectId)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Project.NoTracking();
            List<TimeInvoiceTransactionsProjectView> timeInvoiceTransactions = new List<TimeInvoiceTransactionsProjectView>();
            try
            {
                timeInvoiceTransactions = (from p in entitiesReadOnly.TimeInvoiceTransactionsProjectView
                                           where p.ProjectId == projectId
                                           select p).ToList();
            }
            catch (Exception ex)
            {
                // This is for bypassing when shit happens in ling and system hangs  
                base.LogError(ex, this.log);
            }

            return timeInvoiceTransactions;
        }

        #endregion

        #region Rows

        private ProjectCentralStatusDTO CreateProjectCentralStatusTimeValueRow(ProjectCentralHeaderGroupType groupType, string groupTypeName, ProjectCentralBudgetRowType rowTypeIdendifier, SoeOriginType originType, int associatedId, string description, string name, string typeName, decimal value, decimal budget = 0, string mod = "", string modBy = "", bool isVisible = false, bool? isEditable = null, string costTypeName = "")
        {
            return CreateProjectCentralStatusRow(groupType, groupTypeName, rowTypeIdendifier, ProjectCentralStatusRowType.TimeValueRow, originType, associatedId, description, name, typeName, value, budget, mod, modBy, isVisible: isVisible, isEditable: isEditable.HasValue ? isEditable : true, costTypeName: costTypeName);
        }

        private ProjectCentralStatusDTO CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType groupType, string groupTypeName, ProjectCentralBudgetRowType rowTypeIdendifier, SoeOriginType originType, int associatedId, string description, string name, string typeName, decimal value, decimal budget = 0, string mod = "", string modBy = "", string info = null, bool isVisible = false, decimal value2 = 0, decimal budgetTime = 0, bool? isEditable = null, string costTypeName = "", DateTime? date = null, string actorName = "", int employeeId = 0, string employeeName = "")
        {
            return CreateProjectCentralStatusRow(groupType, groupTypeName, rowTypeIdendifier, ProjectCentralStatusRowType.AmountValueRow, originType, associatedId, description, name, typeName, value, budget, info, isVisible: isVisible, value2: value2, budgetTime: budgetTime, isEditable: isEditable.HasValue ? isEditable : true, costTypeName: costTypeName, date: date, actorName: actorName, employeeId: employeeId, employeeName: employeeName);
        }

        private ProjectCentralStatusDTO CreateProjectCentralStatusRow(ProjectCentralHeaderGroupType groupType, string groupTypeName, ProjectCentralBudgetRowType rowTypeIdendifier, ProjectCentralStatusRowType rowType, SoeOriginType originType, int associatedId, string description, string name, string typeName, decimal value, decimal budget = 0, string mod = "", string modBy = "", string info = null, bool isVisible = false, decimal value2 = 0, decimal budgetTime = 0, bool? isEditable = null, string costTypeName = "", DateTime? date = null, string actorName = "", int employeeId = 0, string employeeName = "")
        {
            ProjectCentralStatusDTO dto = new ProjectCentralStatusDTO()
            {
                GroupRowType = groupType,
                GroupRowTypeName = groupTypeName,
                AssociatedId = associatedId,
                Description = description,
                RowType = rowType,
                OriginType = originType,
                Name = name,
                TypeName = typeName,
                Value = value,
                Value2 = value2,
                Budget = budget,
                BudgetTime = budgetTime,
                Type = rowTypeIdendifier,
                IsVisible = isVisible,
                IsEditable = isEditable != null ? isEditable.Value : false,
                Modified = mod,
                ModifiedBy = modBy,
                CostTypeName = costTypeName,
                Date = date,
                ActorName = actorName,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
            };

            if (!string.IsNullOrEmpty(info))
            {
                dto.HasInfo = true;
                dto.Info = info;
            }

            return dto;
        }
        public List<ProjectProductRowDTO> GetProjectProductRows(int projectId, int originType, bool includeChildProjects, DateTime? fromDate, DateTime? toDate)
        {
            var result = new List<ProjectProductRowDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var productRows = entitiesReadOnly.GetProjectProductRows(projectId, originType, includeChildProjects, this.ActorCompanyId);
            DateTime dateFrom = new DateTime();
            DateTime dateTo = new DateTime();
            if (fromDate != null)
            {
                var date = (DateTime)fromDate;
                dateFrom = new DateTime(date.Year, date.Month, date.Day);
            }
            if (toDate != null)
            {
                var date = (DateTime)toDate;
                dateTo = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
            }

            foreach (var productRow in productRows)
            {
                var suppName = productRow.Wholeseller.HasValue() ? productRow.Wholeseller : productRow.SupplierName.HasValue() ? productRow.SupplierName : "";

                var productRowDTO = new ProjectProductRowDTO
                {
                    ProjectName = productRow.ProjectName,
                    ProjectNumber = productRow.ProjectNumber,
                    ProjectId = productRow.ProjectId,
                    InvoiceNumber = productRow.InvoiceNumber,
                    InvoiceId = productRow.InvoiceId,
                    InvoiceDate = productRow.InvoiceDate,
                    ArticleNumber = productRow.ArticleNumber,
                    ArticleName = productRow.ArticleName,
                    ProductType = productRow.ProductType,
                    ProductGroupName = productRow.ProductGroupName,
                    MaterialCode = productRow.MaterialCode,
                    Description = productRow.Description,
                    AttestState = productRow.AttestState,
                    AttestColor = productRow.AttestColor,
                    Unit = productRow.Unit,
                    CustomerInvoiceRowId = productRow.CustomerInvoiceRowId,
                    Quantity = productRow.Quantity,
                    PurchasePrice = productRow.PurchasePrice,
                    PurchaseAmount = productRow.PurchaseAmount,
                    SalesPrice = productRow.SalesPrice,
                    SalesAmount = productRow.SalesAmount,
                    DiscountPercent = productRow.DiscountPercent,
                    MarginalIncome = productRow.MarginalIncome,
                    MarginalIncomeRatio = productRow.MarginalIncomeRatio,
                    IsTimeProjectRow = productRow.IsTimeProjectRow,
                    SupplierInvoiceId = productRow.SupplerInvoiceId,
                    EdiEntryId = productRow.EdiEntryId,
                    Date = productRow.Date,
                    Created = productRow.Created,
                    CreatedBy = productRow.CreatedBy,
                    Modified = productRow.Modified,
                    ModifiedBy = productRow.ModifiedBy,
                    SupplierName = suppName,
                    OriginStatus = productRow.OriginStatus
                };

                var date = productRow.Date.HasValue ? productRowDTO.Date : productRowDTO.Created;

                if (fromDate == null && toDate == null)
                {
                    result.Add(productRowDTO);
                }
                else if (date != null && ((toDate == null || date < dateTo) && (fromDate == null || date >= dateFrom)))
                {
                    result.Add(productRowDTO);
                }
            }
            return result;
        }

        #endregion
    }
}
