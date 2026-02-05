import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { INotificationService } from "../../../Core/Services/NotificationService";

export class RecordNavigatorDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('RecordNavigator', 'RecordNavigator.html'),
            scope: {
                records: '=',
                selectedRecord: '=',
                showAlways: '=?',
                showPosition: '=?',
                showRecordName: '=?',
                showDropdown: '=?',
                dropdownTextProperty: '@',
                dropdownAlignRight: '=?',
                isDate: '=?',
                onSelectionChanged: '&'
            },
            restrict: 'E',
            replace: true,
            controller: RecordNavigatorController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class RecordNavigatorController {

    private index: number;
    private records: any[];small
    private selectedRecord: any;
    private showAlways: boolean;
    private showPosition: boolean;
    private showRecordName: boolean;
    private showDropdown: boolean;
    private dropdownTextProperty: string;
    private dropdownAlignRight: boolean;
    private isDate: boolean;

    public onSelectionChanged: (record: any) => void;

    private isDirty: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService) {
    }

    public $onInit() {
        if (this.selectedRecord)
            this.setIndex();

        this.setupWatchers();

        // Subscribe to modified (dirty) event
        this.messagingService.subscribe(Constants.EVENT_SET_TAB_MODIFIED, (params) => {
            this.isDirty = params.dirty;
        }, this.$scope);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.selectedRecord, (newVal, oldVal) => {
            if (newVal)
                this.setIndex();
        });
        this.$scope.$watch(() => this.records, (newVal, oldVal) => {
            if (!oldVal) {
                this.setIndex();
            }
        });
    }

    private setIndex() {
        let prevIndex = this.index;
        this.index = _.indexOf(this.records, this.selectedRecord);
        if (prevIndex !== this.index && this.selectedRecord && this.onSelectionChanged)
            this.publishSelectionChange();
    }

    private moveFirst() {
        this.validateMove().then(okToMove => {
            if (okToMove) {
                this.selectedRecord = _.first(this.records);
                this.setIndex();
            }
        });
    }

    private movePrev() {
        if (this.index === 0)
            return;

        this.validateMove().then(okToMove => {
            if (okToMove) {
                this.selectedRecord = this.records[this.index - 1];
                this.setIndex();
            }
        });
    }

    private moveNext() {
        if (this.index === this.records.length - 1)
            return;

        this.validateMove().then(okToMove => {
            if (okToMove) {
                this.selectedRecord = this.records[this.index + 1];
                this.setIndex();
            }
        });
    }

    private moveLast() {
        this.validateMove().then(okToMove => {
            if (okToMove) {
                this.selectedRecord = _.last(this.records);
                this.setIndex();
            }
        });
    }

    private selectRecord(record: any) {
        this.validateMove().then(okToMove => {
            if (okToMove) {
                this.selectedRecord = record;
                this.setIndex();
            }
        });
    }

    private validateMove(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (this.isDirty) {
            this.notificationService.showConfirmOnExit().then(ok => {
                deferral.resolve(ok);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private publishSelectionChange = _.debounce(() => {
        this.onSelectionChanged({ record: this.selectedRecord });
    }, 200, { leading: false, trailing: true });
}