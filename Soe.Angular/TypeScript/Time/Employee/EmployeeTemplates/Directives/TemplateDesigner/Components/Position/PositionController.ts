import { EmployeeTemplatePositionDTO } from "../../../../../../../Common/Models/EmployeeTemplateDTOs";
import { IUrlHelperService } from "../../../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../../../Scripts/TypeLite.Net4";
import { IEmployeeService } from "../../../../../EmployeeService";

export class PositionFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/Position/Position.html'),
            scope: {
                model: '=',
                isEditMode: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PositionController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PositionController {

    // Init parameters
    private model: string;
    private position: EmployeeTemplatePositionDTO;
    private isEditMode: boolean;
    private onChange: Function;

    // Data
    private positions: ISmallGenericType[];

    // Properties
    private positionId: number;
    private default: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private employeeService: IEmployeeService) {

        this.$q.all([
            this.loadPositions()
        ]).then(() => {
            if (this.model) {
                this.setModel();
            }
        });
    }

    // SERVICE CALLS

    private loadPositions(): ng.IPromise<any> {
        return this.employeeService.getPositionsDict(true, true).then(x => {
            this.positions = x;
        });
    }

    // EVENTS

    private setDirty() {
        if (this.onChange) {
            this.$timeout(() => {
                this.onChange({ jsonString: this.getJsonFromModel() });
            });
        }
    }

    // HELP-METHODS

    private getJsonFromModel(): string {
        this.position = new EmployeeTemplatePositionDTO();
        this.position.positionId = this.positionId;
        this.position.default = this.default;

        return JSON.stringify(this.position);
    }

    private setModel() {
        this.position = new EmployeeTemplatePositionDTO();
        angular.extend(this.position, JSON.parse(this.model));

        this.positionId = this.position.positionId;
        this.default = this.position.default;
    }
}
