import { TestBed } from '@angular/core/testing';
import { of, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SignatoryContractPermissionsService } from './signatory-contract-permissions.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';
import { getPermissionTerms } from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('SignatoryContractPermissionsService', () => {
  let service: SignatoryContractPermissionsService;
  let mockHttpClient: any;

  const mockPermissionItems: ISignatoryContractPermissionEditItem[] = [
    {
      id: 1,
      name: 'Permission 1',
      isSelected: true
    },
    {
      id: 2,
      name: 'Permission 2',
      isSelected: false
    },
    {
      id: 3,
      name: 'Permission 3',
      isSelected: true
    }
  ];

  beforeEach(async () => {
    const httpClientSpy = {
      get: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        SignatoryContractPermissionsService,
        { provide: SoeHttpClient, useValue: httpClientSpy }
      ]
    }).compileComponents();

    service = TestBed.inject(SignatoryContractPermissionsService);
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
      const expectedUrl = getPermissionTerms(testId);
      
      mockHttpClient.get.mockReturnValue(of(mockPermissionItems));

      service.getGrid(testId).subscribe();

      expect(mockHttpClient.get).toHaveBeenCalledWith(expectedUrl);
    });

    it('should return permission items from backend', async () => {
      mockHttpClient.get.mockReturnValue(of(mockPermissionItems));

      const result = await firstValueFrom(service.getGrid(123));
      
      expect(result).toEqual(mockPermissionItems);
    });
  });

});
