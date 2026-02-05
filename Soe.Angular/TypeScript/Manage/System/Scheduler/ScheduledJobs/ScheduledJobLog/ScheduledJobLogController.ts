import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../../../Util/ToolBarUtility";
import { ISystemService } from "../../../SystemService";
import { IconLibrary } from "../../../../../Util/Enumerations";

export class ScheduledJobLogController {
    private soeGridOptions: ISoeGridOptionsAg;
    private loading: boolean;
    protected buttons = new Array<ToolBarButtonGroup>();   

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private systemService: ISystemService,
        private title: string,
        private sysScheduledJobId: number,
        private terms: { [index: string]: string; }) {
        
        this.soeGridOptions = new SoeGridOptionsAg("Manage.System.Scheduler.ScheduledJobs.Log", this.$timeout);
        this.buttons = new Array<ToolBarButtonGroup>();
        this.setupToolbar();
        this.setupGrid();
        this.loadLog();
    }

    private setupToolbar() {
        this.buttons.push(ToolBarUtility.createGroup(new ToolBarButton("", "common.dashboard.reload", IconLibrary.FontAwesome, "fal fa-sync", () => {
            this.loadLog();
        })));
    }

    private setupGrid() {
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.setMinRowsToShow(30);

        this.soeGridOptions.addColumnNumber("batchNr", this.terms["manage.system.scheduler.batchnr"], 100, { enableHiding: true, maxWidth: 100 });
        this.soeGridOptions.addColumnText("logLevelName", this.terms["common.dashboard.syslog.level"], 100, { maxWidth: 100 });
        this.soeGridOptions.addColumnDateTime("time", this.terms["common.time"], 100,null,null,null, { maxWidth: 100 });
        this.soeGridOptions.addColumnText("message", this.terms["common.message"], null);

        this.soeGridOptions.finalizeInitGrid();
    }

    private loadLog(): ng.IPromise<any> {
        this.loading = true;
        return this.systemService.getScheduledJobLog(this.sysScheduledJobId).then(log => {
            this.soeGridOptions.setData(log);
            this.loading = false;
        });
    }  

    close() {
        this.$uibModalInstance.close();
    }
}