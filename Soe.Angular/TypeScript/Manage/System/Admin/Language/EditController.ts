import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, TermGroup_Languages } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IFieldSettingDTO, ISmallGenericType, ISysTermDTO } from "../../../../Scripts/TypeLite.Net4";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISystemService } from "../../SystemService";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { lang } from "moment";
import { SysTermDTO } from "../../../../Common/Models/SysTermDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Data
    private items: SysTermDTO[];

    //Lookups
    private terms: { [index: string]: string; };
    private termGroups: ISmallGenericType[];
    private primaryLanguages: ISmallGenericType[];
    private secondaryLanguages: ISmallGenericType[];

    // Grid
    protected fromGridOptions: ISoeGridOptionsAg;
    protected toGridOptions: ISoeGridOptionsAg;

    // Properties
    protected selectedTermGroup: any;
    protected selectedDateFrom: Date;

    private _selectedPrimaryLanguage: number;
    get selectedPrimaryLanguage() {
        return this._selectedPrimaryLanguage;
    }
    set selectedPrimaryLanguage(value: any) {
        this._selectedPrimaryLanguage = value;

        this.secondaryLanguages = _.filter(this.primaryLanguages, (i) => i.id !== this._selectedPrimaryLanguage);

        var resetSecondary: boolean = false;
        if (this._selectedPrimaryLanguage === this._selectedSecondaryLanguage) {
            this._selectedSecondaryLanguage = this.secondaryLanguages[0].id;
            resetSecondary = true;
        }

        if (this._selectedPrimaryLanguage)
            this.setData(true, resetSecondary);
    }

    private _selectedSecondaryLanguage: number;
    get selectedSecondaryLanguage() {
        return this._selectedSecondaryLanguage;
    }
    set selectedSecondaryLanguage(value: any) {
        this._selectedSecondaryLanguage = value;

        if (this._selectedSecondaryLanguage)
            this.setData(false, true);
    }

    private _onlyNotTranslated: number;
    get onlyNotTranslated() {
        return this._onlyNotTranslated;
    }
    set onlyNotTranslated(value: any) {
        this._onlyNotTranslated = value;
        this.setData(true, true);
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private systemService: ISystemService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this._selectedPrimaryLanguage = TermGroup_Languages.Swedish;
        this._selectedSecondaryLanguage = TermGroup_Languages.English;

        this.selectedDateFrom = CalendarUtility.getDateToday();

        this.fromGridOptions = new SoeGridOptionsAg("FromLanguageGrid", this.$timeout);
        this.toGridOptions = new SoeGridOptionsAg("ToLanguageGrid", this.$timeout);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_FieldSettings_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_FieldSettings_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_FieldSettings_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    // LOOKUPS
    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadLanguages(),
            () => this.loadTerms(),
            () => this.loadTermGroups(),
        ]).then(() => {
            this.termGroups.splice(0, 0, new SmallGenericType(-1, this.terms["common.all"]));
            this.setupGrid();
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, false, true).then((x) => {
            this.primaryLanguages = x;
            this.secondaryLanguages = _.filter(x, (i) => i.id !== TermGroup_Languages.Swedish);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all",
            "common.name",
            "manage.admin.language.translationkey",
            "common.created",
            "common.modified",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadTermGroups(): ng.IPromise<any> {
        return this.systemService.getSysTermGroups().then((x) => {
            this.termGroups = x;
        });
    }

    private setupGrid() {

        this.fromGridOptions.enableGridMenu = false;
        this.fromGridOptions.enableRowSelection = false;
        this.fromGridOptions.setMinRowsToShow(20);

        this.fromGridOptions.addColumnIsModified("isModified", "", 20);
        this.fromGridOptions.addColumnNumber("sysTermId", "TermId", null, { editable: false });
        this.fromGridOptions.addColumnNumber("sysTermGroupId", "TermGruppId", null, { editable: false });
        this.fromGridOptions.addColumnText("name", this.terms["common.name"], null, { editable: true });
        this.fromGridOptions.addColumnText("translationKey", this.terms["manage.admin.language.translationkey"], null, { editable: true });
        this.fromGridOptions.addColumnDateTime("created", this.terms["common.created"], null);
        this.fromGridOptions.addColumnDateTime("modified", this.terms["common.modified"], null);
        //this.fromGridOptions.addColumnIcon(null, this.terms["manage.admin.language.suggestion"], null, { icon: "fal fa-search", onClick: this.getPrimarySuggestion.bind(this), showIcon: this.showSuggestionIcon.bind(this) });

        this.fromGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        var fromEvents: GridEvent[] = [];
        fromEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue, true); }));

        this.fromGridOptions.subscribe(fromEvents);

        this.toGridOptions.enableGridMenu = false;
        this.toGridOptions.enableRowSelection = false;
        this.toGridOptions.setMinRowsToShow(20);

        this.toGridOptions.addColumnIsModified("isModified", "", 20);
        this.toGridOptions.addColumnNumber("sysTermId", "TermId", null, { editable: false });
        this.toGridOptions.addColumnNumber("sysTermGroupId", "TermGruppId", null, { editable: false });
        this.toGridOptions.addColumnText("name", this.terms["common.name"], null, { editable: true });
        this.toGridOptions.addColumnText("translationKey", this.terms["manage.admin.language.translationkey"], null, { editable: true });
        this.toGridOptions.addColumnDateTime("created", this.terms["common.created"], null);
        this.toGridOptions.addColumnDateTime("modified", this.terms["common.modified"], null);
        this.toGridOptions.addColumnIcon(null, this.terms["manage.admin.language.suggestion"], null, { icon: "fal fa-search", onClick: this.getSecondarySuggestion.bind(this), showIcon: this.showSuggestionIcon.bind(this) });

        this.toGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        var toEvents: GridEvent[] = [];
        toEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue, false); }));

        this.toGridOptions.subscribe(toEvents);

        this.fromGridOptions.finalizeInitGrid();
        this.toGridOptions.finalizeInitGrid();
    }

    private showSuggestionIcon(row: any) {
        var primaryItem = _.find(this.items, (item) => (item.langId === this.selectedPrimaryLanguage && item.sysTermGroupId === row.sysTermGroupId && item.sysTermId === row.sysTermId));
        return (!row.name || row.name.length === 0) && primaryItem && primaryItem.name && primaryItem.name.length > 0;
    }

    private setData(primary: boolean, secondary: boolean) {
        if (this.onlyNotTranslated) {
            var filteredItems: SysTermDTO[] = [];
            if (secondary) {
                if (this.selectedSecondaryLanguage && this.selectedSecondaryLanguage > 0) {
                    filteredItems = _.filter(this.items, { 'langId': this.selectedSecondaryLanguage, 'notTranslated': true });
                    this.toGridOptions.setData(filteredItems);
                }
            }
            if (primary && filteredItems.length > 0) {
                if (this.selectedPrimaryLanguage && this.selectedPrimaryLanguage > 0) {
                    var filteredItems2 = this.items.filter(x => x.langId === this.selectedPrimaryLanguage && filteredItems.filter(t => t.sysTermId === x.sysTermId && t.sysTermGroupId === x.sysTermGroupId).length > 0);
                    this.fromGridOptions.setData(filteredItems2);
                    //this.fromGridOptions.setData(_.filter(this.items, (item) => (item.langId === this.selectedPrimaryLanguage && _.includes(filteredIds, item.sysTermId))));
                }
            }
        }
        else {
            if (primary) {
                if (this.selectedPrimaryLanguage && this.selectedPrimaryLanguage > 0)
                    this.fromGridOptions.setData(_.filter(this.items, { 'langId': this.selectedPrimaryLanguage }));
            }
            if (secondary) {
                if (this.selectedSecondaryLanguage && this.selectedSecondaryLanguage > 0)
                    this.toGridOptions.setData(_.filter(this.items, { 'langId': this.selectedSecondaryLanguage }));
            }
        }
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue, from: boolean) {
        if (newValue === oldValue)
            return;

        row.isModified = true;
        this.$timeout(() => {
            if (from)
                this.fromGridOptions.refreshRows(row);
            else
                this.toGridOptions.refreshRows(row);
        }, 10);
    }

    // ACTIONS
    public search() {
        this.progress.startLoadingProgress([
            () => {
                return this.systemService.getSysTerms(this.selectedTermGroup < 0 ? 0 : this.selectedTermGroup, 0).then((x: SysTermDTO[]) => {
                    this.items = [];
                    const groups = _.groupBy(x, s => s.sysTermId+ "_" + s.sysTermGroupId);
                    _.forEach(groups, (group) => {
                        _.forEach(this.primaryLanguages, (l) => {
                            var item: SysTermDTO = _.find(group, { 'langId': l.id });
                            if (item) {
                                item['notTranslated'] = (item.name.length === 0);
                                this.items.push(item);
                            }
                            else {
                                var newItem = new SysTermDTO();
                                newItem.sysTermGroupId = group[0].sysTermGroupId;
                                newItem.sysTermId = group[0].sysTermId;
                                newItem.langId = l.id;
                                newItem.translationKey = group[0].translationKey ? group[0].translationKey : "";
                                newItem.name = "";
                                newItem['notTranslated'] = true;
                                
                                this.items.push(newItem);
                            }
                        });
                    });

                // Set data for both grids
                this.setData(true, true);
                });
            }
        ]);
    }

    public getPrimarySuggestion(row: any) {
        var primaryItem = _.find(this.items, { 'sysTermId': row.sysTermId, 'sysTermGroupId': row.sysTermGroupId, 'langId': this.selectedSecondaryLanguage });
        if (primaryItem) {
            this.progress.startLoadingProgress([
                () => {
                    return this.systemService.getSysTermSuggestion(primaryItem.name, this.selectedSecondaryLanguage, this.selectedPrimaryLanguage).then((x) => {
                        if (x && x.length > 0) {
                            row.name = x;
                            row.isModified = true;
                            this.fromGridOptions.refreshRows(row);
                        }
                    });
                }
            ]);
        }
    }

    public getSecondarySuggestion(row: any) {
        var primaryItem = _.find(this.items, { 'sysTermId': row.sysTermId, 'sysTermGroupId': row.sysTermGroupId, 'langId': this.selectedPrimaryLanguage });
        if (primaryItem) {
            this.progress.startLoadingProgress([
                () => {
                    return this.systemService.getSysTermSuggestion(primaryItem.name, this.selectedPrimaryLanguage, this.selectedSecondaryLanguage).then((x) => {
                        if (x && x.length > 0) {
                            row.name = x;
                            row.isModified = true;
                            this.toGridOptions.refreshRows(row);
                        }
                    });
                }
            ]);
        }
    }

    public next() {

    }

    public previous() {

    }

    public save() {
        this.fromGridOptions.stopEditing(false);
        this.toGridOptions.stopEditing(false);
        this.$timeout(() => {
            var itemsToSave = _.filter(this.items, { 'isModified': true });
            if (itemsToSave.length > 0) {
                this.progress.startSaveProgress((completion) => {
                    
                    this.systemService.saveSysTerms(itemsToSave).then((result) => {
                        if (result.success) {
                            completion.completed(Constants.EVENT_EDIT_SAVED);
                        } else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, this.guid)
                .then(data => {
                    this.search();
                }, error => {
                });
            }
        }, 10);
    }

    public saveNext() {
    }
}