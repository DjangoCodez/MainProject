import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { SignatoryContractAuthDialogComponent } from './signatory-contract-auth-dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TranslateService } from '@ngx-translate/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SignatoryContractAuthDialogData } from '../../models/signatory-contract-auth-dialog-data.model';
import {
  TermGroup_SignatoryContractPermissionType,
  SignatoryContractAuthenticationMethodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { of } from 'rxjs';
import { IGetPermissionResultDTO } from '@shared/models/generated-interfaces/GetPermissionResultDTO';
import { IAuthenticationDetailsDTO } from '@shared/models/generated-interfaces/AuthenticationDetailsDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

describe('SignatoryContractAuthDialogComponent', () => {
  let component: SignatoryContractAuthDialogComponent;
  let fixture: ComponentFixture<SignatoryContractAuthDialogComponent>;
  let mockDialogRef: any;
  let mockHttpClient: any;
  let mockProgressService: any;
  let mockTranslateService: any;
  let mockValidationHandler: any;
  let dialogData: SignatoryContractAuthDialogData;

  beforeEach(async () => {
    dialogData = new SignatoryContractAuthDialogData();
    dialogData.title = 'Authentication';
    dialogData.permissionType =
      TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts;

    mockDialogRef = {
      close: vi.fn(),
      addPanelClass: vi.fn(),
      removePanelClass: vi.fn(),
      updateSize: vi.fn(),
      updatePosition: vi.fn(),
    };

    // Default mock response to prevent HTTP errors during component initialization
    // Set to require authentication so dialog doesn't auto-close
    const defaultMockResult: IGetPermissionResultDTO = {
      permissionType:
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
      permissionLabel: 'Edit Contracts',
      hasPermission: true,
      isAuthorized: false,
      isAuthenticationRequired: true,
      authenticationDetails: {
        authenticationRequestId: 1,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Test authentication required',
        validUntilUTC: new Date(),
      },
    };

    mockHttpClient = {
      post: vi.fn().mockReturnValue(of(defaultMockResult)),
    };

    mockProgressService = {
      show: vi.fn(),
      hide: vi.fn(),
      load: vi.fn(),
      save: vi.fn(),
      delete: vi.fn(),
      work: vi.fn(),
      loadError: vi.fn(),
      saveError: vi.fn(),
      deleteError: vi.fn(),
      workError: vi.fn(),
      loadComplete: vi.fn(),
      saveComplete: vi.fn(),
      deleteComplete: vi.fn(),
      workComplete: vi.fn(),
    };

    mockTranslateService = {
      instant: vi.fn().mockReturnValue('Authentication'),
    };

    mockValidationHandler = {
      handle: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, SignatoryContractAuthDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        { provide: SoeHttpClient, useValue: mockHttpClient },
        { provide: ProgressService, useValue: mockProgressService },
        { provide: TranslateService, useValue: mockTranslateService },
        { provide: ValidationHandler, useValue: mockValidationHandler },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignatoryContractAuthDialogComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('loadData', () => {
    it('should set hasPermission to false when user has no permission', async () => {
      const mockResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: false,
        isAuthorized: false,
        isAuthenticationRequired: false,
        authenticationDetails: null as any,
      };

      mockHttpClient.post.mockReturnValue(of(mockResult));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['hasPermission']()).toBe(false);
    });

    it('should call onSuccess when user is already authorized', async () => {
      const mockResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: true,
        isAuthenticationRequired: false,
        authenticationDetails: null as any,
      };

      mockHttpClient.post.mockReturnValue(of(mockResult));
      vi.spyOn(component as any, 'onSuccess').mockImplementation(() => {});

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['onSuccess']).toHaveBeenCalled();
    });

    it('should setup authentication when authentication is required', async () => {
      const authDetails: IAuthenticationDetailsDTO = {
        authenticationRequestId: 123,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Please authenticate',
        validUntilUTC: new Date(),
      };

      const mockResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: false,
        isAuthenticationRequired: true,
        authenticationDetails: authDetails,
      };

      mockHttpClient.post.mockReturnValue(of(mockResult));
      vi.spyOn(component as any, 'setupAuthenticate').mockImplementation(
        () => {}
      );

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['setupAuthenticate']).toHaveBeenCalled();
    });

    it('should pass signatoryContractId when provided in dialog data', async () => {
      dialogData.signatoryContractId = 123;
      component['data'] = dialogData;

      const mockResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: true,
        isAuthenticationRequired: false,
        authenticationDetails: null as any,
      };

      mockHttpClient.post.mockReturnValue(of(mockResult));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));

      expect(mockHttpClient.post).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          permissionType:
            TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
          signatoryContractId: 123,
        })
      );
    });

    it('should work without signatoryContractId when not provided', async () => {
      dialogData.signatoryContractId = undefined;
      component['data'] = dialogData;

      const mockResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: true,
        isAuthenticationRequired: false,
        authenticationDetails: null as any,
      };

      mockHttpClient.post.mockReturnValue(of(mockResult));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));

      expect(mockHttpClient.post).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          permissionType:
            TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
          signatoryContractId: undefined,
        })
      );
    });
  });

  describe('setupAuthenticate', () => {
    it('should configure UI for Password authentication method', () => {
      component['authenticationDetails'] = {
        authenticationRequestId: 123,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Enter password',
        validUntilUTC: new Date(),
      };

      component['setupAuthenticate']();

      expect(component['showPassword']()).toBe(true);
      expect(component['showCode']()).toBe(false);
      expect(component['message']()).toBe('Enter password');
      expect(component['couldNotAuth']()).toBe(false);
    });

    it('should configure UI for PasswordSMSCode authentication method', () => {
      component['authenticationDetails'] = {
        authenticationRequestId: 123,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.PasswordSMSCode,
        message: 'Enter password and SMS code',
        validUntilUTC: new Date(),
      };

      component['setupAuthenticate']();

      expect(component['showPassword']()).toBe(true);
      expect(component['showCode']()).toBe(true);
    });

    it('should set couldNotAuth when authenticationRequestId is 0', () => {
      component['authenticationDetails'] = {
        authenticationRequestId: 0,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Error',
        validUntilUTC: new Date(),
      };

      component['setupAuthenticate']();

      expect(component['couldNotAuth']()).toBe(true);
    });
  });

  describe('ok', () => {
    it('should call onSuccess when authentication is successful', async () => {
      component['authenticationDetails'] = {
        authenticationRequestId: 999,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Test',
        validUntilUTC: new Date(),
      };

      component['form'].username.setValue('testuser');
      component['form'].password.setValue('testpass');

      const mockResponse = {
        success: true,
        stringValue: 'Authentication successful',
      } as BackendResponse;

      mockHttpClient.post.mockReturnValue(of(mockResponse));
      vi.spyOn(component as any, 'onSuccess').mockImplementation(() => {});

      component['ok']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['onSuccess']).toHaveBeenCalled();
    });

    it('should update message when authentication fails', async () => {
      component['authenticationDetails'] = {
        authenticationRequestId: 999,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Test',
        validUntilUTC: new Date(),
      };

      component['form'].username.setValue('testuser');
      component['form'].password.setValue('wrongpass');

      const mockResponse = {
        success: false,
        stringValue: 'Invalid credentials',
      } as BackendResponse;

      mockHttpClient.post.mockReturnValue(of(mockResponse));
      vi.spyOn(component as any, 'onSuccess').mockImplementation(() => {});

      component['ok']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['onSuccess']).not.toHaveBeenCalled();
      expect(component['message']()).toBe('Invalid credentials');
    });
  });

  describe('onSuccess', () => {
    it('should close dialog with true value', () => {
      component['onSuccess']();

      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });
  });

  describe('Integration Tests - Complete Flows', () => {
    it('should handle complete flow: password authentication successful', async () => {
      const authDetails: IAuthenticationDetailsDTO = {
        authenticationRequestId: 123,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Please enter password',
        validUntilUTC: new Date(),
      };

      const mockAuthResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: false,
        isAuthenticationRequired: true,
        authenticationDetails: authDetails,
      };

      const mockAuthResponse = {
        success: true,
        stringValue: 'Authenticated successfully',
      } as BackendResponse;

      mockHttpClient.post
        .mockReturnValueOnce(of(mockAuthResult))
        .mockReturnValueOnce(of(mockAuthResponse));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['showPassword']()).toBe(true);
      expect(component['showCode']()).toBe(false);

      component['form'].username.setValue('john.doe');
      component['form'].password.setValue('password123');
      component['ok']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should handle complete flow: password + SMS authentication successful', async () => {
      const authDetails: IAuthenticationDetailsDTO = {
        authenticationRequestId: 456,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.PasswordSMSCode,
        message: 'Please enter password and SMS code',
        validUntilUTC: new Date(),
      };

      const mockAuthResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: false,
        isAuthenticationRequired: true,
        authenticationDetails: authDetails,
      };

      const mockAuthResponse = {
        success: true,
        stringValue: 'Authenticated successfully',
      } as BackendResponse;

      mockHttpClient.post
        .mockReturnValueOnce(of(mockAuthResult))
        .mockReturnValueOnce(of(mockAuthResponse));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['showPassword']()).toBe(true);
      expect(component['showCode']()).toBe(true);

      component['form'].username.setValue('john.doe');
      component['form'].password.setValue('password123');
      component['form'].code.setValue('123456');
      component['ok']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should handle complete flow: authentication fails', async () => {
      const authDetails: IAuthenticationDetailsDTO = {
        authenticationRequestId: 789,
        authenticationMethodType:
          SignatoryContractAuthenticationMethodType.Password,
        message: 'Please enter password',
        validUntilUTC: new Date(),
      };

      const mockAuthResult: IGetPermissionResultDTO = {
        permissionType:
          TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        permissionLabel: 'Edit Contracts',
        hasPermission: true,
        isAuthorized: false,
        isAuthenticationRequired: true,
        authenticationDetails: authDetails,
      };

      const mockAuthResponse = {
        success: false,
        stringValue: 'Invalid password',
      } as BackendResponse;

      mockHttpClient.post
        .mockReturnValueOnce(of(mockAuthResult))
        .mockReturnValueOnce(of(mockAuthResponse));

      component['loadData']();

      await new Promise(resolve => setTimeout(resolve, 10));
      component['form'].username.setValue('john.doe');
      component['form'].password.setValue('wrongpassword');
      component['ok']();

      await new Promise(resolve => setTimeout(resolve, 10));
      expect(component['message']()).toBe('Invalid password');
      expect(mockDialogRef.close).not.toHaveBeenCalledWith(true);
    });
  });
});
