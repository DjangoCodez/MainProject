import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { CompanySettingType, Feature, SoeTimeSalaryExportTarget } from "../../../Util/CommonEnumerations";
import { ITimeService } from "../../Time/TimeService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { TimeSalaryExportDTO } from "../../../Common/Models/TimeSalaryExportDTOs";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };
    private showCreatedAsPreliminary: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Export.Salary", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Export_Salary, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.SalaryExportAllowPreliminary];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.showCreatedAsPreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryExportAllowPreliminary);
        });
    }    

    private setupGrid() {
        var keys: string[] = [
            "common.from",
            "common.to",
            "common.comment",
            "time.export.salary.exportdate",
            "time.export.salary.targetname",
            "time.export.salary.file1",
            "time.export.salary.file1.tooltip",
            "time.export.salary.file2",
            "time.export.salary.file2.tooltip",
            "time.export.salary.email",
            "time.export.salary.email.tooltip",
            "time.export.salary.send",
            "time.export.salary.send.tooltip",
            "time.export.salary.delete.tooltip",
            "time.export.salary.ispreliminary"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnDate("exportDate", terms["time.export.salary.exportdate"], 75);
            this.gridAg.addColumnDate("startInterval", terms["common.from"], 75);
            this.gridAg.addColumnDate("stopInterval", terms["common.to"], 75);
            this.gridAg.addColumnText("targetName", terms["time.export.salary.targetname"], 150);
            this.gridAg.addColumnIcon("fileOne", terms["time.export.salary.file1"], 100, { icon: "fal fa-download", toolTip: terms["time.export.salary.file1.tooltip"], onClick: this.downloadFile1.bind(this) });
            this.gridAg.addColumnIcon("fileTwo", terms["time.export.salary.file2"], 100, { icon: "fal fa-download", toolTip: terms["time.export.salary.file2.tooltip"], onClick: this.downloadFile2.bind(this), showIcon: this.showFile2Icon.bind(this) });
            this.gridAg.addColumnIcon("email", terms["time.export.salary.email"], 100, { icon: "fal fa-envelope", toolTip: terms["time.export.salary.email.tooltip"], onClick: this.email.bind(this) });
            this.gridAg.addColumnIcon("send", terms["time.export.salary.send"], 100, { icon: "fal fa-paper-plane", toolTip: terms["time.export.salary.send.tooltip"], onClick: this.send.bind(this), showIcon: this.showSendIcon.bind(this) });
            if(this.showCreatedAsPreliminary)
                this.gridAg.addColumnText("isPreliminaryText", terms["time.export.salary.ispreliminary"], 30);

            this.gridAg.addColumnText("comment", terms["common.comment"], 200);
            this.gridAg.addColumnDelete(terms["time.export.salary.delete.tooltip"], this.initDelete.bind(this), null, null, "fal fa-undo iconDelete");

            this.gridAg.options.enableRowSelection = false;            
            this.gridAg.finalizeInitGrid("time.export.salary.exportedsalaries", true);
            this.doubleClickToEdit = false;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS   

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.timeService.getExportedSalaries().then(x => {
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

    private downloadFile1(row: TimeSalaryExportDTO) {
        var uri = window.location.protocol + "//" + window.location.host;
        uri += this.getNavigationUrl(row.timeSalaryExportId, row.exportDate, this.terms["time.export.salary.file1"].replace(" ", "_"), row.exportTarget, true);

        window.open(uri, '_blank');
    }

    private downloadFile2(row: TimeSalaryExportDTO) {
        var uri = window.location.protocol + "//" + window.location.host;
        uri += this.getNavigationUrl(row.timeSalaryExportId, row.exportDate, this.terms["time.export.salary.file2"].replace(" ", "_"), row.exportTarget, false);

        window.open(uri, '_blank');
    }

    private email(row: TimeSalaryExportDTO) {
        this.translationService.translate("time.export.salary.email.progress").then(term => {
            this.progress.startWorkProgress((completion) => {
                this.timeService.sendEmailToPayrollAdministrator(row.timeSalaryExportId).then(result => {
                    if (result.success) {
                        completion.completed(null, true);
                        this.reloadData();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, null, term);
        });
    }

    private send(row: TimeSalaryExportDTO) {
        this.translationService.translate("time.export.salary.send.progress").then(term => {
            this.progress.startWorkProgress((completion) => {
                this.timeService.sendPayrollToSftp(row.timeSalaryExportId).then(result => {
                    if (result.success) {
                        completion.completed(null, true);
                        this.reloadData();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, null, term);
        });
    }

    private initDelete(row: TimeSalaryExportDTO) {
        this.askDelete().then(val => {
            if (val) {
                this.progress.startDeleteProgress((completion) => {
                    this.timeService.deleteTimeSalaryExport(row.timeSalaryExportId).then(result => {
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
            "time.export.salary.delete.question.title",
            "time.export.salary.delete.question.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.export.salary.delete.question.title"], terms["time.export.salary.delete.question.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(val);
            },
                (cancel) => {
                    deferral.resolve(false);
                }
            );
        });

        return deferral.promise;
    }

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    // HELP-METHODS

    private showFile2Icon(row: TimeSalaryExportDTO): boolean {
        return !this.hasOneFile(row.exportTarget);
    }

    private showSendIcon(row: TimeSalaryExportDTO): boolean {
        return row.exportTarget === SoeTimeSalaryExportTarget.SvenskLon || row.exportTarget === SoeTimeSalaryExportTarget.BlueGarden || row.exportTarget === SoeTimeSalaryExportTarget.Pol;
    }

    private hasOneFile(type: SoeTimeSalaryExportTarget): boolean {
        switch (type) {
            case SoeTimeSalaryExportTarget.Hogia214006:
            case SoeTimeSalaryExportTarget.Hogia214007:
            case SoeTimeSalaryExportTarget.KontekLon:
            case SoeTimeSalaryExportTarget.Flex:
            case SoeTimeSalaryExportTarget.AgdaLon:
                return false;
            case SoeTimeSalaryExportTarget.SoftOne:
            case SoeTimeSalaryExportTarget.SoeXe:
            case SoeTimeSalaryExportTarget.Undefined:
            case SoeTimeSalaryExportTarget.Hogia214002:
            case SoeTimeSalaryExportTarget.Personec:
            case SoeTimeSalaryExportTarget.Spcs:
            case SoeTimeSalaryExportTarget.PAxml:
            case SoeTimeSalaryExportTarget.PAxml2_1:
            case SoeTimeSalaryExportTarget.DiLonn:
            case SoeTimeSalaryExportTarget.Tikon:
            case SoeTimeSalaryExportTarget.DLPrime3000:
            case SoeTimeSalaryExportTarget.BlueGarden:
            case SoeTimeSalaryExportTarget.Orkla:
            case SoeTimeSalaryExportTarget.AditroL1:
            case SoeTimeSalaryExportTarget.Pol:
            case SoeTimeSalaryExportTarget.SDWorx:
            default:
                return true;
        }
    }

    private getNavigationUrl(timeSalaryExportId: number, exportDate: Date, fileType: string, exportTarget: number, first: boolean): string {
        var clientName: string = "{0}_{1}_{2}{3}{4}_{5}{6}{7}".format(
            soeConfig.companyShortName,
            fileType,
            exportDate.getFullYear().toString(),
            (exportDate.getMonth() + 1).toString().padLeft(2, '0'),
            exportDate.getDate().toString().padLeft(2, '0'),
            exportDate.getHours().toString().padLeft(2, '0'),
            exportDate.getMinutes().toString().padLeft(2, '0'),
            exportDate.getSeconds().toString().padLeft(2, '0'));

        var url: string = "/soe/time/export/salary/default.aspx";
        url += "?c={0}&type={1}&timeSalaryExportId={2}&clientname={3}&first={4}".format(
            CoreUtility.actorCompanyId.toString(),
            exportTarget.toString(),
            timeSalaryExportId.toString(),
            clientName,
            first.toString());

        return url;
    }
}