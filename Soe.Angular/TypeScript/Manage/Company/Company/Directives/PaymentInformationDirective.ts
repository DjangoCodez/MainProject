import { IPaymentInformationDTO, IPaymentInformationRowDTO } from "../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { Feature, TermGroup, TermGroup_SysPaymentType  } from "../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { CoreUtility } from "../../../../Util/CoreUtility";

export class PaymentInformationDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Company/Company/Directives/PaymentInformation.html'),
            scope: {
                paymentInformation: "=",
                isLocked: "="
            },
            restrict: 'E',
            replace: true,
            controller: PaymentInformationController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class PaymentInformationController extends GridControllerBase2Ag implements ICompositionGridController {
    // Collections
    sysPaymentTypes: any = [];
    countries: SmallGenericType[];
    banks: any[];
    currencyCodes: any[];

    paymentInformation: IPaymentInformationDTO;
    paymentInformationRows: IPaymentInformationRowDTO[];
    isIbanValid = true;
    lookupCompleted = false;

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
            .onDoLookUp(() => this.loadLookups())

        this.flowHandler.start({ feature: Feature.Manage_Companies_Edit, loadReadPermissions: true, loadModifyPermissions: true });

        this.$scope.$on('stopEditing', (e, a) => {
            this.gridAg.options.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete();
            }, 100)
        });
    }

    onInit() {
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCurrencies(),
            this.getPaymentInformationFromActor(),
            ]).then(() => {
            this.lookupCompleted = true;
        });
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
    
    private setupGrid(): ng.IPromise<any> {
        this.doubleClickToEdit = false;
        this.gridAg.options.enableFiltering = false;
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableRowSelection = false;
        
        this.gridAg.options.setMinRowsToShow(7);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);

        const keys: string[] = [
            "core.remove", "economy.supplier.supplier.paymenttype", "economy.supplier.supplier.account",
            "economy.supplier.supplier.defaultwithinpayment", "economy.supplier.supplier.bic", "manage.company.bankconnected",
            "common.standard", "manage.company.bankinfo", "manage.company.accountinfo", "manage.company.showoninvoice",
            "economy.supplier.invoice.currencycode"
        ];

        const enableEdit = (row) => !row.bankConnected;

        return this.translationService.translateMany(keys).then((terms) => {
            const bankHeader = this.gridAg.options.addColumnHeader("bank", terms["manage.company.bankinfo"], null);
            bankHeader.marryChildren = true;
            this.gridAg.addColumnText("bic", terms["economy.supplier.supplier.bic"], null, false, { enableHiding: false, editable: enableEdit }, bankHeader);

            const accountHeader = this.gridAg.options.addColumnHeader("account", terms["manage.company.accountinfo"], null);
            accountHeader.marryChildren = true;
            this.gridAg.addColumnSelect("sysPaymentTypeId", terms["economy.supplier.supplier.paymenttype"], null, { editable: enableEdit, selectOptions: this.sysPaymentTypes, displayField: "sysPaymentTypeName", dropdownValueLabel: "label", dropdownIdLabel: "value" }, accountHeader); // "sysPaymentTypeChanged");
            this.gridAg.addColumnText("paymentNr", terms["economy.supplier.supplier.account"], null, false, { enableHiding: false, editable: enableEdit }, accountHeader);
            this.gridAg.addColumnSelect("currencyId", terms["economy.supplier.invoice.currencycode"], null, { enableHiding: true, editable: enableEdit, displayField: "currencyCode", dropdownIdLabel: "value", dropdownValueLabel: "label", selectOptions: this.currencyCodes }, accountHeader);
            this.gridAg.addColumnBoolEx("default", terms["common.standard"], null, { enableEdit: true, onChanged: this.defaultChanged.bind(this), maxWidth: 50 }, accountHeader);
            this.gridAg.addColumnBoolEx("shownInInvoice", terms["manage.company.showoninvoice"], 100, { enableEdit: true, maxWidth: 100 }, accountHeader);

            if (CoreUtility.isSupportAdmin)
                this.gridAg.addColumnIcon("bankConnectedIcon", "", null, { onClick: this.onBankConnected.bind(this), showIcon: () => { return true }, toolTipField: "bankConnectedToolTip" });
            else
                this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-link iconEdit", showIcon: (row) => row.bankConnected, toolTip: terms["manage.company.bankconnected"] });

            this.gridAg.addColumnDelete(terms["common.remove"], this.deleteRow.bind(this), false, (row) => !row.bankConnected);
            this.gridAg.finalizeInitGrid("common.supplier.actorpayments", false);

            this.setupWatches();
            
        });
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
            case "paymentNr":
                {
                    if (row.sysPaymentTypeId == TermGroup_SysPaymentType.BIC) {
                        const paymentNrSplitted: string[] = newValue.split('/');
                        if (paymentNrSplitted.length === 2) {
                            this.validateIban(paymentNrSplitted[1]).then((ok: boolean) => {
                                if (!ok) {
                                    this.focusePaymentNr(row);
                                }
                            });
                        }
                    }
                    else if (row.sysPaymentTypeId == TermGroup_SysPaymentType.SEPA) {
                        this.validateIban(row.paymentNr).then((ok: boolean) => {
                            if (!ok) {
                                this.focusePaymentNr(row);
                            }
                        });
                    }

                    break;
                }
        }

        this.messagingHandler.publishSetDirty();
    }

    public focusePaymentNr(row:any) {
        this.$timeout(() => {
            this.gridAg.options.startEditingCell(row, "paymentNr");
        });
    }

    public onBankConnected(row: IPaymentInformationRowDTO) {
        const bankConnectedTooltip = this.translationService.translateInstant("manage.company.bankconnected");
        row.bankConnected = !row.bankConnected;
        this.setRowIcons(row, bankConnectedTooltip);
        this.gridAg.options.refreshRows(row);
        this.messagingHandler.publishSetDirty();
    }

    public defaultChanged(rowData) {
        const row: IPaymentInformationRowDTO = rowData.data as IPaymentInformationRowDTO;
        this.paymentInformation.rows.forEach((x: IPaymentInformationRowDTO) => {
            if (x.sysPaymentTypeId === row.sysPaymentTypeId && x.paymentInformationRowId !== row.paymentInformationRowId && x.default) {
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

    public loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesGrid().then((x) => {
            this.currencyCodes = [];
            this.currencyCodes.push({ value: 0, label: " " });
            _.forEach(x, (y: any) => {
                this.currencyCodes.push({ value: y.currencyId, label: y.code });
            });
        });
    }

    public getPaymentInformationFromActor(): ng.IPromise<any> {
        const promises: ng.IPromise<any>[] = [];

        promises.push(this.coreService.getTermGroupContent(TermGroup.SysPaymentType, false, false).then((result) => {
            _.forEach(result, (sysPaymentType: any) => {
                if (sysPaymentType.id !== TermGroup_SysPaymentType.SEPA)
                    this.sysPaymentTypes.push({ value: sysPaymentType.id, label: sysPaymentType.name, sysPaymentTypeId: sysPaymentType.id });
            });
        }));

        return this.$q.all(promises);
    }

    protected setpaymentInformationRow(): void {        
        if (!this.paymentInformation) {
            this.gridAg.options.clearData();
            return;
        }
        if (this.paymentInformation.rows) {
            this.paymentInformation.rows.forEach((paymentInformationRow: IPaymentInformationRowDTO) => {
                _.forEach(this.sysPaymentTypes,
                    (sysPaymentType: any) => {
                        if (paymentInformationRow.sysPaymentTypeId === sysPaymentType.value) {
                            paymentInformationRow.sysPaymentTypeName = sysPaymentType.label;
                        }
                    });

                if (paymentInformationRow.currencyId && paymentInformationRow.currencyId > 0) {
                    var currency = _.find(this.currencyCodes, (c) => c.value === paymentInformationRow.currencyId)
                    if (currency)
                        paymentInformationRow.currencyCode = currency.label;
                }

                if (!paymentInformationRow.currencyCode) 
                    paymentInformationRow.currencyCode = "";
            });
        }

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
        return "bic";
    }

    private addRow() {
        if (!this.paymentInformation) {
            this.paymentInformation = <IPaymentInformationDTO>{};
            this.paymentInformation.rows = [];
        }
        if (!this.paymentInformation.rows)
            this.paymentInformation.rows = [];

        const newPaymentInformationRow = <IPaymentInformationRowDTO>{};
        newPaymentInformationRow.paymentNr = "";
        newPaymentInformationRow.intermediaryCode = 0;
        _.forEach(this.sysPaymentTypes, (sysPaymentType: any) => {
            if (sysPaymentType.value === 1) {
                newPaymentInformationRow.sysPaymentTypeId = sysPaymentType.value;
                newPaymentInformationRow.sysPaymentTypeName = sysPaymentType.label;
            };                                                                                                                                                                                                                          
        });

        if (this.paymentInformation.rows.length === 0)
            newPaymentInformationRow.default = true;

        this.paymentInformation.rows.push(newPaymentInformationRow);
        this.loadGridData(true, false);
        this.gridAg.options.startEditingCell(newPaymentInformationRow, this.getFirstColumnName());
    }

    private loadGridData(setDirty: boolean, setPaymentInfo: boolean) {        
        if (!this.paymentInformation) {
            return;
        }

        const bankConnectedTooltip = this.translationService.translateInstant("manage.company.bankconnected");
        if (setDirty) {
            this.messagingHandler.publishSetDirty();
        }

        if (setPaymentInfo) {
            this.setpaymentInformationRow();
        }

        if (this.paymentInformation.rows) {
            this.paymentInformation.rows.forEach(r => {
                this.setRowIcons(r, bankConnectedTooltip);
            })
        }

        this.gridAg.setData(this.paymentInformation.rows);
    }

    private setRowIcons(row: IPaymentInformationRowDTO, bankConnectedToolTip:string) {
        row["bankConnectedIcon"] = (row.bankConnected) ? "fal fa-link iconEdit" : "fal fa-unlink";
        if (row.bankConnected) {
            row["bankConnectedToolTip"] = bankConnectedToolTip;
        }
    }

    private validateIban(iban: string): ng.IPromise<any> {        
        const deferral = this.$q.defer<boolean>();
        this.coreService.validIBANNumber(iban).then(isValid => {            
            if (isValid)
            {
                deferral.resolve(isValid);
            }
            else {
                const keys: string[] = [
                    "core.warning",
                    "economy.supplier.supplier.ibannotvalid"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    const modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.supplier.ibannotvalid"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    modal.result.then(() => {
                        deferral.resolve(isValid);
                    });
                });
            }   
        });        

        return deferral.promise;
    }
}