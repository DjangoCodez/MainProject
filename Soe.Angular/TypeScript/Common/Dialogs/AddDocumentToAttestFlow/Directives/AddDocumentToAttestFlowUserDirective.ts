import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { TermGroup } from "../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { AttestWorkFlowHeadDTO, AttestWorkFlowRowDTO, AttestWorkFlowTemplateRowDTO } from "../../../Models/AttestWorkFlowDTOs";
import { SmallGenericType } from "../../../Models/SmallGenericType";
import { UserSmallDTO } from "../../../Models/UserDTO";
import { IAddDocumentToAttestFlowService } from "../AddDocumentToAttestFlowService";

export class AddDocumentToAttestFlowUserDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Dialogs/AddDocumentToAttestFlow/Directives/addDocumentToAttestFlowUser.html'),
            scope: {
                row: '=',
                head: '=',
                regUserId: '=?',
                endUserId: '=?',
                showTransitionArrow: '=?',
                registerControl: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AddDocumentToAttestFlowUserDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AddDocumentToAttestFlowUserDirectiveController {
    // Init parameters
    private row: AttestWorkFlowTemplateRowDTO;
    private head: AttestWorkFlowHeadDTO;
    private regUserId: number;
    private endUserId: number;
    private showTransitionArrow: boolean;
    private registerControl: Function;

    // Data
    private attestWorkFlowTypes: SmallGenericType[] = [];
    private users: UserSmallDTO[];

    // Properties
    public get attestTransitionId() {
        return this.row.attestTransitionId;
    }

    private get label(): string {
        return this.regUserId ? this.row.attestStateFromName : this.row.sortAndAttestStateToName;
    }

    private get iconColor(): string {
        return this.regUserId ? '#F3F3F3' : this.row.attestStateToColor;
    }

    private get panelIconClass(): string {
        if (this.regUserId)
            return 'fal textColor';
        else if (this.endUserId)
            return 'fas okColor';
        else
            return 'far infoColor';
    }

    private get singleUser(): boolean {
        return !!this.regUserId || !!this.endUserId;
    }

    private get nbrOfSelected(): number {
        return this.users ? this.users.filter(u => u.isSelected).length : 0;
    }

    private get allItemsSelected(): boolean {
        var selected = true;
        _.forEach(this.users, item => {
            if (!item.isSelected) {
                selected = false;
                return false;
            }
        });

        return selected;
    }

    // Sorting
    private userSortBy: string = 'name';
    private userSortByReverse: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private addDocumentToAttestFlowService: IAddDocumentToAttestFlowService) {
    }

    public $onInit() {
        if (this.registerControl)
            this.registerControl({ control: this });

        if (this.row && this.row.attestTransitionId) {
            this.loadTemplateTypes();
            this.loadAttestWorkFlowUsers().then(() => {
                this.setDefaultUsers();
            });
        }
    }

    // SERVICE CALLS

    private loadTemplateTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then(x => {
            this.attestWorkFlowTypes = x;
        });
    }

    private loadAttestWorkFlowUsers(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        if (this.regUserId) {
            this.coreService.getUser(this.regUserId).then(x => {
                this.users = [x];
                deferral.resolve();
            });
        } else if (this.endUserId) {
            this.coreService.getUser(this.endUserId).then(x => {
                this.users = [x];
                deferral.resolve();
            });
        } else {
            this.addDocumentToAttestFlowService.getUsersByAttestRoleMapping(this.row.attestTransitionId).then(x => {
                this.users = x;
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    // EVENTS

    private selectItem(item: UserSmallDTO) {
        if (this.singleUser)
            return;

        item.isSelected = !item.isSelected;

        if (this.row.initial && item.userId === CoreUtility.userId)
            this.messagingService.publish('canSignInitial', item.isSelected);
    }

    private selectAllItems() {
        if (this.singleUser)
            return;

        let selected: boolean = this.allItemsSelected;
        _.forEach(this.users, u => {
            u.isSelected = !selected;
        });
    }

    private userSort(column: string) {
        if (this.singleUser)
            return;

        this.userSortByReverse = !this.userSortByReverse && this.userSortBy === column;
        this.userSortBy = column;
    }

    // HELP-METHODS

    private setDefaultUsers() {
        if (!this.row)
            return;

        if (this.regUserId) {
            let regUser = this.users.find(u => u.userId === this.regUserId);
            if (regUser)
                regUser.isSelected = true;
        } else if (this.endUserId) {
            let endUser = this.users.find(u => u.userId === this.endUserId);
            if (endUser)
                endUser.isSelected = true;
        } else {
            let currUser = this.users.find(u => u.userId === CoreUtility.userId);
            if (currUser)
                this.selectItem(currUser);
        }
    }

    public getRowsToSave(): AttestWorkFlowRowDTO[] {
        let rows = [];
        let row: AttestWorkFlowRowDTO;

        this.users.filter(u => u.isSelected).forEach(user => {
            row = user.attestFlowRowId !== 0 ? _.find(this.head.rows, r => r.attestWorkFlowRowId === user.attestFlowRowId) : null;
            if (!row)
                row = new AttestWorkFlowRowDTO();

            row.attestTransitionId = this.row.attestTransitionId;
            row.attestWorkFlowRowId = user.attestFlowRowId;
            row.type = this.row.type;
            row.userId = user.userId;
            rows.push(row);
        });

        return rows;
    }
}
