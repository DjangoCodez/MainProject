import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'asset',
    loadChildren: () =>
      import('./asset-register/asset-register.module').then(
        m => m.AssetRegisterModule
      ),
  },
  {
    path: 'contract/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'contract/groups',
    loadChildren: () =>
      import('./contract-groups/contract-groups.module').then(
        m => m.ContractGroupsModule
      ),
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
    path: 'distribution/edistribution',
    loadChildren: () =>
      import('./edistribution/edistribution.module').then(
        m => m.EdistributionModule
      ),
  },
  {
    path: 'import/excelimport',
    loadChildren: () =>
      import(
        '../../shared/features/import/excel-import/excel-import.module'
      ).then(m => m.ExcelImportModule),
  },
  {
    path: 'order/handlebilling',
    loadChildren: () =>
      import('./handle-billing/handle-billing.module').then(
        m => m.HandleBillingModule
      ),
  },
  {
    path: 'import/edi',
    loadChildren: () => import('./edi/edi.module').then(m => m.EdiModule),
  },
  {
    path: 'invoice/household',
    loadChildren: () =>
      import('./household-tax-deduction/household-tax-deduction.module').then(
        m => m.HouseholdTaxDeductionModule
      ),
  },
  {
    path: 'order/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'preferences/textblock',
    loadChildren: () =>
      import('./text-block/text-block.module').then(m => m.TextBlockModule),
  },
  {
    path: 'preferences/deliverytype',
    loadChildren: () =>
      import('./delivery-types/delivery-types.module').then(
        m => m.DeliveryTypesModule
      ),
  },
  {
    path: 'preferences/deliverycondition',
    loadChildren: () =>
      import('./delivery-condition/delivery-condition.module').then(
        m => m.DeliveryConditionModule
      ),
  },
  {
    path: 'preferences/invoicesettings/markup',
    loadChildren: () =>
      import('./markup/markup.module').then(m => m.MarkupModule),
  },
  {
    path: 'preferences/invoicesettings/shifttype',
    loadChildren: () =>
      import('../../shared/features/shift-type/shift-type.module').then(
        m => m.ShiftTypeModule
      ),
  },
  {
    path: 'preferences/paycondition',
    loadChildren: () =>
      import(
        '../../shared/features/payment-conditions/payment-conditions.module'
      ).then(m => m.PaymentConditionsModule),
    data: {
      isEconomy: false,
    },
  },
  {
    path: 'preferences/productsettings/productgroup',
    loadChildren: () =>
      import('./product-groups/product-groups.module').then(
        m => m.ProductGroupsModule
      ),
  },
  {
    path: 'preferences/productsettings/productunit',
    loadChildren: () =>
      import('./product-units/product-units.module').then(
        m => m.ProductUnitsModule
      ),
  },
  {
    path: 'preferences/emailtemplate',
    loadChildren: () =>
      import('./email-templates/email-templates.module').then(
        m => m.EmailTemplatesModule
      ),
  },
  {
    path: 'product/products',
    loadChildren: () =>
      import('./products/products.module').then(m => m.ProductsModule),
  },
  {
    path: 'product/pricelists',
    loadChildren: () =>
      import(
        './customer-product-pricelists/customer-product-pricelists.module'
      ).then(m => m.CustomerProductPricelistsModule),
  },
  {
    path: 'preferences/productsettings/materialcode',
    loadChildren: () =>
      import('./material-codes/material-codes.module').then(
        m => m.MaterialCodesModule
      ),
  },
  {
    path: 'preferences/invoicesettings/customerdiscount',
    loadChildren: () =>
      import('./customer-discount/customer-discount.module').then(
        m => m.CustomerDiscountModule
      ),
  },
  {
    path: 'preferences/invoicesettings/pricebasedmarkup',
    loadChildren: () =>
      import('./price-based-markup/price-based-markup.module').then(
        m => m.PriceBasedMarkupModule
      ),
  },
  {
    path: 'preferences/invoicesettings/supplieragreement',
    loadChildren: () =>
      import('./discount-letters/discount-letters.module').then(
        m => m.DiscountLettersModule
      ),
  },
  {
    path: 'preferences/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'project/central',
    loadChildren: () =>
      import('./project-central/project-central.module').then(
        m => m.ProjectCentralModule
      ),
  },
  {
    path: 'project/list',
    loadChildren: () =>
      import('./project/project.module').then(m => m.ProjectModule),
  },
  {
    path: 'purchase/delivery',
    loadChildren: () =>
      import('./purchase-delivery/purchase-delivery.module').then(
        m => m.PurchaseDeliveryModule
      ),
  },
  {
    path: 'purchase/list',
    loadChildren: () =>
      import('./purchase/purchase.module').then(m => m.PurchaseModule),
  },
  {
    path: 'purchase/pricelists',
    loadChildren: () =>
      import(
        './purchase-product-pricelist/purchase-product-pricelist.module'
      ).then(m => m.PurchaseProductPricelistModule),
  },
  {
    path: 'purchase/products',
    loadChildren: () =>
      import('./purchase-products/purchase-products.module').then(
        m => m.PurchaseProductsModule
      ),
  },
  {
    path: 'purchase/pricecompass',
    loadChildren: () =>
      import('./price-optimization/price-optimization.module').then(
        m => m.PriceOptimizationModule
      ),
  },

  {
    path: 'stock/edit',
    loadChildren: () =>
      import('./stock-warehouse/stock-warehouse.module').then(
        m => m.StockWarehouseModule
      ),
  },
  {
    path: 'product/commoditycodes',
    loadChildren: () =>
      import(
        './statistical-commodity-codes/statistical-commodity-codes.module'
      ).then(m => m.StatisticalCommodityCodesModule),
  },
  {
    path: 'product/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'product/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'project/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'project/timesheetuser',
    loadChildren: () =>
      import('./project-time-report/project-time-report.module').then(
        m => m.ProjectTimeReportModule
      ),
  },
  {
    path: 'stock/inventory',
    loadChildren: () =>
      import('./stock-inventory/stock-inventory.module').then(
        m => m.StockInventoryModule
      ),
  },
  {
    path: 'stock/purchase',
    loadChildren: () =>
      import('./stock-purchase/stock-purchase.module').then(
        m => m.StockPurchaseModule
      ),
  },
  {
    path: 'stock/saldo',
    loadChildren: () =>
      import('./stock-balance/stock-balance.module').then(
        m => m.StockBalanceModule
      ),
  },
  {
    path: 'statistics/customer',
    loadChildren: () =>
      import('./sales-statistics/sales-statistics.module').then(
        m => m.SalesStatisticsModule
      ),
  },
  {
    path: 'statistics/product',
    loadChildren: () =>
      import('./product-statistics/product-statistics.module').then(
        m => m.ProductStatisticsModule
      ),
  },
  {
    path: 'statistics/purchase',
    loadChildren: () =>
      import('./purchase-statistics/purchase-statistics.module').then(
        m => m.PurchaseStatisticsModule
      ),
  },
];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class BillingRoutingModule {}
