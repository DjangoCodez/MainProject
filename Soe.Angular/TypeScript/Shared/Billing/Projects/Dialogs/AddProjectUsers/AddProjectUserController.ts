import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IProjectService } from "../../ProjectService";
import { TermGroup, SoeTimeCodeType } from "../../../../../Util/CommonEnumerations";

export class AddProjectUserController {

    //Collections
    types: any[];
    users: any[];
    timecodes: any[];

    //Properties
    private loaded = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $q,
        private coreService: ICoreService,
        private projectService: IProjectService,
        private type: number,
        private user: number,
        private timecode: number,
        private from: Date,
        private to: Date,
        private employeeCalculatedCost: number,
        private userReadonly: boolean,
        private calculatedCostPermission: boolean ) {

        this.load();
    }

    load() {
        this.$q.all([
            this.loadTypes(),
            this.loadUsers(),
            this.loadTimeCodes()]).then(() => {
                this.loaded = true;
            });
    }

    loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ProjectUserType, false, false).then(x => {
            console.log(x)
            this.types = x;
        });
    }

    loadUsers(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, true, false).then(x => {
            this.users = x;
        });
    }

    loadTimeCodes(): ng.IPromise<any> {
        return this.projectService.getTimeCodesByType(SoeTimeCodeType.WorkAndAbsense, true, false).then((x) => {
            this.timecodes = x;
        });
    }

    buttonEnabled() {
        return !!(this.type && this.user);
    }

    buttonOkClick() {
        var t = _.find(this.types, { id: this.type });
        var u = _.find(this.users, { id: this.user });
        var tc = _.find(this.timecodes, { timeCodeId: this.timecode });
        this.$uibModalInstance.close({ type: this.type, typename: t ? t.name : '', user: this.user, username: u ? u.name : '', timecode: this.timecode, timecodename: tc ? tc.name : '', from: this.from, to: this.to, employeeCalculatedCost: this.employeeCalculatedCost });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}