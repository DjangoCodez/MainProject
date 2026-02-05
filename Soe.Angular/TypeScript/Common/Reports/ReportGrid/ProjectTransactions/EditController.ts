import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { ISmallGenericType, IProjectGridDTO, IReportDTO } from "../../../../Scripts/TypeLite.Net4";
import { ProjectGridDTO } from "../../../Models/ProjectDTO";
import { ReportDTO, ProjectTransactionsReportDTO } from "../../../Models/ReportDTOs";
import { Feature, TermGroup, SoeReportTemplateType, SoeCategoryType } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    reportId: number;
    report: IReportDTO;
    selectionName: string;
    exportTypes: ISmallGenericType[];
    
    exportType: number;
    projectList: IProjectGridDTO[];
    selectedList: IProjectGridDTO[];
    statusDef: any;
    includeChildProjects: boolean;

    dim2: any[];
    dim3: any[];
    dim4: any[];
    dim5: any[];
    dim6: any[];

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;
    private projectStopDate: Date;
    private projectStatus: any[] = [];
    private projectCategory: any[] = [];
    private withoutStopDate = false;

    private selectedProjectStatusDict: any[] = [];
    private selectedCategoryDict: any[] = [];

    //Input values
    private payrollProductNrFrom = "";
    private payrollProductNrTo = "";
    private invoiceProductNrFrom = "";
    private invoiceProductNrTo = "";
    private payrollTransactionDateFrom: Date = null;
    private payrollTransactionDateTo: Date = null;
    private invoiceTransactionDateFrom: Date = null;
    private invoiceTransactionDateTo: Date = null;
    private offerNrFrom = "";
    private offerNrTo = "";
    private orderNrFrom = "";
    private orderNrTo = "";
    private invoiceNrFrom = "";
    private invoiceNrTo = "";
    private employeeNrFrom = "";
    private employeeNrTo = "";
    private projectIds: number[] = [];
    private dim2Id = 0;
    private dim2From = "";
    private dim2To = "";
    private dim3Id = 0;
    private dim3From = "";
    private dim3To = "";
    private dim4Id = 0;
    private dim4From = "";
    private dim4To = "";
    private dim5Id = 0;
    private dim5From = "";
    private dim5To = "";
    private dim6Id = 0;
    private dim6From = "";
    private dim6To = "";

    dim2Header: string;
    dim3Header: string;
    dim4Header: string;
    dim5Header: string;
    dim6Header: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private reportService: IReportService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $window: ng.IWindowService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData());

        this.soeGridOptions = new SoeGridOptionsAg("common.report.selection.projectlist", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(15);

        this.projectList = new Array<ProjectGridDTO>();

    }

    public onInit(parameters: any) {
        this.reportId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Economy_Distribution_Reports_Selection, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Distribution_Reports_Selection].readPermission;
        this.modifyPermission = response[Feature.Economy_Distribution_Reports_Selection].modifyPermission;
    }

    private onSetupGui(): ng.IPromise<any> {
        return this.$q.all([
            this.setupProjectGrid(),
        ]);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadStatus(),
            () => this.loadCategories(),
            () => this.loadExportTypes(),
            () => this.loadAccountDims(),
            () => this.setupProjectGrid(),
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadReport(),
        ]);
    }

    private loadReport(): ng.IPromise<ReportDTO> {
        const deferral = this.$q.defer<ReportDTO>();

        if (this.reportId > 0) {
            this.reportService.getReportForPrint(this.reportId, true, true, false, true, false).then((x) => {
                this.isNew = false;
                this.report = x;
                deferral.resolve();
            });
        } else {
            this.isNew = true;
            this.reportId = 0;
            this.report = new ReportDTO();

            deferral.resolve();
        }

        return deferral.promise;
    }

    private searchProjects(): ng.IPromise<any> {
        const categories: number[] = [];
        this.selectedCategoryDict.forEach(o => {
            categories.push(o.id);
        });
        const statuses: number[] = [];
        this.selectedProjectStatusDict.forEach(o => {
            statuses.push(o.id);
        });

        return this.progress.startLoadingProgress([
            () => this.reportService.getProjectsBySearch(true, statuses, categories, this.projectStopDate, this.withoutStopDate).then((x) => {
                this.projectList = x;
                this.soeGridOptions.setData(this.projectList);
            })
        ]);
    }

    private loadExportTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ReportExportType, false, false).then(x => {
            this.exportTypes = x;
        });
    }

    private loadStatus(): ng.IPromise<any> {
        if (this.projectStatus.length === 0) {
            return this.coreService.getTermGroupContent(TermGroup.ProjectStatus, false, false).then((data: any[]) => {
                data.forEach((x) => {
                    this.projectStatus.push({ id: x.id, label: x.name });
                });
            });
        }
    }

    private loadCategories(): ng.IPromise<any> {
        if (this.projectCategory.length === 0) {
            return this.coreService.getCategories(SoeCategoryType.Project, false, false, false, true).then((categories: any[]) => {
                categories.forEach( (c) => {
                    this.projectCategory.push({ id: c.categoryId, label: c.name });
                })
            });
        }
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, true, true, false, false).then(x => {
                let counter: number = 1;
                _.forEach(x, y => {
                    switch (counter) {
                        case 1:
                            counter = counter + 1;
                            break;
                        case 2:
                            this.dim2Header = y.name;
                            this.dim2Id = y.accountDimId;
                            this.dim2 = [];
                            this.dim2.push({ id: " ", name: " " });
                            _.forEach(y.accounts, (z: any) => {
                                this.dim2.push({ id: z.accountNr, name: z.numberName })
                            });

                            counter = counter + 1;
                            break;
                        case 3:
                            this.dim3Header = y.name;
                            this.dim3Id = y.accountDimId;
                            this.dim3 = [];
                            this.dim3.push({ id: " ", name: " " });
                            _.forEach(y.accounts, (z: any) => {
                                this.dim3.push({ id: z.accountNr, name: z.numberName })
                            });

                            counter = counter + 1;
                            break;
                        case 4:
                            this.dim4Header = y.name;
                            this.dim4Id = y.accountDimId;
                            this.dim4 = [];
                            this.dim4.push({ id: " ", name: " " });
                            _.forEach(y.accounts, (z: any) => {
                                this.dim4.push({ id: z.accountNr, name: z.numberName })
                            });

                            counter = counter + 1;
                            break;
                        case 5:
                            this.dim5Header = y.name;
                            this.dim5Id = y.accountDimId;
                            this.dim5 = [];
                            this.dim5.push({ id: " ", name: " " });
                            _.forEach(y.accounts, (z: any) => {
                                this.dim5.push({ id: z.accountNr, name: z.numberName })
                            });

                            counter = counter + 1;
                            break;
                        case 6:
                            this.dim6Header = y.name;
                            this.dim6Id = y.accountDimId;
                            this.dim6 = [];
                            this.dim6.push({ id: " ", name: " " });
                            _.forEach(y.accounts, (z: any) => {
                                this.dim6.push({ id: z.accountNr, name: z.numberName })
                            });

                            counter = counter + 1;
                            break;
                    }
                });

            });
    }

    private setupProjectGrid(): ng.IPromise<any> {

        const keys: string[] = [
            "common.report.selection.projectnr",
            "common.report.selection.projectname",
            "common.report.selection.projectleader",
            "common.report.selection.customernr",
            "common.report.selection.customername",
            "common.report.selection.projectstatus",
            "common.stopdate",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addColumnText("statusName", terms['common.report.selection.projectstatus'], null);
            this.soeGridOptions.addColumnText("number", terms['common.report.selection.projectnr'], null);
            this.soeGridOptions.addColumnText("name", terms['common.report.selection.projectname'], null);
            this.soeGridOptions.addColumnText("managerName", terms['common.report.selection.projectleader'], null);
            this.soeGridOptions.addColumnText("customerNr", terms["common.report.selection.customernr"], null);
            this.soeGridOptions.addColumnText("customerName", terms['common.report.selection.customername'], null);
            this.soeGridOptions.addColumnDate("stopDate", terms['common.stopdate'], null);

            this.soeGridOptions.finalizeInitGrid();

            this.$timeout(() => {
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: terms["core.aggrid.totals.filtered"],
                    total: terms["core.aggrid.totals.total"],
                    selected: terms["core.aggrid.totals.selected"]
                });
            });
        });
    }

    private print() {
        this.projectIds = [];
        _.forEach(this.soeGridOptions.getSelectedRows(), (x) => {
            this.projectIds.push(x.projectId);
        });

        const reportItem = new ProjectTransactionsReportDTO(this.report.reportId, SoeReportTemplateType.ProjectTransactionsReport, this.report.exportType, this.projectIds,
            this.offerNrFrom, this.offerNrTo, this.orderNrFrom, this.orderNrTo, this.invoiceNrFrom, this.invoiceNrTo,
            this.employeeNrFrom, this.employeeNrTo, this.payrollProductNrFrom, this.payrollProductNrTo, this.invoiceProductNrFrom, this.invoiceProductNrTo,
            this.payrollTransactionDateFrom, this.payrollTransactionDateTo, this.invoiceTransactionDateFrom, this.invoiceTransactionDateTo,
            this.includeChildProjects, this.dim2Id, this.dim2From, this.dim2To,
            this.dim3Id, this.dim3From, this.dim3To, this.dim4Id, this.dim4From, this.dim4To, this.dim5Id, this.dim5From, this.dim5To, this.dim6Id, this.dim6From, this.dim6To);

        this.reportService.getProjectTransactionsPrintUrl(reportItem)
            .then((url) => {
                HtmlUtility.openInSameTab(this.$window, url);
            });
    }
}