import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { EditController as SupplierEditController } from "../../../../Shared/Economy/Supplier/Suppliers/EditController";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { Feature } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { SelectSupplierController } from "../../../../Common/Dialogs/SelectSupplier/SelectSupplierController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";

export class SupplierHelper {

    private editSupplierPermission = false;
    private suppliers: ISmallGenericType[] = [];
    private supplier: SupplierDTO;
    private supplierId: number;

    public supplierReferences: ISmallGenericType[];
    public supplierEmails: ISmallGenericType[];
    public paymentConditions: any[];
    public sysLanguageId: number;


    private _selectedSupplier;
    public get selectedSupplier(): ISmallGenericType {
        return this._selectedSupplier;
    }
    public set selectedSupplier(item: ISmallGenericType) {
        this._selectedSupplier = item;
        if (this.selectedSupplier) {
            if (this.supplierId !== this.selectedSupplier.id) {
                this.supplierId = this.selectedSupplier.id;
                this.loadSupplier(this.selectedSupplier.id);
            }
        }
    }

    //@ngInject
    constructor(private parent: EditControllerBase2,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private supplierService: ISupplierService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private modalInstance: any,
        private supplierChanged: (supplier: SupplierDTO) => void ) {

    }

    public AddSetSupplier(supplier: ISmallGenericType) {
        this._selectedSupplier = supplier;
        this.supplierId = supplier.id;
        this.suppliers.push(supplier);
    }

    public setSupplierById(id: number) {
        this.selectedSupplier = this.suppliers.find(s => s.id === id);
    }

    public setPermissions(response: IPermissionRetrievalResponse) {
        this.editSupplierPermission = response[Feature.Economy_Supplier_Suppliers_Edit].modifyPermission;
    }

    public loadSuppliers(useCache: boolean, setSelected = false): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, useCache).then((x: ISmallGenericType[]) => {
            this.suppliers = x;

            if (setSelected)
                this._selectedSupplier = this.suppliers.find(x => x.id === this.supplierId);
        });
    }

    public loadSupplier(supplierId: number, isLoading = false): ng.IPromise<any>{

        if (isLoading) {
            this.supplierId = supplierId;
            this._selectedSupplier = this.suppliers.find(x => x.id === supplierId);
        }

        if (supplierId) {
            return this.supplierService.getSupplier(supplierId, false, true, false, false).then((x: SupplierDTO) => {
                this.supplier = x;
                this.sysLanguageId = x.sysLanguageId;

                if (isLoading && !this._selectedSupplier)
                    this._selectedSupplier = { id: this.supplier.actorSupplierId, name: this.supplier.supplierNr + " " + this.supplier.name };

               return this.$q.all([
                    this.loadSupplierReferences(supplierId),
                    this.loadSupplierEmails(supplierId)
                ]).then(() => {
                    if (!isLoading && this.supplierChanged) {
                        this.supplierChanged(x);
                    }
                });
            });
        } else {
            this.supplier = undefined;
            this._selectedSupplier = undefined;
            if (this.supplierChanged) {
                this.supplierChanged(undefined);
            }
            return;
        }        
    }

    public loadPaymentConditions(): ng.IPromise<any> {
        return this.supplierService.getPaymentConditions().then(x => {
            this.paymentConditions = x;
        });
    }

    private loadSupplierReferences(supplierId: number): ng.IPromise<any> {
        return this.supplierService.getSupplierReferences(supplierId, true).then(x => {
            this.supplierReferences = x;
        });
    }

    private loadSupplierEmails(supplierId: number): ng.IPromise<any> {
        return this.supplierService.getSupplierEmails(supplierId, true, true).then(x => {
            this.supplierEmails = x;
        });
    }

    public openSupplier(shouldPropagateChanges = true) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"),
            controller: SupplierEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope

        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.parent.guid, id: this.selectedSupplier ? this.selectedSupplier.id : 0 });
        });

        modal.result.then(result => {
            if (result.isModified && shouldPropagateChanges) {
                this.loadSuppliers(false, true);
                this.loadSupplier(this.supplierId);
            }
            /*
            if (this.invoice.originStatus === SoeOriginStatus.Draft) {
                this.loadSuppliers(false).then(() => {
                    if ((!this.supplier) || (this.supplier.actorSupplierId !== result.id)) {
                        this.supplierService.getSupplier(result.id, false, true, false, false).then(x => {
                            this.supplier = x;
                            this.supplierChanged();
                            this.loadingInvoice = false;
                        }).then(() => {
                            this._selectedSupplier = _.find(this.suppliers, { id: result.id })
                        });
                    }
                    else if (this.supplier && this.supplier.actorSupplierId === result.id && result.isModified) {
                        // Update the invoice if the supplier is edited.
                        this.loadPaymentInformation();
                        this.dirtyHandler.setDirty();
                    }
                });
            }
            */
        });
    }

    private searchSupplier() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectSupplier", "selectsupplier.html"),
            controller: SelectSupplierController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
            }
        });

        modal.result.then(id => {
            if (id && (!this.selectedSupplier || this.selectedSupplier.id !== id)) {
                this.setSupplierById(id);
            }
        }, function () {
        });
    }

}