export interface ISoeExportExcelOptions {
  termGroupedFooter?: string;
  termGroupedSubTotal?: string;
  termGroupedGrandTotal?: string;
  rowGroupExpandState?: 'expanded' | 'collapsed' | 'match';
  groupedTotals?: boolean; // This will enable grouped totals. However all columns will then be exported as well, even hidden.
}
