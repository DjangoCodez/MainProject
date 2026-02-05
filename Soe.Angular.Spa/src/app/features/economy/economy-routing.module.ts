import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VoucherService } from './voucher/services/voucher.service';

const routes: Routes = [
  {
    path: 'accounting/accountroles',
    loadChildren: () =>
      import(
        '../economy/accounting-coding-levels/accounting-coding-levels.module'
      ).then(m => m.AccountingCodingLevelsModule),
  },
  {
    path: 'accounting/budget',
    loadChildren: () =>
      import('../economy/budget/budget.module').then(m => m.BudgetModule),
  },
  {
    path: 'accounting/liquidityplanning',
    loadChildren: () =>
      import(
        '../economy/accounting-liquidity-planning/accounting-liquidity-planning.module'
      ).then(m => m.AccountingLiquidityPlanningModule),
  },
  {
    path: 'accounting/accountdistribution',
    loadChildren: () =>
      import(
        '../economy/account-distribution/account-distribution.module'
      ).then(m => m.AccountDistributionModule),
  },
  {
    path: 'accounting/reconciliation',
    loadChildren: () =>
      import(
        '../economy/accounting-reconciliation/accounting-reconciliation.module'
      ).then(m => m.AccountingReconciliationModule),
  },
  {
    path: 'accounting/accounts',
    loadChildren: () =>
      import('../economy/accounts/accounts.module').then(m => m.AccountsModule),
  },
  {
    path: 'accounting/companygroup/administration',
    loadChildren: () =>
      import(
        '../economy/company-group-administration/company-group-administration.module'
      ).then(m => m.CompanyGroupAdministrationModule),
  },
  {
    path: 'accounting/companygroup/mapping',
    loadChildren: () =>
      import(
        '../economy/company-group-mappings/company-group-mappings.module'
      ).then(m => m.CompanyGroupMappingsModule),
  },
  {
    path: 'accounting/companygroup/transfer',
    loadChildren: () =>
      import(
        '../economy/company-group-transfer/company-group-transfer.module'
      ).then(m => m.CompanyGroupTransferModule),
  },
  {
    path: 'accounting/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'accounting/vouchersearch',
    loadChildren: () =>
      import('./voucher-search/voucher-search.module').then(
        m => m.VoucherSearchModule
      ),
  },
  {
    path: 'accounting/yearend',
    loadChildren: () =>
      import(
        './account-years-and-periods/account-years-and-periods.module'
      ).then(m => m.YearsAndPeriodsModule),
  },
  {
    path: 'customer/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'customer/customercentral',
    loadChildren: () =>
      import(
        '../../shared/features/customer-central/customer-central.module'
      ).then(m => m.CustomerCentralModule),
  },
  {
    path: 'customer/customers',
    loadChildren: () =>
      import('../../shared/features/customer/customer.module').then(
        m => m.CustomerModule
      ),
  },
  {
    path: 'customer/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'customer/invoice/matches',
    loadChildren: () =>
      import('./customer-invoice-matches/customer-invoice-matches.module').then(
        m => m.CustomerInvoiceMatchesModule
      ),
  },
  {
    path: 'distribution/intrastatexport',
    loadChildren: () =>
      import('./intrastat-export/intrastat-export.module').then(
        m => m.IntrastatExportModule
      ),
  },
  {
    path: 'distribution/saleseu',
    loadChildren: () =>
      import('./distribution-sales-eu/distribution-sales-eu.module').then(
        m => m.DistributionSalesEuModule
      ),
  },
  {
    path: 'accounting/vouchers',
    loadChildren: () =>
      import('./voucher/voucher.module').then(m => m.VoucherModule),
    providers: [VoucherService],
  },
  {
    path: 'accounting/vouchertemplates',
    loadChildren: () =>
      import('./voucher/voucher.module').then(m => m.VoucherModule),
    providers: [VoucherService],
  },
  {
    path: 'export/invoices',
    loadChildren: () =>
      import('./direct-debit/direct-debit.module').then(
        m => m.DirectDebitModule
      ),
  },
  {
    path: 'export/finnish_tax',
    loadChildren: () =>
      import('./finnish-tax-export/finnish-tax-export.module').then(
        m => m.FinnishTaxExportModule
      ),
  },
  {
    path: 'export/saft',
    loadChildren: () =>
      import('./export/saft/saft.module').then(m => m.SaftModule),
  },
  {
    path: 'export/sie',
    loadChildren: () =>
      import('./export/sie/sie.module').then(m => m.SieModule),
  },
  {
    path: 'import/excelimport',
    loadChildren: () =>
      import(
        '../../shared/features/import/excel-import/excel-import.module'
      ).then(m => m.ExcelImportModule),
  },
  {
    path: 'import/spapayments',
    loadChildren: () =>
      import('./import-payments/import-payments.module').then(
        m => m.ImportPaymentsModule
      ),
  },
  {
    path: 'import/payments/customer',
    loadChildren: () =>
      import('./import-payments/import-payments.module').then(
        m => m.ImportPaymentsModule
      ),
  },
  {
    path: 'import/payments/supplier',
    loadChildren: () =>
      import('./import-payments/import-payments.module').then(
        m => m.ImportPaymentsModule
      ),
  },
  {
    path: 'import/sie',
    loadChildren: () =>
      import('./import/sie/sie.module').then(m => m.SieModule),
  },
  {
    path: 'import/xeconnect/batches',
    loadChildren: () =>
      import('./connect-importer/connect-importer.module').then(
        m => m.ConnectImporterModule
      ),
  },
  {
    path: 'inventory/inventories',
    loadChildren: () =>
      import('./inventories/inventories.module').then(m => m.InventoriesModule),
  },
  {
    path: 'inventory/writeoffmethods',
    loadChildren: () =>
      import(
        './inventory-write-off-methods/inventory-write-off-methods.module'
      ).then(m => m.InventoryWriteOffMethodsModule),
  },
  {
    path: 'import/invoices/finvoice',
    loadChildren: () =>
      import(
        './imports-invoices-finvoice/imports-invoices-finvoice.module'
      ).then(m => m.ImportsInvoicesFinvoiceModule),
  },
  {
    path: 'import/xeconnect',
    loadChildren: () =>
      import('./import-connect/import-connect.module').then(
        m => m.ImportConnectModule
      ),
  },
  {
    path: 'inventory/writeofftemplates',
    loadChildren: () =>
      import(
        './inventory-write-off-templates/inventory-write-off-templates.module'
      ).then(m => m.InventoryWriteOffTemplatesModule),
  },
  {
    path: 'inventory/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'inventory/writeoffs',
    loadChildren: () =>
      import('./inventory-writeoffs/inventory-writeoffs.module').then(
        m => m.InventoryWriteoffsModule
      ),
  },
  {
    path: 'preferences/custinvoicesettings/paymentmethods',
    loadChildren: () =>
      import('./customer-payment-methods/customer-payment-methods.module').then(
        m => m.CustomerPaymentMethodsModule
      ),
  },
  {
    path: 'preferences/paycondition',
    loadChildren: () =>
      import(
        '../../shared/features/payment-conditions/payment-conditions.module'
      ).then(m => m.PaymentConditionsModule),
  },
  {
    path: 'preferences/vouchersettings/accountdistributionauto',
    loadChildren: () =>
      import(
        '../economy/account-distribution-auto/account-distribution-auto.module'
      ).then(m => m.AccountDistributionAutoModule),
  },
  {
    path: 'preferences/vouchersettings/distributioncodes',
    loadChildren: () =>
      import('./distribution-codes/distribution-codes.module').then(
        m => m.DistributionCodesModule
      ),
  },
  {
    path: 'preferences/vouchersettings/grossprofitcodes',
    loadChildren: () =>
      import('./gross-profit-codes/gross-profit-codes.module').then(
        m => m.GrossProfitCodesModule
      ),
  },
  {
    path: 'preferences/suppinvoicesettings/paymentmethods',
    loadChildren: () =>
      import('./supplier-payment-methods/supplier-payment-methods.module').then(
        m => m.SupplierPaymentMethodsModule
      ),
  },
  {
    path: 'preferences/vouchersettings/matchsettings',
    loadChildren: () =>
      import('./match-codes/match-codes.module').then(m => m.MatchCodeModule),
  },
  {
    path: 'preferences/vouchersettings/vatcodes',
    loadChildren: () =>
      import('./vat-codes/vat-codes.module').then(m => m.VatCodesModule),
  },
  {
    path: 'preferences/suppinvoicesettings/attestgroups',
    loadChildren: () =>
      import('./attestation-groups/attestation-groups.module').then(
        m => m.AttestationGroupsModule
      ),
  },
  {
    path: 'supplier/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ), 
  },
  {
    path: 'distribution/drilldown',
    loadChildren: () =>
      import('./reports/drill-down-reports/drill-down-reports.module').then(
        m => m.DrillDownReportsModule
      ),
  },
  {
    path: 'preferences/currency',
    loadChildren: () =>
      import('./currencies/currencies.module').then(m => m.CurrenciesModule),
  },
  {
    path: 'supplier/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'supplier/invoice/matches',
    loadChildren: () =>
      import('./supplier-invoice-matches/supplier-invoice-matches.module').then(
        m => m.SupplierInvoiceMatchesModule
      ),
  },
  {
    path: 'supplier/suppliers',
    loadChildren: () =>
      import('./suppliers/suppliers.module').then(m => m.SuppliersModule),
  },
  {
    path: 'supplier/suppliercentral',
    loadChildren: () =>
      import('./supplier-central/supplier-central.module').then(
        m => m.SupplierCentralModule
      ),
  },
  {
    path: 'supplier/supplierinvoicesarrivalhall',
    loadChildren: () =>
      import(
        './supplier-invoices-arrival-hall/supplier-invoices-arrival-hall.module'
      ).then(m => m.SupplierInvoicesArrivalHallModule),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EconomyRoutingModule {}
