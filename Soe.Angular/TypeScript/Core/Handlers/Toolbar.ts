import { ToolBarUtility, ToolBarButtonGroup } from "../../Util/ToolBarUtility";
import { Constants } from "../../Util/Constants";
import { ISmallGenericType } from "../../Scripts/TypeLite.Net4";

export interface IToolbar {
    addButtonGroup(buttonGroup: ToolBarButtonGroup);
    addInclude(path: string);
    setupNavigationRecords(records: ISmallGenericType[], selectedRecordId: number, navigateCallBack: NavigateCallback);
    setupNavigationRecordDates(dates: Date[], selectedDate: Date, navigateCallBack: NavigateDateCallback);
    setSelectedRecord(recordId: number);
    setSelectedDate(date: Date);
    setupNavigationGroup(disabled: () => any, hidden: () => any, navigateCallBack: NavigateCallback, recordsIds: number[], currentRecordId: number);
}

export type NavigateCallback = ((recordId: number) => void);
export type NavigateDateCallback = ((date: Date) => void);

export class Toolbar implements IToolbar {
    private recordsIds: number[];
    private currentRecordId: number;
    navigationMenuButtons = new Array<ToolBarButtonGroup>();

    buttonGroups = new Array<ToolBarButtonGroup>();
    toolbarInclude: string;

    private navigateCallBack: NavigateCallback;
    private navigatorRecords: ISmallGenericType[] | Date[];
    private selectedRecord: ISmallGenericType | Date;

    private navigateDateCallBack: NavigateDateCallback;

    constructor() {
    }

    static createDefault(showCopy: boolean, onCopy: () => {}, disableCopy: () => boolean): IToolbar {
        let tb = new Toolbar();

        if (showCopy)
            tb.addButtonGroup(ToolBarUtility.createGroup(ToolBarUtility.createCopyButton(onCopy, disableCopy)));

        return tb;
    }

    public addButtonGroup(buttonGroup: ToolBarButtonGroup) {
        this.buttonGroups.push(buttonGroup);
    }

    public addInclude(path: string) {
        this.toolbarInclude = path;
    }

    public setupNavigationRecords(records: ISmallGenericType[], selectedRecordId: number, navigateCallBack: NavigateCallback) {
        this.navigateCallBack = navigateCallBack;
        this.navigatorRecords = records;
        this.setSelectedRecord(selectedRecordId);
    }

    public setupNavigationRecordDates(dates: Date[], selectedDate: Date, navigateCallBack: NavigateDateCallback) {
        this.navigateDateCallBack = navigateCallBack;
        this.navigatorRecords = dates;
        this.setSelectedDate(selectedDate);
    }

    public setSelectedRecord(recordId: number) {
        this.selectedRecord = _.find(<ISmallGenericType[]>this.navigatorRecords, e => e.id === recordId);
    }

    public setSelectedDate(date: Date) {
        this.selectedRecord = date;
    }

    private selectedRecordChanged(record: ISmallGenericType) {
        this.navigateCallBack(record.id);
    }

    private selectedDateChanged(date: Date) {
        this.navigateDateCallBack(date);
    }

    public setupNavigationGroup(disabled = () => { }, hidden = () => { }, navigateCallBack: NavigateCallback, recordsIds: number[], currentRecordId: number) {
        this.navigateCallBack = navigateCallBack;
        this.recordsIds = recordsIds;
        this.currentRecordId = currentRecordId;

        const group = ToolBarUtility.createNavigationGroup(
            () => {
                this.navigateTo(Constants.EVENT_NAVIGATE_FIRST);
            },
            () => {
                this.navigateTo(Constants.EVENT_NAVIGATE_LEFT);
            },
            () => {
                this.navigateTo(Constants.EVENT_NAVIGATE_RIGHT);
            },
            () => {
                this.navigateTo(Constants.EVENT_NAVIGATE_LAST);
            },
            disabled,
            hidden
        );
        this.navigationMenuButtons.push(group);
    }

    private navigateTo(event: string) {
        let pos: number;
        switch (event) {
            case Constants.EVENT_NAVIGATE_FIRST:
                this.currentRecordId = this.recordsIds[0];
                this.navigateCallBack(this.currentRecordId);
                break;
            case Constants.EVENT_NAVIGATE_LEFT:
                pos = this.recordsIds.indexOf(this.currentRecordId);
                this.currentRecordId = this.recordsIds[pos - 1];
                this.navigateCallBack(this.currentRecordId);
                break;
            case Constants.EVENT_NAVIGATE_RIGHT:
                pos = this.recordsIds.indexOf(this.currentRecordId);
                if (pos < this.recordsIds.length - 1) {
                    this.currentRecordId = this.recordsIds[pos + 1];
                }
                this.navigateCallBack(this.currentRecordId);
                break;
            case Constants.EVENT_NAVIGATE_LAST:
                this.currentRecordId = this.recordsIds[this.recordsIds.length - 1];
                this.navigateCallBack(this.currentRecordId);
        }
    }
}