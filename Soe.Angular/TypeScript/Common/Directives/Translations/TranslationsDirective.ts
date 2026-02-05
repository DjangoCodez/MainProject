import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CompTermDTO } from "../../Models/CompTermDTO";
import { Feature, SoeEntityState, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";

export class TranslationsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('Translations', 'Translations.html'),
            scope: {
                compTermRecordType: '=',
                recordId: '=',
                compTermRows: '=',
                readOnly: '=?',
                parentGuid: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: TranslationsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class TranslationsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private compTermRecordType: number;
    private recordId: number;
    private readOnly: boolean;
    private parentGuid: string;

    // Collections
    compTermRows: CompTermDTO[] = [];
    sysLanguages: any[] = [];

    //@ngInject
    constructor(controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,

        private $filter: ng.IFilterService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Common.Directives.Translations", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.setGridData())

        this.$scope.$on('stopEditing', (e, a) => {
            this.gridAg.options.stopEditing(false);
        });

        this.onInit();
    }

    public onInit() {
        this.flowHandler.start({
            feature: Feature.None,
            loadReadPermissions: true,
            loadModifyPermissions: true
        })
    }

    private doLookups(): ng.IPromise<any> {
        return this.loadSysLanguages().then(() => this.loadCompTerms());
    }


    private setupGrid() {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = false;
        this.gridAg.options.setMinRowsToShow(5);
        this.gridAg.options.alwaysShowHorizontalScroll = true;

        var keys: string[] = [
            "common.language",
            "common.text",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnSelect("lang", terms["common.language"], null, {
                selectOptions: this.sysLanguages,
                displayField: "langName",
                dropdownValueLabel: "name",
                dropdownIdLabel: "id",
                editable: true
            });
            this.gridAg.addColumnText("name", terms["common.text"], null, null, { editable: true });
            this.gridAg.addColumnDelete(terms["core.delete"], (data) => this.deleteRow(data));
            this.gridAg.finalizeInitGrid("common.translation", false);
        });
    }

    // Lookups
    private loadCompTerms(): ng.IPromise<any> {
        this.compTermRows = [];
        return this.coreService.getTranslations(this.compTermRecordType, this.recordId, true).then(x => {
            this.compTermRows = x;
            this.compTermRows.forEach(r => {
                r.langName = this.sysLanguages.find(s => s.id == r.lang).name || "";
            })
        });
    }

    private loadSysLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, false, true).then(x => {
            this.sysLanguages = x;
        });
    }

    // Actions
    private addRow() {
        const row: CompTermDTO = { ...new CompTermDTO(), recordId: this.recordId, recordType: this.compTermRecordType, state: SoeEntityState.Active, name: "" };
        this.compTermRows.push(row);
        this.messagingHandler.publishEvent(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
        this.setGridData();
    }

    protected deleteRow(row: CompTermDTO) {
        this.compTermRows.forEach(r => {
            if (row.compTermId && r.compTermId == row.compTermId) {
                row.state = SoeEntityState.Deleted;
            }
            else if (row["ag_node_id"] && row["ag_node_id"] == r["ag_node_id"]) {
                this.compTermRows = this.compTermRows.filter(s => s["ag_node_id"] != row["ag_node_id"]);
            }
        })
        this.messagingHandler.publishEvent(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
        this.setGridData();
    }

    private setGridData() {
        this.gridAg.setData(this.compTermRows.filter(r => r.state != SoeEntityState.Deleted));
    }
}