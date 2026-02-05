export type InterpretationClasses =
  | 'value-is-valid'
  | 'value-is-unsettled'
  | 'value-is-not-found';
export type InvoiceFieldClasses = Record<
  string,
  Record<InterpretationClasses, boolean | undefined>
>;

export type InvoiceIds = {
  invoiceId?: number | null;
  ediEntryId?: number | null;
  scanningEntryId?: number | null;
};
