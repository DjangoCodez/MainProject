import { ContactPersonDTO } from "../../../../../Common/Models/ContactPersonDTOs";

export class DeleteSupplierAgreementController {

    get enableSave(): boolean {
        return (this.contactPerson.firstName.length > 0 && this.contactPerson.lastName.length > 0);
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private contactPerson: ContactPersonDTO) {

        if (!contactPerson)
            contactPerson = new ContactPersonDTO();
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ contactPerson: this.contactPerson });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}