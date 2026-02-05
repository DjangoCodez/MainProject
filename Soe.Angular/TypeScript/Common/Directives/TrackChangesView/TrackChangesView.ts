import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

class TrackChangesViewController extends GridControllerBase2Ag implements ICompositionGridController {

    private entity: number;
    private recordId: number;
    private fromDate: Date;
    private toDate: Date;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Supplier.Supplier.ActorPayment", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())

        this.toDate = CalendarUtility.getDateToday();
        this.fromDate = this.toDate.addMonths(-1);

        this.setupWatches();

        //Feature.Economy_Supplier_Suppliers_Edit
        this.flowHandler.start([{ feature: undefined, loadModifyPermissions: true }]);
    }

    onInit() {

    }

    private setupWatches() {
        this.$scope.$watch(() => this.fromDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() > this.toDate.getTime())) {
                this.toDate = this.fromDate.addDays(6);
            }
            else if ((newValue !== oldValue)) {
                this.reloadGridFromFilter();
            }
        });
        this.$scope.$watch(() => this.toDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() < this.fromDate.getTime())) {
                this.fromDate = this.toDate;
            }
            else if ((newValue !== oldValue)) {
                this.reloadGridFromFilter();
            }
        });
    }

    private setupGrid(): ng.IPromise<any> {
        const keys: string[] = ["common.modifiedby",
            "common.field",
            "common.modified",
            "common.from",
            "common.to",
            "common.type"
        ];

        return this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.addColumnText("columnText", terms["common.field"], null, false, { enableHiding: false });
            this.gridAg.addColumnText("actionText", terms["common.type"], null, false, { enableHiding: false });
            this.gridAg.addColumnText("fromValueText", terms["common.from"], null, false, { enableHiding: false });
            this.gridAg.addColumnText("toValueText", terms["common.to"], null, false, { enableHiding: false });
            this.gridAg.addColumnDateTime("created", terms["common.modified"], null);
            this.gridAg.addColumnText("createdBy", terms["common.modifiedby"], null, false, { enableHiding: false });

            this.gridAg.finalizeInitGrid("common.changehistory", false);
        });
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 500, { leading: false, trailing: true });

    private loadGridData(): ng.IPromise<any> {
        return this.coreService.getTrackChangesLog(this.entity, this.recordId, this.fromDate, this.toDate).then(data => {
            this.gridAg.setData(data);
        });
    }

    private decreaseDate() {
        const diffDays = this.fromDate.diffDays(this.toDate) - 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }

    private increaseDate() {
        const diffDays = this.toDate.diffDays(this.fromDate) + 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }
}

//@ngInject
export function TrackChangesViewDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: 'E',
        templateUrl: urlHelperService.getCommonDirectiveUrl('TrackChangesView', 'TrackChangesView.html'),
        scope: {
            entity: "=",
            recordId: "="
        },
        replace: true,
        controller: TrackChangesViewController,
        controllerAs: "ctrl",
        bindToController: true
    }
}