import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Time/Schedule/Absencerequests/EditController";
import { AbsenceRequestGuiMode, AbsenceRequestViewMode, AbsenceRequestParentMode } from "../../../Util/Enumerations";
import { Feature } from "../../../Util/CommonEnumerations";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "time.schedule.absencerequests.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.employeeRequestId)
            .onGetRowEditName(row => row.timeDeviationCauseName)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
                if (soeConfig.employeeRequestId && soeConfig.employeeRequestId > 0) {
                    const parameters = {
                        employeeRequestId: soeConfig.employeeRequestId,
                        employeeId: 0,
                        employeeGroupId: soeConfig.employeeGroupId || 0,
                        viewMode: (soeConfig.feature == Feature.Time_Schedule_AbsenceRequests)
                            ? AbsenceRequestViewMode.Attest
                            : AbsenceRequestViewMode.Employee,
                        guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                        skipXEMailOnShiftChanges: false,
                        loadRequestFromInterval: false,
                        parentMode: AbsenceRequestParentMode.SchedulePlanning,
                    };
                    this.tabs.addEditTab(
                        { employeeRequestId: soeConfig.employeeRequestId },
                        EditController,
                        parameters,
                        this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html")
                    );
                }
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "absencerequest.short", part + "absencerequests.short", part + "new.short");
    }

    public tabs: ITabHandler;
    private edit(row: any) {

        // Open edit page
        var parameters =
            {
                employeeRequestId: row.employeeRequestId,
                employeeId: row.employeeId,
                employeeGroupId: (soeConfig.employeeGroupId) ? soeConfig.employeeGroupId : 0,
                viewMode: (soeConfig.feature && soeConfig.feature == Feature.Time_Schedule_AbsenceRequests) ? AbsenceRequestViewMode.Attest : AbsenceRequestViewMode.Employee,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                skipXEMailOnShiftChanges: false,
                loadRequestFromInterval: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,

            };
        this.tabs.addEditTab(row, EditController, parameters, this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"));
    }
    private add() {
        var parameters =
            {
                employeeRequestId: 0,
                employeeId: (soeConfig.employeeId) ? soeConfig.employeeId : 0,
                employeeGroupId: (soeConfig.employeeGroupId) ? soeConfig.employeeGroupId : 0,
                viewMode: (soeConfig.feature && soeConfig.feature == Feature.Time_Schedule_AbsenceRequests) ? AbsenceRequestViewMode.Attest : AbsenceRequestViewMode.Employee,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                skipXEMailOnShiftChanges: false,
                loadRequestFromInterval: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
            };

        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"), parameters);
    }
}