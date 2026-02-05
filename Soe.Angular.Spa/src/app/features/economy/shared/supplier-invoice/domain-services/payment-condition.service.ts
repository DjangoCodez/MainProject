import { computed, Injectable, signal } from '@angular/core';
import { PaymentConditionDTO } from '@shared/features/payment-conditions/models/payment-condition.model';

@Injectable()
export class InvoicePaymentConditionService {
  public readonly paymentCondition = signal<PaymentConditionDTO | null>(null);
  private readonly discountDays = computed(
    () => this.paymentCondition()?.discountDays ?? 0
  );
  private readonly discountPercent = computed(
    () => this.paymentCondition()?.discountPercent ?? 0
  );
  private readonly paymentConditionDays = computed(
    () => this.paymentCondition()?.days ?? 0
  );

  public changePaymentCondition(paymentCondition?: PaymentConditionDTO) {
    this.paymentCondition.set(paymentCondition ?? null);
  }

  public getTimeDiscount(invoiceDate: Date) {
    const date = new Date(invoiceDate);
    date.setDate(date.getDate() + this.discountDays());

    return {
      days: this.discountDays(),
      percent: this.discountPercent(),
      date,
    };
  }

  public getDueDate(invoiceDate: Date) {
    const dueDate = new Date(invoiceDate);
    dueDate.setDate(dueDate.getDate() + this.paymentConditionDays());
    return dueDate;
  }
}
