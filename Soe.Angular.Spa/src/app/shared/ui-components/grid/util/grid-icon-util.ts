export class GridIconUtil {
  static icons: { [key: string]: ((...args: any[]) => any) | string } = {
    // Use Font Awesome icons

    // header column group shown when expanded (click to contract)
    columnGroupOpened: '<i class="fal fa-chevron-left"/>',
    // header column group shown when contracted (click to expand)
    columnGroupClosed: '<i class="fal fa-chevron-right"/>',
    // tool panel column group contracted (click to expand)
    columnSelectClosed: '<i class="fal fa-chevron-right"/>',
    // tool panel column group expanded (click to contract)
    columnSelectOpen: '<i class="fal fa-chevron-down"/>',
    // column tool panel header expand/collapse all button, shown when some children are expanded and others are collapsed
    columnSelectIndeterminate: '<i class="fal fa-dash"/>',
    // shown on ghost icon while dragging column to the side of the grid to pin
    columnMovePin: '<i class="fal fa-thumbtack"/>',
    // shown on ghost icon while dragging over part of the page that is not a drop zone
    columnMoveHide: '<i class="fal fa-eye-slash"/>',
    // shown on ghost icon while dragging columns to reorder
    columnMoveMove: '<i class="fal fa-arrows-up-down-left-right"/>',
    // animating icon shown when dragging a column to the right of the grid causes horizontal scrolling
    columnMoveLeft: '<i class="fal fa-chevron-left"/>',
    // animating icon shown when dragging a column to the left of the grid causes horizontal scrolling
    columnMoveRight: '<i class="fal fa-chevron-right"/>',
    // shown on ghost icon while dragging over Row Groups drop zone
    columnMoveGroup: '<i class="fal fa-indent"/>',
    // shown on ghost icon while dragging over Values drop zone
    columnMoveValue: '<i class="fal fa-sigma"/>',
    // shown on ghost icon while dragging over pivot drop zone
    columnMovePivot: '<i class="fal fa-table-pivot"/>',
    // shown on ghost icon while dragging over drop zone that doesn't support it, e.g. string column over aggregation drop zone
    dropNotAllowed: '<i class="fal fa-ban"/>',
    // shown on row group when contracted (click to expand)
    groupContracted: '<i class="fal fa-chevron-right"/>',
    // shown on row group when expanded (click to contract)
    groupExpanded: '<i class="fal fa-chevron-down"/>',
    // set filter tree list group contracted (click to expand)
    setFilterGroupClosed: '<i class="fal fa-chevron-right"/>',
    // set filter tree list group expanded (click to contract)
    setFilterGroupOpen: '<i class="fal fa-chevron-down"/>',
    // set filter tree list expand/collapse all button, shown when some children are expanded and others are collapsed
    setFilterGroupIndeterminate: '<i class="fal fa-dash"/>',
    // context menu chart item
    chart: '<i class="fal fa-chart-simple"/>',
    // chart window title bar
    close: '<i class="fal fa-times"/>',
    // X (remove) on column 'pill' after adding it to a drop zone list
    cancel: '<i class="fal fa-times"/>',
    // indicates the currently active pin state in the "Pin column" sub-menu of the column menu
    check: '<i class="fal fa-check"/>',
    // "go to first" button in pagination controls
    first: '<i class="fal fa-chevrons-left"/>',
    // "go to previous" button in pagination controls
    previous: '<i class="fal fa-chevron-left"/>',
    // "go to next" button in pagination controls
    next: '<i class="fal fa-chevron-right"/>',
    // "go to last" button in pagination controls
    last: '<i class="fal fa-chevrons-right"/>',
    // shown on top right of chart when chart is linked to range data (click to unlink)
    linked: '<i class="fal fa-link"/>',
    // shown on top right of chart when chart is not linked to range data (click to link)
    unlinked: '<i class="fal fa-link-slash"/>',
    // "Choose colour" button on chart settings tab
    colorPicker: '<i class="fal fa-fill-drip"/>',
    // rotating spinner shown by the loading cell renderer
    groupLoading: '<i class="fal fa-loader"/>',
    // button to launch enterprise column menu
    menu: '<i class="far fa-bars"/>',
    // filter tool panel tab
    filter: '<i class="fal fa-filter"/>',
    // column tool panel tab
    columns: '<i class="fal fa-columns-3"/>',
    // button in chart regular size window title bar (click to maximise)
    maximize: '<i class="fal fa-arrows-maximize"/>',
    // button in chart maximised window title bar (click to make regular size)
    minimize: '<i class="fal fa-arrows-minimize"/>',
    // "Pin column" item in column header menu
    menuPin: '<i class="fal fa-thumbtack"/>',
    // "Value aggregation" column menu item (shown on numeric columns when grouping is active)"
    menuValue: '<i class="fal fa-sigma"/>',
    // "Group by {column-name}" item in column header menu
    menuAddRowGroup: '<i class="fal fa-indent"/>',
    // "Un-Group by {column-name}" item in column header menu
    menuRemoveRowGroup: '<i class="fal fa-indent"/>',
    // context menu copy item
    clipboardCopy: '<i class="fal fa-copy"/>',
    // context menu cut item
    clipboardCut: '<i class="fal fa-scissors"/>',
    // context menu paste item
    clipboardPaste: '<i class="fal fa-paste"/>',
    // identifies the pivot drop zone
    pivotPanel: '<i class="fal fa-table-pivot"/>',
    // "Row groups" drop zone in column tool panel
    rowGroupPanel: '<i class="fal fa-indent"/>',
    // columns tool panel Values drop zone
    valuePanel: '<i class="fal fa-sigma"/>',
    // drag handle used to pick up draggable columns
    columnDrag: '<i class="fal fa-grip-vertical"/>',
    // drag handle used to pick up draggable rows
    rowDrag: '<i class="fal fa-grip-vertical"/>',
    // context menu export item
    save: '<i class="fal fa-floppy-disk"/>',
    // csv export
    csvExport: '<i class="fal fa-file-csv"/>',
    // excel export
    excelExport: '<i class="fal fa-file-excel"/>',
    // icon on dropdown editors
    //smallDown: '<i class="fal fa-chevron-down"/>',
    // version of small-right used in RTL mode
    //smallLeft: '<i class="fal fa-chevron-left"/>',
    // separater between column 'pills' when you add multiple columns to the header drop zone
    //smallRight: '<i class="fal fa-chevron-right"/>',
    smallUp: '<i class="fal fa-chevron-up"/>',
    // show on column header when column is sorted ascending
    sortAscending: '<i class="fal fa-arrow-up"/>',
    // show on column header when column is sorted descending
    sortDescending: '<i class="fal fa-arrow-down"/>',
    // show on column header when column has no sort, only when enabled with gridOptions.unSortIcon=true
    sortUnSort: '<i class="fal fa-arrow-down-arrow-up"/>',
    // Builder button in Advanced Filter
    advancedFilterBuilder: '<i class="fal fa-block-quote"/>',
    // drag handle used to pick up Advanced Filter Builder rows
    advancedFilterBuilderDrag: '<i class="fal fa-grip-vertical"/>',
    // Advanced Filter Builder row validation error
    advancedFilterBuilderInvalid: '<i class="fal fa-ban"/>',
    // shown on Advanced Filter Builder rows to move them up
    advancedFilterBuilderMoveUp: '<i class="fal fa-arrow-up"/>',
    // shown on Advanced Filter Builder rows to move them down
    advancedFilterBuilderMoveDown: '<i class="fal fa-arrow-down"/>',
    // shown on Advanced Filter Builder rows to add new rows
    advancedFilterBuilderAdd: '<i class="fal fa-plus"/>',
    // shown on Advanced Filter Builder rows to remove row
    advancedFilterBuilderRemove: '<i class="fal fa-times"/>',
    // Edit Chart menu item shown in Integrated Charts menu
    chartsMenuEdit: '<i class="fal fa-chart-simple"/>',
    // Advanced Settings menu item shown in Integrated Charts menu
    chartsMenuAdvancedSettings: '<i class="fal fa-gear"/>',
    // shown in Integrated Charts menu add fields
    chartsMenuAdd: '<i class="fal fa-plus"/>',
    // checked checkbox
    checkboxChecked: '<i class="fal fa-square-check"/>',
    // indeterminate checkbox
    checkboxIndeterminate: '<i class="fal fa-square-minus"/>',
    // unchecked checkbox
    checkboxUnchecked: '<i class="fal fa-square"/>',
    // radio button on
    radioButtonOn: '<i class="fal fa-circle-dot"/>',
    // radio button off
    radioButtonOff: '<i class="fal fa-circle"/>',
  };
}
