import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Feature, TermGroup, SoeEntityState, SoeTimeCodeType, SoeTimeRuleType, SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { TimeRuleEditDTO, TimeRuleExpressionDTO, TimeRuleOperandDTO } from "../../../Common/Models/TimeRuleDTOs";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { FormulaWidget, ExpressionWidget, OperatorWidget } from "../../../Common/Models/FormulaBuilderDTOs";
import { Guid } from "../../../Util/StringUtility";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ImportedDetailsDialogController } from "./Dialogs/ImportedDetails/ImportedDetailsDialogController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeRuleId: number;
    private timeRule: TimeRuleEditDTO;
    private types: ISmallGenericType[] = [];
    private directions: ISmallGenericType[] = [];
    private timeCodes: ISmallGenericType[] = [];
    private employeeGroups: ISmallGenericType[] = [];
    private scheduleTypes: ISmallGenericType[] = [];
    private timeDeviationCauses: ISmallGenericType[] = [];
    private dayTypes: ISmallGenericType[] = [];

    // Properties
    private selectedEmployeeGroups: ISmallGenericType[] = [];
    private selectedScheduleTypes: ISmallGenericType[] = [];
    private selectedTimeDeviationCauses: ISmallGenericType[] = [];
    private selectedDayTypes: ISmallGenericType[] = [];

    private get isTypeAbsence() {
        return this.timeRule && this.timeRule.type === SoeTimeRuleType.Absence;
    }

    // Flags
    private isLoading: boolean = false;

    // Rule definitions
    private operatorWidth: number = 80;
    private expWidgets: ExpressionWidget[] = [];
    private opWidgets: OperatorWidget[] = [];
    private startWidgets: FormulaWidget[] = [];
    private stopWidgets: FormulaWidget[] = [];
    private startFormulaValid: string;
    private startFormulaError: string;
    private stopFormulaValid: string;
    private stopFormulaError: string;

    private edit: ng.IFormController;

    private modal;
    private isModal = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private scheduleService: IScheduleService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;

            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeRuleId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeRule_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
        this.navigatorRecords = parameters.navigatorRecords;
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeRule_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeRule_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        if (!this.isModal) {
            this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeRuleId);

            this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeRuleId, recordId => {
                if (recordId !== this.timeRuleId) {
                    this.timeRuleId = recordId;
                    this.onLoadData();
                }
            });

            if (CoreUtility.isSupportAdmin) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.time.timerule.imported", "time.time.timerule.imported.tooltip", IconLibrary.FontAwesome, "fa-cloud-download", () => {
                    this.showImportedDetails();
                }, null, () => !this.timeRule.imported)));
            }
        }
    }

    private setupWidgets() {
        this.expWidgets = [];
        this.expWidgets.push(new ExpressionWidget("scheduleInWidget", null, null, { isStandby: this.timeRule.isStandby }));
        this.expWidgets.push(new ExpressionWidget("scheduleOutWidget", null, null, { isStandby: this.timeRule.isStandby }));
        this.expWidgets.push(new ExpressionWidget("clockWidget"));
        this.expWidgets.push(new ExpressionWidget("balanceWidget", null, 355));
        this.expWidgets.push(new ExpressionWidget("notWidget"));

        this.opWidgets = [];
        //this.opWidgets.push(new OperatorWidget("startParanthesisWidget", this.operatorWidth));
        //this.opWidgets.push(new OperatorWidget("endParanthesisWidget", this.operatorWidth));
        this.opWidgets.push(new OperatorWidget("andWidget", this.operatorWidth));
        this.opWidgets.push(new OperatorWidget("orWidget", this.operatorWidth));
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.startWidgets, (newVal, oldVal) => {
            this.validateRuleStructure(this.startWidgets, 0);
        }, true);
        this.$scope.$watch(() => this.stopWidgets, (newVal, oldVal) => {
            this.validateRuleStructure(this.stopWidgets, 1);
        }, true);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadTypes(),
                this.loadDirections(),
                this.loadTimeCodes(),
                this.loadEmployeeGroups(),
                this.loadScheduleTypes(),
                this.loadTimeDeviationCauses(),
                this.loadDayTypes()
            ]);
        });
    }

    private onLoadData(): ng.IPromise<any> {
        this.isLoading = true;
        this.setupWatchers();

        if (this.timeRuleId) {
            return this.load().then(() => {
                this.isLoading = false;
            });
        } else {
            this.isLoading = false;
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all",
            "common.formulabuilder.expression.standbyin",
            "common.formulabuilder.expression.standbyout",
            "time.time.timerule.scheduletype.all",
            "time.time.timerule.validatesuccess",
            "time.time.timerule.formulaerror.nowidgets",
            "time.time.timerule.formulaerror.firstwidgetincorrect",
            "time.time.timerule.formulaerror.operatorafterparentheses",
            "time.time.timerule.formulaerror.severaloperatorsinarow",
            "time.time.timerule.formulaerror.severalexpressionsinarow",
            "time.time.timerule.formulaerror.lastwidgetincorrect",
            "time.time.timerule.formulaerror.incorrectamountofparenthesis"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.startFormulaValid = this.stopFormulaValid = this.terms["time.time.timerule.validatesuccess"];
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeRuleType, false, false).then(x => {
            this.types = x;
        });
    }

    private loadDirections(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeRuleDirection, false, false).then(x => {
            this.directions = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction, false, false).then(x => {
            this.timeCodes = x;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            this.employeeGroups = x;
            this.employeeGroups.splice(0, 0, new SmallGenericType(0, this.terms["common.all"]));
        });
    }

    private loadScheduleTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTypesDict(true, false).then(x => {
            this.scheduleTypes = x;
            this.scheduleTypes.splice(0, 0, new SmallGenericType(0, this.terms["time.time.timerule.scheduletype.all"]));
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
            this.timeDeviationCauses = x;
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.scheduleService.getDayTypesDict(false).then(x => {
            this.dayTypes = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeRule(this.timeRuleId).then(x => {
                this.isNew = false;
                this.timeRule = x;

                if (!this.timeRule) {
                    if (this.isModal)
                        this.closeModal();
                    else
                        this.new();
                } else {
                    this.selectedEmployeeGroups = _.filter(this.employeeGroups, g => (this.timeRule.employeeGroupIds.length > 0 ? _.includes(this.timeRule.employeeGroupIds, g.id) : g.id === 0));
                    this.selectedScheduleTypes = _.filter(this.scheduleTypes, s => (this.timeRule.timeScheduleTypeIds.length > 0 ? _.includes(this.timeRule.timeScheduleTypeIds, s.id) : s.id === 0));
                    this.selectedTimeDeviationCauses = _.filter(this.timeDeviationCauses, t => _.includes(this.timeRule.timeDeviationCauseIds, t.id));
                    this.selectedDayTypes = _.filter(this.dayTypes, d => _.includes(this.timeRule.dayTypeIds, d.id));

                    this.populateFormulas();

                    this.dirtyHandler.clean();
                    this.messagingHandler.publishSetTabLabel(this.guid, '{0} [{1}]'.format(this.timeRule.name, this.timeRule.timeRuleId.toString()));
                }
                this.setupWidgets();
            });
        }]);
    }

    private new() {
        this.isNew = true;
        this.timeRuleId = 0;
        this.timeRule = new TimeRuleEditDTO();
        this.timeRule.type = SoeTimeRuleType.Presence;
        if (this.directions.length > 0)
            this.timeRule.ruleStartDirection = this.directions[0].id;
        this.timeRule.state = SoeEntityState.Active;

        this.validateRuleStructure(this.startWidgets, 0);
        this.validateRuleStructure(this.stopWidgets, 1);

        this.setupWidgets();
        this.focusService.focusByName("ctrl_timeRule_name");
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeRuleId = 0;
        this.timeRule.timeRuleId = 0;

        this.focusService.focusByName("ctrl_timeRule_name");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.timeRule.employeeGroupIds = this.selectedEmployeeGroups.map(e => e.id);
        this.timeRule.timeScheduleTypeIds = this.selectedScheduleTypes.map(e => e.id);
        this.timeRule.timeDeviationCauseIds = this.selectedTimeDeviationCauses.map(e => e.id);
        this.timeRule.dayTypeIds = this.selectedDayTypes.map(e => e.id);
        this.timeRule.factor = 1;

        this.timeRule.timeRuleExpressions = [];
        this.convertWidgetsToTimeRuleExpressions(true);
        this.convertWidgetsToTimeRuleExpressions(false);

        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeRule(this.timeRule).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeRuleId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeRule.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.timeRuleId = result.integerValue;
                        this.timeRule.timeRuleId = this.timeRuleId;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeRule);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            if (this.isModal)
                this.closeModal();
            else
                this.onLoadData();
        }, error => {
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeRules().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeRuleId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeRuleId) {
                    this.timeRuleId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeRule(this.timeRule.timeRuleId).then(result => {
                if (result.success) {
                    completion.completed(this.timeRule, true);
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

    private showImportedDetails() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeRules/Dialogs/ImportedDetails/ImportedDetailsDialog.html"),
            controller: ImportedDetailsDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                timeRuleId: () => { return this.timeRuleId },
            }
        }
        this.$uibModal.open(options);
    }

    // EVENTS

    public closeModal() {
        if (this.isModal) {
            this.modal.dismiss();
        }
    }

    private typeChanged() {
        this.$timeout(() => {
            if (this.timeRule.type === SoeTimeRuleType.Absence)
                this.timeRule.isStandby = false;
        });
    }


    private standardMinutesChanged() {
        this.$timeout(() => {
            if (this.timeRule)
                this.timeRule.timeCodeMaxLength = null;
        });
    }

    private timeCodeMaxLengthChanged() {
        this.$timeout(() => {
            if (this.timeRule)
                this.timeRule.standardMinutes = null;
        });
    }

    private isStandbyChanged() {
        this.$timeout(() => {
            this.setupWidgets();
            this.populateFormulas();
        });
    }

    private selectedEmployeeGroupsChanged() {
        if (this.selectedEmployeeGroups.length === 0)
            this.selectedEmployeeGroups = _.filter(this.employeeGroups, g => g.id === 0);
        else if (_.filter(this.selectedEmployeeGroups, g => g.id === 0).length > 0 && _.filter(this.selectedEmployeeGroups, g => g.id > 0).length > 0)
            this.selectedEmployeeGroups = _.filter(this.selectedEmployeeGroups, g => g.id > 0);
        this.setDirty();
    }

    private widgetDropped(widget: FormulaWidget, containerIndex: number) {
        if (widget) {
            if (containerIndex === 0) {
                let maxId: number = (this.startWidgets && this.startWidgets.length > 0) ? _.maxBy(this.startWidgets, w => w.internalId).internalId : 0;
                widget.internalId = maxId + 1;
                widget.sort = this.startWidgets.length + 1;
                this.startWidgets.push(widget);
            } else if (containerIndex === 1) {
                let maxId: number = (this.stopWidgets && this.stopWidgets.length > 0) ? _.maxBy(this.stopWidgets, w => w.internalId).internalId : 0;
                widget.internalId = maxId + 1;
                widget.sort = this.stopWidgets.length + 1;
                this.stopWidgets.push(widget);
            }
            this.setDirty();
        }
    }

    // VALIDATION

    private validateRuleStructure(widgets: FormulaWidget[], containerIndex: number) {
        if (this.isLoading)
            return;

        _.forEach(widgets, widget => {
            if (widget.data) {
                widget['minutes'] = widget.data.minutes;
                widget['comparisonOperator'] = widget.data.comparisonOperator;
                widget['leftValueId'] = widget.data.leftValueId;
                widget['rightValueId'] = widget.data.rightValueId;
            }
        });

        this.timeService.validateTimeRuleStructure(widgets).then(result => {
            let message = result.errorMessage;
            if (containerIndex === 0)
                this.startFormulaError = message;
            else
                this.stopFormulaError = message;
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['edit'].$error;

            if (this.timeRule) {
                if (!this.timeRule.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.timeRule.type)
                    mandatoryFieldKeys.push("common.type");
                if (!this.timeRule.ruleStartDirection)
                    mandatoryFieldKeys.push("time.time.timerule.rulestartdirection");
                if (!this.timeRule.timeCodeId)
                    mandatoryFieldKeys.push("common.timecode");

                if (errors['ruleDefinition'])
                    validationErrorKeys.push("time.time.timerule.formulaerror");
            }
        });
    }

    private validateSave(): boolean {
        return true;
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private populateFormulas() {
        this.startWidgets = [];
        this.stopWidgets = [];

        if (!this.timeRule || !this.timeRule.timeRuleExpressions || this.timeRule.timeRuleExpressions.length === 0)
            return;

        _.forEach(this.timeRule.timeRuleExpressions, outerExpression => {
            let expressionId: number = outerExpression.timeRuleExpressionId;
            let nested: boolean = false;

            _.forEach(this.timeRule.timeRuleExpressions, innerExpression => {
                if (innerExpression.timeRuleExpressionId !== expressionId && !nested)
                    nested = this.isNestedExpression(innerExpression, expressionId);
            });

            if (!nested)
                this.populateFormula(outerExpression);
        });
    }

    private populateFormula(outerExpression: TimeRuleExpressionDTO) {
        _.forEach(this.timeRule.timeRuleExpressions, expression => {
            if (outerExpression.timeRuleExpressionId === expression.timeRuleExpressionId) {
                outerExpression = expression;
                return false;
            }
        });

        // Sort
        let counter: number = 0;
        let sortedOperands: TimeRuleOperandDTO[] = [];
        while (sortedOperands.length !== outerExpression.timeRuleOperands.length) {
            // Prevent continous loop
            if (counter > 100)
                break;

            let operand: TimeRuleOperandDTO = _.find(outerExpression.timeRuleOperands, o => o.orderNbr === counter);
            if (operand)
                sortedOperands.push(operand);

            counter++;
        }

        // Create widgets
        _.forEach(sortedOperands, operand => {
            if (operand.timeRuleExpressionRecursive) {
                this.createFormulaWidget(outerExpression.isStart, new TimeRuleOperandDTO(SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis));
                this.populateFormula(operand.timeRuleExpressionRecursive);
                this.createFormulaWidget(outerExpression.isStart, new TimeRuleOperandDTO(SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis));
            } else {
                this.createFormulaWidget(outerExpression.isStart, operand);
            }
        });
    }

    private isNestedExpression(expression: TimeRuleExpressionDTO, id: number): boolean {
        if (expression.timeRuleExpressionId === id)
            return true;

        let nested: boolean = false;
        _.forEach(expression.timeRuleOperands, operand => {
            if (!operand.timeRuleExpressionRecursive)
                return false;

            nested = this.isNestedExpression(operand.timeRuleExpressionRecursive, id);
        });

        return nested;
    }

    private createFormulaWidget(isStart: boolean, operand: TimeRuleOperandDTO) {
        let name: string;
        let isExpression: boolean = false;
        let isOperator: boolean = false;
        let data: any;

        switch (operand.operatorType) {
            case SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn:
                name = "scheduleInWidget";
                isExpression = true;
                data = { minutes: operand.minutes, comparisonOperator: operand.comparisonOperator, isStandby: this.timeRule.isStandby };
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut:
                name = "scheduleOutWidget";
                isExpression = true;
                data = { minutes: operand.minutes, comparisonOperator: operand.comparisonOperator, isStandby: this.timeRule.isStandby };
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorClock:
                name = "clockWidget";
                isExpression = true;
                data = { minutes: operand.minutes, comparisonOperator: operand.comparisonOperator };
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorBalance:
                name = "balanceWidget";
                isExpression = true;
                data = { minutes: operand.minutes, comparisonOperator: operand.comparisonOperator, leftValueId: operand.leftValueId, rightValueId: operand.rightValueId };
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorNot:
                name = "notWidget";
                isExpression = true;
                data = { leftValueId: operand.leftValueId };
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis:
                name = "startParanthesisWidget";
                isOperator = true;
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis:
                name = "endParanthesisWidget";
                isOperator = true;
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorAnd:
                name = "andWidget";
                isOperator = true;
                break;
            case SoeTimeRuleOperatorType.TimeRuleOperatorOr:
                name = "orWidget";
                isOperator = true;
                break;
        }

        let widget: FormulaWidget = new FormulaWidget(!this.modifyPermission, name, isExpression ? (operand.operatorType === SoeTimeRuleOperatorType.TimeRuleOperatorBalance ? 350 : 170) : this.operatorWidth);
        widget.timeRuleType = operand.operatorType;
        widget.isExpression = isExpression;
        widget.isOperator = isOperator;
        widget.isFormula = true;
        widget.internalId = widget.sort = (isStart ? this.startWidgets.length : this.stopWidgets.length) + 1;
        widget.data = data;

        if (isStart)
            this.startWidgets.push(widget);
        else
            this.stopWidgets.push(widget);
    }

    private convertWidgetsToTimeRuleExpressions(isStart: boolean) {
        let timeRuleExpressions: TimeRuleExpressionDTO[] = [];

        let expression: TimeRuleExpressionDTO = new TimeRuleExpressionDTO();
        expression.isStart = isStart;
        expression.timeRuleOperands = [];

        let seqNr: number = 0;
        let nestedLevel: number = 0;
        let operand: TimeRuleOperandDTO = null;
        let nestedOperand: TimeRuleOperandDTO = null;

        _.forEach(_.orderBy(isStart ? this.startWidgets : this.stopWidgets, w => w.sort), widget => {
            operand = null;
            if (widget.timeRuleType !== SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis && widget.timeRuleType !== SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis) {
                operand = widget.isExpression ? this.parseExpressionControl(widget) : this.parseOperatorControl(widget);
                operand.orderNbr = seqNr;
            }

            // Handle nesting
            if (widget.timeRuleType === SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis) {
                nestedLevel++;
                if (!nestedOperand) {
                    nestedOperand = new TimeRuleOperandDTO(SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis);
                    nestedOperand.orderNbr = seqNr;
                }
                seqNr++;
                this.addNewNesting(nestedOperand);
            } else if (widget.timeRuleType === SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis) {
                nestedLevel--;
                if (nestedLevel === 0) {
                    expression.timeRuleOperands.push(nestedOperand);
                    nestedOperand = null;
                }
                let endNestedOperand = new TimeRuleOperandDTO(SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis);
                endNestedOperand.orderNbr = seqNr;
                expression.timeRuleOperands.push(endNestedOperand);
                seqNr++;
            } else if (nestedLevel > 0) {
                this.addToNestedOperand(nestedLevel, 1, operand, nestedOperand);
                seqNr++;
            } else {
                // Normal add
                expression.timeRuleOperands.push(operand);
                seqNr++;
            }
        });

        this.addExpressionsRecursive(expression);
    }

    private parseExpressionControl(widget: FormulaWidget): TimeRuleOperandDTO {
        let operand: TimeRuleOperandDTO = new TimeRuleOperandDTO(widget.timeRuleType);
        operand.minutes = widget.data.minutes;
        operand.comparisonOperator = widget.data.comparisonOperator;
        operand.leftValueId = widget.data.leftValueId;
        operand.rightValueId = widget.data.rightValueId;
        return operand;
    }

    private parseOperatorControl(widget: FormulaWidget): TimeRuleOperandDTO {
        let operand: TimeRuleOperandDTO = new TimeRuleOperandDTO(widget.timeRuleType);
        return operand;
    }

    private addNewNesting(nestedOperand: TimeRuleOperandDTO) {
        if (nestedOperand.timeRuleExpressionRecursive)
            this.addNewNesting(_.last(nestedOperand.timeRuleExpressionRecursive.timeRuleOperands));
        else {
            nestedOperand.timeRuleExpressionRecursive = new TimeRuleExpressionDTO();
            nestedOperand.timeRuleExpressionRecursive.timeRuleOperands = [];
        }
    }

    private addToNestedOperand(nestedLevel: number, currentLevel: number, operand: TimeRuleOperandDTO, nestedOperand: TimeRuleOperandDTO) {
        if (nestedLevel === currentLevel)
            nestedOperand.timeRuleExpressionRecursive.timeRuleOperands.push(operand);
        else
            this.addToNestedOperand(nestedLevel, currentLevel++, operand, _.last(nestedOperand.timeRuleExpressionRecursive.timeRuleOperands));
    }

    private addExpressionsRecursive(expression: TimeRuleExpressionDTO) {
        this.timeRule.timeRuleExpressions.push(expression);
        _.forEach(expression.timeRuleOperands, operand => {
            if (operand.timeRuleExpressionRecursive)
                this.addExpressionsRecursive(operand.timeRuleExpressionRecursive);
        });
    }
}
