import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeProjectRecordType } from "../../../Util/CommonEnumerations";
import { OrderEditProjectFunctions, SOEMessageBoxButton, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IOrderService } from "../Orders/OrderService";

export class ProjectHelper {
    public projectId: number;
    public showProjectsWithoutCustomer = false;
    public projectNotSaved = true;
    public showAllProjects: boolean = false;

    public get projectCentralLink(): any {
        return "/soe/billing/project/central/?project=" + this.projectId;
    }
    public set projectCentralLink(value: any) {}

    //@ngInject
    constructor(private parent: EditControllerBase2,
        private orderService: IOrderService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private $window: ng.IWindowService,
        private $uibModal: any,
        private projectChanged: (projectId: number, projectNumber: string) => void,
    ) {

    }

    public executeProjectFunction(option) {
        console.log("executeProjectFunction", option);
        switch (option.id) {
            //case OrderEditProjectFunctions.Create:
            //    this.openNewProject();
            //    break;
            case OrderEditProjectFunctions.Link:
                this.openSelectProject(option.actorId);
                break;
            case OrderEditProjectFunctions.Change:
                this.openSelectProject(option.actorId);
                break;
            //case OrderEditProjectFunctions.Remove:
            //    this.removeProject();
                break;
            case OrderEditProjectFunctions.OpenProjectCentral:
                this.openProjectCentral();
                break;
        }
    }

    private openProjectCentral() {
        if (!this.projectId)
            return;

        HtmlUtility.openInSameTab(this.$window, "/soe/billing/project/central/?project=" + this.projectId);
    }

    private openSelectProject(actorId) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectProject", "selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                projects: () => { return null },
                customerId: () => { return actorId },
                projectsWithoutCustomer: () => { return this.showProjectsWithoutCustomer },
                showFindHidden: () => { return null },
                loadHidden: () => { return false },
                useDelete: () => { return false },
                currentProjectNr: () => { return null },
                currentProjectId: () => { return null },
                excludedProjectId: () => { return null },
                showAllProjects: () => { return this.showAllProjects },
            }
        });

        modal.result.then(project => {
            const projectId: number = (project ? project.projectId : 0);
            const projectNumber: string = (project ? project.number : "");
            this.projectChanged(projectId, projectNumber);
        });

    }

    public changeProject(recordId: number, record: any, recordType: SoeProjectRecordType, projectId: number, overwriteDefaultDimes): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        
        if (recordId) {
            this.parent.progress.startSaveProgress((completion) => {
                this.orderService.changeProjectOnInvoice(projectId, recordId, recordType, overwriteDefaultDimes).then(result => {
                    if (result.success) {
                        completion.completed("", record);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, this.parent.guid).then(() => deferral.resolve(true), () => deferral.resolve(false));
        }
        else {
            deferral.resolve(false);
        }
        return deferral.promise;
    }
}