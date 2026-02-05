import { computed, Injectable, signal } from '@angular/core';
import { VatCodeDTO } from '@features/economy/models/vat-code.model';
import {
  TermGroup_InvoiceVatType,
  TermGroup_Languages,
} from '@shared/models/generated-interfaces/Enumerations';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Injectable()
export class InvoiceVatService {
  private readonly defaultVatRate =
    SoeConfigUtil.sysCountryId === TermGroup_Languages.Finnish ? 25.5 : 25;

  private readonly vatCode = signal<VatCodeDTO | null>(null);
  private readonly vatCodeRate = computed(
    () => this.vatCode()?.percent ?? undefined
  );
  public readonly vatRate = computed(
    () => this.vatCodeRate() ?? this.defaultVatRate
  );
  public readonly vatCodeId = computed(
    () => this.vatCode()?.vatCodeId ?? undefined
  );
  public readonly purchaseVATAccountId = computed(
    () => this.vatCode()?.purchaseVATAccountId ?? undefined
  );

  public setVatCode(vatCode: VatCodeDTO | null) {
    this.vatCode.set(vatCode);
  }

  public calculateInvoiceVat(
    totalAmountCurrency: number,
    vatType: TermGroup_InvoiceVatType
  ) {
    if (this.calculatesVatFromGross(vatType))
      return this.calculateVatFromGross(totalAmountCurrency);

    if (this.calculatesVatFromNet(vatType))
      return this.calculateVatFromNet(totalAmountCurrency);

    return 0;
  }

  public calculateAccountingVat(
    totalAmountCurrency: number,
    vatType: TermGroup_InvoiceVatType
  ) {
    if (this.calculatesVatFromNet(vatType)) {
      return this.calculateVatFromNet(totalAmountCurrency);
    }

    return this.calculateVatFromGross(totalAmountCurrency);
  }

  public isVatLocked(vatType: TermGroup_InvoiceVatType): boolean {
    return (
      vatType == TermGroup_InvoiceVatType.Contractor ||
      vatType === TermGroup_InvoiceVatType.NoVat
    );
  }

  public isVatExempt(vatType: TermGroup_InvoiceVatType): boolean {
    return vatType === TermGroup_InvoiceVatType.NoVat;
  }

  public calculatesVatFromNet(vatType: TermGroup_InvoiceVatType): boolean {
    return (
      vatType === TermGroup_InvoiceVatType.Contractor ||
      vatType === TermGroup_InvoiceVatType.EU
    );
  }

  public calculatesVatFromGross(vatType: TermGroup_InvoiceVatType): boolean {
    return (
      vatType === TermGroup_InvoiceVatType.Merchandise ||
      vatType === TermGroup_InvoiceVatType.NonEU
    );
  }

  public shouldShowVatAsZero(vatType: TermGroup_InvoiceVatType) {
    return (
      vatType === TermGroup_InvoiceVatType.EU ||
      vatType === TermGroup_InvoiceVatType.NonEU ||
      vatType === TermGroup_InvoiceVatType.Contractor
    );
  }

  private calculateVatFromGross(totalAmountCurrency: number) {
    // 100 SEK & 25% => 20 SEK
    return (totalAmountCurrency * (1 - 1 / (this.vatRate() / 100 + 1))).round(
      2
    );
  }

  private calculateVatFromNet(totalAmountCurrency: number) {
    // 100 SEK & 25% => 25 SEK
    return (totalAmountCurrency * (this.vatRate() / 100)).round(2);
  }
}
