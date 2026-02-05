import '../../Module';
import '../../../Shared/Billing/Module';

import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { SelectProjectService } from "../../../Common/Dialogs/SelectProject/SelectProjectService";
import { OrderService } from "../../../Shared/Billing/Orders/OrderService";
import { ProductService } from "../../../Shared/Billing/Products/ProductService";
import { ProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { ScheduleService } from '../../../Shared/Time/Schedule/ScheduleService';


angular.module("Soe.Billing.Projects.TimeSheets.Module", ['Soe.Billing', 'Soe.Shared.Billing'])
    .service("commonCustomerService", CommonCustomerService)
    .service("orderService", OrderService)
    .service("invoiceService", InvoiceService)
    .service("productService", ProductService)
    .service("selectProjectService", SelectProjectService)
    .service("projectService", ProjectService)
    .service("scheduleService", ScheduleService);
