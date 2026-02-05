import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPayrollService } from "../PayrollService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    payrollPriceFormulaId : number;
    payrollPriceFormula: any;
    selectedPayrollPriceType: any;
    selectedFixedValue: any;
    selectedPayrollPriceFormula: any;

    // Lookups
    payrollPriceTypes: any;
    fixedValues: any;
    payrollPriceFormulas: any;

    caretPosition: number = 0;

    result: number = 0;
    errorMessage: string;
    hasIllegalIdentifier: boolean = false;
    showInfo: boolean = false;

    terms: any = [];

    // Properties
    get code() {
        if (this.payrollPriceFormula && this.payrollPriceFormula.code)
            return this.payrollPriceFormula.code;
        else
            return "";
    }
    set code(value: string) {
        if (value !== undefined)
            this.payrollPriceFormula.code = value.toUpperCase();
        else
            this.payrollPriceFormula.code = value;
    }

    get formula() {
        if (this.payrollPriceFormula && this.payrollPriceFormula.formulaPlain)
            return this.payrollPriceFormula.formulaPlain;
        else
            return "";
    }
    set formula(value: string) {
        this.payrollPriceFormula.formulaPlain = value;
        this.addIdentifiersFromFormula();
        this.evaluateFormula();
    }

    private identifiers: string = "";
    get identifiersList() {
        return this.identifiers;
    }
    set identifiersList(value: string) {
        this.identifiers = value;
        this.evaluateFormula();
    }

    //@ngInject
    constructor(
        private $q: ng.IQService, 
        private $timeout: ng.ITimeoutService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private notificationService: INotificationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.payrollPriceFormulaId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_SalarySettings_PriceType_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_SalarySettings_PriceType_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PriceType_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.payrollPriceFormulaId, recordId => {
            if (recordId !== this.payrollPriceFormulaId) {
                this.payrollPriceFormulaId = recordId;
                this.onLoadData();
            }
        });
    }

    // LOOKUPS

    protected doLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadPayrollPriceTypes(),
                this.loadFixedValues(),
                this.loadPayrollPriceFormulas()
            ]).then(x => {
                this.addIdentifiersFromFormula();
            });
        });
    }

    private onLoadData() {
        if (this.payrollPriceFormulaId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    } 

    private load(): ng.IPromise<any>{
        return this.payrollService.getPayrollPriceFormula(this.payrollPriceFormulaId,).then((x) => {
            this.isNew = false;
            this.payrollPriceFormula = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.payroll.payrollpriceformula.payrollpriceformula"] + ' ' + this.payrollPriceFormula.name);
        });        
    }

    private loadTerms() {
        var keys: string[] = [
            "time.payroll.payrollpriceformula.payrollpriceformula"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadPayrollPriceTypes() {
        this.payrollService.getPayrollPriceTypesForFormulaBuilder().then((x) => {
            this.payrollPriceTypes = x;
            this.selectedPayrollPriceType = this.payrollPriceTypes[0].code;
        });
    }

    private loadFixedValues() {
        this.payrollService.getFixedValuesForFormulaBuilder().then((x) => {
            this.fixedValues = x;
            this.selectedFixedValue = this.fixedValues[0].code;
        });
    }

    private loadPayrollPriceFormulas() {
        if (this.payrollPriceFormulaId > 0) {
            this.payrollService.getPayrollPriceFormulasForFormulaBuilder(this.payrollPriceFormulaId).then((x) => {
                this.payrollPriceFormulas = x;
                this.selectedPayrollPriceFormula = this.payrollPriceFormulas[0].code;
            });
        }
    }

    // EVENTS

    private formulaFocused(id: string) {
        // Reposition cursor
        this.$timeout(() => {
            HtmlUtility.setCaretPosition(id, this.caretPosition);
        });
    }

    private formulaBlurred(id: string) {
        // Remember current cursor position
        // To be able to reposition it after adding text from buttons etc
        this.caretPosition = HtmlUtility.getCaretPosition(id);
        this.formula = this.formula.toLocaleUpperCase();
    }

    private operandSelected($event) {
        var btn: HTMLButtonElement = $event.target;
        if (btn)
            this.addToFormula(btn.innerText);
    }

    private payrollPriceTypeSelected(code) {
        if (code) {
            this.addToFormula(code);
        }

        // Clear selected value
        this.$timeout(() => {
            this.selectedPayrollPriceType = this.payrollPriceTypes[0].code;
        });
    }

    private fixedValueSelected(code) {
        if (code) {
            this.addToFormula(code);
        }

        // Clear selected value
        this.$timeout(() => {
            this.selectedFixedValue = this.fixedValues[0].code;
        });
    }

    private payrollPriceFormulaSelected(code) {
        if (code) {
            this.addToFormula(code);
        }

        // Clear selected value
        this.$timeout(() => {
            this.selectedPayrollPriceFormula = this.payrollPriceFormulas[0].code;
        });
    }

    private toggleInfo() {
        this.showInfo = !this.showInfo;
    }

    // ACTIONS        

    private evaluateFormula() {
        if (this.formula) {
            this.payrollService.evaluateFormula(this.formula, this.getIdentifiers()).then((result) => {
                if (result.success) {
                    this.errorMessage = "";
                    this.result = Number(result.stringValue);
                } else {
                    this.errorMessage = result.errorMessage;
                    this.result = 0;
                }
            });
        } else {
            this.errorMessage = "";
            this.result = 0;
        }
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollPriceFormula(this.payrollPriceFormula).then((result) => {
                if (result.success && result.canUserOverride) {
                    const modal = this.notificationService.showDialogEx(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    modal.result.then(modalResult => {
                        
                    });
                }
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.payrollPriceFormulaId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.payrollPriceFormula.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.payrollPriceFormulaId = result.integerValue;
                        this.payrollPriceFormula.payrollPriceFormulaId = this.payrollPriceFormulaId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payrollPriceFormula);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.payrollService.getPayrollPriceFormulas().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.payrollPriceFormulaId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.payrollPriceFormulaId) {
                    this.payrollPriceFormulaId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {

        if (!this.payrollPriceFormula.payrollPriceFormulaId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deletePayrollPriceFormula(this.payrollPriceFormula.payrollPriceFormulaId).then((result) => {
                if (result.success) {
                    completion.completed(this.payrollPriceFormula, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS

    protected copy() {
        super.copy();
        this.isNew = true;
        this.payrollPriceFormulaId = 0;
        this.payrollPriceFormula.payrollPriceFormulaId = 0;
    }

    private new() {
        this.isNew = true;
        this.payrollPriceFormulaId = 0;
        this.payrollPriceFormula = {
        };
    }

    private addToFormula(text: string) {
        if (text && text.length > 0) {
            text = text.toUpperCase();

            // Add specified text to the formula at the cursor position
            var currentText = this.formula;
            if (currentText && currentText.length > 0) {
                this.formula = currentText.substr(0, this.caretPosition) + text + currentText.substr(this.caretPosition);
            }
            else
                this.formula = text;

            this.caretPosition = this.caretPosition + text.length;

            var elem: HTMLElement = document.getElementById("formula");
            if (elem)
                elem.focus();
        }
    }

    private addToIdentifiers(text: string) {
        if (text && text.length > 0) {
            text = text.toUpperCase();

            // Check if identifier already exists
            var exists: boolean = false;
            var identifiers: string[] = this.getIdentifiers();
            _.forEach(identifiers, (identifier) => {
                var pos: number = identifier.indexOf('=');
                if (pos > 0 && identifier.substr(0, pos).trim() === text) {
                    exists = true;
                }
            });

            // Add it to the list if it does not exist
            if (!exists) {
                if (this.identifiersList.length > 0)
                    this.identifiersList = this.identifiersList + "\n";

                this.identifiersList = this.identifiersList + text + "=0";
            }
        }
    }

    private addIdentifiersFromFormula() {
        // Split formula on words and check if the codes exists in price types, fixed values or formulas,
        // in that case add it to the identifiers list.
        this.hasIllegalIdentifier = false;
        this.removeUnusedIdentifiers();

        // Get codes from formula
        var codes: string[] = this.getFormulaCodes();
        _.forEach(codes, (code) => {
            if (_.find(this.payrollPriceTypes, { code: code })) {
                this.addToIdentifiers(code);
            } else if (_.find(this.fixedValues, { code: code })) {
                this.addToIdentifiers(code);
            } else if (_.find(this.payrollPriceFormulas, { code: code })) {
                this.addToIdentifiers(code);
            } else {
                this.hasIllegalIdentifier = true;
            }
        });
    }

    private removeUnusedIdentifiers() {
        // Get codes from formula
        var codes: string[] = this.getFormulaCodes();
        // Get existing identifiers
        var identifiers: string[] = this.getIdentifiers();
        // Loop through identifiers and only copy those in the formula
        this.identifiersList = "";
        _.forEach(identifiers, (identifier) => {
            var pos: number = identifier.indexOf('=');
            if (pos > 0 && _.includes(codes, identifier.substr(0, pos).trim())) {
                if (this.identifiersList.length > 0)
                    this.identifiersList = this.identifiersList + "\n";

                this.identifiersList = this.identifiersList + identifier;
            }
        });
    }

    private getIdentifiers() {
        // Split identifiers into a list
        var identifiers: string[] = [];
        if (this.identifiersList.length > 0)
            identifiers = this.identifiersList.split("\n");

        return identifiers;
    }

    private getFormulaCodes(): string[] {
        var frm: string = this.formula;
        frm = frm.replace(" ", "").toUpperCase();

        var regex = /[A-ZÅÄÖ0-9_]+/gi;
        var match;
        var codes = new Array();
        while ((match = regex.exec(frm)) !== null) {
            codes.push(match[0]);
        }

        return codes;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollPriceFormula) {
                // Mandatory fields
                if (!this.payrollPriceFormula.code)
                    mandatoryFieldKeys.push("common.code");
                if (!this.payrollPriceFormula.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.payrollPriceFormula.formula)
                    mandatoryFieldKeys.push("time.payroll.payrollpriceformula.formula");
            }
        });
    }
}
