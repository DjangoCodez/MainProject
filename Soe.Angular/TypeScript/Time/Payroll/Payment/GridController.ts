import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature, CompanySettingType, TermGroup_TimeSalaryPaymentExportType, TermGroup_Currency } from "../../../Util/CommonEnumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { TimeSalaryPaymentExportGridDTO } from "../../../Common/Models/TimeSalaryExportDTOs";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IPayrollService } from "../PayrollService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ExportUtility } from "../../../Util/ExportUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ExtendedExportDialogController } from "./Dialogs/ExtendedExport/ExtendedExportDialogController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };
    private hasSelectedPayments: boolean;

    buttonFunctions: any = [];
    gridFooterComponentUrl: any;
    publishPayrollSlipWhenLockingPeriod: boolean;
    exportType: TermGroup_TimeSalaryPaymentExportType = TermGroup_TimeSalaryPaymentExportType.Undefined;
    useExtendedNOK: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private payrollService: IPayrollService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Payroll.Payment", progressHandlerFactory, messagingHandlerFactory);

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.onDoLookups())
           // .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Payroll_Payment, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onDoLookups() {
        return this.loadCompanySettings().then(() => {
            return this.$q.all([
                this.setupGrid()
            ]);
        });
    }

    private setupGrid() {
        var keys: string[] = [
            "core.exportexcel",
            "common.name",
            "time.employee.employee.employeenr",
            "time.employee.employee.employeenrshort",
            "time.export.salary.exportdate",
            "time.export.salary.targetname",
            "time.payroll.payment.accountdepositnetamount",
            "time.payroll.payment.accountnr",
            "time.payroll.payment.cashdepositnetamount",
            "time.payroll.payment.file.tooltip",
            "time.payroll.payment.paymentdate",
            "time.payroll.payment.payrolldateinterval",
            "time.payroll.payment.netamount",
            "time.payroll.payment.salaryspecificationpublishdate",
            "time.payroll.payment.setsalaryspecificationpublishdate",
            "time.payroll.payment.debitdate",
            "time.time.timeperiod.timeperiodhead",
            "time.time.timeperiod.timeperiod",
            "time.payroll.payment.createextendedfilenorway",
            "time.payroll.payment.currencydate",
            "time.payroll.payment.currencyrate",
            "common.customer.invoices.currencycode",
            "time.payroll.payment.currencynetamount",
            "time.payroll.payment.currencyaccountdepositnetamount",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            // Details
            this.gridAg.enableMasterDetail(false);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                params.successCallback(params.data['employees']);
            });

            this.gridAg.detailOptions.addColumnText("employeeNr", this.terms["time.employee.employee.employeenr"], 100);
            this.gridAg.detailOptions.addColumnText("name", this.terms["common.name"], 100);
            this.gridAg.detailOptions.addColumnText("accountNrGridStr", this.terms["time.payroll.payment.accountnr"], 100);
            this.gridAg.detailOptions.addColumnNumber("netAmount", terms["time.payroll.payment.netamount"], 100, { decimals: 2 });
            if (this.useExtendedNOK) {
                this.gridAg.detailOptions.addColumnNumber("netAmountCurrency", terms["time.payroll.payment.currencynetamount"], 100, { decimals: 2 });
            }
            this.gridAg.detailOptions.enableFiltering = false;
            this.gridAg.detailOptions.enableRowSelection = false;
            this.gridAg.detailOptions.finalizeInitGrid();

            // Master
            let colDef = this.gridAg.addColumnDate("exportDate", terms["time.export.salary.exportdate"], 100);
            colDef.cellRenderer = 'agGroupCellRenderer';
            this.gridAg.addColumnText("timePeriodHeadName", terms["time.time.timeperiod.timeperiodhead"], null);
            this.gridAg.addColumnText("timePeriodName", terms["time.time.timeperiod.timeperiod"], 100);
            this.gridAg.addColumnText("payrollDateInterval", terms["time.payroll.payment.payrolldateinterval"], 120);
            if(this.exportType == TermGroup_TimeSalaryPaymentExportType.ISO20022)
                this.gridAg.addColumnDate("debitDate", terms["time.payroll.payment.debitdate"], 75);
            this.gridAg.addColumnDate("paymentDate", terms["time.payroll.payment.paymentdate"], 75);            
            this.gridAg.addColumnDate("salarySpecificationPublishDate", terms["time.payroll.payment.salaryspecificationpublishdate"], 80);
            this.gridAg.addColumnNumber("accountDepositNetAmount", terms["time.payroll.payment.accountdepositnetamount"], 75, { decimals: 2 });
            if (this.useExtendedNOK) {
                this.gridAg.addColumnNumber("accountDepositNetAmountCurrency", terms["time.payroll.payment.currencyaccountdepositnetamount"], 85, { decimals: 2 });
            }

            this.gridAg.addColumnNumber("cashDepositNetAmount", terms["time.payroll.payment.cashdepositnetamount"], 75, { decimals: 2 });
            this.gridAg.addColumnText("typeName", terms["time.export.salary.targetname"], 60);

            if (this.useExtendedNOK) {
                this.gridAg.addColumnText("currencyCode", terms["common.customer.invoices.currencycode"], 60);
                this.gridAg.addColumnNumber("currencyRate", terms["time.payroll.payment.currencyrate"], 60, { decimals: 4 });
                this.gridAg.addColumnDate("currencyDate", terms["time.payroll.payment.currencydate"], 75);
            }

            this.gridAg.addColumnIcon("file", null, 50, { icon: "fal fa-download", toolTip: terms["time.payroll.payment.file.tooltip"], onClick: this.downloadFile.bind(this) });
            this.gridAg.addColumnDelete(terms["time.export.salary.delete.tooltip"], this.initDelete.bind(this), null, null, "fal fa-undo iconDelete");

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.IsRowMaster, (rowNode) => {
                return rowNode ? rowNode.employees.length > 0 : false;
            }));

            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: uiGrid.IGridRow) => {
                this.hasSelectedPayments = (Array.isArray(rows) && rows.length > 0);         
                this.setupMenuButton();
                this.$scope.$applyAsync();
            }));

            this.gridAg.options.subscribe(events);
            this.gridAg.finalizeInitGrid("time.payroll.payment.payments",false)
            
            this.doubleClickToEdit = false;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        //this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.exportexcel", "core.exportexcel", IconLibrary.FontAwesome, "fal fa-download", () => {
        //    this.exportPayments(this.gridAg.options.getSelectedRows());
        //},null, () => {
        //    return !this.hasSelectedPayments;
        //})));
    }

    // SERVICE CALLS   

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.PublishPayrollSlipWhenLockingPeriod);
        settingTypes.push(CompanySettingType.SalaryPaymentExportType);
        settingTypes.push(CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK)
        return this.coreService.getCompanySettings(settingTypes).then(x => {

            this.publishPayrollSlipWhenLockingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PublishPayrollSlipWhenLockingPeriod);
            this.exportType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentExportType);
            this.useExtendedNOK = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK);
        });
    }

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getTimeSalaryPaymentExports().then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // EVENTS   

    private exportPayments(rows: TimeSalaryPaymentExportGridDTO[]) {
        
        let headers: string[] = [];        
        headers.push(this.terms["time.employee.employee.employeenrshort"]);        
        headers.push(this.terms["common.name"]);
        headers.push(this.terms["time.payroll.payment.accountnr"]);
        headers.push(this.terms["time.payroll.payment.netamount"].replace('ö','o'));
        if (this.useExtendedNOK) 
            headers.push(this.terms["time.payroll.payment.currencynetamount"].replace('ö', 'o'));
        

        let content: string = headers.join(';') + '\r\n';
        let paymentDates: string[] = [];        
        let fileName: string = this.terms["time.payroll.payment.netamount"].replace('ö', 'o') + " ";
        _.forEach(rows, row => {
            paymentDates.push(row.paymentDate.getFullYear() + "-" + row.paymentDate.getMonth() + "-" + row.paymentDate.getDate());

            _.forEach(row.employees, rowDetails => {
                let rowContent: string[] = [];
                rowContent.push(rowDetails.employeeNr);
                rowContent.push(rowDetails.name);
                rowContent.push(rowDetails.accountNrGridStr + '\'');
                rowContent.push(rowDetails.netAmount.toFixed(2).replace('.', ','));
                if (this.useExtendedNOK) 
                    rowContent.push(rowDetails.netAmountCurrency.toFixed(2).replace('.', ','));
                content += rowContent.join(';') + '\r\n'
            });
        });
        fileName += paymentDates.join(', ');
        
        ExportUtility.ExportToCSV(content,   fileName + '.csv');
    }

    private downloadFile(row: TimeSalaryPaymentExportGridDTO) {
        var uri = window.location.protocol + "//" + window.location.host;
        uri += this.getNavigationUrl(row.timeSalaryPaymentExportId, row.exportDate, "Loneutbetalning", row.exportType);

        window.open(uri, '_blank');
    }

    private initDelete(row: TimeSalaryPaymentExportGridDTO) {
        this.askDelete().then(val => {
            if (val) {
                this.progress.startWorkProgress((completion) => {
                    this.payrollService.deleteSalaryPaymentExport(row.timeSalaryPaymentExportId).then(result => {
                        if (result.success) {
                            completion.completed(null, true);
                            this.reloadData();
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                });
            }
        });
    }

    private askDelete(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var keys: string[] = [
            "time.payroll.payment.delete.question.title",
            "time.payroll.payment.delete.question.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.payroll.payment.delete.question.title"], terms["time.payroll.payment.delete.question.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(val);
            }, (cancel) => {
                deferral.resolve(false);
            });
        });

        return deferral.promise;
    }

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private preSetPublishDate(row: TimeSalaryPaymentExportGridDTO) {   
        var modal = this.notificationService.showDialogEx(this.terms["time.payroll.payment.setsalaryspecificationpublishdate"], "", SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showDatePicker: true, datePickerLabel: this.terms["time.payroll.payment.setsalaryspecificationpublishdate"], datePickerValue: row.salarySpecificationPublishDate  });
        modal.result.then(val => {
            modal.result.then(result => {
                if (result.result) {
                    this.progress.startWorkProgress((completion) => {
                        this.payrollService.setSalarySpecificationPublishDate(row.timeSalaryPaymentExportId, result.datePickerValue).then(res => {
                            if (res.success) {
                                completion.completed(null, true);
                                this.reloadData();
                            } else {
                                completion.failed(res.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    });                   
                }
            });
        });
    
    }

    // HELP-METHODS
    private setupMenuButton() {
        this.buttonFunctions = [];
        
        if (this.hasSelectedPayments) {
            if (this.gridAg.options.getSelectedRows().length == 1 && !this.publishPayrollSlipWhenLockingPeriod)
                this.buttonFunctions.push({ id: 1, name: this.terms["time.payroll.payment.setsalaryspecificationpublishdate"] });
            this.buttonFunctions.push({ id: 2, name: this.terms["core.exportexcel"] });
            if (this.useExtendedNOK)
                this.buttonFunctions.push({ id: 3, name: this.terms["time.payroll.payment.createextendedfilenorway"] });
        }
    }

    private executeButtonFunction(option) {        
        if (this.gridAg.options.getSelectedRows().length == 0)
            return;

        switch (option.id) {
            case 1://Set publishdate
                this.preSetPublishDate(this.gridAg.options.getSelectedRows()[0]);
                break;
            case 2:
                this.exportPayments(this.gridAg.options.getSelectedRows());
                break;   
            case 3:
                this.openExtendedExportNOK(this.gridAg.options.getSelectedRows());
                break;  
        }
    }

    private getNavigationUrl(timeSalaryExportId: number, exportDate: Date, fileType: string, exportTarget: number): string {
        var clientName: string = "{0}_{1}_{2}{3}{4}_{5}{6}{7}".format(
            soeConfig.companyShortName,
            fileType,
            exportDate.getFullYear().toString(),
            (exportDate.getMonth() + 1).toString().padLeft(2, '0'),
            exportDate.getDate().toString().padLeft(2, '0'),
            exportDate.getHours().toString().padLeft(2, '0'),
            exportDate.getMinutes().toString().padLeft(2, '0'),
            exportDate.getSeconds().toString().padLeft(2, '0'));

        var url: string = "/soe/time/payroll/payment/default.aspx";
        url += "?c={0}&type={1}&timeSalaryPaymentExportId={2}&clientname={3}".format(
            CoreUtility.actorCompanyId.toString(),
            exportTarget.toString(),
            timeSalaryExportId.toString(),
            clientName);

        return url;
    }
    protected openExtendedExportNOK(selection: any[]) {
        if (selection.length == 0 || selection.length > 1)
            return;
        
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getUrl("Dialogs/ExtendedExport/ExtendedExportDialog.html"),
            controller: ExtendedExportDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                selection: () => { return selection[0] },
                currency: () => { return TermGroup_Currency.NOK },
            }
        });

        modal.result.then(result => {
            if (result && result.created) {
                this.reloadData();
                this.hasSelectedPayments = false;
            }
        });
    }
}