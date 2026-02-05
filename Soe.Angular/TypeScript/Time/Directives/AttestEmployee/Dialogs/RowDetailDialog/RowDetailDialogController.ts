import { AttestEmployeeDayDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { IToolbar } from "../../../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../../../Core/Handlers/ToolbarFactory";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../../../../Util/ToolBarUtility";

export class RowDetailDialogController {

    private toolbar: IToolbar;
    private isDirty: boolean;
    private terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private toolbarFactory: IToolbarFactory,
        private days: AttestEmployeeDayDTO[],
        private data: AttestEmployeeDayDTO,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private showMultipleDays: boolean) {
    }
    
    $onInit() {
        this.loadTerms();
        this.onCreateToolbar();
    }
    private loadTerms() {
            var keys: string[] = [
                "core.warning",
                "core.confirmonleave",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.terms = terms;
            });
    }
    private onCreateToolbar() {
        this.toolbar = this.toolbarFactory.createEmpty();
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.close", "core.close", IconLibrary.FontAwesome, "fa-times", () => this.cancel())));

        if (this.showMultipleDays) {
            let expCollGroup: ToolBarButtonGroup = ToolBarUtility.createGroup(new ToolBarButton("core.expandall", "core.expandall", IconLibrary.FontAwesome, "fa-angle-double-down", () => { this.expandAll(); }));
            expCollGroup.buttons.push(new ToolBarButton("core.collapseall", "core.collapseall", IconLibrary.FontAwesome, "fa-angle-double-up", () => { this.collapseAll(); }));
            this.toolbar.addButtonGroup(expCollGroup);
        } else {
            this.toolbar.setupNavigationRecordDates(this.days.map(d => d.date), this.data.date, date => {
                if (date) {
                    date = CalendarUtility.convertToDate(date);
                    if (!date.isSameDayAs(this.data.date)) {
                        if (this.isDirty) {
                            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["core.confirmonleave"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                            modal.result.then(val => {
                                if (val != null && val === true) {
                                    this.data = this.days.find(d => d.date.isSameDayAs(date));
                                    this.$scope.$broadcast('DateChanged', this.data);
                                } 
                            });
                        } else {
                            this.data = this.days.find(d => d.date.isSameDayAs(date));
                            this.$scope.$broadcast('DateChanged', this.data);
                        }
                    }
                }
            });
        }
    }

    private expandAll() {
        this.$scope.$broadcast('ExpandAllDays');
    }

    private collapseAll() {
        this.$scope.$broadcast('CollapseAllDays');
    }

    private cancel() {
        if (this.isDirty) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["core.confirmonleave"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.$uibModalInstance.close();
                }
            });
           
        } else {
            this.$uibModalInstance.close();
        }
       
    }
}
