import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { SignatoryContractPermissionEditGridComponent } from './signatory-contract-permission-edit-grid.component';
import { SignatoryContractPermissionsService } from '../../services/signatory-contract-permissions.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('SignatoryContractPermissionEditGridComponent', () => {
  let component: SignatoryContractPermissionEditGridComponent;
  let fixture: ComponentFixture<SignatoryContractPermissionEditGridComponent>;
  let mockSignatoryContractPermissionsService: any;

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
    const signatoryContractPermissionsServiceSpy = {
      getGrid: vi.fn().mockReturnValue(of(mockPermissionItems))
    };
    const flowHandlerServiceSpy = {
      startFlow: vi.fn()
    };
    const toolbarServiceSpy = {
      createItemGroup: vi.fn(),
      createToolbarButton: vi.fn()
    };

    await TestBed.configureTestingModule({
      declarations: [SignatoryContractPermissionEditGridComponent],
      imports: [SoftOneTestBed],
      providers: [
        { provide: SignatoryContractPermissionsService, useValue: signatoryContractPermissionsServiceSpy },
        { provide: FlowHandlerService, useValue: flowHandlerServiceSpy },
        { provide: ToolbarService, useValue: toolbarServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignatoryContractPermissionEditGridComponent);
    component = fixture.componentInstance;
    
    mockSignatoryContractPermissionsService = TestBed.inject(SignatoryContractPermissionsService) as any;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have correct grid name', () => {
      expect(component.gridName).toBe('Manage.Registry.SignatoryContract.PermissionEdit');
    });
  });

  describe('onGridReadyToDefine', () => {
    it('should suppress grid menu and filtering', () => {
      const mockGrid = {
        context: {},
        addColumnBool: vi.fn(),
        addColumnText: vi.fn(),
        finalizeInitGrid: vi.fn(),
        columns: []
      };

      component.terms = {
        'manage.registry.signatorycontract.permission': 'Permission'
      };

      vi.spyOn(component as any, 'setColumns').mockImplementation(() => {});
      (component as any).editable = vi.fn().mockReturnValue(true) as any;
      component.grid = mockGrid as any;

      component.onGridReadyToDefine(mockGrid as any);

      expect((mockGrid.context as any).suppressGridMenu).toBe(true);
      expect((mockGrid.context as any).suppressFiltering).toBe(true);
    });
  });

  describe('setColumns', () => {
    it('should add boolean and text columns with correct configuration', () => {
      component.grid = {
        addColumnBool: vi.fn(),
        addColumnText: vi.fn()
      } as any;
      component.terms = {
        'manage.registry.signatorycontract.permission': 'Permission'
      };
      (component as any).editable = vi.fn().mockReturnValue(true) as any;

      (component as any).setColumns();

      expect(component.grid.addColumnBool).toHaveBeenCalledWith(
        'isSelected',
        '',
        {
          width: 40,
          editable: true,
          columnSeparator: true,
          onClick: expect.any(Function)
        }
      );
      expect(component.grid.addColumnText).toHaveBeenCalledWith('name', 'Permission');
    });

    it('should respect editable input', () => {
      component.grid = {
        addColumnBool: vi.fn(),
        addColumnText: vi.fn()
      } as any;
      component.terms = { 'manage.registry.signatorycontract.permission': 'Permission' };
      (component as any).editable = vi.fn().mockReturnValue(false) as any;
      
      (component as any).setColumns();

      expect(component.grid.addColumnBool).toHaveBeenCalledWith(
        'isSelected',
        '',
        expect.objectContaining({ editable: false })
      );
    });
  });

  describe('loadData', () => {
    it('should load data with signatory contract ID', async () => {
      (component as any).signatoryContractId = vi.fn().mockReturnValue(123) as any;
      
      const result = await firstValueFrom(component.loadData());
      
      expect(mockSignatoryContractPermissionsService.getGrid).toHaveBeenCalledWith(123, undefined);
      expect(result).toEqual(mockPermissionItems);
    });
  });

  describe('toggleSelected', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should emit selected permission IDs', () => {
      component.rowData = { value: mockPermissionItems } as any;
      vi.spyOn(component.permissionChanged, 'emit');
      
      component['toggleSelected'](true, mockPermissionItems[0]);
      vi.advanceTimersByTime(1);

      expect(component.permissionChanged.emit).toHaveBeenCalledWith([1, 3]);
    });

    it('should emit empty array when no permissions are selected', () => {
      const noSelectedItems = [
        { id: 1, name: 'Permission 1', isSelected: false },
        { id: 2, name: 'Permission 2', isSelected: false },
        { id: 3, name: 'Permission 3', isSelected: false }
      ];
      component.rowData = { value: noSelectedItems } as any;
      vi.spyOn(component.permissionChanged, 'emit');
      
      component['toggleSelected'](false, noSelectedItems[0]);
      vi.advanceTimersByTime(1);

      expect(component.permissionChanged.emit).toHaveBeenCalledWith([]);
    });
  });

  describe('resetGrid', () => {
    it('should clear and recreate columns', () => {
      component.grid = {
        columns: [{ field: 'test' }],
        resetColumns: vi.fn(),
        addColumnBool: vi.fn(),
        addColumnText: vi.fn()
      } as any;
      component.terms = {
        'manage.registry.signatorycontract.permission': 'Permission'
      };
      (component as any).editable = vi.fn().mockReturnValue(true) as any;
      
      component.resetGrid();

      expect(component.grid.columns).toEqual([]);
      expect(component.grid.resetColumns).toHaveBeenCalled();
    });
  });
});
