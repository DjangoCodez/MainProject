import { ICoreService } from "../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class SelectRolesController {

    private selectableRoles: ISmallGenericType[] = [];
    private selectedRoles: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        private $uibModalInstance,
        private coreService: ICoreService,
        private rolesMandatory: boolean,
        private selectedRoleIds: number[]) {

        this.loadRoles().then(() => {
            this.setSelectedRoles();
        });
    }

    private loadRoles(): ng.IPromise<any> {
        return this.coreService.getCompanyRolesDict(false, false).then(x => {
            this.selectableRoles = x
        });
    }

    private setSelectedRoles() {
        this.selectedRoles = _.filter(this.selectableRoles, r => _.includes(this.selectedRoleIds, r.id));
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ selectedRoles: this.selectedRoles });
    }
}