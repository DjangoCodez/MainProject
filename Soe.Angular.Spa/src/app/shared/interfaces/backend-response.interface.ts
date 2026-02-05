import { CrudResponse } from './crud-response.interface';

export type BackendResponse = EvoBackendResponse | CrudResponse;

export interface EvoBackendResponse {
  trackId: string;
  response: ServiceResponse;
  success: boolean;
}

export interface ServiceResponse {
  backend: string;
  entityId: number;
  entity: string;
  success: boolean;
  exception: string;
  message: string;
  errorMessage: string; // legacy but in use
  errorNumber: number; // legacy but in use
  value: any; // legacy but in use
  value2: any; // legacy but in use
  booleanValue: boolean; // legacy but in use
  booleanValue2: boolean; // legacy but in use
  stringValue: string; // legacy but in use
  numberValue: number; // instead of legace field integerValue2
  decimalValue: number; // legacy but in use
  dateTimeValue: string; // legacy but in use
}
