import { TestBed } from '@angular/core/testing';
import { of, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SubSignatoryContractService } from './sub-signatory-contract.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { getSignatoryContractSubContract } from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';

describe('SubSignatoryContractService', () => {
  let service: SubSignatoryContractService;
  let mockHttpClient: any;

  const mockSubContracts: ISignatoryContractDTO[] = [
    {
      signatoryContractId: 1,
      actorCompanyId: 100,
      parentSignatoryContractId: 0,
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
    },
    {
      signatoryContractId: 2,
      actorCompanyId: 100,
      parentSignatoryContractId: 0,
      signedByUserId: 2,
      signedByUserName: 'Manager User',
      recipientUserId: 3,
      recipientUserName: 'Jane Smith',
      recipients: [],
      creationMethodType: 2,
      canPropagate: false,
      revokedBy: '',
      revokedReason: '',
      created: new Date('2024-01-02'),
      createdBy: 'System',
      revokedAtUTC: undefined,
      revokedAt: undefined,
      requiredAuthenticationMethodType: 2,
      permissionTypes: [3],
      permissionNames: ['Permission3'],
      permissions: 'Permission3',
      subContracts: []
    },
    {
      signatoryContractId: 3,
      actorCompanyId: 100,
      parentSignatoryContractId: 0,
      signedByUserId: 3,
      signedByUserName: 'Supervisor User',
      recipientUserId: 4,
      recipientUserName: 'Bob Wilson',
      recipients: [],
      creationMethodType: 1,
      canPropagate: true,
      revokedBy: 'Admin',
      revokedReason: 'Contract expired',
      created: new Date('2024-01-03'),
      createdBy: 'System',
      revokedAtUTC: new Date('2024-01-15'),
      revokedAt: new Date('2024-01-15'),
      requiredAuthenticationMethodType: 1,
      permissionTypes: [1, 2, 3],
      permissionNames: ['Permission1', 'Permission2', 'Permission3'],
      permissions: 'Permission1, Permission2, Permission3',
      subContracts: []
    }
  ];

  const mockAdditionalProps = {
    signatoryContractParentId: 123
  };

  beforeEach(() => {
    const httpClientSpy = {
      get: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        SubSignatoryContractService,
        { provide: SoeHttpClient, useValue: httpClientSpy }
      ]
    });

    service = TestBed.inject(SubSignatoryContractService);
    mockHttpClient = TestBed.inject(SoeHttpClient);
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('getGrid Method', () => {
    it('should call http.get with correct endpoint', () => {
      const expectedUrl = getSignatoryContractSubContract(mockAdditionalProps.signatoryContractParentId);
      
      mockHttpClient.get.mockReturnValue(of(mockSubContracts));

      service.getGrid(123, mockAdditionalProps).subscribe();

      expect(mockHttpClient.get).toHaveBeenCalledWith(expectedUrl);
    });

    it('should return sub contracts from backend', async () => {
      mockHttpClient.get.mockReturnValue(of(mockSubContracts));

      const result = await firstValueFrom(service.getGrid(123, mockAdditionalProps));
      
      expect(result).toEqual(mockSubContracts);
    });
  });

});
