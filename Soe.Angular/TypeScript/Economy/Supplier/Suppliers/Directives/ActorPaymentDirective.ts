import { IPaymentInformationDTO, IPaymentInformationRowDTO, IPaymentInformationViewDTO, ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { UrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { Feature, TermGroup, TermGroup_SysPaymentType, TermGroup_ForeignPaymentMethod, TermGroup_ForeignPaymentIntermediaryCode, TermGroup_ForeignPaymentForm, TermGroup_Languages } from "../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { Validators } from "../../../../Core/Validators/Validators";
import { CoreUtility } from "../../../../Util/CoreUtility";

export class ActorPaymentController extends GridControllerBase2Ag implements ICompositionGridController {
    actorSupplierId: number;
    sysPaymentTypes: any = [];
    foreignPaymentForms: any = [];
    foreignPaymentMethod: any = [];
    foreignPaymentChargeCode: any = [];
    foreignPaymentIntermediaryCode: any = [];
    defaultSysPaymentTypeId: number;
    paymentInformation: IPaymentInformationDTO;
    paymentInformationRows: IPaymentInformationRowDTO[];
    paymentCodes: any[] = [];
    isForegin: boolean;
    isNew: number;
    isIbanValid = true;
    lookupCompleted = false;
    isCompany: boolean;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {
        super(gridHandlerFactory, "Supplier.Supplier.ActorPayment", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false, true))
            .onBeforeSetUpGrid(() => this.loadLookups())

        this.flowHandler.start({ feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: true, loadModifyPermissions: true });

        this.$scope.$on('stopEditing', (e, a) => {
            this.gridAg.options.stopEditing(false);
        });
    }

    onInit() {
    }

    private loadLookups(): ng.IPromise<any> {
        if (this.isForegin && soeConfig.sysCountryId === 1) {
            return this.$q.all([
                this.getPaymentInformationFromActor(),
                this.loadPaymentCodes()
            ]).then(() => {
                this.lookupCompleted = true;
            });
        }
        else {
            return this.$q.all([
                this.getPaymentInformationFromActor(),
            ]).then(() => {
                this.lookupCompleted = true;
            });
        }
    }

    private setupWatches() {
        this.$scope.$watch(() => this.paymentInformation, () => {
            if (this.paymentInformation) {
                if (this.lookupCompleted) {
                    this.loadGridData(false, true);
                }
                else {
                    this.$timeout(() => {
                        this.loadGridData(false, true);
                    }, 200);
                }
            }
        });
    }

    private loadPaymentCodes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.CentralBankCode, true, false, true).then(data => {
            this.paymentCodes = [];
            data.forEach((code: any) => {
                if (code.id)
                    this.paymentCodes.push({ value: code.id, label: code.id + " - " + code.name, paymentCode: code.id });
                else
                    this.paymentCodes.push({ value: undefined, label: "", paymentCode: undefined });
            });
        });
    }

    private setupGrid(): ng.IPromise<any> {
        if (this.isCompany)
            this.gridAg.options.enableFiltering = false;
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(5);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (val) => null));
        this.gridAg.options.subscribe(events);

        const keys: string[] = [
            "core.remove", "economy.supplier.supplier.actorpayment", "economy.supplier.supplier.paymenttype", "economy.supplier.supplier.account",
            "economy.supplier.supplier.checkbox", "economy.supplier.supplier.defaultwithinpayment", "economy.supplier.supplier.bic", "economy.supplier.supplier.accountcode", "economy.supplier.supplier.accountcurrency",
            "economy.supplier.supplier.paymentcode", "economy.supplier.supplier.paymentintermediary", "economy.supplier.supplier.paymentform", "economy.supplier.supplier.paymentmethod", "economy.supplier.supplier.handlingfee",
            "economy.supplier.supplier.changingofbankaccountnumber", "common.standard", "economy.supplier.supplier.iban"
        ];

        return this.translationService.translateMany(keys).then((terms) => {

            if (!this.isForegin) {
                this.gridAg.addColumnSelect("sysPaymentTypeId", terms["economy.supplier.supplier.paymenttype"], null, { editable: true, selectOptions: this.sysPaymentTypes, displayField: "sysPaymentTypeName", dropdownValueLabel: "label", dropdownIdLabel: "value" }); // "sysPaymentTypeChanged");                
            }

            this.gridAg.addColumnText("bic", terms["economy.supplier.supplier.bic"], null, false, { enableHiding: false, editable: true });
            this.gridAg.addColumnText("paymentNr", this.isForegin ? terms["economy.supplier.supplier.iban"] : terms["economy.supplier.supplier.account"], null, false, { enableHiding: false, editable: true });

            if (this.isForegin) {
                this.gridAg.addColumnText("clearingCode", terms["economy.supplier.supplier.accountcode"], null, false, { editable: true, enableHiding: false });
                this.gridAg.addColumnText("currencyAccount", terms["economy.supplier.supplier.accountcurrency"], null, false, { editable: true, enableHiding: false });

                if (soeConfig.sysCountryId === 1)
                    this.gridAg.addColumnSelect("paymentCode", terms["economy.supplier.supplier.paymentcode"], null, { editable: true, selectOptions: this.paymentCodes, displayField: "paymentCodeName", dropdownValueLabel: "label", dropdownIdLabel: "value" });
                else
                    this.gridAg.addColumnText("paymentCode", terms["economy.supplier.supplier.paymentcode"], null, false, { editable: true, enableHiding: false });

                this.gridAg.addColumnSelect("intermediaryCode", terms["economy.supplier.supplier.paymentintermediary"], null, { editable: true, selectOptions: this.foreignPaymentIntermediaryCode, displayField: "intermediaryCodeName", dropdownValueLabel: "label", dropdownIdLabel: "value" });// onChanged: "intermediaryCodeNameChanged");
                this.gridAg.addColumnSelect("paymentForm", terms["economy.supplier.supplier.paymentform"], null, { editable: true, selectOptions: this.foreignPaymentForms, displayField: "paymentFormName", dropdownValueLabel: "label", dropdownIdLabel: "value" });
                this.gridAg.addColumnSelect("paymentMethodCode", terms["economy.supplier.supplier.paymentmethod"], null, { editable: true, selectOptions: this.foreignPaymentMethod, displayField: "paymentMethodCodeName", dropdownValueLabel: "label", dropdownIdLabel: "value" });//, "spaymentMethodCodeChanged");
                this.gridAg.addColumnSelect("chargeCode", terms["economy.supplier.supplier.handlingfee"], null, { editable: true, selectOptions: this.foreignPaymentChargeCode, displayField: "chargeCodeName", dropdownValueLabel: "label", dropdownIdLabel: "value" }); //, "chargeCodeChanged");
            }

            this.gridAg.addColumnBoolEx("default", terms["common.standard"], null, { enableEdit: true, onChanged: this.defaultChanged.bind(this), maxWidth: 50 });

            this.gridAg.addColumnDelete(terms["common.remove"], this.deleteRow.bind(this))
            this.gridAg.finalizeInitGrid("common.supplier.actorpayments", false);

            this.setupWatches();
        }
        );
    }

    private afterCellEdit(row: IPaymentInformationRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            /*
            case "paymentForm":
                {
                    this.paymentFormChanged(row, newValue);
                    break;
                }
            case "chargeCode":
                {
                    this.chargeCodeChanged(row, newValue);
                    break;
                }
                case "paymentMethodCode":
                {
                    this.paymentMethodCodeChanged(row, newValue);
                    break;
                }
            case "intermediaryCode":
                {
                    break;
                }
                */
            case "bic":
                {
                    const isValidBic = Validators.isValidBic(newValue, !this.isForegin);
                    if (!isValidBic) {
                        this.showWarningMessage("economy.supplier.supplier.invalidbic").then(() => {
                            this.focuseBic(row);
                        });
                    }
                    break;
                }
            case "paymentNr":
                {
                    if (newValue.substr(0, 2).toLowerCase() === "fi") {
                        this.handlePaymentNrAndGetBic(row, newValue);
                    }
                    else { 
                        if (row.sysPaymentTypeId == TermGroup_SysPaymentType.BIC || this.isForegin) {
                            const isValidBic = Validators.isValidBic(row.bic, false);
                            if (isValidBic) {
                                this.validateIban(newValue).then((ok: boolean) => {
                                    if (!ok) {
                                        this.focusePaymentNr(row);
                                    }
                                })
                            }
                            else {
                                if (CoreUtility.sysCountryId !== TermGroup_Languages.Finnish || this.isForegin) {
                                    this.showWarningMessage("economy.supplier.supplier.mandatorybic").then(() => {
                                        this.focuseBic(row);
                                    });
                                }
                                else {
                                    this.validateIban(newValue).then((ok: boolean) => {
                                        if (!ok) {
                                            this.focusePaymentNr(row);
                                        }
                                    })
                                }
                            }
                        }
                        else if (row.sysPaymentTypeId == TermGroup_SysPaymentType.SEPA) {
                            this.validateIban(row.paymentNr).then((ok: boolean) => {
                                if (!ok) {
                                    this.focusePaymentNr(row);
                                }
                            });
                        }
                    }
                    break;
                }
            case "paymentCode":
                {
                    if (this.paymentCodes.length > 0) {
                        const paymentCode = this.paymentCodes.find(x => x.label === newValue);
                        row.paymentCode = paymentCode?.value ?? "";
                    }
                        
                    break;
                }
        }

        this.messagingHandler.publishSetDirty();
    }

    public handlePaymentNrAndGetBic(row: IPaymentInformationRowDTO, iban: string) {
        if (row.sysPaymentTypeId == TermGroup_SysPaymentType.BIC || row.sysPaymentTypeId == TermGroup_SysPaymentType.SEPA) {
            this.getBicFromIban(row, iban);

            this.validateIban(iban).then((ok: boolean) => {
                if (!ok) {
                    this.focusePaymentNr(row);
                }
            });
        }
    }

    public focusePaymentNr(row:any) {
        this.$timeout(() => {
            this.gridAg.options.startEditingCell(row, "paymentNr");
        });
    }
    public focuseBic(row: any) {
        this.$timeout(() => {
            this.gridAg.options.startEditingCell(row, "bic");
        });
    }

    public defaultChanged(rowData) {
        const row: IPaymentInformationRowDTO = rowData.data as IPaymentInformationRowDTO;
        this.paymentInformation.rows.forEach((x: IPaymentInformationRowDTO) => {
            if (x.paymentInformationRowId !== row.paymentInformationRowId && x.default) {
                x.default = false;
            }
        });

        if (!row.default) {
            this.paymentInformation.rows[0].default = true;
        }
        this.$timeout(() => {
            this.gridAg.options.refreshGrid();
        });
    }

    public getPaymentInformationFromActor(): ng.IPromise<any> {
        const promises: ng.IPromise<any>[] = [];

        promises.push(this.coreService.getTermGroupContent(TermGroup.SysPaymentType, false, false).then((result) => {
            _.forEach(result, (sysPaymentType: any) => {
                this.sysPaymentTypes.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
            });
        }));

        if (this.isForegin) {
            promises.push(this.coreService.getTermGroupContent(TermGroup.ForeignPaymentForm, false, false).then((result) => {
                _.forEach(result, (sysPaymentType: any) => {
                    this.foreignPaymentForms.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
                });
            }));

            promises.push(this.coreService.getTermGroupContent(TermGroup.ForeignPaymentMethod, false, false).then((result: ISmallGenericType[]) => {
                result.forEach( (sysPaymentType) => {
                    this.foreignPaymentMethod.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
                });
            }));

            promises.push(this.coreService.getTermGroupContent(TermGroup.ForeignPaymentChargeCode, false, false).then((result) => {
                _.forEach(result, (sysPaymentType: any) => {
                    this.foreignPaymentChargeCode.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
                });
            }));

            promises.push(this.coreService.getTermGroupContent(TermGroup.ForeignPaymentIntermediaryCode, false, false).then((result) => {
                _.forEach(result, (sysPaymentType: any) => {
                    this.foreignPaymentIntermediaryCode.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
                });
            }));
        }

        return this.$q.all(promises);
    }

    protected setpaymentInformationRow(): void {
        if (!this.paymentInformation) {
            this.gridAg.options.clearData();
            return;
        }

        this.paymentInformation.rows.forEach((row: IPaymentInformationRowDTO) => {

            row["paymentCodeName"] = this.paymentCodes.find(x => x.value == row.paymentCode)?.label;
            _.forEach(this.sysPaymentTypes,
                (sysPaymentType: any) => {
                    if (row.sysPaymentTypeId === sysPaymentType.value) {
                        row.sysPaymentTypeName = sysPaymentType.label;
                    }
                });
            _.forEach(this.foreignPaymentIntermediaryCode,
                (intermediaryCode: any) => {
                    if (row.intermediaryCode === intermediaryCode.value) {
                        row.intermediaryCodeName = intermediaryCode.label;
                    }
                });
            _.forEach(this.foreignPaymentForms,
                (foreignPaymentForm: any) => {
                    if (row.paymentForm === foreignPaymentForm.value) {
                        row.paymentFormName = foreignPaymentForm.label;
                    }
                });
            _.forEach(this.foreignPaymentMethod,
                (paymentMethod: any) => {
                    if (row.paymentMethodCode === paymentMethod.value) {
                        row.paymentMethodCodeName = paymentMethod.label;
                    }
                });
            _.forEach(this.foreignPaymentChargeCode,
                (chargeCode: any) => {
                    if (row.chargeCode === chargeCode.value) {
                        row.chargeCodeName = chargeCode.label;
                    }
                });
        });

        this.defaultSysPaymentTypeId = this.paymentInformation.defaultSysPaymentTypeId;
        this.loadGridData(false, false);
    }

    private deleteRow(row: IPaymentInformationRowDTO) {
        const index = this.paymentInformation.rows.findIndex(y => y === row);
        if (index >= 0) {
            this.paymentInformation.rows.splice(index, 1);
            this.loadGridData(true, false);
        }
    }

    private getFirstColumnName(): string {
        return (this.isForegin) ? "bic" : "sysPaymentTypeId";
    }

    private addRow() {
        if (!this.paymentInformation) {
            this.paymentInformation = <IPaymentInformationDTO>{};
            this.paymentInformation.rows = [];
        }

        const newPaymentInformationRow = <IPaymentInformationRowDTO>{};
        newPaymentInformationRow.paymentNr = "";
        newPaymentInformationRow.intermediaryCode = 0;
        _.forEach(this.sysPaymentTypes, (sysPaymentType: any) => {
            if (this.isForegin) {
                if (sysPaymentType.value === TermGroup_SysPaymentType.BIC) {
                    newPaymentInformationRow.sysPaymentTypeId = sysPaymentType.value;
                    newPaymentInformationRow.sysPaymentTypeName = sysPaymentType.label;
                }
            } else {
                if (sysPaymentType.value === 1) {
                    newPaymentInformationRow.sysPaymentTypeId = sysPaymentType.value;
                    newPaymentInformationRow.sysPaymentTypeName = sysPaymentType.label;
                };
            }
        });

        if (this.paymentInformation.rows.length === 0)
            newPaymentInformationRow.default = true;

        if (this.isForegin) {
            newPaymentInformationRow.bic = "";
            newPaymentInformationRow.clearingCode = "";
            newPaymentInformationRow.currencyAccount = "";
            _.forEach(this.foreignPaymentMethod, (fpaymentMethod: any) => {
                if (fpaymentMethod.value === TermGroup_ForeignPaymentMethod.Normal) {
                    newPaymentInformationRow.paymentMethodCode = fpaymentMethod.value;
                    newPaymentInformationRow.paymentMethodCodeName = fpaymentMethod.label;
                }
            });
            _.forEach(this.foreignPaymentForms, (foreignPayment: any) => {
                if (foreignPayment.value === TermGroup_ForeignPaymentForm.Account) {
                    newPaymentInformationRow.paymentForm = foreignPayment.value;
                    newPaymentInformationRow.paymentFormName = foreignPayment.label;
                }
            });
            _.forEach(this.foreignPaymentChargeCode, (chargeCode: any) => {
                if (chargeCode.value === TermGroup_ForeignPaymentMethod.Normal) {
                    newPaymentInformationRow.chargeCode = chargeCode.value;
                    newPaymentInformationRow.chargeCodeName = chargeCode.label;
                }
            });
            _.forEach(this.foreignPaymentIntermediaryCode, (intermediaryCode: any) => {
                if (intermediaryCode.value === TermGroup_ForeignPaymentIntermediaryCode.BGC) {
                    newPaymentInformationRow.intermediaryCode = intermediaryCode.value;
                    newPaymentInformationRow.intermediaryCodeName = intermediaryCode.label;
                }
            });
        }

        this.paymentInformation.rows.push(newPaymentInformationRow);
        this.loadGridData(true, false);
        this.gridAg.options.startEditingCell(newPaymentInformationRow, this.getFirstColumnName());
    }

    private loadGridData(setDirty: boolean, setPaymentInfo: boolean) {
        if (!this.paymentInformation) {
            return;
        }

        if (setDirty) {
            this.messagingHandler.publishSetDirty();
        }

        if (setPaymentInfo) {
            this.setpaymentInformationRow();
        }

        this.gridAg.setData(this.paymentInformation.rows);
    }

    private validateIban(iban: string): ng.IPromise<any> {  
        const deferral = this.$q.defer<boolean>();
        this.coreService.validIBANNumber(iban.replace(/\s/g, "")).then(isValid => {            
            if (isValid)
            {
                deferral.resolve(isValid);
            }
            else {
                this.showWarningMessage("economy.supplier.supplier.ibannotvalid").then(() => {
                    deferral.resolve(isValid);
                });
            }   
        });        

        return deferral.promise;
    }

    private getBicFromIban(row: IPaymentInformationRowDTO, iban: string) {
        this.coreService.getBicFromIban(iban.replace(/\s/g, "")).then(bic => {
            if (bic) {
                this.$timeout(() => {
                    row.bic = bic;
                    this.gridAg.options.refreshRows(row);
                });
            }
        });
    }

    private showWarningMessage(messageKey: string): ng.IPromise<any> {
        const deferral = this.$q.defer<boolean>();
        const keys: string[] = [
            "core.warning",
            messageKey
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms[messageKey], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            modal.result.then(() => {
                deferral.resolve();
            });
        });

        return deferral.promise;
    }
}

//@ngInject
export function ActorPaymentDirective(urlHelperService: UrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getGlobalUrl('Economy/Supplier/Suppliers/Views/ActorPayment.html'),
        replace: true,
        scope: {
            paymentInformation: "=",
            supplierIsLoaded: "=",
            isForegin: "=",
            isNew: "=",
            isCompany: "=?"

        },
        bindToController: true,
        controllerAs: "ctrl",
        controller: ActorPaymentController
    }
}