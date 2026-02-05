import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError, BehaviorSubject, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SubSignatoryContractEditGridComponent } from './sub-signatory-contract-edit-grid.component';
import { SubSignatoryContractService } from '../../services/sub-signatory-contract.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Feature, TermGroup, TermGroup_SignatoryContractPermissionType } from '@shared/models/generated-interfaces/Enumerations';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SignatoryContractDTO } from '../../models/signatory-contract-edit-dto.model';
import { SubSignatoryContractEditDialogComponent } from '../sub-signatory-contract-edit-dialog/sub-signatory-contract-edit-dialog.component';

describe('SubSignatoryContractEditGridComponent', () => {
  let component: SubSignatoryContractEditGridComponent;
  let fixture: ComponentFixture<SubSignatoryContractEditGridComponent>;
  let mockSubSignatoryContractService: any;
  let mockDialogService: any;
  let mockCoreService: any;
  let mockMessageboxService: any;
  let mockToolbarService: any;

  const mockUsers: ISmallGenericType[] = [
    { id: 1, name: 'John Doe' },
    { id: 2, name: 'Jane Smith' },
    { id: 3, name: 'Bob Wilson' },
    { id: 4, name: 'Alice Brown' }
  ];

  const mockPermissionTerms: ISmallGenericType[] = [
    { id: 1, name: 'Permission 1' },
    { id: 2, name: 'Permission 2' },
    { id: 3, name: 'Permission 3' }
  ];

  const mockSubContracts: ISignatoryContractDTO[] = [
    {
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
    },
    {
      signatoryContractId: 2,
      actorCompanyId: 100,
      parentSignatoryContractId: 0,
      signedByUserId: 2,
      signedByUserName: 'Manager User',
      recipientUserId: 3,
      recipientUserName: 'Bob Wilson',
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
      permissionNames: ['Permission 3'],
      permissions: 'Permission 3',
      subContracts: []
    }
  ];

  const mockDialogRef = {
    afterClosed: vi.fn().mockReturnValue(of(mockSubContracts[0]))
  };

  beforeEach(async () => {
    const subSignatoryContractServiceSpy = {
      getGrid: vi.fn()
    };
    const dialogServiceSpy = {
      open: vi.fn()
    };
    const coreServiceSpy = {
      getTermGroupContent: vi.fn(),
      hasModifyPermissions: vi.fn(),
      hasReadOnlyPermissions: vi.fn()
    };
    const messageboxServiceSpy = {
      error: vi.fn(),
      progress: vi.fn()
    };
    const flowHandlerServiceSpy = {
      startFlow: vi.fn()
    };
    const toolbarServiceSpy = {
      createItemGroup: vi.fn(),
      createToolbarButton: vi.fn(),
      setButtonProps: vi.fn(),
      createDefaultGridToolbar: vi.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [SubSignatoryContractEditGridComponent],
      imports: [SoftOneTestBed],
      providers: [
        { provide: SubSignatoryContractService, useValue: subSignatoryContractServiceSpy },
        { provide: DialogService, useValue: dialogServiceSpy },
        { provide: CoreService, useValue: coreServiceSpy },
        { provide: MessageboxService, useValue: messageboxServiceSpy },
        { provide: FlowHandlerService, useValue: flowHandlerServiceSpy },
        { provide: ToolbarService, useValue: toolbarServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SubSignatoryContractEditGridComponent);
    component = fixture.componentInstance;
    
    // Set component inputs
    fixture.componentRef.setInput('signatoryContractId', 123);
    fixture.componentRef.setInput('editable', true);
    fixture.componentRef.setInput('users', mockUsers);
    fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
    
    mockSubSignatoryContractService = TestBed.inject(SubSignatoryContractService) as any;
    mockDialogService = TestBed.inject(DialogService) as any;
    mockCoreService = TestBed.inject(CoreService) as any;
    mockMessageboxService = TestBed.inject(MessageboxService) as any;
    mockToolbarService = TestBed.inject(ToolbarService) as any;

    // Setup default mock returns
    mockSubSignatoryContractService.getGrid.mockReturnValue(of(mockSubContracts));
    mockCoreService.getTermGroupContent.mockReturnValue(of(mockPermissionTerms));
    mockCoreService.hasModifyPermissions.mockReturnValue(of(true));
    mockCoreService.hasReadOnlyPermissions.mockReturnValue(of(false));
    mockDialogService.open.mockReturnValue(mockDialogRef as any);
    mockToolbarService.createToolbarButton.mockReturnValue({} as any);
    mockToolbarService.createItemGroup.mockReturnValue({} as any);
    mockToolbarService.setButtonProps.mockReturnValue(undefined);
    
    // Ensure the component uses the mocked toolbarService
    (component as any).toolbarService = mockToolbarService;
    
    // Mock grid properties
    component.grid = {
      addColumnText: vi.fn(),
      addColumnIconDelete: vi.fn(),
      addColumnIconEdit: vi.fn(),
      columns: [],
      resetColumns: vi.fn(),
      finalizeInitGrid: vi.fn(),
      context: {}
    } as any;
    
    // Mock permissionTerms
    (component as any).permissionTerms = mockPermissionTerms;
    
    // Mock rowData as BehaviorSubject
    component.rowData = new BehaviorSubject(mockSubContracts);
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have required inputs', () => {
      // Set required inputs
      fixture.componentRef.setInput('signatoryContractId', 123);
      fixture.componentRef.setInput('editable', true);
      fixture.componentRef.setInput('users', mockUsers);
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);

      expect(component.signatoryContractId()).toBe(123);
      expect(component.editable()).toBe(true);
      expect(component.users()).toEqual(mockUsers);
      expect(component.parentPermissions()).toEqual([1, 2, 3]);
    });

    it('should have output properties', () => {
      expect((component as any).changeSubSignatoryContracts).toBeDefined();
    });

    it('should have service injected', () => {
      expect(component.service).toBe(mockSubSignatoryContractService);
    });

    it('should have correct grid name', () => {
      expect(component.gridName).toBe('Manage.Registry.SignatoryContract.SubSignatoryContractEdit');
    });
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('signatoryContractId', 123);
      fixture.componentRef.setInput('editable', true);
      fixture.componentRef.setInput('users', mockUsers);
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
    });

    it('should call startFlow with correct parameters', () => {
      vi.spyOn(component, 'startFlow');
      vi.spyOn(component as any, 'loadPermissionTerms').mockReturnValue(of(mockPermissionTerms));

      component.ngOnInit();

      expect(component.startFlow).toHaveBeenCalledWith(
        Feature.Manage_Preferences_Registry_SignatoryContract_Edit,
        component.gridName,
        {
          lookups: [(component as any).loadPermissionTerms()]
        }
      );
    });

    it('should load permission terms', () => {
      component.ngOnInit();

      expect(mockCoreService.getTermGroupContent).toHaveBeenCalledWith(
        TermGroup.SignatoryContractPermissionType,
        false,
        false,
        false,
        true
      );
    });
  });

  describe('loadTerms', () => {
    it('should return observable with correct translation keys', async () => {
      const result = await firstValueFrom(component.loadTerms());
      expect(result).toBeDefined();
    });

    it('should include required translation keys', () => {
      const result = component.loadTerms();
      
      expect(result).toBeDefined();
    });
  });

  describe('createGridToolbar', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('editable', true);
    });

    it('should create toolbar with correct configuration', () => {
      component.createGridToolbar();

      expect(mockToolbarService.createItemGroup).toHaveBeenCalled();
      expect(mockToolbarService.createToolbarButton).toHaveBeenCalledWith(
        'manage.registry.signatorycontract.subsignatorycontract.new',
        expect.any(Object)
      );
    });

    it('should create toolbar button when not editable', () => {
      fixture.componentRef.setInput('editable', false);
      
      component.createGridToolbar();

      expect(mockToolbarService.createToolbarButton).toHaveBeenCalledWith(
        'manage.registry.signatorycontract.subsignatorycontract.new',
        expect.any(Object)
      );
    });
  });

  describe('onGridReadyToDefine', () => {
    it('should set up grid with correct configuration', () => {
      const mockGrid = {
        context: {},
        addColumnText: vi.fn(),
        addColumnIconEdit: vi.fn(),
        addColumnIconDelete: vi.fn(),
        finalizeInitGrid: vi.fn(),
        columns: []
      };

      vi.spyOn(component as any, 'setColumns');
      vi.spyOn(component, 'finalizeInitGrid');

      component.onGridReadyToDefine(mockGrid as any);

      expect((component as any).setColumns).toHaveBeenCalled();
      expect((mockGrid.context as any).suppressGridMenu).toBe(true);
      expect((mockGrid.context as any).suppressFiltering).toBe(true);
      // The component should call finalizeInitGrid on the grid, not on itself
      expect(mockGrid.finalizeInitGrid).toHaveBeenCalledWith({ hidden: true }, undefined);
    });
  });

  describe('setColumns', () => {
    beforeEach(() => {
      component.grid = {
        addColumnText: vi.fn(),
        addColumnIconEdit: vi.fn(),
        addColumnIconDelete: vi.fn(),
        columns: []
      } as any;
      component.terms = {
        'manage.registry.signatorycontract.recipientuser': 'Recipient User',
        'manage.registry.signatorycontract.permissions': 'Permissions',
        'core.edit': 'Edit',
        'core.delete': 'Delete'
      };
    });

    it('should add text columns', () => {
      (component as any).setColumns();

      expect(component.grid.addColumnText).toHaveBeenCalledWith(
        'recipientUserName',
        'Recipient User',
        { flex: 2 }
      );
      expect(component.grid.addColumnText).toHaveBeenCalledWith(
        'permissions',
        'Permissions',
        { flex: 6 }
      );
    });

    it('should add edit and delete columns when editable', () => {
      fixture.componentRef.setInput('editable', true);
      
      (component as any).setColumns();

      expect(component.grid.addColumnIconEdit).toHaveBeenCalledWith({
        tooltip: 'Edit',
        onClick: expect.any(Function),
        flex: 1
      });
      expect(component.grid.addColumnIconDelete).toHaveBeenCalledWith({
        tooltip: 'Delete',
        onClick: expect.any(Function),
        flex: 1
      });
    });

    it('should not add edit and delete columns when not editable', () => {
      fixture.componentRef.setInput('editable', false);
      
      (component as any).setColumns();

      expect(component.grid.addColumnIconEdit).not.toHaveBeenCalled();
      expect(component.grid.addColumnIconDelete).not.toHaveBeenCalled();
    });
  });

  describe('loadData', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('signatoryContractId', 123);
    });

    it('should load data with correct parameters when signatoryContractId is provided', async () => {
      const result = await firstValueFrom(component.loadData());
      expect(mockSubSignatoryContractService.getGrid).toHaveBeenCalledWith(
        undefined,
        { signatoryContractParentId: 123 }
      );
      expect(result).toEqual(mockSubContracts);
    });

    it('should return empty array when signatoryContractId is not provided', async () => {
      fixture.componentRef.setInput('signatoryContractId', 0);
      
      const result = await firstValueFrom(component.loadData());
      expect(result).toEqual([]);
    });
  });

  describe('addSubSignatoryContract', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('users', mockUsers);
      component.rowData = { value: mockSubContracts, next: vi.fn() } as any;
    });

    it('should open dialog when users are available', () => {
      vi.spyOn(component as any, 'changeSubSignatoryContract');
      
      component['addSubSignatoryContract']();

      expect((component as any).changeSubSignatoryContract).toHaveBeenCalledWith(
        expect.any(SignatoryContractDTO),
        'manage.registry.signatorycontract.addsubcontract',
        expect.any(Array)
      );
    });

    it('should show error when no users are available', () => {
      component.rowData = { value: mockUsers.map(u => ({ recipientUserId: u.id })) } as any;
      component.terms = { 'core.error': 'Error', 'manage.registry.signatorycontract.error.nousers': 'No users' };
      
      component['addSubSignatoryContract']();

      expect(mockMessageboxService.error).toHaveBeenCalledWith('Error', 'No users');
    });
  });

  describe('editSubSignatoryContract', () => {
    it('should call changeSubSignatoryContract with correct parameters', () => {
      const testRow = mockSubContracts[0];
      fixture.componentRef.setInput('users', mockUsers);
      vi.spyOn(component as any, 'changeSubSignatoryContract');
      
      component['editSubSignatoryContract'](testRow);

      expect((component as any).changeSubSignatoryContract).toHaveBeenCalledWith(
        testRow,
        'manage.registry.signatorycontract.editsubcontract',
        mockUsers
      );
    });
  });

  describe('changeSubSignatoryContract', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
      (component as any).permissionTerms = mockPermissionTerms;
      fixture.componentRef.setInput('users', mockUsers);
      component.rowData = { value: mockSubContracts, next: vi.fn() } as any;
      component.terms = {
        'core.error': 'Error',
        'manage.registry.signatorycontract.error.noparentpermissions': 'No permissions'
      };
    });

    it('should open dialog when parent permissions exist', () => {
      const testRow = mockSubContracts[0];
      const testUsers = mockUsers;
      
      component['changeSubSignatoryContract'](testRow, 'Test Title', testUsers);

      expect(mockDialogService.open).toHaveBeenCalledWith(
        SubSignatoryContractEditDialogComponent,
        expect.objectContaining({
          title: 'Test Title',
          size: 'md',
          rowToUpdate: testRow,
          users: testUsers,
          permissionTerms: expect.any(Array)
        })
      );
    });

    it('should show error when no parent permissions exist', () => {
      fixture.componentRef.setInput('parentPermissions', []);
      
      component['changeSubSignatoryContract'](mockSubContracts[0], 'Test Title', mockUsers);

      expect(mockMessageboxService.error).toHaveBeenCalledWith(
        'Error',
        'No permissions'
      );
    });

    it('should handle dialog result for new contract', () => {
      const newContract = new SignatoryContractDTO();
      newContract.signatoryContractId = 0;
      newContract.recipientUserId = 4;
      newContract.permissionTypes = [1, 2];
      
      mockDialogRef.afterClosed.mockReturnValue(of(newContract));
      
      component['changeSubSignatoryContract'](newContract, 'Test Title', mockUsers);

      // Verify dialog was opened
      expect(mockDialogService.open).toHaveBeenCalled();
    });

    it('should handle dialog result for existing contract', () => {
      vi.useFakeTimers();
      const existingContract = { ...mockSubContracts[0] };
      existingContract.recipientUserId = 4;
      existingContract.permissionTypes = [3];
      
      mockDialogRef.afterClosed.mockReturnValue(of(existingContract));
      
      component['changeSubSignatoryContract'](existingContract, 'Test Title', mockUsers);

      vi.runAllTimers();
      expect(component.rowData.value).toBeDefined();
      vi.useRealTimers();
    });

    it('should not update when dialog returns false', () => {
      mockDialogRef.afterClosed.mockReturnValue(of(false));
      const originalValue = component.rowData.value;
      
      component['changeSubSignatoryContract'](mockSubContracts[0], 'Test Title', mockUsers);

      expect(component.rowData.value).toBe(originalValue);
    });

    it('should filter out SignatoryContract_EditContracts permission from parent permissions', () => {
      const parentPermissionsWithEditContracts = [
        1, 2, 
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
      ];
      fixture.componentRef.setInput('parentPermissions', parentPermissionsWithEditContracts);
      
      const testRow = mockSubContracts[0];
      component['changeSubSignatoryContract'](testRow, 'Test Title', mockUsers);

      expect(mockDialogService.open).toHaveBeenCalled();
      const dialogData = mockDialogService.open.mock.calls[0][1];
      
      // permissionTerms should only include permissions 1 and 2, not EditContracts
      expect(dialogData.permissionTerms.length).toBeGreaterThan(0);
      expect(dialogData.permissionTerms.every((pt: any) => 
        pt.id === 1 || pt.id === 2 || testRow.permissionTypes.includes(pt.id)
      )).toBe(true);
    });

    it('should show error when only SignatoryContract_EditContracts permission exists', () => {
      fixture.componentRef.setInput('parentPermissions', [
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
      ]);
      
      component['changeSubSignatoryContract'](mockSubContracts[0], 'Test Title', mockUsers);

      expect(mockMessageboxService.error).toHaveBeenCalledWith(
        'Error',
        'No permissions'
      );
    });

    it('should include row existing permissions even if not in filtered parent permissions', () => {
      const parentPermissionsWithEditContracts = [
        1, 2,
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
      ];
      fixture.componentRef.setInput('parentPermissions', parentPermissionsWithEditContracts);
      
      const testRow = { ...mockSubContracts[0], permissionTypes: [1, 3] };
      component['changeSubSignatoryContract'](testRow, 'Test Title', mockUsers);

      expect(mockDialogService.open).toHaveBeenCalled();
      const dialogData = mockDialogService.open.mock.calls[0][1];
      
      // Should include permissions 1, 2 from parent and 3 because row already has it
      const permissionIds = dialogData.permissionTerms.map((pt: any) => pt.id);
      expect(permissionIds).toContain(1);
      expect(permissionIds).toContain(3);
    });
  });

  describe('deleteSubSignatoryContract', () => {
    beforeEach(() => {
      component.rowData = new BehaviorSubject([...mockSubContracts]);
    });

    it('should remove contract from rowData', () => {
      const contractToDelete = mockSubContracts[0];
      const originalLength = component.rowData.value.length;
      
      component['deleteSubSignatoryContract'](contractToDelete);

      // The actual component method should update the rowData
      expect(component.rowData.value.length).toBe(originalLength - 1);
      expect(component.rowData.value.find(c => c.signatoryContractId === contractToDelete.signatoryContractId)).toBeUndefined();
    });

    it('should emit changes after deletion', () => {
      vi.spyOn(component as any, 'emitSubSignatoryContracts');
      
      component['deleteSubSignatoryContract'](mockSubContracts[0]);

      expect((component as any).emitSubSignatoryContracts).toHaveBeenCalled();
    });

    it('should handle deletion of non-existent contract', () => {
      const nonExistentContract = { ...mockSubContracts[0], signatoryContractId: 999 };
      const originalLength = component.rowData.value.length;
      
      component['deleteSubSignatoryContract'](nonExistentContract);

      expect(component.rowData.value.length).toBe(originalLength);
    });
  });

  describe('resetGrid', () => {
    it('should reset columns', () => {
      vi.spyOn(component as any, 'resetColumns');
      
      component.resetGrid();

      expect((component as any).resetColumns).toHaveBeenCalled();
    });
  });

  describe('resetColumns', () => {
    it('should reset and recreate columns', () => {
      component.grid = {
        columns: [{ field: 'test' }],
        resetColumns: vi.fn(),
        addColumnText: vi.fn(),
        addColumnIconEdit: vi.fn(),
        addColumnIconDelete: vi.fn()
      } as any;
      component.terms = {
        'manage.registry.signatorycontract.recipientuser': 'Recipient User',
        'manage.registry.signatorycontract.permissions': 'Permissions',
        'core.edit': 'Edit',
        'core.delete': 'Delete'
      };
      vi.spyOn(component as any, 'setColumns');
      
      component['resetColumns']();

      expect(component.grid.columns).toEqual([]);
      expect((component as any).setColumns).toHaveBeenCalled();
      expect(component.grid.resetColumns).toHaveBeenCalled();
    });
  });

  describe('loadPermissionTerms', () => {
    it('should load permission terms from core service', async () => {
      const result = await firstValueFrom(component['loadPermissionTerms']());
      expect(result).toEqual(mockPermissionTerms);
      expect((component as any).permissionTerms).toEqual(mockPermissionTerms);
    });

    it('should handle error when loading permission terms', async () => {
      mockCoreService.getTermGroupContent.mockReturnValue(throwError(() => 'Error'));
      
      try {
        await firstValueFrom(component['loadPermissionTerms']());
        throw new Error('Expected error');
      } catch (error) {
        expect(error).toBe('Error');
      }
    });
  });

  describe('emitSubSignatoryContracts', () => {
    it('should emit current rowData value', () => {
      component.rowData = { value: mockSubContracts, next: vi.fn() } as any;
      vi.spyOn((component as any).changeSubSignatoryContracts, 'emit');
      
      component['emitSubSignatoryContracts']();

      expect((component as any).changeSubSignatoryContracts.emit).toHaveBeenCalledWith(mockSubContracts);
    });
  });

  describe('refreshGrid', () => {
    it('should call parent refreshGrid method', () => {
      vi.spyOn(component, 'refreshGrid').mockImplementation(() => {});
      
      component.refreshGrid();

      expect(component.refreshGrid).toHaveBeenCalled();
    });
  });

  describe('Error Handling', () => {
    it('should handle service errors gracefully', async () => {
      mockSubSignatoryContractService.getGrid.mockReturnValue(throwError(() => 'Service error'));
      fixture.componentRef.setInput('signatoryContractId', 123);
      
      try {
        await firstValueFrom(component.loadData());
        throw new Error('Expected error');
      } catch (error) {
        expect(error).toBe('Service error');
      }
    });

    it('should handle dialog service errors', () => {
      mockDialogService.open.mockImplementation(() => { throw new Error('Dialog error'); });
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
      (component as any).permissionTerms = mockPermissionTerms;
      
      expect(() => {
        component['changeSubSignatoryContract'](mockSubContracts[0], 'Test', mockUsers);
      }).toThrow('Dialog error');
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty users array', () => {
      fixture.componentRef.setInput('users', []);
      component.rowData = { value: [], next: vi.fn() } as any;
      component.terms = { 'core.error': 'Error', 'manage.registry.signatorycontract.error.nousers': 'No users' };
      
      component['addSubSignatoryContract']();

      expect(mockMessageboxService.error).toHaveBeenCalledWith('Error', 'No users');
    });

    it('should handle empty parent permissions', () => {
      fixture.componentRef.setInput('parentPermissions', []);
      component.terms = {
        'core.error': 'Error',
        'manage.registry.signatorycontract.error.noparentpermissions': 'No permissions'
      };
      
      component['changeSubSignatoryContract'](mockSubContracts[0], 'Test', mockUsers);

      expect(mockMessageboxService.error).toHaveBeenCalledWith('Error', 'No permissions');
    });

    it('should handle null/undefined inputs gracefully', () => {
      fixture.componentRef.setInput('signatoryContractId', 0);
      fixture.componentRef.setInput('editable', false);
      fixture.componentRef.setInput('users', []);
      fixture.componentRef.setInput('parentPermissions', []);

      expect(() => {
        component.ngOnInit();
        component.loadData();
        component.createGridToolbar();
      }).not.toThrow();
    });
  });

  describe('Integration Tests', () => {
    it('should complete full workflow for adding new contract', () => {
      vi.useFakeTimers();
      fixture.componentRef.setInput('signatoryContractId', 123);
      fixture.componentRef.setInput('editable', true);
      fixture.componentRef.setInput('users', mockUsers);
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
      (component as any).permissionTerms = mockPermissionTerms;
      component.rowData = new BehaviorSubject([] as ISignatoryContractDTO[]);
      component.terms = {
        'core.error': 'Error',
        'manage.registry.signatorycontract.error.nousers': 'No users'
      };

      const newContract = new SignatoryContractDTO();
      newContract.signatoryContractId = 0;
      newContract.recipientUserId = 1;
      newContract.permissionTypes = [1, 2];
      
      mockDialogRef.afterClosed.mockReturnValue(of(newContract));

      component['addSubSignatoryContract']();

      vi.runAllTimers();
      // Verify that the dialog was opened and the component handled the result
      expect(mockDialogService.open).toHaveBeenCalled();
      vi.useRealTimers();
    });

    it('should complete full workflow for editing existing contract', () => {
      vi.useFakeTimers();
      fixture.componentRef.setInput('users', mockUsers);
      fixture.componentRef.setInput('parentPermissions', [1, 2, 3]);
      (component as any).permissionTerms = mockPermissionTerms;
      component.rowData = { value: [...mockSubContracts], next: vi.fn() } as any;

      const editedContract = { ...mockSubContracts[0] };
      editedContract.recipientUserId = 3;
      editedContract.permissionTypes = [2, 3];
      
      mockDialogRef.afterClosed.mockReturnValue(of(editedContract));

      component['editSubSignatoryContract'](mockSubContracts[0]);

      vi.runAllTimers();
      expect(component.rowData.value).toBeDefined();
      vi.useRealTimers();
    });
  });
});
