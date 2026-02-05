import { TestBed } from '@angular/core/testing';
import { of, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SignatoryContractAuthService } from './signatory-contract-auth.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { IGetPermissionResultDTO } from '@shared/models/generated-interfaces/GetPermissionResultDTO';
import { IAuthenticationResponseDTO } from '@shared/models/generated-interfaces/AuthenticationResponseDTO';
import { IAuthenticationDetailsDTO } from '@shared/models/generated-interfaces/AuthenticationDetailsDTO';
import { CrudResponse } from '@shared/interfaces';
import { TermGroup_SignatoryContractPermissionType, SignatoryContractAuthenticationMethodType } from '@shared/models/generated-interfaces/Enumerations';
import {
  signatoryContractAuthorize,
  signatoryContractAuthenticate,
} from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('SignatoryContractAuthService', () => {
  let service: SignatoryContractAuthService;
  let mockHttpClient: any;

  const mockAuthenticationDetails: IAuthenticationDetailsDTO = {
    authenticationRequestId: 123,
    authenticationMethodType: SignatoryContractAuthenticationMethodType.Password,
    message: 'Authentication required',
    validUntilUTC: new Date('2024-01-01T12:00:00Z'),
  };

  const mockPermissionResult: IGetPermissionResultDTO = {
    permissionType: TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
    permissionLabel: 'Edit Contracts',
    hasPermission: true,
    isAuthorized: true,
    isAuthenticated: true,
    isAuthenticationRequired: false,
    authenticationDetails: mockAuthenticationDetails,
  };

  const mockAuthenticationResponse: IAuthenticationResponseDTO = {
    signatoryContractAuthenticationRequestId: 123,
    username: 'john.doe',
    password: 'password123',
    code: 'ABC123',
  };

  const mockCrudResponse: CrudResponse = {
    booleanValue: true,
    booleanValue2: false,
    canUserOverride: true,
    dateTimeValue: '2024-01-01T00:00:00Z',
    decimalValue: 0,
    errorMessage: undefined,
    errorNumber: undefined,
    integerValue: 1,
    integerValue2: 0,
    modified: '2024-01-01T00:00:00Z',
    objectsAffected: 1,
    success: true,
    successNumber: 1,
    infoMessage: 'Authentication successful',
    stringValue: 'Success',
    value: {},
    value2: {},
  };

  beforeEach(async () => {
    const httpClientSpy = {
      post: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        SignatoryContractAuthService,
        { provide: SoeHttpClient, useValue: httpClientSpy },
      ],
    }).compileComponents();

    service = TestBed.inject(SignatoryContractAuthService);
    mockHttpClient = TestBed.inject(SoeHttpClient);
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('authorize Method', () => {
    it('should call http.post with correct endpoint', () => {
      const authorizeRequest = {
        permissionType: TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        signatoryContractId: 1
      };
      const expectedUrl = signatoryContractAuthorize();

      mockHttpClient.post.mockReturnValue(of(mockPermissionResult));

      service.authorize(authorizeRequest).subscribe();

      expect(mockHttpClient.post).toHaveBeenCalledWith(expectedUrl, authorizeRequest);
    });

    it('should return permission result from backend', async () => {
      const authorizeRequest = {
        permissionType: TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts,
        signatoryContractId: undefined
      };
      mockHttpClient.post.mockReturnValue(of(mockPermissionResult));

      const result = await firstValueFrom(
        service.authorize(authorizeRequest)
      );

      expect(result).toEqual(mockPermissionResult);
    });
  });

  describe('authenticate Method', () => {
    it('should call http.post with correct endpoint and data', () => {
      const expectedUrl = signatoryContractAuthenticate();

      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      service.authenticate(mockAuthenticationResponse).subscribe();

      expect(mockHttpClient.post).toHaveBeenCalledWith(
        expectedUrl,
        mockAuthenticationResponse
      );
    });

    it('should return CrudResponse from backend', async () => {
      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      const result = await firstValueFrom(
        service.authenticate(mockAuthenticationResponse)
      );

      expect(result).toEqual(mockCrudResponse);
    });
  });

});

