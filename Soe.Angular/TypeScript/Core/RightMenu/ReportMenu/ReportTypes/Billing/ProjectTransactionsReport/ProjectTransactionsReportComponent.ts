import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { ProjectDTO } from "../../../../../../Common/Models/ProjectDTO";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, AccountFilterSelectionDTO, TextSelectionDTO, IdSelectionDTO, IdListSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { ISmallGenericType, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";
import { GridFilterSelectionObj } from "../../../Selections/GridFilterSelection/GridFilterSelectionComponent";

export class ProjectTransactionsReport {
    selectedProject: SmallGenericType;
    public static component(): ng.IComponentOptions {
        return {
            controller: ProjectTransactionsReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/ProjectTransactionsReport/ProjectTransactionsReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "projectTransactionsReport";

    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    private selectableCustomerCategorySorting: ISmallGenericType[];
    selectableCodeList: any[];

    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;
    private project: ProjectDTO;

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private projectReportTitle = "";
    private accountTitle = "Konto- och internkontourval";

    projectsDict: ISmallGenericType[] = [];
    employeesDict: ISmallGenericType[] = [];
    projects: any[] = [];
    projectId: number;

    private projStatus: number[];
    private projectCategorySelection: number[];
    private withoutEndDateSelection: boolean;

    private projectSelections: any;
    dim2: any[];
    dim3: any[];
    dim4: any[];
    dim5: any[];
    dim6: any[];

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
    showDim2: boolean = false;
    dim3Header: string;
    showDim3: boolean = false;
    dim4Header: string;
    showDim4: boolean = false;
    dim5Header: string;
    showDim5: boolean = false;
    dim6Header: string;
    showDim6: boolean = false;

    private projectStopDate: Date;
    private projectStatus: any[] = [];
    private projectCategory: any[] = [];
    private withoutStopDate = false;

    private selecteddim2FromItem: SmallGenericType;
    private selecteddim2ToItem: SmallGenericType;
    private selecteddim3FromItem: SmallGenericType;
    private selecteddim3ToItem: SmallGenericType;
    private selecteddim4FromItem: SmallGenericType;
    private selecteddim4ToItem: SmallGenericType;
    private selecteddim5FromItem: SmallGenericType;
    private selecteddim5ToItem: SmallGenericType;
    private selecteddim6FromItem: SmallGenericType;
    private selecteddim6ToItem: SmallGenericType;

    private includeChildProjectSelected: BoolSelectionDTO;
    private excludeInternalOrdersSelected: BoolSelectionDTO;
    private projectIdList: number[];

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService) {

        this.project = new ProjectDTO();
        this.projectIdList = [];
        this.includeChildProjectSelected = new BoolSelectionDTO(false);
        this.excludeInternalOrdersSelected = new BoolSelectionDTO(false);
        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {

            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
        });

        const keys = [
            "billing.project.central.projectreports",
            "common.report.daterangeselection",
            "common.report.seperatereport",
            "common.report.accountselection",
            "common.report.distributionreport",
            "common.report.standardselection"
        ]

        this.translationService.translateMany(keys).then(terms => {
            this.projectReportTitle = terms["billing.project.central.projectreports"];
        });

    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {
        this.includeChildProjectSelected = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CHILD_PROJECT); 
        this.excludeInternalOrdersSelected = this.userSelection.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_INTERNAL_ORDERS);

        this.selectedDateRange = savedValues.getDateRangeSelectionFromKey(Constants.REPORTMENU_SELECTION_KEY_SELECTED_TRANSACTION_DATERANGE);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM2_FROM) != null)
            this.selecteddim2FromItem = _.find(this.dim2, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM2_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM2_TO) != null)
            this.selecteddim2ToItem = _.find(this.dim2, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM2_TO).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM3_FROM) != null)
            this.selecteddim3FromItem = _.find(this.dim3, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM3_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM3_TO) != null)
            this.selecteddim3ToItem = _.find(this.dim3, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM3_TO).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM4_FROM) != null)
            this.selecteddim4FromItem = _.find(this.dim4, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM4_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM4_TO) != null)
            this.selecteddim4ToItem = _.find(this.dim4, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM4_TO).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM5_FROM) != null)
            this.selecteddim5FromItem = _.find(this.dim5, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM5_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM5_TO) != null)
            this.selecteddim5ToItem = _.find(this.dim5, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM5_TO).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM6_FROM) != null)
            this.selecteddim6FromItem = _.find(this.dim6, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM6_FROM).text);

        if (this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM6_TO) != null)
            this.selecteddim6ToItem = _.find(this.dim6, d => d.id === this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_DIM6_TO).text);

    }

    public $onInit() {
        this.loadAccountDims();
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

                        this.showDim2 = true;
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

                        this.showDim3 = true;
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

                        this.showDim4 = true;
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

                        this.showDim5 = true;
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

                        this.showDim6 = true;
                        counter = counter + 1;
                        break;
                }
            });

            const selectDim2 = new IdSelectionDTO(this.dim2Id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM2_ID, selectDim2);
            const selectDim3 = new IdSelectionDTO(this.dim3Id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM3_ID, selectDim3);

            const selectDim4 = new IdSelectionDTO(this.dim4Id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM4_ID, selectDim4);

            const selectDim5 = new IdSelectionDTO(this.dim5Id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM5_ID, selectDim5);
            const selectDim6 = new IdSelectionDTO(this.dim6Id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM6_ID, selectDim6);
        });

    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SELECTED_TRANSACTION_DATERANGE, dateRange);
    }

    private onDim2FromChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM2_FROM, select);
        }
    }
    private onDim2ToChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM2_TO, select);
        }
    }

    private onDim3FromChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM3_FROM, select);
        }
    }
    private onDim3ToChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM3_TO, select);
        }
    }

    private onDim4FromChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM4_FROM, select);
        }
    }
    private onDim4ToChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM4_TO, select);
        }
    }
    private onDim5FromChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM5_FROM, select);
        }
    }
    private onDim5ToChanged(selection) {
        if (typeof selection.id != "number") {
            var select = new TextSelectionDTO(selection.id);
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DIM5_TO, select);
        }
    }
    private onIncludeChildProjectSelected(selection) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INCLUDE_CHILD_PROJECT, selection);
    }
    private onExcludeInternalOrdersSelected(selection) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EXCLUDE_INTERNAL_ORDERS, selection);
    }
    private onSearchProjects(projectStatusSelection: GridFilterSelectionObj) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_STATUS, projectStatusSelection.projectStatus);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_CATEGORY, projectStatusSelection.projectCategory);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_WITHOUT_STOP_DATE, projectStatusSelection.withoutStop);
    }

    private onSelectedProjectIds(projectIds: IdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SELECTED_PROJECT_IDS, projectIds);
    }
}