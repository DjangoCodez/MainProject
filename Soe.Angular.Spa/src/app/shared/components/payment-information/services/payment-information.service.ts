import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getBicFromIban,
  isIbanValid,
} from '@shared/services/generated-service-endpoints/economy/PaymentInformation.endpoints';

@Injectable({
  providedIn: 'root',
})
export class PaymentInformationService {
  constructor(private http: SoeHttpClient) {}

  getBicFromIban(iban: string) {
    const ibanWithoutSpaces = iban.replace(/\s/g, '');
    return this.http.get<string>(getBicFromIban(ibanWithoutSpaces));
  }

  isIbanValid(iban: string) {
    const ibanWithoutSpaces = iban.replace(/\s/g, '');
    return this.http.get<boolean>(isIbanValid(ibanWithoutSpaces));
  }

  isValidBic(bic: string, acceptEmpty = false) {
    if (!bic || bic.removeWhitespaces().length === 0) return acceptEmpty;

    const regex = new RegExp('^[A-Z0-9]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$');

    return regex.test(bic);
  }

  isBgValid(bankgiro: string): boolean {
    const bgFormat = /^(\d{3,4})-(\d{4})$/;
    const match = bankgiro.match(bgFormat);

    if (!match) {
      return false;
    }

    // Remove the dash for validation
    const sanitized = match[1] + match[2];

    // Validate the length (should be 7 or 8 digits)
    if (sanitized.length < 7 || sanitized.length > 8) {
      return false;
    }

    return this.isValidLuhn(sanitized);
  }

  isValidLuhn(number: string): boolean {
    let sum = 0;
    let alternate = false;

    for (let i = number.length - 1; i >= 0; i--) {
      let n = parseInt(number[i], 10);

      if (alternate) {
        n *= 2;
        if (n > 9) {
          n -= 9;
        }
      }

      sum += n;
      alternate = !alternate;
    }

    return sum % 10 === 0;
  }
}
