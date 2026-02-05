import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectEinvoiceRecipientDialogComponent } from './select-einvoice-recipient-dialog.component';

describe('SelectEinvoiceRecipientDialogComponent', () => {
  let component: SelectEinvoiceRecipientDialogComponent;
  let fixture: ComponentFixture<SelectEinvoiceRecipientDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SelectEinvoiceRecipientDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectEinvoiceRecipientDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
