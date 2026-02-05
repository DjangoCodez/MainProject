import {
  BackendResponse,
  EvoBackendResponse,
} from '@shared/interfaces/backend-response.interface';
import { CrudResponse } from '@shared/interfaces/crud-response.interface';

export class ResponseUtil {
  //   static isEvoBackend(response: BackendResponse): boolean {
  //     return response.hasOwnProperty('response');
  //   }

  static isEvoBackend(
    response: BackendResponse
  ): response is EvoBackendResponse {
    return 'response' in response;
  }

  // Getters

  static getDateTimeValue(response: BackendResponse): string {
    if (this.isEvoBackend(response)) {
      return response.response.dateTimeValue;
    } else {
      return response.dateTimeValue;
    }
  }

  static getEntityId(response: BackendResponse): number {
    if (this.isEvoBackend(response)) {
      return response.response.entityId;
    } else {
      return response.integerValue;
    }
  }

  static getErrorMessage(response: BackendResponse): string {
    if (this.isEvoBackend(response)) {
      return response.response.errorMessage || '';
    } else {
      return response.errorMessage || '';
    }
  }

  static getErrorNumber(response: BackendResponse): number {
    if (this.isEvoBackend(response)) {
      return response.response.errorNumber || 0;
    } else {
      return response.errorNumber || 0;
    }
  }

  static getObjectsAffected(response: BackendResponse): number {
    if (this.isEvoBackend(response)) {
      return 0; // EvoBackendResponse does not have objectsAffected field
    } else {
      return response.objectsAffected || 0;
    }
  }

  static getValueObject(response: BackendResponse): any {
    if (this.isEvoBackend(response)) {
      return response.response.value;
    } else {
      return response.value;
    }
  }

  static getValue2Object(response: BackendResponse): any {
    if (this.isEvoBackend(response)) {
      return response.response.value2 || {};
    } else {
      return response.value2 || {};
    }
  }

  static getBooleanValue(response: BackendResponse): boolean {
    if (this.isEvoBackend(response)) {
      return response.response.booleanValue || false;
    } else {
      return response.booleanValue || false;
    }
  }

  static getBooleanValue2(response: BackendResponse): boolean {
    if (this.isEvoBackend(response)) {
      return response.response.booleanValue2 || false;
    } else {
      return response.booleanValue2 || false;
    }
  }

  static getStringValue(response: BackendResponse): string {
    if (this.isEvoBackend(response)) {
      return response.response.stringValue || '';
    } else {
      return response.stringValue || '';
    }
  }

  static getNumberValue(response: BackendResponse): number {
    if (this.isEvoBackend(response)) {
      return response.response.numberValue || 0;
    } else {
      return response.integerValue2 || 0;
    }
  }

  static getDecimalValue(response: BackendResponse): number | undefined {
    if (this.isEvoBackend(response)) {
      return response.response.decimalValue || undefined;
    } else {
      return response.decimalValue || undefined;
    }
  }

  static getMessageValue(response: BackendResponse): string {
    if (this.isEvoBackend(response)) {
      return response.response.message || '';
    } else {
      return response.infoMessage || '';
    }
  }

  static getResponseObject(response: BackendResponse): BackendResponse {
    if (this.isEvoBackend(response)) {
      return <EvoBackendResponse>response;
    }
    return <CrudResponse>response;
  }

  // Setters

  static setBooleanValue(
    response: BackendResponse,
    booleanValue: boolean
  ): void {
    if (this.isEvoBackend(response)) {
      response.response.booleanValue = booleanValue;
    } else {
      response.booleanValue = booleanValue;
    }
  }

  static setBooleanValue2(
    response: BackendResponse,
    booleanValue: boolean
  ): void {
    if (this.isEvoBackend(response)) {
      response.response.booleanValue2 = booleanValue;
    } else {
      response.booleanValue2 = booleanValue;
    }
  }

  static setEntityId(response: BackendResponse, entityId: number): void {
    if (this.isEvoBackend(response)) {
      response.response.entityId = entityId;
    } else {
      response.integerValue = entityId;
    }
  }

  static setErrorMessage(
    response: BackendResponse,
    errorMessage: string
  ): void {
    if (this.isEvoBackend(response)) {
      response.response.errorMessage = errorMessage;
    } else {
      response.errorMessage = errorMessage;
    }
  }

  static setErrorNumber(response: BackendResponse, errorNumber: number): void {
    if (this.isEvoBackend(response)) {
      response.response.errorNumber = errorNumber;
    } else {
      response.errorNumber = errorNumber;
    }
  }

  static setStringValue(response: BackendResponse, stringValue: string): void {
    if (this.isEvoBackend(response)) {
      response.response.stringValue = stringValue;
    } else {
      response.stringValue = stringValue;
    }
  }

  static setDecimalValue(
    response: BackendResponse,
    decimalValue: number
  ): void {
    if (this.isEvoBackend(response)) {
      response.response.decimalValue = decimalValue;
    } else {
      response.decimalValue = decimalValue;
    }
  }
}
