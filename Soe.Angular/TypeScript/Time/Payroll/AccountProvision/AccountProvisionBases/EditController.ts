import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../Util/SoeGridOptions";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { IPayrollService } from "../../PayrollService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { Feature, TermGroup_TimePeriodType } from "../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase {

    // Data
    timePeriodAccountValues: any;
    timePeriodHeadId: number;
    timePeriodId: number;
    selectedTimePeriodHeadId: number;
    selectedTimePeriodId: number;
    isLocked: boolean = true;

    // Lookups
    timePeriodHeads: ISmallGenericType[];
    timePeriods: any;

    // Subgrid
    protected gridOptions: ISoeGridOptions;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    constructor(
        private timePeriodAccountValueId: number,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        $uibModal,
        coreService: ICoreService,
        private payrollService: IPayrollService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Payroll.AccountProvision.AccountProvisionBases.Edit", Feature.Time_Payroll_Provision_AccountProvisionBase, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
        this.initGrid();
    }

    // SETUP

    protected setupLookups() {
        this.setupGrid(); //must be called after permissions in base class is done   
        this.lookups = 1;
        this.startLoad();
        this.loadTimePeriodHeads();
    }

    private setupToolBar() {
        //this.setupDefaultToolBar();
    }

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Payroll.AccountProvision.AccountProvisionBases", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.showGridFooter = false;
    }

    private setupGrid() {
        var keys: string[] = [
            "time.payroll.accountprovision.accountnr",
            "time.payroll.accountprovision.accountname",
            "time.payroll.accountprovision.accountdesc",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridOptions.addColumnIsModified();
            this.gridOptions.addColumnText("accountNr", terms["time.payroll.accountprovision.accountnr"], "10%");
            this.gridOptions.addColumnText("accountName", terms["time.payroll.accountprovision.accountname"], "10%");
            this.gridOptions.addColumnText("accountDescription", terms["time.payroll.accountprovision.accountdesc"], "10%");

            _.forEach(this.gridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableColumnResizing = false;
                colDef.enableFiltering = false;
                colDef.enableSorting = false;
                colDef.enableColumnMenu = false;
            });
        });
    }

    // LOOKUPS

    private load() {
        this.lookups = 1;
        this.startLoad();
        if (this.timePeriodId > 0) {
            this.payrollService.getAccountProvisionBase(this.timePeriodId).then((x) => {
                this.isNew = false;
                this.timePeriodAccountValues = x;
                if (this.timePeriodAccountValues) {
                    this.gridOptions.setData(this.timePeriodAccountValues);

                    // As default, last column is editable.
                    // If period is locked, disable cell edit on last column.
                    this.isLocked = _.some(this.timePeriodAccountValues, (row: any) => { return row.isLocked; });
                    this.gridOptions.enableCellEdit(this.gridOptions.nbrOfColumns() - 1, !this.isLocked);
                }

                this.lookupLoaded();
            });
        }
        else {
            this.new();
            this.lookupLoaded();
        }
    }

    private loadTimePeriodHeads() {
        this.payrollService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.Payroll, false).then((x) => {
            this.timePeriodHeads = x;
            this.lookupLoaded();
        });
    }

    private loadTimePeriods() {
        this.lookups = 1;
        this.startLoad();
        this.payrollService.getTimePeriodsDict(this.timePeriodHeadId, false).then((x) => {
            this.timePeriods = x;
            this.lookupLoaded();
            // Selecting a period head will set the dirty flag, reset it
            this.isDirty = false;
        });
    }

    private loadColumnNames() {
        this.payrollService.getAccountProvisionBaseColumns(this.timePeriodId).then((columns) => {
            // Remove existing period columns
            if (this.gridOptions.nbrOfColumns() > 4) {
                this.gridOptions.removeColumn(4, 12);
            }

            // Add new columns
            var period = 1;
            _.forEach(columns, (col) => {
                var colDef = this.gridOptions.addColumnNumber("period" + period + "Value", col.toString(), null);
                colDef.enableColumnResizing = false;
                colDef.enableFiltering = false;
                colDef.enableSorting = false;
                colDef.enableColumnMenu = false;
                colDef.enableCellEdit = (period === 12);
                period++;
            });

            // Selecting a period will set the dirty flag, reset it
            this.isDirty = false;

            // Load data
            this.load();
        });
    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.setupToolBar();
        }
    }

    private timePeriodHeadSelected(id) {
        if (this.isDirty) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "time.payroll.accountprovision.changeperiodwarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.accountprovision.changeperiodwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    if (val) {
                        this.changeTimePeriodHead(id);
                    } else {
                        // Revert back to loaded period head
                        this.timePeriodHeadId = this.selectedTimePeriodHeadId;
                    }
                });
            });
        } else {
            this.changeTimePeriodHead(id);
        }
    }

    private timePeriodSelected(id) {
        if (this.isDirty) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "time.payroll.accountprovision.changeperiodwarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.accountprovision.changeperiodwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    if (val) {
                        this.changeTimePeriod(id);
                    } else {
                        // Revert back to loaded period
                        this.timePeriodId = this.selectedTimePeriodId;
                    }
                });
            });
        } else {
            this.changeTimePeriod(id);
        }
    }

    private initLock() {
        // Show verification dialog
        var keys: string[] = [
            "time.payroll.accountprovision.locktitle",
            "time.payroll.accountprovision.lockmessage"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["time.payroll.accountprovision.locktitle"], terms["time.payroll.accountprovision.lockmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.lock();
                }
            });
        });
    }

    private initUnlock() {
        // Show verification dialog
        var keys: string[] = [
            "time.payroll.accountprovision.unlocktitle",
            "time.payroll.accountprovision.unlockmessage"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["time.payroll.accountprovision.unlocktitle"], terms["time.payroll.accountprovision.unlockmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.unlock();
                }
            });
        });
    }

    // ACTIONS

    private save() {
        this.startSave();
        var filteredList = _.filter(this.timePeriodAccountValues, function (o) {
            return o['isModified']
        });

        this.payrollService.saveAccountProvisionBase(filteredList).then((result) => {
            if (result.success) {
                this.completedSave(_.filter(this.timePeriodAccountValues, function (o) {
                    return o['isModified'];
                }));
                this.load();
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    private lock() {
        this.startWork();
        this.payrollService.lockAccountProvisionBase(this.timePeriodId).then((result) => {
            if (result.success) {
                this.completedWork(this.timePeriodId);
                this.load();
            } else {
                this.failedWork(result.errorMessage);
            }
        }, error => {
            this.failedWork(error.message);
        });
    }

    private unlock() {
        this.startWork();
        this.payrollService.unlockAccountProvisionBase(this.timePeriodId).then((result) => {
            if (result.success) {
                this.completedWork(this.timePeriodId);
                this.load();
            } else {
                this.failedWork(result.errorMessage);
            }
        }, error => {
            this.failedWork(error.message);
        });
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.timePeriodAccountValueId = 0;
        this.timePeriodAccountValues = {};
        this.isLocked = true;
    }

    private changeTimePeriodHead(id: number) {
        this.timePeriodHeadId = id;
        // Remember selected TimePeriodHeadId to be able to revert if user cancels a period change
        this.selectedTimePeriodHeadId = id;
        this.loadTimePeriods();
    }

    private changeTimePeriod(id: number) {
        this.timePeriodId = id;
        // Remember selected TimePeriodId to be able to revert if user cancels a period change
        this.selectedTimePeriodId = id;
        this.loadColumnNames();
    }

    // VALIDATION

    protected validate() {
    }
}
