import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectEinvoiceRecipientResultGridComponent } from './select-einvoice-recipient-result-grid.component';

describe('SelectEinvoiceRecipientResultGridComponent', () => {
  let component: SelectEinvoiceRecipientResultGridComponent;
  let fixture: ComponentFixture<SelectEinvoiceRecipientResultGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SelectEinvoiceRecipientResultGridComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectEinvoiceRecipientResultGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
