import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SaveUserSelectionDialogComponent } from './save-user-selection-dialog.component';

describe('SaveUserSelectionDialogComponent', () => {
  let component: SaveUserSelectionDialogComponent;
  let fixture: ComponentFixture<SaveUserSelectionDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SaveUserSelectionDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SaveUserSelectionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
