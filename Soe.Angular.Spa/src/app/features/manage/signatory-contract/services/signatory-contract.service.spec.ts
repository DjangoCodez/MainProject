import { TestBed } from '@angular/core/testing';
import { of, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SignatoryContractService } from './signatory-contract.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISignatoryContractGridDTO } from '@shared/models/generated-interfaces/SignatoryContractGridDTO';
import { SignatoryContractDTO } from '../models/signatory-contract-edit-dto.model';
import { SignatoryContractRevokeDTO } from '../models/signatory-contract-revoke-dto';
import { CrudResponse } from '@shared/interfaces';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  getSignatoryContractsGrid,
  getSignatoryContract,
  saveSignatoryContract,
  revokeSignatoryContract,
} from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';

describe('SignatoryContractService', () => {
  let service: SignatoryContractService;
  let mockHttpClient: any;

  const mockGridItems: ISignatoryContractGridDTO[] = [
    {
      signatoryContractId: 1,
      actorCompanyId: 100,
      parentSignatoryContractId: undefined,
      signedByUserId: 1,
      creationMethodType: 1,
      requiredAuthenticationMethodType: 1,
      canPropagate: true,
      created: new Date('2024-01-01'),
      revokedAtUTC: undefined,
      revokedAt: undefined,
      revokedBy: '',
      revokedReason: '',
      permissionTypes: [1, 2],
      permissionNames: ['Permission1', 'Permission2'],
      permissions: 'Permission1, Permission2',
      authenticationMethod: 'Password',
      recipientUserId: 2,
      recipientUserName: 'John Doe'
    },
    {
      signatoryContractId: 2,
      actorCompanyId: 100,
      parentSignatoryContractId: 1,
      signedByUserId: 2,
      creationMethodType: 2,
      requiredAuthenticationMethodType: 2,
      canPropagate: false,
      created: new Date('2024-01-02'),
      revokedAtUTC: new Date('2024-01-15'),
      revokedAt: new Date('2024-01-15'),
      revokedBy: 'Admin',
      revokedReason: 'Contract expired',
      permissionTypes: [3],
      permissionNames: ['Permission3'],
      permissions: 'Permission3',
      authenticationMethod: 'SMS Code',
      recipientUserId: 3,
      recipientUserName: 'Jane Smith'
    }
  ];

  const mockSignatoryContract: SignatoryContractDTO = {
    signatoryContractId: 1,
    actorCompanyId: 100,
    parentSignatoryContractId: undefined,
    signedByUserId: 1,
    signedByUserName: 'Admin User',
    recipientUserId: 2,
    recipientUserName: 'John Doe',
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
    permissionNames: ['Permission1', 'Permission2'],
    permissions: 'Permission1, Permission2',
    subContracts: []
  };

  const mockRevokeDTO: SignatoryContractRevokeDTO = {
    signatoryContractId: 1,
    revokedReason: 'Contract no longer needed'
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
    infoMessage: 'Operation completed successfully',
    stringValue: 'Success',
    value: {},
    value2: {}
  };

  beforeEach(async () => {
    const httpClientSpy = {
      get: vi.fn(),
      post: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        SignatoryContractService,
        { provide: SoeHttpClient, useValue: httpClientSpy }
      ]
    }).compileComponents();

    service = TestBed.inject(SignatoryContractService);
    mockHttpClient = TestBed.inject(SoeHttpClient);
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getGrid Method', () => {
    it('should call http.get with correct endpoint', () => {
      const testId = 123;
      const expectedUrl = getSignatoryContractsGrid(testId);
      
      mockHttpClient.get.mockReturnValue(of(mockGridItems));

      service.getGrid(testId).subscribe();

      expect(mockHttpClient.get).toHaveBeenCalledWith(expectedUrl);
    });

    it('should return grid items from backend', async () => {
      mockHttpClient.get.mockReturnValue(of(mockGridItems));

      const result = await firstValueFrom(service.getGrid(123));
      
      expect(result).toEqual(mockGridItems);
    });
  });

  describe('get Method', () => {
    it('should call http.get with correct endpoint', () => {
      const testId = 123;
      const expectedUrl = getSignatoryContract(testId);
      
      mockHttpClient.get.mockReturnValue(of(mockSignatoryContract));

      service.get(testId).subscribe();

      expect(mockHttpClient.get).toHaveBeenCalledWith(expectedUrl);
    });

    it('should return signatory contract from backend', async () => {
      mockHttpClient.get.mockReturnValue(of(mockSignatoryContract));

      const result = await firstValueFrom(service.get(123));
      
      expect(result).toEqual(mockSignatoryContract);
    });
  });

  describe('save Method', () => {
    it('should call http.post with correct endpoint and data', () => {
      const expectedUrl = saveSignatoryContract();
      
      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      service.save(mockSignatoryContract).subscribe();

      expect(mockHttpClient.post).toHaveBeenCalledWith(expectedUrl, mockSignatoryContract);
    });

    it('should return CrudResponse from backend', async () => {
      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      const result = await firstValueFrom(service.save(mockSignatoryContract));
      
      expect(result).toEqual(mockCrudResponse);
    });
  });

  describe('delete Method', () => {
    it('should return empty observable', async () => {
      const result = await firstValueFrom(service.delete(123), { defaultValue: undefined });
      
      expect(result).toBeUndefined();
    });
  });

  describe('revoke Method', () => {
    it('should call http.post with correct endpoint and revoke data', () => {
      const expectedUrl = revokeSignatoryContract(mockRevokeDTO.signatoryContractId);
      
      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      service.revoke(mockRevokeDTO).subscribe();

      expect(mockHttpClient.post).toHaveBeenCalledWith(expectedUrl, mockRevokeDTO);
    });

    it('should return CrudResponse from backend', async () => {
      mockHttpClient.post.mockReturnValue(of(mockCrudResponse));

      const result = await firstValueFrom(service.revoke(mockRevokeDTO));
      
      expect(result).toEqual(mockCrudResponse);
    });
  });

});
