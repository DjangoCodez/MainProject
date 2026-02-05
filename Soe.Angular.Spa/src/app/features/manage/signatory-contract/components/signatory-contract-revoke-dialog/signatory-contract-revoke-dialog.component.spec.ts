import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { SignatoryContractRevokeDialogComponent } from './signatory-contract-revoke-dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogData } from '@ui/dialog/models/dialog';
import { SignatoryContractRevokeDTO } from '../../models/signatory-contract-revoke-dto';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('SignatoryContractRevokeDialogComponent', () => {
  let component: SignatoryContractRevokeDialogComponent;
  let fixture: ComponentFixture<SignatoryContractRevokeDialogComponent>;
  let mockDialogRef: any;

  const mockDialogData: DialogData = {
    title: 'Revoke Signatory Contract',
    size: 'md',
    hideCloseButton: false
  };


  beforeEach(async () => {
    const validationHandlerSpy = {
      validate: vi.fn()
    };
    const dialogRefSpy = {
      close: vi.fn(),
      addPanelClass: vi.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [SignatoryContractRevokeDialogComponent],
      imports: [SoftOneTestBed],
      providers: [
        { provide: ValidationHandler, useValue: validationHandlerSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: mockDialogData }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignatoryContractRevokeDialogComponent);
    component = fixture.componentInstance;
    
    mockDialogRef = TestBed.inject(MatDialogRef) as any;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      expect((component as any).form).toBeDefined();
      expect((component as any).form.signatoryContractId.value).toBe(0);
      expect((component as any).form.revokedReason.value).toBe('');
    });
  });

  describe('ok Method', () => {
    it('should close dialog with revoked reason value', () => {
      const testReason = 'Contract no longer needed';
      (component as any).form.revokedReason.setValue(testReason);
      
      (component as any).ok();

      expect(mockDialogRef.close).toHaveBeenCalledWith(testReason);
    });
  });

  describe('Form Validation', () => {
    it('should require revoked reason', () => {
      (component as any).form.revokedReason.setValue('');
      
      expect((component as any).form.revokedReason.valid).toBeFalsy();
      expect((component as any).form.revokedReason.errors?.['required']).toBeTruthy();
    });
  });
});
