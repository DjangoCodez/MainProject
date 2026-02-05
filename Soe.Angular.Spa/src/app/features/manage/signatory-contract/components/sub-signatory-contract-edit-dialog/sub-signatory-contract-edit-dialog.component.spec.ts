import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { vi } from 'vitest';
import { SubSignatoryContractEditDialogComponent } from './sub-signatory-contract-edit-dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { TranslateService } from '@ngx-translate/core';
import { ChangeDetectorRef } from '@angular/core';
import { ISubSignatoryContractEditDialogData } from '../../models/sub-signatory-contract-edit-dialog-data';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

describe('SubSignatoryContractEditDialogComponent', () => {
  let component: SubSignatoryContractEditDialogComponent;
  let fixture: ComponentFixture<SubSignatoryContractEditDialogComponent>;
  let mockChangeDetectorRef: any;
  let mockDialogRef: any;


  const mockUsers: ISmallGenericType[] = [
    { id: 1, name: 'John Doe' },
    { id: 2, name: 'Jane Smith' },
    { id: 3, name: 'Bob Wilson' }
  ];

  const mockPermissionTerms: ISmallGenericType[] = [
    { id: 1, name: 'Permission 1' },
    { id: 2, name: 'Permission 2' },
    { id: 3, name: 'Permission 3' }
  ];

  const mockSignatoryContract: ISignatoryContractDTO = {
    signatoryContractId: 1,
    actorCompanyId: 100,
    parentSignatoryContractId: 0,
    signedByUserId: 1,
    signedByUserName: 'Admin User',
    recipientUserId: 2,
    recipientUserName: 'Jane Smith',
    recipients: [],
    creationMethodType: 1,
    canPropagate: true,
    revokedBy: '',
    revokedReason: '',
    created: new Date('2024-01-01'),
    createdBy: 'System',
    revokedAtUTC: undefined,
    revokedAt: undefined,
    requiredAuthenticationMethodType: 1,
    permissionTypes: [1, 2],
    permissionNames: ['Permission 1', 'Permission 2'],
    permissions: 'Permission 1, Permission 2',
    subContracts: []
  };

  const mockDialogData: ISubSignatoryContractEditDialogData = {
    title: 'Edit Sub Signatory Contract',
    size: 'md',
    rowToUpdate: mockSignatoryContract,
    users: mockUsers,
    permissionTerms: mockPermissionTerms
  };

  const mockPermissionEditItems: ISignatoryContractPermissionEditItem[] = [
    { id: 1, name: 'Permission 1', isSelected: true },
    { id: 2, name: 'Permission 2', isSelected: true },
    { id: 3, name: 'Permission 3', isSelected: false }
  ];

  beforeEach(async () => {
    const validationHandlerSpy = {
      validate: vi.fn()
    };
    const translateServiceSpy = {
      get: vi.fn().mockReturnValue(of({
        'manage.registry.signatorycontract.permission': 'Permission'
      }))
    };
    const changeDetectorRefSpy = {
      detectChanges: vi.fn()
    };
    const dialogRefSpy = {
      close: vi.fn(),
      addPanelClass: vi.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [SubSignatoryContractEditDialogComponent],
      imports: [SoftOneTestBed],
      providers: [
        { provide: ValidationHandler, useValue: validationHandlerSpy },
        { provide: TranslateService, useValue: translateServiceSpy },
        { provide: ChangeDetectorRef, useValue: changeDetectorRefSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: mockDialogData }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SubSignatoryContractEditDialogComponent);
    component = fixture.componentInstance;
    
    mockChangeDetectorRef = TestBed.inject(ChangeDetectorRef) as any;
    mockDialogRef = TestBed.inject(MatDialogRef) as any;

    // Ensure the component uses our mocked ChangeDetectorRef
    (component as any).cdRef = mockChangeDetectorRef;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form and grid with correct data', () => {
      expect((component as any).form).toBeDefined();
      expect((component as any).form.signatoryContractId.value).toBe(mockSignatoryContract.signatoryContractId);
      expect((component as any).form.recipientUserId.value).toBe(mockSignatoryContract.recipientUserId);
      expect((component as any).form.permissionTypes.value).toEqual(mockSignatoryContract.permissionTypes);
      expect((component as any).gridRows.value).toEqual(mockPermissionEditItems);
    });
  });

  describe('ok Method', () => {
    it('should close dialog with form values', () => {
      const mockFormValues = {
        signatoryContractId: 1,
        recipientUserId: 2,
        permissionTypes: [1, 2]
      };

      vi.spyOn((component as any).form, 'getAllValues').mockReturnValue(mockFormValues);
      
      (component as any).ok();

      expect(mockDialogRef.close).toHaveBeenCalledWith(mockFormValues);
    });
  });

  describe('getRows Method', () => {
    it('should map permission terms to rows with correct selection state', () => {
      const rows = component['getRows']();
      
      expect(rows).toEqual(mockPermissionEditItems);
      expect(rows[0].isSelected).toBe(true); // Permission 1 is selected
      expect(rows[1].isSelected).toBe(true); // Permission 2 is selected
      expect(rows[2].isSelected).toBe(false); // Permission 3 is not selected
    });
  });

  describe('toggleSelected Method', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should update form with selected permission IDs', () => {
      (component as any).gridRows.next([
        { id: 1, name: 'Permission 1', isSelected: true },
        { id: 2, name: 'Permission 2', isSelected: true },
        { id: 3, name: 'Permission 3', isSelected: true }
      ]);

      vi.spyOn((component as any).form.permissionTypes, 'setValue');

      component['toggleSelected'](true, { id: 3, name: 'Permission 3', isSelected: true });
      vi.advanceTimersByTime(1);

      expect((component as any).form.permissionTypes.setValue).toHaveBeenCalledWith([1, 2, 3]);
    });

    it('should handle empty selection', () => {
      (component as any).gridRows.next([
        { id: 1, name: 'Permission 1', isSelected: false },
        { id: 2, name: 'Permission 2', isSelected: false },
        { id: 3, name: 'Permission 3', isSelected: false }
      ]);

      vi.spyOn((component as any).form.permissionTypes, 'setValue');

      component['toggleSelected'](false, { id: 1, name: 'Permission 1', isSelected: false });
      vi.advanceTimersByTime(1);

      expect((component as any).form.permissionTypes.setValue).toHaveBeenCalledWith([]);
    });
  });
});
