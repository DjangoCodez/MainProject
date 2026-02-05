import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";

export class SelectUsersController {
    private soeGridOptions: ISoeGridOptionsAg;
    private users: any[] = [];
    private sendMessage: boolean = false;

    private _selectAll: boolean = false;

    get selectAll(): boolean { return this._selectAll; }

    set selectAll(value: boolean) {
        this.selectAllChanged();
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private title: string,
        private selectedUsers: any[],
        private showMain: boolean,
        private showParticipant: boolean,
        private showSendMessage: boolean,
        private showCategories: boolean) {
        
        this.soeGridOptions = new SoeGridOptionsAg("Common.Dialogs.SelectUsers", this.$timeout);
        this.setupGrid();

        this.loadUsers().then(() => {
            this.$timeout(() => {
                this.setSelectedUsers();
            }, 250);
        });
    }

    private setupGrid() {

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.disableHorizontalScrollbar = true;
        this.soeGridOptions.setMinRowsToShow(10);

        // Columns
        const keys: string[] = [
            "common.selected",
            "common.username",
            "common.name",
            "common.main",
            "billing.order.selectusers.responsible",
            "billing.order.selectusers.participant",
            "common.categories"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.showParticipant)
                this.soeGridOptions.addColumnBool("isSelected", terms["billing.order.selectusers.participant"], null, { enableEdit: true, suppressFilter: true, onChanged: this.participantSelected.bind(this) });
            else
                this.soeGridOptions.addColumnBool("isSelected", terms["common.selected"], null, { enableEdit: true, suppressFilter: true });
            if (this.showMain)
                this.soeGridOptions.addColumnBool("main", terms["billing.order.selectusers.responsible"], null, { enableEdit: true, onChanged: this.mainSelected.bind(this), suppressFilter: true });
            this.soeGridOptions.addColumnText("loginName", terms["common.username"], null);
            this.soeGridOptions.addColumnText("name", terms["common.name"], null);
            this.soeGridOptions.addColumnText("categories", terms["common.categories"], null);
        });
    }

    private loadUsers(): ng.IPromise<any> {
        return this.coreService.getUsersForOrigin(true).then(x => {
            this.users = x;
        });
    }    

    private selectAllChanged() {
        this._selectAll = (!this._selectAll);
        _.forEach(this.users, (usr) => {
            usr.isSelected = this.selectAll;
        });

        this.setData();
    }

    private setSelectedUsers() {

        const keys: string[] = [           
            "billing.order.selectusers.inactivateduser"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            // Set pre-selected        
            _.forEach(this.selectedUsers, user => {
                var usr = _.find(this.users, { userId: user.userId });
                if (usr) {
                    usr.isSelected = true;
                    // Set main
                    if (user.main)
                        usr.main = true;
                }
                else {
                    // user is inactivated and therefore it excists in selectedUsers, but not in users
                    this.users.push({
                        userId: user.userId,
                        main: user.main,
                        name: user.name,
                        loginName: terms["billing.order.selectusers.inactivateduser"],
                        isSelected: true
                    })

                }
            });

            this.setData();
        });

        
    }

    private mainSelected(item) {
        if (item.data.main) {
            item.data.isSelected = true;
            _.filter(this.users, (u) => u.userId !== item.data.userId ).forEach((u) => {
                if (u.main) 
                    u.main = false
            });
        }

        this.setData();
    }

    private participantSelected(item) {
        if (!item.data.isSelected) {
            item.data.main = false;
            this.setData();
        }
    }

    private setData() {
        this.soeGridOptions.setData(_.orderBy(this.users, 'name' ));
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {
        this.selectedUsers = _.filter(this.users, i => i.isSelected == true);
        this.$uibModalInstance.close({ selectedUsers: this.selectedUsers, sendMessage: this.sendMessage });
    }
}