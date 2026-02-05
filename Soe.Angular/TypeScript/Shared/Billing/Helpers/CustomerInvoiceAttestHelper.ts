import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IAttestTransitionDTO } from "../../../Scripts/TypeLite.Net4";
import { CompanySettingType, SoeInvoiceRowType, TermGroup_AttestEntity } from "../../../Util/CommonEnumerations";
import { SOEMessageBoxButtons, SOEMessageBoxImage, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ISoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";


//Should encapsulate all attest transitions / state logic in HandleBilling, PurchaseCustomerInvoiceRows and PurchaseRows directive.
//Only used at PurchaseCustomerInvoiceRows for now.

export class CustomerInvoiceAttestHelper {
    private initialAttestState: AttestStateDTO;

    private attestTransitions: IAttestTransitionDTO[] = [];
    private attestStates: AttestStateDTO[] = [];

    private availableAttestStates: AttestStateDTO[] = [];
    private availableAttestStateOptions: { name: string, id: number }[];
    private excludedAttestStates: number[] = [];

    public selectedAttestState: number;

    constructor(
        private $q,
        private $timeout,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private grid: ISoeGridOptionsAg,
    ) {}

    public setup(entity: TermGroup_AttestEntity, startDate?: Date, stopDate?: Date): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadUserAttestTransitions(entity, startDate, stopDate),
        ])
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.BillingStatusTransferredOrderToInvoice,
            CompanySettingType.BillingStatusTransferredOrderToContract,
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            const attestStateTransferredOrderToInvoiceId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToInvoice);
            if (attestStateTransferredOrderToInvoiceId !== 0)
                this.excludedAttestStates.push(attestStateTransferredOrderToInvoiceId);

            const attestStateTransferredOrderToContractId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToContract);
            if (attestStateTransferredOrderToContractId !== 0)
                this.excludedAttestStates.push(attestStateTransferredOrderToContractId);
        });
    }

    private attestStateChanged(ignoreDeselect = false) {
        this.$timeout(() => {

            if (!this.selectedAttestState) return;

            const attestState = this.getSelectedAttestState();

            if (!attestState) {
                if (!ignoreDeselect) {
                    const selectedRows = this.grid.getSelectedRows();
                    selectedRows.forEach(r => {
                        if (r.type === SoeInvoiceRowType.BaseProductRow || r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow)
                            this.grid.unSelectRow(r);
                    })
                }
            }
            else {
                const filteredRows = this.grid.getFilteredRows();
                filteredRows.forEach(r => {
                    if (r.type !== SoeInvoiceRowType.AccountingRow) {
                        if (this.attestTransitions.find(a => a.attestStateFromId === r.attestStateId && attestState.attestStateId === a.attestStateToId))
                            this.grid.selectRow(r);
                        else
                            this.grid.unSelectRow(r);
                    }
                })
            }
            this.grid.refreshRows();
        });
    }

    public getSelectedAttestState(): AttestStateDTO {
        return this.attestStates.find(a => a.attestStateId === this.selectedAttestState);
    }

    private loadUserAttestTransitions(entity: TermGroup_AttestEntity, startDate?: Date, stopDate?: Date): ng.IPromise<any> {
        return this.coreService.getUserAttestTransitions(entity, startDate, stopDate).then(x => {
            this.attestTransitions = x;

            // Add states from returned transitions
            this.attestTransitions.forEach(t => {
                if (!this.attestStates.find(a => a.attestStateId === t.attestStateToId))
                    this.attestStates.push(t.attestStateTo)
            })

            // Sort states
            this.attestStates = this.attestStates.sort(a => a.sort);

            // Get initial state
            this.initialAttestState = this.attestStates.find(a => a.initial === true);
            if (!this.initialAttestState) {
                this.loadInitialAttestState(entity);
            }

            // Setup available states (exclude finished states)
            this.availableAttestStates = [];
            this.attestStates.forEach(attestState => {
                if (!this.excludedAttestStates.find(ex => ex === attestState.attestStateId)) {
                    this.availableAttestStates.push({ ...new AttestStateDTO(), ...attestState });
                }
            })

            // Setup available states for selector
            this.availableAttestStateOptions = [];
            this.translationService.translate("billing.productrows.changeatteststate").then(s => {
                this.availableAttestStateOptions = [
                    { id: 0, name: s },
                    ...this.availableAttestStates.map(a => { return { id: a.attestStateId, name: a.name } })
                ];

                this.selectedAttestState = 0;
            })
        });
    }

    private loadInitialAttestState(entity: TermGroup_AttestEntity) {
        this.coreService.getAttestStateInitial(entity).then(x => {
            this.initialAttestState = x;

            if (!this.initialAttestState) {
                const keys: string[] = [
                    "billing.productrows.initialstatemissing.title",
                    "billing.productrows.initialstatemissing.message"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(
                        terms["billing.productrows.initialstatemissing.title"],
                        terms["billing.productrows.initialstatemissing.message"],
                        SOEMessageBoxImage.Error,
                        SOEMessageBoxButtons.OK,
                        SOEMessageBoxSize.Large
                    );
                });
            } else {
                this.attestStates.push(this.initialAttestState);
                // Sort states
                this.attestStates = _.orderBy(this.attestStates, 'sort');
            }
        });
    }
}