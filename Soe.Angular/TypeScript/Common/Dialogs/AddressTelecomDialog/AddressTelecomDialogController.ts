import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { ContactAddressItemType, TermGroup_SysContactAddressRowType, TermGroup_SysContactEComType } from "../../../Util/CommonEnumerations";
import { ContactAddressItemDTO } from "../../Models/ContactAddressDTOs";
import { Validators } from "../../../Core/Validators/Validators";
import { Guid, StringUtility } from "../../../Util/StringUtility";

export class AddressTelecomDialogController {
    private isValid: boolean;
    private validationErrors: string;
    private addressTypes1: any[];

    private termsInternal: any;
    private options: any;
    private originalAddressRows: ContactAddressItemDTO[];
    private address: any;

    private modified = false;
    public guid: Guid;

    //@ngInject
    constructor(
        $scope: ng.IScope,
        shortCutService: IShortCutService,
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private isNew: boolean,
        private allowShowSecret: boolean,
        private projectId: number,
        private terms?: any,
        private selectedAddress?: ContactAddressItemDTO,
        private addressRows?: ContactAddressItemDTO[],
        private addressRowTypes?: any[],
        private addressTypes?: any[],
        private eComTypes?: any[]) {

        shortCutService.bindEnterCloseDialog($scope, () => { this.ok(); })
    }

    private $onInit() {
        this.originalAddressRows = angular.copy(this.addressRows);
        if (typeof (this.selectedAddress.contactAddressId) == "undefined" || this.selectedAddress.contactAddressId == null)
            this.addAddress(this.isNew, this.selectedAddress);
    }

    private addAddress(isNew: boolean, selectedAddress: ContactAddressItemDTO) {
        let row: any;
        let addressItemType: ContactAddressItemType = this.projectId;
        if (isNew) {
            row = new ContactAddressItemDTO();
            row.contactAddressItemType = addressItemType;

            // Address or ECom
            switch (addressItemType) {
                case ContactAddressItemType.AddressDistribution:
                case ContactAddressItemType.AddressVisiting:
                case ContactAddressItemType.AddressBilling:
                case ContactAddressItemType.AddressDelivery:
                case ContactAddressItemType.AddressBoardHQ:
                    row.isAddress = true;
                    row.sysContactAddressTypeId = addressItemType;
                    break;
                default:
                    row.isAddress = false;
                    row.sysContactEComTypeId = (addressItemType - 10);
                    break;
            }

            // Set default name
            if (row.isAddress) {
                let addresType = _.find(this.addressTypes, { id: addressItemType });
                if (addresType)
                    row.name = addresType.name;
            } else {
                let eComType = _.find(this.eComTypes, { id: addressItemType });
                if (eComType) {
                    row.name = eComType.name;
                }
            }

            this.addressRows.push(row);
        }
        else {
            row = this.selectedAddress;
        }

        this.selectedAddress = row;

        this.setDisplayAddress(row);
    }

    private setDisplayAddress(address: ContactAddressItemDTO) {
        if (address) {
            if (address.isSecret && !this.allowShowSecret)
                address.displayAddress = '';
            else
                ContactAddressItemDTO.setDisplayAddress(address);

            this.validate();
        }
    }

    private showAddress(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.Address });
    }

    private showAddressCO(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.AddressCO });
    }

    private showStreetAddress(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.StreetAddress });
    }

    private showEntranceCode(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.EntranceCode });
    }

    private showPostalCode(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.PostalCode });
    }

    private showPostalAddress(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.PostalAddress });
    }

    private showCountry(): boolean {
        return _.find(this.addressRowTypes, { field1: this.selectedAddress.sysContactAddressTypeId, field2: TermGroup_SysContactAddressRowType.Country });
    }

    private showEmail(): boolean {
        return (this.selectedAddress && (this.selectedAddress.sysContactEComTypeId == TermGroup_SysContactEComType.Email || this.selectedAddress.sysContactEComTypeId == TermGroup_SysContactEComType.CompanyAdminEmail));
    }

    private showClosestRelative(): boolean {
        return (this.selectedAddress && (this.selectedAddress.sysContactEComTypeId == TermGroup_SysContactEComType.ClosestRelative || this.selectedAddress.contactAddressItemType == ContactAddressItemType.ClosestRelative));
    }

    private joinEComDescription(address: ContactAddressItemDTO, value1: string, value2: string) {
        if (value1 || value2)
            address.eComDescription = StringUtility.nullToEmpty(value1) + ';' + StringUtility.nullToEmpty(value2);
        else
            address.eComDescription = null;
    }

    private     setDisplayAddressFromGUI() {
        // Wait until changes are pushed to the model
        this.$timeout(() => {
            if (this.selectedAddress.contactAddressItemType === ContactAddressItemType.ClosestRelative) {
                this.joinEComDescription(this.selectedAddress, this.selectedAddress.closestRelativeName, this.selectedAddress.closestRelativeRelation);
            }

            if (this.selectedAddress.contactAddressItemType === ContactAddressItemType.Coordinates) {
                this.joinEComDescription(this.selectedAddress, this.selectedAddress.longitude, this.selectedAddress.latitude);
                this.selectedAddress.eComText = this.selectedAddress.eComDescription;
            }

            this.setDisplayAddress(this.selectedAddress);
            this.modified = true;
        });
    }

    private validate() {
        this.isValid = true;
        this.validationErrors = '';

        const emails = _.filter(this.addressRows, c => c.sysContactEComTypeId == TermGroup_SysContactEComType.Email);
        _.forEach(emails, email => {
            if (!Validators.isValidEmailAddress(email.eComText)) {
                if (this.validationErrors.length > 0)
                    this.validationErrors += '\n';
                this.validationErrors += this.terms["common.contactaddresses.invalidemail"];
                this.isValid = false;
                return false;
            }
        });

        const ecoms = _.filter(this.addressRows, c => c.isECom && c.sysContactEComTypeId != TermGroup_SysContactEComType.Email);
        _.forEach(ecoms, ecom => {
            if (!ecom.eComText) {
                if (this.validationErrors.length > 0)
                    this.validationErrors += '\n';
                this.validationErrors += this.terms["common.contactaddresses.missingecom"];
                this.isValid = false;
                return false;
            }
        });

        const rels = _.filter(this.addressRows, c => c.isClosestRelative);
        _.forEach(rels, rel => {
            if (!rel.eComText || !rel.closestRelativeName || !rel.closestRelativeRelation) {
                if (this.validationErrors.length > 0)
                    this.validationErrors += '\n';
                this.validationErrors += this.terms["common.contactaddresses.missingclosestrelative"];
                this.isValid = false;
                return false;
            }
        });

        const coords = _.filter(this.addressRows, c => c.isCoordinates);
        _.forEach(coords, coord => {
            if (!coord.latitude || !coord.longitude) {
                if (this.validationErrors.length > 0)
                    this.validationErrors += '\n';
                this.validationErrors += this.terms["common.contactaddresses.missingcoordinate"];
                this.isValid = false;
                return false;
            }
        });
    }

    private ok() {
        this.$uibModalInstance.close({ modified: this.modified, selectedAddress: this.selectedAddress, _selectedAddress: this.selectedAddress, action: this.selectedAddress, addressRows: this.addressRows });
    }

    private cancel() {
        if (this.isNew)
            _.pull(this.addressRows, this.selectedAddress);
        this.$uibModalInstance.close({ modified: false, remove: false, addressRows: this.originalAddressRows, selectedAddress: this.selectedAddress });
    }
}