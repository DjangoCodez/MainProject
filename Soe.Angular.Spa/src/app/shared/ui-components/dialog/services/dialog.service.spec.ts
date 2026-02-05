import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DialogService } from './dialog.service';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogSize } from '@ui/dialog/models/dialog';
import { EditComponentDialogComponent } from '../edit-component-dialog/edit-component-dialog.component';
import { Mock, vi } from 'vitest';

describe('DialogServiceV2', () => {
  let service: DialogService;
  let matDialogMock: { open: Mock };

  beforeEach(() => {
    // MatDialog mock
    matDialogMock = {
      open: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [{ provide: MatDialog, useValue: matDialogMock }],
    });
    service = TestBed.inject(DialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
  it('should open the confirmation dialog with the correct data', () => {
    const dialogRefMock: MatDialogRef<DialogComponent<any>> =
      {} as MatDialogRef<DialogComponent<any>>;
    matDialogMock.open.mockReturnValue(dialogRefMock);

    const dialogData = {
      title: 'Confirm Delete',
      content: 'Are you sure you want to delete this item?',
      primaryText: 'Yes',
      secondaryText: 'No',
      size: 'md' as DialogSize,
    };

    const result = service.confirm(dialogData);

    expect(matDialogMock.open).toHaveBeenCalledWith(DialogComponent, {
      data: dialogData,
    });

    expect(result).toBe(dialogRefMock);
  });

  it('should open a generic component dialog with the correct data', () => {
    const dialogRefMock: MatDialogRef<any> = {} as MatDialogRef<any>;
    matDialogMock.open.mockReturnValue(dialogRefMock);

    const component = DialogComponent;
    const dialogData = {
      title: 'Custom Dialog',
      content: 'Custom content for the dialog',
    };

    const result = service.open(component, dialogData);

    expect(matDialogMock.open).toHaveBeenCalledWith(component, {
      data: dialogData,
      disableClose: undefined,
      hasBackdrop: true,
    });

    expect(result).toBe(dialogRefMock);
  });

  it('should open the edit component dialog with the correct data', () => {
    const dialogRefMock: MatDialogRef<any> = {} as MatDialogRef<any>;
    matDialogMock.open.mockReturnValue(dialogRefMock);

    const editComponentData = {
      item: { id: 1, name: 'Test Item' },
      apiService: {} as any,
      formGroup: {} as any,
      form: {} as any,
      editComponent: {} as any,
      title: 'Edit Item',
    };

    const result = service.openEditComponent(editComponentData);

    expect(matDialogMock.open).toHaveBeenCalledWith(
      EditComponentDialogComponent,
      {
        data: editComponentData,
        disableClose: undefined,
        hasBackdrop: true,
      }
    );

    expect(result).toBe(dialogRefMock);
  });
});
