/**
 * @deprecated This interface is deprecated and will be removed in future versions. Use BackendResponse instead.
 */
export interface CrudResponse {
  booleanValue: boolean;
  booleanValue2: boolean;
  canUserOverride: boolean;
  dateTimeValue: string;
  decimalValue: number;
  errorMessage?: string;
  errorNumber?: number;
  integerValue: number;
  integerValue2: number;
  modified: string;
  objectsAffected: number;
  success: boolean;
  successNumber: number;
  infoMessage: string;
  stringValue: string;
  value: object;
  value2: object;
}
