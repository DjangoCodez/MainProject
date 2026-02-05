

export enum MenuID {
  Purchase = "Billing_Purchase_menu",
  Agreements = "Billing_Contract_menu",
  Order = "Billing_Order_menu",
  Invoice = "Billing_Invoice_menu",
  Supplier = "Economy_Supplier_menu",
  Projects = "Billing_Project_menu",
  Product = "Billing_Product_menu",
  Stock = "Billing_Stock_menu",
  Settings_Sales = "Billing_Preferences_menu",
  Accounting = "Economy_Accounting_menu",
  Customer = "Economy_Customer_menu",
  Settings_Finance = "Economy_Preferences_menu",
  Employee = "Time_Employee_menu",
  Customer_Sale = "Billing_Customer_menu",
  Planning = "Time_Schedule_menu",
  Settings_Staff = "Time_Preferences_menu",
  Time = "Time_Time_menu",
  Salary = "Time_Payroll_menu",
}


export async function getMenuID(value: string): Promise<MenuID | undefined> {
  const menuMap: { [key: string]: MenuID } = {
    PURCHASE: MenuID.Purchase,
    AGREEMENTS: MenuID.Agreements,
    ORDER: MenuID.Order,
    INVOICE: MenuID.Invoice,
    SUPPLIER: MenuID.Supplier,
    PROJECTS: MenuID.Projects,
    PRODUCT: MenuID.Product,
    STOCK: MenuID.Stock,
    SETTINGS_SALES: MenuID.Settings_Sales,
    ACCOUNTING: MenuID.Accounting,
    CUSTOMER: MenuID.Customer,
    SETTINGS_FINANCE: MenuID.Settings_Finance,
    EMPLOYEE: MenuID.Employee,
    CUSTOMER_SALE: MenuID.Customer_Sale,
    PLANNING: MenuID.Planning,
    SETTINGS_STAFF: MenuID.Settings_Staff,
    TIME: MenuID.Time,
    SALARY: MenuID.Salary,
  };

  const key = value.trim().toUpperCase();
  return menuMap[key];
}