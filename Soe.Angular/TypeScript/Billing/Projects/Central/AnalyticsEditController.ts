import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { Feature, SoeOriginType, ProjectCentralHeaderGroupType, ProjectCentralBudgetRowType} from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { ProjectGridDTO } from "../../../Common/Models/ProjectDTO";
import { AgChartUtility, AgChartOptionsPie } from "../../../Util/ag-chart/AgChartUtility";

declare var agCharts;

interface IProjectTransaction {
    date: any,
    rowTypeName: string,
    rowTypeId: number,
    description: string,
    value: number,
    value2?: number,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    //Parameters

    //Terms
    terms: { [index: string]: string; };

    //Properties
    private projectChanged: boolean = true;

    private project: ProjectGridDTO;
    private fromDate: Date;
    private toDate: Date;
    private includeChildProjects: boolean = false;
    private projectId: number;
    private employeeId: number;
    private supplierInvoices: any;
    private customerInvoices: any;
    private employees: any[];
    private employeesDict: any;
    private productRows: any[];
    private timeRows: any[];
    private projectCentralRows: any;

    private activated: boolean;
    infoAccordionOpen: boolean = false;
    loadDetails = false;
    private doReload = false;

    private firstDate: Date;
    private lastDate: Date;
    private projectStartDate: Date;
    private projectStopDate: Date;

    //Chart data
    private materialCosts: number;
    private incomeBudget: number;
    private incomeBalance: number;
    private costPersonellBudget: number;
    private costPersonellBudgetBalance: number;
    private costPersonellBudgetHours: number;
    private costPersonellBudgetHoursBalance: number;
    private costMaterialBudget: number;
    private costMaterialBalance: number;


    //Chart materialCostsLineGraph
    private materialCostsLineGraphElem: Element;
    private materialCostsLineGraphOptions: any;
    private materialCostsLineGraphData: any[] = [];
    private enableGraph: boolean = false;
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartYAxisTerm: string;


    //@ngInject
    constructor(
        private $q: ng.IQService,
        private messagingService: IMessagingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private projectService: IProjectService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        dirtyHandlerFactory: IDirtyHandlerFactory,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.employeeId = soeConfig.employeeId;
        this.setTabCallbacks(this.onTabActivated, undefined)
    }

    private onTabActivated() {
        if (this.projectId && (!this.activated || this.projectChanged)) {
            this.flowHandler.start([
                { feature: Feature.Billing_Project_List, loadReadPermissions: false, loadModifyPermissions: true },
            ]
            );
            this.doLookups();
            this.activated = true;
        } else if (this.doReload) {
                this.doLookups()
        }
    }

    //Setup
    protected init() {
        this.$q.all([
            this.loadTerms(),
        ])
    }

    onInit(parameters) {
        this.guid = parameters.guid;
        this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
            this.projectChanged = true;
            this.projectId = x.projectId;
            this.includeChildProjects = x.includeChildProjects;
            this.fromDate = x.fromDate;
            this.toDate = x.toDate;
            this.projectCentralRows = x.projectCentralRows;

            this.supplierInvoices = x.supplierInvoices;
            this.customerInvoices = x.customerInvoices;

            if (this.activated)
                this.doReload = true;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadProject(),
            this.loadProductRows(),
            this.loadTimeRows(),
            this.loadTerms(),
        ]).then(() => {
            if(this.projectId)
                this.createCharts()
        })
    }

    //Action
    private loadTerms(): ng.IPromise<any> {
        if (this.terms) return
        const keys: string[] = [
            "billing.project.central.changed",
            "billing.project.central.changedby",
            "billing.project.central.showinfo",
            "billing.project.central.gettingdata",
            "common.customer",
            "billing.project.project",
            "billing.project.central.analytics.materialcostovertime",
            "billing.project.central.analytics.workedhoursovertime",
            "billing.project.central.analytics.invoicedovertime",
            "billing.project.central.analytics.cost",
            "billing.project.central.analytics.hours",
            "billing.project.central.analytics.income",
            "billing.project.central.analytics.materialcostpermaterialcode",
            "billing.project.central.analytics.costperorder",
            "billing.project.central.analytics.incomefromorderperproduct",
            "billing.project.central.analytics.hoursperperson",
            "billing.project.central.analytics.hourspertimecode",
            "billing.project.central.analytics.workedhoursperperson",
            "billing.project.central.analytics.outcome",
            "billing.project.central.analytics.budget",
            "billing.project.central.analytics.productincomeexplanation",
            "billing.project.timesheet.workedtime",
            "billing.project.timesheet.invoicedtime",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }



    private loadProject() {
        return this.projectService.getProjectGridDTO(this.projectId).then((x) => {
            this.project = x;
            this.projectStartDate = x.startDate ? new Date(x.startDate) : undefined;
            this.projectStopDate = x.stopDate ? new Date(x.stopDate) : undefined;

        })
    }

    private loadProductRows(): ng.IPromise<any> {
        return this.projectService.getProjectProductRows(this.projectId, SoeOriginType.Order, this.includeChildProjects, this.fromDate, this.toDate).then(x => {
            this.productRows = x;
        })
    }

    private loadTimeRows(): ng.IPromise<any> {
        return this.projectService.getProjectTimeBlocksForTimeSheetFilteredByProject(this.fromDate, this.toDate, this.projectId, this.includeChildProjects, this.employeeId).then(x => {
            this.timeRows = x;
        })
    }

    //Charts
    private prepareGraphData(): any {
        let personellHours = []
        const sumPerDay = {}
        let materialCosts: IProjectTransaction[] = [];
        let customerInvoices: IProjectTransaction[] = [];
        let timeRows: IProjectTransaction[] = [];
        let orderCosts: IProjectTransaction[] = [];

        let materialCostPerTimeCode = {}
        //let materialCostPerSupplier = {}
        let salesAmountPerProduct = {}
        let timePerTimeCode = {}
        let timePerPerson = {}
        let timePayrollPerPerson = {}

        this.projectCentralRows.forEach(row => {
            if (row.budget && row.budget != 0) {
                if (row.type == ProjectCentralBudgetRowType.IncomeInvoiced) {
                    this.incomeBudget = row.budget;
                    this.incomeBalance = row.value || 0;
                }
                else if (row.type == ProjectCentralBudgetRowType.CostMaterial) {
                    this.costMaterialBudget = row.budget;
                    this.costMaterialBalance = row.value || 0;
                }
                else if (row.type == ProjectCentralBudgetRowType.CostPersonell) {
                    this.costPersonellBudget = row.budget;
                    this.costPersonellBudgetBalance = row.value || 0;
                    this.costPersonellBudgetHours = row.budgetTime || 0;
                    this.costPersonellBudgetHoursBalance = row.value2 || 0;
                }
            }
            else if (row.type === ProjectCentralBudgetRowType.CostMaterial && row.originType === SoeOriginType.SupplierInvoice) {
                materialCosts.push({
                    date: row.date,
                    rowTypeName: row.costTypeName,
                    rowTypeId: row.rowType,
                    description: `${row.costTypeName} - ${ row.typeName}`,
                    value: row.value,
                })
                
                if (materialCostPerTimeCode[row.costTypeName || ""]) {
                    materialCostPerTimeCode[row.costTypeName || ""] += row.value;
                }
                else {
                    materialCostPerTimeCode[row.costTypeName || ""] = row.value;
                }

            }
            else if (row.type == ProjectCentralBudgetRowType.IncomeInvoiced) {
                customerInvoices.push({
                    date: row.date,
                    rowTypeName: row.groupTypeName,
                    rowTypeId: row.rowType,
                    description: row.typeName,
                    value: row.value,
                })
            }

            if (row.originType == SoeOriginType.Order &&
                (row.groupRowType == ProjectCentralHeaderGroupType.CostsMaterial ||
                row.groupRowType == ProjectCentralHeaderGroupType.CostsPersonell ||
                row.groupRowType == ProjectCentralHeaderGroupType.CostsExpense)) {
                orderCosts.push({
                    date: row.date,
                    rowTypeName: row.groupTypeName,
                    rowTypeId: row.rowType,
                    description: row.typeName,
                    value: row.value,
                })
            }

        })

        this.productRows.forEach(row => {
            if (!row.isTimeProjectRow && row.productType < 2) {
                materialCosts.push({
                    date: CalendarUtility.toFormattedDate(row.date || row.created),
                    rowTypeName: row.materialCode,
                    rowTypeId: 0,
                    description: `${row.materialCode} - ${row.articleNumber} - ${row.description}`,
                    value: row.purchaseAmount,
                })
                if (materialCostPerTimeCode[row.materialCode || ""]) {
                    materialCostPerTimeCode[row.materialCode || ""] += row.purchaseAmount;
                }
                else {
                    materialCostPerTimeCode[row.materialCode || ""] = row.purchaseAmount;
                }

                if (salesAmountPerProduct[row.articleNumber]) {
                    salesAmountPerProduct[row.articleNumber].salesAmount += row.salesAmount;
                    salesAmountPerProduct[row.articleNumber].purchaseAmount += row.purchaseAmount;
                    salesAmountPerProduct[row.articleNumber].quantity += row.quantity;
                }
                else {
                    salesAmountPerProduct[row.articleNumber] = {
                        label: `${row.articleNumber} - ${row.articleName}`,
                        salesAmount: row.salesAmount,
                        purchaseAmount: row.purchaseAmount,
                        quantity: row.quantity,
                        unit: row.unit
                    };
                }
            }
        })

        this.timeRows.forEach(row => {
            personellHours.push({
                date: row.date,
                rowTypeName: row.timeCodeName,
                rowTypeId: row.timeCodeId,
                description: `${row.timeCodeName ? row.timeCodeName + " - " : ""}${row.employeeName}`,
                value: row.timePayrollQuantity / 60,
                value2: row.invoiceQuantity / 60,
            })

            if (timePerTimeCode[row.timeCodeName || ""]) {
                timePerTimeCode[row.timeCodeName || ""].payrollQuantity += row.timePayrollQuantity / 60;
                timePerTimeCode[row.timeCodeName || ""].invoiceQuantity += row.invoiceQuantity / 60;
            }
            else {
                timePerTimeCode[row.timeCodeName || ""] = { payrollQuantity: row.timePayrollQuantity / 60, invoiceQuantity: row.invoiceQuantity / 60 };
            }

            if (timePerPerson[row.employeeName]) {
                timePerPerson[row.employeeName || ""].payrollQuantity += row.timePayrollQuantity / 60;
                timePerPerson[row.employeeName || ""].invoiceQuantity += row.invoiceQuantity / 60;
            }
            else {
                timePerPerson[row.employeeName || ""] = { payrollQuantity: row.timePayrollQuantity / 60, invoiceQuantity: row.invoiceQuantity / 60 };
            }

            if (timePayrollPerPerson[row.employeeName]) {
                if (timePayrollPerPerson[row.employeeName][row.timeCodeName || ""]) {
                    timePayrollPerPerson[row.employeeName][row.timeCodeName || ""] += row.timePayrollQuantity / 60;
                }
                else {
                    timePayrollPerPerson[row.employeeName][row.timeCodeName || ""] = (row.timePayrollQuantity || 0) / 60;
                }
            } else {
                timePayrollPerPerson[row.employeeName] = { }
                timePayrollPerPerson[row.employeeName][row.timeCodeName || ""] = (row.timePayrollQuantity || 0) / 60;
            }
        })

        return [materialCosts, personellHours, customerInvoices, this.hashToListSimple(materialCostPerTimeCode), this.hashToList(timePerTimeCode), this.hashToList(timePerPerson), salesAmountPerProduct, orderCosts, this.hashToList(timePayrollPerPerson)]
    }

    setDateInterval(date: any) {
        let dateObj = new Date(date);
        if (!this.firstDate) {
            this.firstDate = dateObj;
            this.lastDate = dateObj;
        } else if (this.firstDate > dateObj) {
            this.firstDate = dateObj;
        } else if (this.lastDate < dateObj) {
            this.lastDate = dateObj;
        }
    }

    private hashToListSimple(hash: any) {
        let data = [];
        Object.keys(hash).forEach(e => {
            data.push({ label: e, value: hash[e] })
        });
        return data;
    }

    private hashToList(hash: any) {
        let data = [];
        Object.keys(hash).forEach(e => {
            data.push({ label: e, ...hash[e] })
        });
        return data;
    }

    aggregatePerDate(items: any[], budgetValue, groupAttr?) {
        items.sort((a, b) => {
            const date1 = new Date(a.date)
            const date2 = new Date(b.date)
            return date1.getTime() - date2.getTime()
        })

        if (items && items.length > 0) {
            this.setDateInterval(items[0].date);
            this.setDateInterval(items[items.length - 1].date);
        }

        let newObj = {};
        let groupAttrTotals = {}
        let runningTotal = 0;

        items.forEach(r => {
            const attr = r.date;
            const groupName = r[groupAttr] || "";
            if (!groupAttrTotals[groupName])
                groupAttrTotals[groupName] = 0;

            if (!newObj[attr]) {
                newObj[attr] = { total: runningTotal + r.value, rows: [] }
                if (groupAttr)
                    Object.keys(groupAttrTotals).forEach(g => {
                        newObj[attr][g] = groupAttrTotals[g] || 0;
                    })
                    newObj[attr][groupName] += r.value;
            }
            else {
                newObj[attr].total += r.value;
                if (groupAttr && newObj[attr][groupName]) {
                    newObj[attr][groupName] += r.value;
                } else {
                    newObj[attr][groupName] = r.value + groupAttrTotals[groupName];
                }
            }
            runningTotal += r.value;
            groupAttrTotals[groupName] += r.value;
            
            if (r.value != 0)
                newObj[attr].rows.push(r);
        })
        
        const graphData = [];

        const groupNames = Object.keys(groupAttrTotals);
        Object.keys(newObj).forEach(e => {
            let date = new Date(e)
            let groups = {}
            groupNames.forEach(g => {
                groups[g] = newObj[e][g] || 0;
            })
            graphData.push({ date, total: newObj[e].total, rows: newObj[e].rows, budget: this.getBudgetTrendValueByDate(budgetValue, date), ...groups})
        });


        return [graphData, groupNames];
    }

    private getBudgetTrendValueByDate(budgetValue, date) {
        if (!this.projectStopDate)
            return budgetValue
        let transDate = new Date(date);
        let startDate = new Date(this.project.startDate || this.firstDate);
        if (startDate > transDate)
            return 0;

        const totalDiff = (this.projectStopDate.getTime() - startDate.getTime()) / (1000 * 3600 * 24);
        const startToTransDiff = (transDate.getTime()- startDate.getTime()) / (1000 * 3600 * 24);
        return (budgetValue / totalDiff) * startToTransDiff
    }

    private createCharts() {

        if (!agCharts)
            return;

        const [masterialCosts, timeRows, customerInvoices, materialPerTimeCode, timePerTimeCode, timePerPerson, salesPerProduct, orderCosts, timePayrollPerPerson] = this.prepareGraphData();

        this.createMaterialCostsLineGraph(masterialCosts);
        this.createPersonellHoursLineGraph(timeRows);
        this.createIncomeLineGraph(customerInvoices);

        this.createSalesPerProductPieGraph(salesPerProduct);
        this.createMaterialCostPerTimeCodePieGraph(materialPerTimeCode);
        this.createCostPerOrderPieGraph(orderCosts);
        this.createHoursPerPersonColumnGraph(timePerPerson);
        this.createHoursPerTimeCodeColumnGraph(timePerTimeCode);
        this.createTimePayrollPerPersonColumnGraph(timePayrollPerPerson);
        this.projectChanged = false;
    }

    private createMaterialCostsLineGraph(materialCosts: IProjectTransaction[]) {
        const [materialCostsPerDate, groupNames] = this.aggregatePerDate(materialCosts, this.costMaterialBudget, "rowTypeName");
        const element = this.getElement("#materialCostsLineGraph");

        const chartInfo = {
            title: this.terms["billing.project.central.analytics.materialcostovertime"],
            yAxis: this.terms["billing.project.central.analytics.cost"]
        }
        const options: any = this.createDefaultLineChart(element, materialCostsPerDate, this.costMaterialBudget != 0, chartInfo);
        options.series[0].type = "area";
        options.series[0].yKeys = groupNames;
        options.height = 500;

        agCharts.AgChart.create(options)
    }

    private createPersonellHoursLineGraph(timeRows: any[]) {
        const [hoursPersDate, groupNames] = this.aggregatePerDate(timeRows, this.costPersonellBudgetHours / 60, "rowTypeName")
        const element = this.getElement("#personellHoursLineGraph");

        const chartInfo = {
            title: this.terms["billing.project.central.analytics.workedhoursovertime"],
            yAxis: this.terms["billing.project.central.analytics.hours"]
        }
        const options: any = this.createDefaultLineChart(element, hoursPersDate, this.costPersonellBudget != 0, chartInfo);
        options.series[0].type = "area";
        options.series[0].yKeys = groupNames;
        options.height = 500;

        agCharts.AgChart.create(options);
    }

    private createIncomeLineGraph(customerInvoices: any[]) {
        const [incomePerDate, groupNames] = this.aggregatePerDate(customerInvoices, this.incomeBudget)
        const element = this.getElement("#incomeLineGraph");

        const chartInfo = {
            title: this.terms["billing.project.central.analytics.invoicedovertime"],
            yAxis: this.terms["billing.project.central.analytics.income"]
        }
        const options: any = this.createDefaultLineChart(element, incomePerDate, this.incomeBudget != 0, chartInfo);
        options.height = 500;

        agCharts.AgChart.create(options);
    }

    private createMaterialCostPerTimeCodePieGraph(materialPerTimeCode) {
        const element = this.getElement("#materialCostsPerTimeCodePieGraph");

        let chart = new AgChartOptionsPie();
        chart.title = this.terms["billing.project.central.analytics.materialcostpermaterialcode"];
        const options: any = AgChartUtility.createDefaultPieChart(element, materialPerTimeCode, chart)
        options.series[0].tooltipRenderer = function (params) {
            return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                params.datum[params.labelKey] +
                '</div>' +
                '<div class="ag-chart-tooltip-content">' +
                NumberUtility.printDecimal(params.datum.value) +
                '</div>';
        }
        options.legend.enabled = false;
        options.height = 500;

        agCharts.AgChart.create(options);
    }

    private createCostPerOrderPieGraph(orderCosts) {
        let hash = {}
        orderCosts.forEach(r => {
            let desc = r.description.split(",")[0];
            if (hash[desc]) {
                hash[desc] += r.value;
            } else {
                hash[desc] = r.value;
            }
        })

        const orderList = this.hashToListSimple(hash);
        const element = this.getElement("#costPerOrderPieGraph");

        let chart = new AgChartOptionsPie();
        chart.title = this.terms["billing.project.central.analytics.costperorder"];
        const options: any = AgChartUtility.createDefaultPieChart(element, orderList, chart)

        options.series[0].tooltipRenderer = function (params) {
            return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                params.datum[params.labelKey] +
                '</div>' +
                '<div class="ag-chart-tooltip-content">' +
                NumberUtility.printDecimal(params.datum.value) +
                '</div>';
        }
        options.series[0].label = { minAngle: 20 }
        options.legend.enabled = false;
        options.height = 500;

        agCharts.AgChart.create(options);
    }

    private createSalesPerProductPieGraph(salesPerProduct) {
        let data = []
        Object.keys(salesPerProduct).forEach(e => {
            data.push({ label: salesPerProduct[e].label, value: salesPerProduct[e].salesAmount, purchaseAmount: salesPerProduct[e].purchaseAmount, quantity: salesPerProduct[e].quantity, unit: salesPerProduct[e].unit })
        });

        const element = this.getElement("#salesPerProductPieGraph");

        let chart = new AgChartOptionsPie();
        chart.title = this.terms["billing.project.central.analytics.incomefromorderperproduct"];
        const options: any = AgChartUtility.createDefaultPieChart(element, data, chart)

        options.series[0].tooltipRenderer = function (params) {
            let marginal = params.datum.purchaseAmount ? (100 * params.datum.value / params.datum.purchaseAmount) - 100 : 0;
            marginal = Number(marginal.toFixed(2))
            return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                params.datum[params.labelKey] +
                '</div>' +
                '<div class="ag-chart-tooltip-content">' +
                NumberUtility.printDecimal(params.datum.value) + ` (${NumberUtility.printDecimal(marginal)}%) (${NumberUtility.printDecimal(params.datum.quantity)} ${params.datum.unit})` +
                '</div>';
        }
        options.series[0].label = { minAngle: 20 }
        options.height = 500;
        options.legend.enabled = false;
        options.subtitle = { text: this.terms["billing.project.central.analytics.productincomeexplanation"] }

        agCharts.AgChart.create(options);
    }

    private createHoursPerPersonColumnGraph(timePerPerson) {
        const element = this.getElement("#hoursPerPersonPieGraph");
        const options = {
            container: element,
            autoSize: true,
            height: 500,
            data: timePerPerson,
            title: {
                text: this.terms["billing.project.central.analytics.hoursperperson"],
                fontSize: 18,
            },
            series: [
                {
                    type: 'column',
                    xKey: 'label',
                    yKeys: [
                        'payrollQuantity',
                        'invoiceQuantity',
                    ],
                    yNames: [
                        this.terms["billing.project.timesheet.workedtime"],
                        this.terms["billing.project.timesheet.invoicedtime"]
                    ],
                    grouped: true,
                },
            ],
                axes: [
                    {
                        type: 'category',
                        position: 'bottom',
                    },
                    {
                        type: 'number',
                        position: 'left',
                        label: {
                            formatter: function (params) {
                                return params.value + 'H';
                            },
                        },
                    },
                ],
         }
        agCharts.AgChart.create(options);
    }

    private createHoursPerTimeCodeColumnGraph(timePerTimeCode) {
        const element = this.getElement("#hoursPerTimeCodePieGraph");
        const options = {
            container: element,
            autoSize: true,
            height: 500,
            data: timePerTimeCode,
            title: {
                text: this.terms["billing.project.central.analytics.hourspertimecode"],
                fontSize: 18,
            },
            series: [
                {
                    type: "column",
                    xKey: "label",
                    yKeys: [
                        "payrollQuantity",
                        "invoiceQuantity",
                    ],
                    yNames: [
                        this.terms["billing.project.timesheet.workedtime"],
                        this.terms["billing.project.timesheet.invoicedtime"]
                    ],
                    grouped: true,
                },
            ],
            axes: [
                {
                    type: "category",
                    position: "bottom",
                },
                {
                    type: "number",
                    position: "left",
                    label: {
                        formatter: function (params) {
                            return params.value + 'H';
                        },
                    },
                },
            ],
        }

        agCharts.AgChart.create(options);
    }

    private createTimePayrollPerPersonColumnGraph(timePerPerson) {
        const element = this.getElement("#hoursPayrollPerPersonColGraph");

        const yKeys = []
        timePerPerson.forEach(item => {
            Object.keys(item).forEach(key => {
                if (yKeys.indexOf(key) == -1 && key != "label")
                    yKeys.push(key)
            })
        })

        const options = {
            container: element,
            autoSize: true,
            height: 500,
            data: timePerPerson,
            title: {
                text: this.terms["billing.project.central.analytics.workedhoursperperson"],
                fontSize: 18,
            },
            series: [
                {
                    type: 'column',
                    xKey: 'label',
                    yKeys,
                },
            ],
            axes: [
                {
                    type: 'number',
                    position: 'left',
                    label: {
                        formatter: function (params) {
                            return params.value + 'h';
                        },
                    },
                },
                {
                    type: "category",
                    position: "bottom"
                }
            ],
        }
        agCharts.AgChart.create(options);
    }

    private defaultTooltipRender(): Function {
        let renderer = function (params) {
            const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #CCCCCC">';
            const divEnd = '</div>';
            const rowsLen = 12
            const rows = params.datum.rows.filter(r => {
                if (r.rowTypeName == params.yKey)
                    return r;
            });
            const date = CalendarUtility.toFormattedDate(params.datum.date);
            const value = NumberUtility.printDecimal(params.datum.total, 2, 2);
            const partValue = params.yValue ? ` - ${params.yKey || "()"}: ${NumberUtility.printDecimal(params.yValue, 2, 2)} ` : "";
            let html = "";

            const items = rows.length > rowsLen ? rows.slice(0, rowsLen) : rows;

            items.forEach(r => {
                html += htmlStart + r.description + " (" + NumberUtility.printDecimal(r.value, 2, 2) + ")" + divEnd;
            })

            if (rows.length > rowsLen) {
                html += htmlStart + "..." + divEnd;
            }

            return '<div class="ag-chart-tooltip-title" style="background-color: #e3e3e3; color: #333333">' +
                date + partValue + " (" + value + ")" +
                divEnd +
                html;
        }

        return renderer
    }

    private createDefaultLineChart(container: Element, data: any[], includeBudget: boolean, chartConstants: any) {
        let series: any = [
            {
                type: 'line',
                xKey: 'date',
                yKey: 'total',
                yName: this.terms["billing.project.central.analytics.outcome"],
                tooltipRenderer: this.defaultTooltipRender(),
                marker: {
                    enabled: true
                }
            },
        ]

        if (includeBudget) {
            series.push({
                xKey: "date",
                yKey: "budget",
                yName: this.terms["billing.project.central.analytics.budget"],
                marker: {
                    enabled: false,
                },
                tooltipRenderer: (params) => {
                    const date = CalendarUtility.toFormattedDate(params.datum.date);
                    const value = NumberUtility.printDecimal(params.datum.budget, 2, 2);
                    return '<div class="ag-chart-tooltip-title" style="background-color: #e3e3e3; color: #333333">' +
                        date + " (" + value + ")" + "</div>"
                },
                lineDash: [6, 3]
            })
        }

        if (data.length === 0)
            data.push({ date: new Date() })

        const options = {
            container: container,
            data: data,
            fontFamily: 'Roboto Condensed',
            fontSize: 14,
            title: {
                text: chartConstants.title,
                fontSize: 18,
            },
            autoSize: true,
            padding: {
                top: 20,
                right: 20,
                bottom: 20,
                left: 20,
            },
            navigator: {
                enabled: true,
            },
            legend: {
                enabled: true,
            },
            background: {
                visible: false
            },
            series,
            axes: [
                {
                    position: 'bottom',
                    type: 'time',
                    label: {
                        formatter: function (params) {
                            return CalendarUtility.toFormattedDate(params.value)
                        },
                    }
                },
                {
                    position: 'left',
                    type: 'number',
                    title: {
                        text: chartConstants.yAxis,
                    },
                    label: {
                        formatter: function (params) {
                            return NumberUtility.printDecimal(params.value, 0, 0)
                        }
                    }
                },
            ],
        };

        return options;
    }

    private getElement(elementId: string): Element {
        const element = document.querySelector(elementId);
        if (element.firstChild)
            element.removeChild(element.firstChild)
        return element;
    }
}