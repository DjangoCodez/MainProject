import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../Util/SoeGridOptions";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { ISystemService } from "../../SystemService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IconLibrary } from "../../../../Util/Enumerations";
import { ISysCompanyDTO } from "../../../../Scripts/TypeLite.Net4";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data       
    sysCompany: ISysCompanyDTO;
    countries: any;
    currencies: any;
    syswholesellerTypes: any;
    settingTypes: any[] = [];

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    private sysCompanyId: number = 0;
    public parameters: any;

    //Subgrid
    protected parameterGridOptions: ISoeGridOptions;


    //@ngInject
    constructor(
        $uibModal,
        coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService) {

        super("Time.Employee.SysCompanyDTO.Edit",
            Feature.Manage_System,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
        this.initRowsGrid();
    }

    public init() {
        this.sysCompanyId = this.parameters.id || 0;
        this.$q.all([this.load(), this.loadCountries(), this.loadCurrencies(), this.loadSysCompanyTypes(), this.loadSettingTypes()])
            .then(() => {
                this.setperformanceSettingParameterDTOs();
                this.setupToolBar();
            });
    }

    // SETUP

    private setupToolBar() {
        if (this.setupDefaultToolBar()) {
            this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            })));
        }
        super.stopProgress()
    }

    protected startLoad() { }

    private setSysCompanyId(id: number) {
        this.sysCompanyId = id;
    }

    private initRowsGrid() {
        this.parameterGridOptions = new SoeGridOptions("Setting", this.$timeout, this.uiGridConstants);
        this.parameterGridOptions.enableGridMenu = false;
        this.parameterGridOptions.showGridFooter = false;
        this.parameterGridOptions.setMinRowsToShow(10);
    }

    private setupParameterGrid() {
        var keys: string[] = [
            "common.type",
            "common.id",
            "common.string",
            "common.int",
            "common.decimal",
            "common.bool",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.parameterGridOptions.addColumnSelect("settingType", terms["common.type"], null, this.settingTypes, false, true, "settingTypeName", "value", "label");
            this.parameterGridOptions.addColumnNumber("sysCompanySettingId", terms["common.id"], null, null, null, null, "true");
            this.parameterGridOptions.addColumnText("stringValue", terms["common.string"], null);
            this.parameterGridOptions.addColumnNumber("intValue", terms["common.int"], null);
            this.parameterGridOptions.addColumnNumber("decimalvalue", terms["common.decimal"], null);
            this.parameterGridOptions.addColumnBool("boolValue", terms["common.bool"], null);
            this.parameterGridOptions.addColumnDelete(terms["core.delete"]);

            _.forEach(this.parameterGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableCellEdit = true;
            });



        });
    }

    // SERVICE CALLS

    private loadSysCompanyTypes(): ng.IPromise<any> {
        this.syswholesellerTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.SysCountry, false, false).then(x => {
            this.syswholesellerTypes = x;
        });
    }

    private loadCountries(): ng.IPromise<any> {
        this.countries = [];

        return this.coreService.getTermGroupContent(TermGroup.SysCountry, false, false).then(x => {

            this.countries = x;
        });
    }


    private loadCurrencies(): ng.IPromise<any> {
        this.currencies = [];
        return this.coreService.getTermGroupContent(TermGroup.SysCurrency, false, false).then(x => {
            this.currencies = x;
        });
    }

    private loadSettingTypes(): ng.IPromise<any> {
        this.settingTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.SysCurrency, false, false).then(x => {
            _.forEach(x, (row) => {
                this.settingTypes.push({ value: row.name, label: row.name, id: row.name, id2: row.id });
            });
        });
    }

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.sysCompanyId > 0) {

            this.systemService.getSysCompanyWithId(this.sysCompanyId)
                .then((x) => {
                    this.sysCompany = x;
                    this.isNew = false;
                    deferral.resolve();
                });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    // ACTIONS

    private save() {
        this.startSave();
        this.systemService.saveSysCompany(this.sysCompany).then((result) => {
            if (result.success) {
                if (!this.sysCompanyId) {
                    this.setSysCompanyId(result.integerValue);
                }
            }
            return result;
        },
            error => {
                this.failedSave(error.message);
            }).then(result => {
                if (result.success) {
                    this.completedSave(this.sysCompany);
                    this.closeMe(false);
                } else {
                    this.failedSave(result.errorMessage);
                }
            },
                error => this.failedSave(error.message));
    }

    protected delete() {
        this.failedDelete("Delete not allowed");
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.sysCompanyId = 0;
        this.sysCompany = ({} as ISysCompanyDTO);
    }

    private addRow() {
        var row = {
            sysCompanySettingType: 0,
            id: 0,
            stringValue: "",
            intValue: 0,
            decimalValue: 0,
            boolValue: false,
        };

        this.parameterGridOptions.addRow(row);
    }

    protected deleteRow(row) {
        this.parameterGridOptions.deleteRow(row);
    }

    protected grid_ParameterChanged(row) {
        var obj = (_.filter(this.settingTypes, { parameterType: row.parameterType }))[0];
    }

    private setperformanceSettingParameterDTOs() {
        this.setupParameterGrid();
        if (this.sysCompany && this.sysCompany.sysCompanySettingDTOs) {
            _.forEach(this.sysCompany.sysCompanySettingDTOs, (row) => {
                row['settingTypeName'] = _.find(this.settingTypes, s => s.id2 === row.settingType).label;
            });
            this.parameterGridOptions.setData(this.sysCompany.sysCompanySettingDTOs);

        }
    }

    // VALIDATION

    protected validate() {
        if (this.sysCompany) {
            if (!this.sysCompany.name) {
                this.mandatoryFieldKeys.push("common.name");
            }
        }
    }
}
