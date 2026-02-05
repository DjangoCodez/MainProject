import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IGoogleMapsService } from "../../../Common/GoogleMaps/GoogleMapsService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { GoogleMapsController } from "../../../Common/GoogleMaps/GoogleMapsController";
import { Guid, StringUtility } from "../../../Util/StringUtility";
import { ContactAddressItemDTO } from "../../Models/ContactAddressDTOs";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { TermGroup_SysContactType, Feature, ContactAddressItemType, TermGroup, TermGroup_SysContactEComType, SoeEntityType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { Validators } from "../../../Core/Validators/Validators";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { AddressTelecomDialogController } from "../../Dialogs/AddressTelecomDialog/AddressTelecomDialogController";
import { INotificationService } from "../../../Core/Services/NotificationService";

export class ContactAddressesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('ContactAddresses', 'ContactAddresses.html'),
            scope: {
                contactType: '@',
                addressRows: '=',
                readOnly: '=',
                allowShowSecret: '=',
                isValid: '=?',
                validationErrors: '=?',
                onInitialized: '&',
                onChange: '&',
                parentGuid: '=?',
                ignoreSys: "=?",
                customerId: "=",
                validateDeletion: "=?",
                entityType: "=?"
            },
            restrict: 'E',
            replace: true,
            controller: ContactAddressesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ContactAddressesController extends GridControllerBase2Ag implements ICompositionGridController {

    // Setup
    private contactType: TermGroup_SysContactType;
    private addressRows: ContactAddressItemDTO[];
    private readOnly: boolean;
    private allowShowSecret: boolean;
    private isValid: boolean;
    private validationErrors: string;
    private onInitialized: Function;
    private onChange: Function;
    private ignoreSys: boolean;
    private cancelModification: boolean;
    private validateDeletion: boolean;
    private entityType: SoeEntityType;
    private parentGuid: Guid;

    // Collections
    private terms: any;
    private addressRowTypes: any[];
    private addressTypes: any[];
    private eComTypes: any[];

    private _selectedAddress: ContactAddressItemDTO;

    originalSelectAddress: ContactAddressItemDTO;
    private get selectedAddress(): ContactAddressItemDTO {

        return this._selectedAddress;
    }
    private set selectedAddress(item: ContactAddressItemDTO) {
        this._selectedAddress = item;
        if (item)
            this.gridAg.options.selectRow(item, true);
        this.setFocus();
    }

    // GoogleMaps
    private modalInstance: any;

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        private googleMapsService: IGoogleMapsService,
        uiGmapGoogleMapApi,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
    ) {

        super(gridHandlerFactory, "Common.Directives.ContactAddresses", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.$scope.$on('stopEditing', (e, a) => {
            this.gridAg.options.stopEditing(false);
        });

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.None, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = true;
        this.modifyPermission = true;
    }

    private afterSetup() {
        this.setupWatchers();
    }

    // SETUP

    public $onInit() {
        if (!this.addressRows)
            this.addressRows = [];

        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = false;
        this.gridAg.options.setMinRowsToShow(8);

        this.doubleClickToEdit = false;

        this.modalInstance = this.$uibModal;

        this.$scope.$on('selectContactAddresRow', (e, a) => {

            var index = 0;
            if (a && a.index)
                index = a.index;
            this.gridAg.options.selectRowByVisibleIndex(index);
        });

        this.$scope.$on('addContactAddresRow', (e, a) => {

            this.addAddress(a.type, true);
        });

        this.$scope.$on('deleteEmptyContactAddresRow', (e, a) => {
            this.deleteEmptyAddress(a.type);
        });
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadAddressRowTypes(),
            this.loadAddressTypes(),
            this.loadEComTypes()]);
    }

    public setupGrid() {
        // Grid events
        this.gridAg.options.setMinRowsToShow(8);

        this.gridAg.addColumnIcon("icon", null, null, null);
        this.gridAg.options.addColumnText("name", this.terms["common.contactaddresses.name"], 125, { editable: false });
        this.gridAg.addColumnText("displayAddress", this.terms["common.contactaddresses.value"], null);
        this.gridAg.addColumnBool("isSecret", this.terms["common.contactaddresses.issecret"], 75, !this.readOnly, this.isSecretChanged.bind(this), "true");
        this.gridAg.addColumnIcon(null, this.terms["common.contactaddresses.showmap"], 100, { onClick: this.showMap.bind(this), icon: "fal fa-map", showIcon: this.showMapIcon.bind(this) });
        this.gridAg.addColumnEdit(this.terms["core.edit"], (row: any) => { this.showAddressTelecomDialog(row, false) }, false);

        if (!this.readOnly)
            this.gridAg.addColumnDelete(this.terms["core.deleterow"], this.initDeleteAddress.bind(this));

        this.gridAg.options.enableRowSelection = false;

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => {
            this.showAddressTelecomDialog(row, false);
        }));

        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("common.contactaddresses.name", false);
        this.gridAg.setData(this.addressRows);
        this.isDirty = false;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.addressRows, () => {
            _.forEach(this.addressRows, row => {
                this.setDisplayAddress(row);
                this.setIcon(row);
                this.setSpecialInfo(row);
            });
            this.gridAg.setData(this.addressRows);
            this.isDirty = false;
            this.selectedAddress = null;
            this.validate();
        });

        if (this.onInitialized)
            this.onInitialized();
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.contactaddresses.name",
            "common.contactaddresses.value",
            "common.contactaddresses.issecret",
            "common.contactaddresses.showmap",
            "common.contactaddresses.addressrow.name",
            "common.contactaddresses.addressrow.address",
            "common.contactaddresses.addressrow.co",
            "common.contactaddresses.addressrow.postaladdress",
            "common.contactaddresses.addressrow.postalcode",
            "common.contactaddresses.addressrow.street",
            "common.contactaddresses.addressrow.entrancecode",
            "common.contactaddresses.addressrow.country",
            "common.contactaddresses.ecom.closestrelative.name",
            "common.contactaddresses.ecom.closestrelative.relation",
            "common.contactaddresses.ecom.latitude",
            "common.contactaddresses.ecom.longitude",
            "common.contactaddresses.addressmenu.label",
            "common.contactaddresses.addressmenu.distribution",
            "common.contactaddresses.addressmenu.visiting",
            "common.contactaddresses.addressmenu.billing",
            "common.contactaddresses.addressmenu.delivery",
            "common.contactaddresses.addressmenu.boardhq",
            "common.contactaddresses.ecommenu.label",
            "common.contactaddresses.ecommenu.companyadminemail",
            "common.contactaddresses.ecommenu.email",
            "common.contactaddresses.ecommenu.phonehome",
            "common.contactaddresses.ecommenu.phonejob",
            "common.contactaddresses.ecommenu.phonemobile",
            "common.contactaddresses.ecommenu.fax",
            "common.contactaddresses.ecommenu.web",
            "common.contactaddresses.ecommenu.closestrelative",
            "common.contactaddresses.ecommenu.coordinates",
            "common.contactaddresses.ecommenu.individualtaxnumber",
            "core.deleterow",
            "common.contactaddresses.invalidemail",
            "common.contactaddresses.missingecom",
            "common.contactaddresses.missingclosestrelative",
            "common.contactaddresses.missingcoordinate",
            "common.contact.information",
            "core.edit"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadAddressRowTypes(): ng.IPromise<any> {
        return this.coreService.getAddressRowTypes(this.contactType).then(x => {
            this.addressRowTypes = x;
        });
    }

    private loadAddressTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.coreService.getAddressTypes(this.contactType).then(x => {
            let addressTypeIds = x;

            this.coreService.getTermGroupContent(TermGroup.SysContactAddressType, false, true).then(aTypes => {
                // Filter address types based on contact type
                this.addressTypes = [];
                _.forEach(aTypes, aType => {
                    if (_.includes(addressTypeIds, aType['id'])) {
                        aType['icon'] = ContactAddressItemDTO.getIcon(aType['id']);
                        this.addressTypes.push(aType);
                    }
                });

                deferral.resolve();
            });
        });

        return deferral.promise;
    }

    private loadEComTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.coreService.getEComTypes(this.contactType).then(x => {
            var eComTypeIds = x;

            if (this.ignoreSys)
                _.pull(eComTypeIds, 8);

            this.coreService.getTermGroupContent(TermGroup.SysContactEComType, false, true).then(eTypes => {
                // Filter ecom types based on contact type
                this.eComTypes = [];
                _.forEach(eTypes, eType => {
                    if (_.includes(eComTypeIds, eType['id'])) {
                        eType['id'] += 10;
                        eType['icon'] = ContactAddressItemDTO.getIcon(eType['id']);
                        this.eComTypes.push(eType);
                    }
                });

                deferral.resolve();
            });
        });

        return deferral.promise;
    }

    // ACTIONS

    private addAddress(option: any, isNew: any) {
        this.showAddressTelecomDialog(option, isNew);
    }

    addNewRow(option: any, addRow: ContactAddressItemDTO, isNew: any) {
        var addressItemType: ContactAddressItemType = option.id;

        let row: ContactAddressItemDTO = new ContactAddressItemDTO();
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
            var addresType = _.find(this.addressTypes, { id: addressItemType });
            if (addresType)
                row = addRow;
        } else {
            var eComType = _.find(this.eComTypes, { id: addressItemType });
            if (eComType) {
                row = addRow;
            }
        }

        this.selectedAddress = row;
        this.setDisplayAddress(row);
    }

    private showAddressTelecomDialog(row: any, isNew: boolean): any {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/AddressTelecomDialog", "AddressTelecomDialog.html"),
            controller: AddressTelecomDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                isNew: () => { return isNew },
                allowShowSecret: () => { return this.allowShowSecret },
                projectId: () => { return row.id || null },
                terms: () => { return this.terms },
                selectedAddress: () => { return row },
                addressRows: () => { return this.addressRows },
                addressRowTypes: () => { return this.addressRowTypes },
                addressTypes: () => { return this.addressTypes },
                eComTypes: () => { return this.eComTypes },
            }
        });

        modal.result.then(x => {
            if (!x.modified) {
                this.addressRows = x.addressRows;
                return false;
            }

            if (x.modified) {
                if (!x.selectedAddress.contactAddressId) {
                    this.addressRows = x.addressRows;
                    this.addNewRow(row, x.selectedAddress, true);
                }

                this.refreshAddressGrid();
                this.setDirtyGrid();
            }
        });
    }

    private isSecretChanged(row: ContactAddressItemDTO) {
        this.setDirtyGrid();
    }

    private setDirtyGrid(): any {
        this.isDirty = true;
        
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
        if (this.onChange)
            this.onChange();
    }

    private showMap(row: ContactAddressItemDTO): any {
        // Coordinates
        var latitude: number = 0;
        var longitude: number = 0;
        if (row.contactAddressItemType === ContactAddressItemType.Coordinates && row.eComDescription) {
            var split: string[] = row.eComDescription.split(';')
            if (split.length > 0)
                longitude = Number(split[0]);
            if (split.length > 1)
                latitude = Number(split[1]);
        }

        return this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/GoogleMaps/Views/GoogleMaps.html"),
            controller: GoogleMapsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'modal-wide',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                urlHelperService: () => { return this.urlHelperService },
                googleMapsService: () => { return this.googleMapsService },
                title: () => { return "" },
                zoom: () => { return 15 },
                latitude: () => { return latitude },
                longitude: () => { return longitude },
                search: () => { return row.displayAddress }
            }
        });
    }

    private initDeleteAddress(row) {
        if (this.validateDeletion && this.entityType && row.contactAddressItemType == ContactAddressItemType.EComEmail) {
            this.progress.startLoadingProgress([() => {
                return this.coreService.validateEcomDeletion(this.entityType, row.contactAddressItemType, row.contactEComId).then(valid => {
                    if (!valid) {
                        const keys: string[] = [
                            "core.verifyquestion",
                            "common.contactaddresses.validationdeletemessage"
                        ];

                        this.translationService.translateMany(keys).then((terms) => {
                            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.contactaddresses.validationdeletemessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                            modal.result.then(val => {
                                if (val != null && val === true) {
                                    this.deleteAddress(row, false);
                                }
                            });
                        });
                    }
                    else {
                        this.deleteAddress(row, true);
                    }
                });
            }]);
        }
        else {
            this.deleteAddress(row, true);
        }
    }

    private deleteAddress(row, useDigest) {
        const index = this.addressRows.indexOf(row);
        if (index >= 0) {
            this.addressRows.splice(index, 1);
        }

        this.refreshAddressGrid();

        this.setDirtyGrid();

        // Trigger validation directive
        if(useDigest)
            this.$scope.$digest();
    }

    private refreshAddressGrid() {
        this.gridAg.setData(this.addressRows);
    }

    private deleteEmptyAddress(type: ContactAddressItemType) {
        const row = _.find(this.addressRows, r => r.contactAddressItemType === type && !r.displayAddress);
        if (row)
            this.deleteAddress(row, true);
    }

    // HELP-METHODS

    private setFocus() {
        if (!this.selectedAddress)
            return;

        // Set focus
        let focusName: string = '';
        if (this.selectedAddress.isAddressDelivery)
            focusName = 'addressName';
        else if (this.selectedAddress.isAddressVisiting)
            focusName = 'streetAddress';
        else if (this.selectedAddress.isAddressBoardHQ)
            focusName = 'postalAddress';
        else if (this.selectedAddress.isCoordinates)
            focusName = 'longitude';
        else if (this.selectedAddress.isAddress)
            focusName = 'address';
        else
            focusName = 'eComText';

        this.focusService.focusByName("ctrl_selectedAddress_" + focusName);
    }

    private showMapIcon(row: ContactAddressItemDTO): boolean {
        return (row.isAddress && row.sysContactAddressTypeId != ContactAddressItemType.AddressBoardHQ) || row.sysContactEComTypeId == TermGroup_SysContactEComType.Coordinates;
    }
    
    private splitEComDescription(address: ContactAddressItemDTO, field1: string, field2: string) {
        address[field1] = '';
        address[field2] = '';

        if (address.eComDescription) {
            var split: string[] = [];
            split = address.eComDescription.split(';')
            if (split.length > 0) {
                address[field1] = split[0];
            }
            if (split.length > 1) {
                address[field2] = split[1];
            }
        }
    }

    private joinEComDescription(address: ContactAddressItemDTO, value1: string, value2: string) {
        if (value1 || value2)
            address.eComDescription = StringUtility.nullToEmpty(value1) + ';' + StringUtility.nullToEmpty(value2);
        else
            address.eComDescription = null;
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

    private setIcon(address: ContactAddressItemDTO) {
        if (address) {
            ContactAddressItemDTO.setIcon(address);
        }
    }

    private setSpecialInfo(address: ContactAddressItemDTO) {
        if (address.contactAddressItemType == ContactAddressItemType.ClosestRelative)
            this.splitEComDescription(address, "closestRelativeName", "closestRelativeRelation");
        if (address.contactAddressItemType == ContactAddressItemType.Coordinates)
            this.splitEComDescription(address, "longitude", "latitude");
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
}