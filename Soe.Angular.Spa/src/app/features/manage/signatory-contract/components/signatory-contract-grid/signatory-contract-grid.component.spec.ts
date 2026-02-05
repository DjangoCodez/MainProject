import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SignatoryContractGridComponent } from './signatory-contract-grid.component';
import { SignatoryContractService } from '../../services/signatory-contract.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISignatoryContractGridDTO } from '@shared/models/generated-interfaces/SignatoryContractGridDTO';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('SignatoryContractGridComponent', () => {
  let component: SignatoryContractGridComponent;
  let fixture: ComponentFixture<SignatoryContractGridComponent>;
  let mockSignatoryContractService: any;
  let mockFlowHandlerService: any;
  let mockToolbarService: any;

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
      permissionNames: ['Permission 1', 'Permission 2'],
      permissions: 'Permission 1, Permission 2',
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
      permissionNames: ['Permission 3'],
      permissions: 'Permission 3',
      authenticationMethod: 'SMS Code',
      recipientUserId: 3,
      recipientUserName: 'Jane Smith'
    },
    {
      signatoryContractId: 3,
      actorCompanyId: 100,
      parentSignatoryContractId: undefined,
      signedByUserId: 3,
      creationMethodType: 1,
      requiredAuthenticationMethodType: 1,
      canPropagate: true,
      created: new Date('2024-01-03'),
      revokedAtUTC: undefined,
      revokedAt: undefined,
      revokedBy: '',
      revokedReason: '',
      permissionTypes: [1, 2, 3],
      permissionNames: ['Permission 1', 'Permission 2', 'Permission 3'],
      permissions: 'Permission 1, Permission 2, Permission 3',
      authenticationMethod: 'Password',
      recipientUserId: 4,
      recipientUserName: 'Bob Wilson'
    }
  ];

  beforeEach(async () => {
    const signatoryContractServiceSpy = {
      getGrid: vi.fn()
    };
    const flowHandlerServiceSpy = {
      startFlow: vi.fn()
    };
    const toolbarServiceSpy = {
      createItemGroup: vi.fn(),
      createToolbarButton: vi.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [SignatoryContractGridComponent],
      imports: [SoftOneTestBed],
      providers: [
        { provide: SignatoryContractService, useValue: signatoryContractServiceSpy },
        { provide: FlowHandlerService, useValue: flowHandlerServiceSpy },
        { provide: ToolbarService, useValue: toolbarServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignatoryContractGridComponent);
    component = fixture.componentInstance;
    
    mockSignatoryContractService = TestBed.inject(SignatoryContractService);

    // Setup default mock returns
    mockSignatoryContractService.getGrid.mockReturnValue(of(mockGridItems));
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have correct grid name', () => {
      expect(component.gridName).toBe('Manage.Registry.SignatoryContract');
    });
  });

  describe('onGridReadyToDefine', () => {
    it('should set up grid with all required columns', () => {
      const mockGrid = {
        addColumnDate: vi.fn(),
        addColumnText: vi.fn(),
        addColumnIconEdit: vi.fn(),
        finalizeInitGrid: vi.fn(),
        context: {}
      };

      component.terms = {
        'manage.registry.signatorycontract.createddate': 'Created Date',
        'manage.registry.signatorycontract.recipientuser': 'Recipient User',
        'manage.registry.signatorycontract.permissions': 'Permissions',
        'manage.registry.signatorycontract.authenticationmethod': 'Authentication Method',
        'manage.registry.signatorycontract.revokedat': 'Revoked At',
        'core.edit': 'Edit'
      };

      vi.spyOn(component, 'finalizeInitGrid').mockImplementation(() => {});

      component.onGridReadyToDefine(mockGrid as any);

      expect(mockGrid.addColumnDate).toHaveBeenCalledTimes(2); // created and revokedAt
      expect(mockGrid.addColumnText).toHaveBeenCalledTimes(3); // recipientUserName, permissions, authenticationMethod
      expect(mockGrid.addColumnIconEdit).toHaveBeenCalledTimes(1);
      expect(mockGrid.addColumnIconEdit).toHaveBeenCalledWith({
        tooltip: 'Edit',
        onClick: expect.any(Function)
      });
    });
  });

  describe('Data Loading', () => {
    it('should load data from service', async () => {
      const result = await firstValueFrom(component.loadData());
      expect(mockSignatoryContractService.getGrid).toHaveBeenCalled();
      expect(result).toEqual(mockGridItems);
    });
  });
});
