//Special strategy for leaving / tabbing from a cell editor to make sure grid events are flushed before tabbing.
export class DelayedTabToNextCellStrategy {
    constructor(private api, private rowIndex: number, private colId: string) {
    }

    public tabToNextCell(shiftKey: boolean) {
        // finalize stop editing cell, triggers all the propiate grid events.
        this.api.stopEditing();
        // need to set a timeout to delay the tab, so the grid can flush the event queue. 
        // in our case we need to make sure cell changed event are triggered before tabbing due to business logic.
        setTimeout(() => {
            //make sure the cell is in edit mode again, as the internal grid logic sets the next cell in edit mode if the current cell is in edit mode.
            console.log();
            this.api.startEditingCell({ rowIndex: this.rowIndex, colKey: this.colId, keyPress: $.ui.keyCode.ENTER });
            if (shiftKey) {
                this.api.tabToPreviousCell();
            }
            else {
                this.api.tabToNextCell();
            }
        }, 10);
    }
}