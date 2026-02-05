import { ITimeService } from "../../../../Time/TimeService";
import { TimeStampEntryDTO } from "../../../../../Common/Models/TimeStampDTOs";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { UserAgentClientInfoDTO } from "../../../../../Common/Models/UserAgentClientInfoDTO";

export class TimeStampDetailsController {

    private isSupportAdmin: boolean = false;
    private statuses: ISmallGenericType[];
    private originTypes: ISmallGenericType[];
    private timeStampEntry: TimeStampEntryDTO;
    private clientInfo: UserAgentClientInfoDTO;

    //@ngInject
    constructor(private $uibModalInstance,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private timeStampEntryId: number) {

        this.isSupportAdmin = CoreUtility.isSupportAdmin;

        this.$q.all([
            this.loadStatuses(),
            this.loadOriginTypes()
        ]).then(() => {
            this.loadTimeStampEntry();
        });
    }

    private loadStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeStampEntryStatus, false, false).then(x => {
            this.statuses = x;
        });
    }

    private loadOriginTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeStampEntryOriginType, false, false).then(x => {
            this.originTypes = x;
        });
    }

    private loadTimeStampEntry(): ng.IPromise<any> {
        return this.timeService.getTimeStamp(this.timeStampEntryId).then(x => {
            this.timeStampEntry = x;
            if (this.timeStampEntry) {
                if (this.timeStampEntry.timeTerminalId)
                    this.timeStampEntry.terminalInfo = "{0} (ID: {1})".format(this.timeStampEntry.timeTerminalName, this.timeStampEntry.timeTerminalId.toString());
                this.timeStampEntry.statusText = "{0}: {1}".format(this.timeStampEntry.status.toString(), this.statuses.find(s => s.id === this.timeStampEntry.status)?.name);
                this.timeStampEntry.originTypeText = "{0}: {1}".format(this.timeStampEntry.originType.toString(), this.originTypes.find(o => o.id === this.timeStampEntry.originType)?.name);
            }
        })
    }

    private expandUserAgentClientInfo() {
        if (!this.clientInfo) {
            this.timeService.getTimeStampUserAgentClientInfo(this.timeStampEntryId).then(x => {
                this.clientInfo = x;
            });
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}