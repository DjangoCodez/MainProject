import { SOEMessageBoxButtons, SOEMessageBoxImage} from "../Util/Enumerations";
import { TimeEmployeeTreeDTO, TimeEmployeeTreeGroupNodeDTO, TimeEmployeeTreeNodeDTO } from "../Common/Models/TimeEmployeeTreeDTO";
import { TimePeriodDTO } from "../Common/Models/TimePeriodDTO";

export class TimePayrollUtility {

    static getPreviousTimePeriodByPaymentDate(timePeriods: TimePeriodDTO[], currentTimePeriodId: number): any {
        var currentTimePeriod = _.find(timePeriods, { timePeriodId: currentTimePeriodId });
        if (!currentTimePeriod)
            return null;

        var filteredList: any[] = [];
        _.forEach(timePeriods, (timePeriod: TimePeriodDTO) => {
            if (new Date(<any>timePeriod.paymentDate).date() < new Date(<any>currentTimePeriod.paymentDate).date()) {
                filteredList.push(timePeriod);
            }
        });

        var sortedList = _.sortBy(filteredList, function (x: TimePeriodDTO) { return new Date(<any>x.paymentDate).date(); });
        sortedList = sortedList.reverse();

        return sortedList[0];
    }

    static getPreviousTimePeriod(timePeriods: TimePeriodDTO[], currentTimePeriodId: number): any {
        var currentTimePeriod = _.find(timePeriods, { timePeriodId: currentTimePeriodId });
        if (!currentTimePeriod)
            return null;

        var filteredList: any[] = [];
        _.forEach(timePeriods, (timePeriod: TimePeriodDTO) => {
            if (new Date(<any>timePeriod.startDate).date() < new Date(<any>currentTimePeriod.startDate).date()) {
                filteredList.push(timePeriod);
            }
        });

        var sortedList = _.sortBy(filteredList, function (x: TimePeriodDTO) { return new Date(<any>x.startDate).date(); });
        sortedList = sortedList.reverse();

        return sortedList[0];
    }

    static getNextTimePeriodByPaymentDate(timePeriods: TimePeriodDTO[], currentTimePeriodId: number): any {
        var currentTimePeriod = _.find(timePeriods, { timePeriodId: currentTimePeriodId });
        if (!currentTimePeriod)
            return null;

        var filteredList: any[] = [];
        _.forEach(timePeriods, (timePeriod: TimePeriodDTO) => {
            if (new Date(<any>timePeriod.paymentDate).date() > new Date(<any>currentTimePeriod.paymentDate).date()) {
                filteredList.push(timePeriod);
            }
        });

        var sortedList = _.sortBy(filteredList, function (x: TimePeriodDTO) { return new Date(<any>x.paymentDate).date(); });

        return sortedList[0];
    }

    static getTimePeriodFromDate(timePeriods: TimePeriodDTO[], date: Date): TimePeriodDTO {
        var timePeriod = _.filter(timePeriods, s => s.startDate.isSameOrBeforeOnDay(date) && s.stopDate.isSameOrAfterOnDay(date))[0];
        if (timePeriod) {
            timePeriod = this.setTimePeriodDates(timePeriod);
        }
        return timePeriod;
    }

    static getNextTimePeriod(timePeriods: TimePeriodDTO[], currentTimePeriodId: number): any {
        var currentTimePeriod = _.find(timePeriods, { timePeriodId: currentTimePeriodId });
        if (!currentTimePeriod)
            return null;

        var filteredList: any[] = [];
        _.forEach(timePeriods, (timePeriod: TimePeriodDTO) => {
            if (new Date(<any>timePeriod.startDate).date() > new Date(<any>currentTimePeriod.startDate).date()) {
                filteredList.push(timePeriod);
            }
        });

        var sortedList = _.sortBy(filteredList, function (x: TimePeriodDTO) { return new Date(<any>x.startDate).date(); });

        return sortedList[0];
    }
    
    static getEmployeeNode(groupNode: TimeEmployeeTreeGroupNodeDTO, employeeId: number) {
        return _.find(groupNode.getVisibleEmployeeNodes(true), e => e.employeeId === employeeId);
    }

    static getEmployeeNodesInGroup(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO) {
        if (!groupNodes || !currentEmployeeNode)
            return null;

        var groupNode = this.getGroupNode(groupNodes, currentEmployeeNode.groupId, true);
        if (!groupNode)
            return null;

        return groupNode.getVisibleEmployeeNodes(true);
    }

    static getGroupNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], groupId: number, searchInChildGroupNodes: boolean): TimeEmployeeTreeGroupNodeDTO {
        if (!groupNodes || groupNodes.length === 0)
            return null;

        for (let groupNode of groupNodes) {
            if (groupNode.id === groupId)
                return groupNode;

            if (searchInChildGroupNodes && groupNode.childGroupNodes && groupNode.childGroupNodes.length > 0) {
                let foundNode = this.getGroupNode(groupNode.childGroupNodes, groupId, searchInChildGroupNodes);
                if (foundNode)
                    return foundNode;
            }
        }
        return null;
    }

    static getPrevEmployeeNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO): TimeEmployeeTreeNodeDTO {
        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return null;

        var maxIndex = employeeNodesForGroup.length - 1;
        if (maxIndex < 0)
            return;

        var index = this.getEmployeeNodeIndex(employeeNodesForGroup, currentEmployeeNode.employeeId);
        if (index <= 0 || index > maxIndex)
            return null;

        return employeeNodesForGroup[index - 1];
    }

    static getNextEmployeeNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO): TimeEmployeeTreeNodeDTO {
        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return null;

        var maxIndex = employeeNodesForGroup.length - 1;
        if (maxIndex < 0)
            return;

        var index = this.getEmployeeNodeIndex(employeeNodesForGroup, currentEmployeeNode.employeeId);
        if (index < 0 || index >= maxIndex)
            return null;

        return employeeNodesForGroup[index + 1];
    }

    static refreshCurrentEmployeeNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO, currentEmployeeNodeIndex: number): TimeEmployeeTreeNodeDTO {
        if (!currentEmployeeNode || currentEmployeeNodeIndex < 0)
            return;

        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return null;

        var maxIndex = employeeNodesForGroup.length - 1;
        return currentEmployeeNodeIndex <= maxIndex ? employeeNodesForGroup[currentEmployeeNodeIndex] : employeeNodesForGroup[currentEmployeeNodeIndex - 1];
    }

    static getCurrentEmployeeNodeIndex(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO): number {
        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return -1;

        var index = this.getEmployeeNodeIndex(employeeNodesForGroup, currentEmployeeNode.employeeId);
        return index < 0 ? 0 : index;
    }

    static hasPrevEmployeeNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO): boolean {
        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return false;

        var index = this.getEmployeeNodeIndex(employeeNodesForGroup, currentEmployeeNode.employeeId);
        return index > 0;
    }

    static hasNextEmployeeNode(groupNodes: TimeEmployeeTreeGroupNodeDTO[], currentEmployeeNode: TimeEmployeeTreeNodeDTO): boolean {
        var employeeNodesForGroup = this.getEmployeeNodesInGroup(groupNodes, currentEmployeeNode);
        if (!employeeNodesForGroup)
            return false;

        var maxIndex = employeeNodesForGroup.length - 1;
        var index = this.getEmployeeNodeIndex(employeeNodesForGroup, currentEmployeeNode.employeeId);
        return index >= 0 && index < maxIndex;
    }

    static getEmployeeNodeIndex(employeeNodesForGroup: TimeEmployeeTreeNodeDTO[], employeeId: number) {
        if (!employeeNodesForGroup)
            return -1;
        return employeeNodesForGroup.findIndex(x => x.employeeId === employeeId);
    }

    static getSaveAttestValidationMessageIcon(validationResult: any): SOEMessageBoxImage {
        var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
        if (!validationResult)
            return image;

        if (validationResult.success && validationResult.canOverride)
            image = SOEMessageBoxImage.Information;
        else if (!validationResult.success && !validationResult.canOverride)
            image = SOEMessageBoxImage.Error;
        else
            image = SOEMessageBoxImage.Warning;

        return image;
    }

    static getSaveAttestValidationMessageButton(validationResult: any): SOEMessageBoxButtons {
        var button: SOEMessageBoxButtons = SOEMessageBoxButtons.None;
        if (!validationResult)
            return button;

        if (validationResult.success && validationResult.canOverride)
            button = SOEMessageBoxButtons.OKCancel;
        else if (!validationResult.success && !validationResult.canOverride)
            button = SOEMessageBoxButtons.OK;
        else
            button = SOEMessageBoxButtons.OKCancel;
        return button;
    }

    static getExpandedGroupIds(tree: TimeEmployeeTreeDTO, isMyTime: boolean) {
        var expandedGroupIds: number[] = [];
        if (isMyTime || !tree || !tree.groupNodes || tree.groupNodes.length === 0)
            return expandedGroupIds;

        _.forEach(tree.groupNodes, (groupNode: TimeEmployeeTreeGroupNodeDTO) => {
            _.forEach(this.getExpandedGroupIdsRecursive(groupNode), (id: number) => {
                expandedGroupIds.push(id);
            });
        });
        return expandedGroupIds;
    }

    static getExpandedGroupIdsRecursive(groupNode: TimeEmployeeTreeGroupNodeDTO): number[] {
        var expandedGroupIds: number[] = [];
        if (groupNode.expanded) {
            expandedGroupIds.push(groupNode.id);
            _.forEach(groupNode.childGroupNodes, (childGroupNode: TimeEmployeeTreeGroupNodeDTO) => {
                _.forEach(this.getExpandedGroupIdsRecursive(childGroupNode), (id: number) => {
                    expandedGroupIds.push(id);
                });
            });
        }
        return expandedGroupIds;
    }

    static getCollectionIds(collection: any): any[] {
        var ids = [];
        if (collection && collection.length > 0) {
            _.forEach(collection, (item: any) => {
                ids.push(item.id);
            });
        }
        return ids;
    }

    static trySetGroupsExpanded(tree: TimeEmployeeTreeDTO, expandedGroupIds: number[], hasSelection: boolean, hasSearch: boolean) {
        if (tree == null || !tree || !tree.groupNodes)
            return;
        
        if (expandedGroupIds && expandedGroupIds.length > 0) {
            if (this.setGroupsExpanded(tree, expandedGroupIds)) {
                return;
            }
        }

        if (hasSelection || hasSearch)
            this.setAllGroupsExpanded(tree, true);
        else if (tree.groupNodes.length === 1)
            this.expandFirstGroup(tree);
    }

    static setFilterVisibility(tree: TimeEmployeeTreeDTO, filterText: string) {
        if (!tree)
            return;

        tree.filterEmployees(filterText);
        if (filterText && filterText.length > 0)
            this.setAllGroupsExpanded(tree, true);
        else
            this.expandFirstGroup(tree);

        this.applyFilterOnTree(tree, filterText);
    }

    static setTimePeriodDates(timePeriod: TimePeriodDTO): TimePeriodDTO {
        if (timePeriod) {
            timePeriod.startDate = new Date(<any>timePeriod.startDate).beginningOfDay();
            timePeriod.stopDate = new Date(<any>timePeriod.stopDate).date().endOfDay();
            timePeriod.payrollStartDate = new Date(<any>timePeriod.payrollStartDate).beginningOfDay();
            timePeriod.payrollStopDate = new Date(<any>timePeriod.payrollStopDate).date().endOfDay();
            timePeriod.paymentDate = new Date(<any>timePeriod.paymentDate).date().endOfDay();
        }
        return timePeriod;
    }

    static setGroupsExpanded(tree: TimeEmployeeTreeDTO, expandedGroupIds?: number[]): boolean {
        if (!tree)
            return;

        var hasExpandedGroups = false;
        _.forEach(tree.groupNodes, (groupNode: any) => {
            hasExpandedGroups = this.setGroupsExpandedRecursive(groupNode, expandedGroupIds);
        });
        return hasExpandedGroups;
    }

    static setGroupsExpandedRecursive(groupNode: TimeEmployeeTreeGroupNodeDTO, expandedGroupIds?: number[]): boolean {
        if (!groupNode)
            return;

        var hasExpandedGroups = false;
        if (groupNode.hasVisibleEmployees() && _.filter(expandedGroupIds, id => id == groupNode.id).length > 0) {
            groupNode.expanded = true;
            hasExpandedGroups = true;
        }

        if (groupNode.childGroupNodes && groupNode.childGroupNodes.length) {
            _.forEach(groupNode.childGroupNodes, (childGroupNode: TimeEmployeeTreeGroupNodeDTO) => {
                if (this.setGroupsExpandedRecursive(childGroupNode, expandedGroupIds))
                    hasExpandedGroups = true;
            });
        }

        return hasExpandedGroups;
    }

    static setAllGroupsExpanded(tree: TimeEmployeeTreeDTO, expanded?: boolean) {
        if (!tree)
            return;

        _.forEach(tree.groupNodes, (groupNode: any) => {
            this.setAllGroupsExpandedRecursive(groupNode, expanded);
        });
    }

    static setAllGroupsExpandedRecursive(groupNode: TimeEmployeeTreeGroupNodeDTO, expanded?: boolean) {
        if (!groupNode)
            return;

        if (groupNode.hasVisibleEmployees()) {
            groupNode.expanded = expanded;
            if (groupNode.childGroupNodes && groupNode.childGroupNodes.length > 0) {
                _.forEach(groupNode.childGroupNodes, (childGroupNode: TimeEmployeeTreeGroupNodeDTO) => {
                    this.setAllGroupsExpandedRecursive(childGroupNode, expanded);
                });
            }
        }        
    }

    static expandFirstGroup(tree: TimeEmployeeTreeDTO) {
        if (!tree)
            return;

        this.setAllGroupsExpanded(tree, false);
        if (tree && tree.groupNodes && tree.groupNodes.length == 1)
            this.tryExpandGroup(tree.groupNodes[0]);
    }

    static tryExpandGroup(groupNode: TimeEmployeeTreeGroupNodeDTO) {
        if (!groupNode)
            return;

        groupNode.expanded = true;
        if (groupNode.employeeNodes && groupNode.employeeNodes.length == 0 && groupNode.childGroupNodes && groupNode.childGroupNodes.length === 1)
            this.tryExpandGroup(groupNode.childGroupNodes[0]);        
    }

    static applyFilterOnTree(tree: TimeEmployeeTreeDTO, filterText: string) {
        if (!tree)
            return;

        _.forEach(tree.groupNodes, groupNode => {
            this.applyFilterOnTreeRecursive(groupNode, filterText);
        });
    }

    static applyFilterOnTreeRecursive(groupNode: TimeEmployeeTreeGroupNodeDTO, filterText: string) {
        if (!groupNode)
            return;

        groupNode.setDefaultWarnings();
        groupNode.setDefaultAttestState();

        if (filterText && filterText.length > 0) {

            var attestStateSort = Number.MAX_SAFE_INTEGER;
            var attestStateName = ' ';
            var attestStateColor = ' ';

            var warningMessageTime: string[] = [];
            var warningMessagePayroll: string[] = [];
            var warningMessagePayrollStopping: string[] = [];

            _.forEach(groupNode.getVisibleEmployeeNodes(true), employeeNode => {
                if (employeeNode.visible) {
                    
                    if (attestStateSort > employeeNode.attestStateSort && employeeNode.attestStateColor !== '#FFFFFF') {
                        attestStateSort = employeeNode.attestStateSort;
                        attestStateColor = employeeNode.attestStateColor;
                        attestStateName = employeeNode.attestStateName;
                    }

                    warningMessageTime = employeeNode.getWarningMessagesTime();
                    warningMessagePayroll = employeeNode.getWarningMessagesPayroll();
                    warningMessagePayrollStopping = employeeNode.getWarningMessagesPayrollStopping();
                }
            });

            groupNode.setCurrentWarnings(warningMessageTime, warningMessagePayroll, warningMessagePayrollStopping);
            groupNode.setAttestState(attestStateColor, attestStateName);
        }

        if (groupNode.childGroupNodes) {
            _.forEach(groupNode.childGroupNodes, childGroupNode => {
                this.applyFilterOnTreeRecursive(childGroupNode, filterText);
            });
        }
    }
}
