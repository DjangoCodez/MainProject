import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { AddInvoiceToAttestFlowDialogComponent } from './add-invoice-to-attest-flow-dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TranslateService } from '@ngx-translate/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { AddInvoiceToAttestFlowDialogData } from '../../models/add-invoice-to-attest-flow-dialog-data.model';
import { SupplierService } from '@features/economy/services/supplier.service';
import { CoreService } from '@shared/services/core.service';
import { AttestationGroupsService } from '@features/economy/attestation-groups/services/attestation-groups.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { of, throwError } from 'rxjs';
import {
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowTemplateHeadDTO,
  IAttestWorkFlowTemplateRowDTO,
  IAttestWorkFlowRowDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  CompanySettingType,
  TermGroup_AttestWorkFlowRowProcessType,
} from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

describe('AddInvoiceToAttestFlowDialogComponent', () => {
  let component: AddInvoiceToAttestFlowDialogComponent;
  let fixture: ComponentFixture<AddInvoiceToAttestFlowDialogComponent>;
  let mockDialogRef: any;
  let mockSupplierService: any;
  let mockCoreService: any;
  let mockAttestationService: any;
  let mockMessageboxService: any;
  let mockProgressService: any;
  let mockTranslateService: any;
  let mockValidationHandler: any;
  let dialogData: AddInvoiceToAttestFlowDialogData;

  const mockAttestGroups: ISmallGenericType[] = [
    { id: 1, name: 'Group 1' },
    { id: 2, name: 'Group 2' },
  ];

  const mockTemplates: IAttestWorkFlowTemplateHeadDTO[] = [
    {
      attestWorkFlowTemplateHeadId: 10,
      name: 'Template 1',
    } as IAttestWorkFlowTemplateHeadDTO,
    {
      attestWorkFlowTemplateHeadId: 20,
      name: 'Template 2',
    } as IAttestWorkFlowTemplateHeadDTO,
  ];

  const mockGroupTypes: ISmallGenericType[] = [
    { id: 1, name: 'Type 1' },
    { id: 2, name: 'Type 2' },
  ];

  const mockRoleUser: ISmallGenericType[] = [
    { id: 0, name: 'Users' },
    { id: 1, name: 'Roles' },
  ];

  const mockUser: IUserSmallDTO = {
    userId: 999,
    name: 'Required User',
    loginName: 'requser',
  } as IUserSmallDTO;

  const mockAttestHead: Partial<IAttestWorkFlowHeadDTO> = {
    attestWorkFlowHeadId: 1,
    attestWorkFlowGroupId: 1,
    attestWorkFlowTemplateHeadId: 10,
    type: 1,
    sendMessage: true,
    rows: [],
  };

  const mockTemplateRows: Partial<IAttestWorkFlowTemplateRowDTO>[] = [
    {
      attestWorkFlowTemplateRowId: 1,
      attestTransitionId: 100,
      attestTransitionName: 'Approve',
      type: 1,
    },
    {
      attestWorkFlowTemplateRowId: 2,
      attestTransitionId: 200,
      attestTransitionName: 'Review',
      type: undefined,
    },
  ];

  beforeEach(async () => {
    dialogData = {
      title: 'Add to Attest Flow',
      size: 'lg',
      supplierInvoices: [
        { invoiceId: 1, totalAmount: 5000 },
        { invoiceId: 2, totalAmount: 3000 },
      ],
    };

    mockDialogRef = {
      close: vi.fn(),
      addPanelClass: vi.fn(),
      removePanelClass: vi.fn(),
      updateSize: vi.fn(),
      updatePosition: vi.fn(),
    };

    mockSupplierService = {
      getAttestWorkFlowGroupsDict: vi
        .fn()
        .mockReturnValue(of(mockAttestGroups)),
      getAttestWorkFlowTemplateHeadsForCurrentCompany: vi
        .fn()
        .mockReturnValue(of(mockTemplates)),
      getAttestWorkFlowHead: vi
        .fn()
        .mockReturnValue(of(mockAttestHead as IAttestWorkFlowHeadDTO)),
      getAttestWorkFlowTemplateHeadRows: vi
        .fn()
        .mockReturnValue(
          of(mockTemplateRows as IAttestWorkFlowTemplateRowDTO[])
        ),
      getAttestWorkFlowHeadFromInvoiceIds: vi.fn().mockReturnValue(of([])),
    };

    mockCoreService = {
      getTermGroupContent: vi.fn((termGroup: number) => {
        if (termGroup === 1) return of(mockGroupTypes);
        if (termGroup === 2) return of(mockRoleUser);
        return of([]);
      }),
      getCompanySettings: vi.fn().mockReturnValue(
        of({
          [CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired]: 999,
          [CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired]: 10000,
          [CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup]: 1,
        })
      ),
      getUser: vi.fn().mockReturnValue(of(mockUser)),
    };

    mockAttestationService = {
      saveAttestWorkFlowMultiple: vi.fn().mockReturnValue(
        of({
          success: true,
          integerValue: 2,
        } as BackendResponse)
      ),
    };

    mockMessageboxService = {
      question: vi.fn().mockReturnValue({
        afterClosed: vi.fn().mockReturnValue(of({ result: true })),
      }),
      error: vi.fn().mockReturnValue({
        afterClosed: vi.fn().mockReturnValue(of({})),
      }),
      success: vi.fn().mockReturnValue({
        afterClosed: vi.fn().mockReturnValue(of({})),
      }),
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
      resetLoadCounter: vi.fn(),
    };

    mockTranslateService = {
      instant: vi.fn((key: string) => {
        const translations: { [key: string]: string } = {
          'economy.supplier.invoice.addtoattestflow': 'Add to Attest Flow',
          'economy.supplier.attestgroup.invoicerequiresspecificuser':
            'Invoice requires user {0} for amounts over {1}',
          'core.verifyquestion': 'Verify',
          'economy.supplier.invoice.existingattestflowmessage':
            'Existing attest flows found',
          'core.error': 'Error',
          'economy.supplier.invoice.attesthasinvalidrows':
            'Attest flow has invalid rows',
        };
        return translations[key] || key;
      }),
    };

    mockValidationHandler = {
      handle: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, AddInvoiceToAttestFlowDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        { provide: SupplierService, useValue: mockSupplierService },
        { provide: CoreService, useValue: mockCoreService },
        {
          provide: AttestationGroupsService,
          useValue: mockAttestationService,
        },
        { provide: MessageboxService, useValue: mockMessageboxService },
        { provide: ProgressService, useValue: mockProgressService },
        { provide: TranslateService, useValue: mockTranslateService },
        { provide: ValidationHandler, useValue: mockValidationHandler },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddInvoiceToAttestFlowDialogComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with correct controls', () => {
      expect(component['form']).toBeTruthy();
      expect(component['form'].controls.attestWorkFlowHeadId).toBeTruthy();
      expect(
        component['form'].controls.attestWorkFlowTemplateHeadId
      ).toBeTruthy();
      expect(component['form'].controls.roleOrUser).toBeTruthy();
      expect(component['form'].controls.numberOfInvoicesText).toBeTruthy();
      expect(component['form'].controls.adminText).toBeTruthy();
      expect(component['form'].controls.sendMessage).toBeTruthy();
    });
  });

  describe('ngOnInit', () => {
    it('should set supplier invoice IDs from dialog data', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(component['supplierInvoiceIds']).toEqual([1, 2]);
    });

    it('should calculate highest amount from invoices', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(component['highestAmount']).toBe(5000);
    });

    it('should set numberOfInvoicesText to invoice count', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(component['form'].numberOfInvoicesText.value).toBe('2');
    });

    it('should call loadData on init', async () => {
      const loadDataSpy = vi.spyOn(component as any, 'loadData');
      component.ngOnInit();

      expect(loadDataSpy).toHaveBeenCalled();
    });
  });

  describe('loadData', () => {
    it('should load all lookups in parallel', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(
        mockSupplierService.getAttestWorkFlowGroupsDict
      ).toHaveBeenCalledWith(true);
      expect(
        mockSupplierService.getAttestWorkFlowTemplateHeadsForCurrentCompany
      ).toHaveBeenCalled();
      expect(mockCoreService.getTermGroupContent).toHaveBeenCalled();
      expect(mockCoreService.getCompanySettings).toHaveBeenCalled();
    });

    it('should populate attestGroups signal', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['attestGroups']()).toEqual(mockAttestGroups);
    });

    it('should populate templates signal', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['templates']()).toEqual(mockTemplates);
    });

    it('should set isLoaded to true after loading', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['isLoaded']()).toBe(true);
    });

    it('should set default attest group if configured', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['form'].attestWorkFlowHeadId.value).toBe(1);
    });

    it('should load required user when requiredUserId is set', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockCoreService.getUser).toHaveBeenCalledWith(999);
      expect(component['requiredUser']).toEqual(mockUser);
    });
  });

  describe('loadCompanySettings', () => {
    it('should load and set company settings', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['requiredUserId']).toBe(999);
      expect(component['totalAmountWhenUserRequired']).toBe(10000);
      expect(component['defaultAttestGroupId']).toBe(1);
    });

    it('should handle missing company settings gracefully', async () => {
      mockCoreService.getCompanySettings.mockReturnValue(of({}));

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['requiredUserId']).toBe(0);
      expect(component['totalAmountWhenUserRequired']).toBe(0);
    });
  });

  describe('setRequiredUserMessage', () => {
    it('should show warning when amount exceeds threshold', async () => {
      component['requiredUserId'] = 999;
      component['totalAmountWhenUserRequired'] = 4000;
      component['highestAmount'] = 5000;
      component['requiredUser'] = mockUser;

      component['setRequiredUserMessage']();

      expect(component['showUserRequiredWarning']()).toBe(true);
    });

    it('should not show warning when amount below threshold', async () => {
      component['requiredUserId'] = 999;
      component['totalAmountWhenUserRequired'] = 6000;
      component['highestAmount'] = 5000;
      component['requiredUser'] = mockUser;

      component['setRequiredUserMessage']();

      expect(component['showUserRequiredWarning']()).toBe(false);
    });

    it('should not show warning when requiredUserId is 0', async () => {
      component['requiredUserId'] = 0;
      component['totalAmountWhenUserRequired'] = 4000;
      component['highestAmount'] = 5000;

      component['setRequiredUserMessage']();

      expect(component['showUserRequiredWarning']()).toBe(false);
    });
  });

  describe('groupChanged', () => {
    it('should load attest work flow head', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockSupplierService.getAttestWorkFlowHead).toHaveBeenCalledWith(
        1,
        false,
        true
      );
      expect(component['attestWorkFlowHead']().attestWorkFlowGroupId).toBe(1);
    });

    it('should update sendMessage form control from head', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['form'].sendMessage.value).toBe(true);
    });

    it('should set template head ID if present', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['form'].attestWorkFlowTemplateHeadId.value).toBe(10);
    });

    it('should handle head without sendMessage', async () => {
      const headWithoutSendMessage: Partial<IAttestWorkFlowHeadDTO> = {
        ...mockAttestHead,
        sendMessage: undefined,
      };
      mockSupplierService.getAttestWorkFlowHead.mockReturnValue(
        of(headWithoutSendMessage as IAttestWorkFlowHeadDTO)
      );

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['form'].sendMessage.value).toBe(false);
    });
  });

  describe('loadCompanyTemplateRows', () => {
    it('should load template rows', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowTemplateHeadId.setValue(10);
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(
        mockSupplierService.getAttestWorkFlowTemplateHeadRows
      ).toHaveBeenCalledWith(10);
      expect(component['templateRows']().length).toBe(2);
    });

    it('should merge row types from existing head rows', async () => {
      const headWithRows: Partial<IAttestWorkFlowHeadDTO> = {
        ...mockAttestHead,
        rows: [
          {
            attestTransitionId: 100,
            type: 1,
          } as Partial<IAttestWorkFlowRowDTO> as IAttestWorkFlowRowDTO,
        ],
      };
      mockSupplierService.getAttestWorkFlowHead.mockReturnValue(
        of(headWithRows as IAttestWorkFlowHeadDTO)
      );

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowTemplateHeadId.setValue(10);
      await new Promise(resolve => setTimeout(resolve, 50));

      const rows = component['templateRows']();
      const matchedRow = rows.find(r => r.attestTransitionId === 100);
      expect(matchedRow?.type).toBe(1);
    });

    it('should use head type when row type is null', async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowTemplateHeadId.setValue(10);
      await new Promise(resolve => setTimeout(resolve, 50));

      const rows = component['templateRows']();
      const rowWithNullType = rows.find(r => r.attestTransitionId === 200);
      expect(rowWithNullType?.type).toBe(1);
    });
  });

  describe('buttonOkClick', () => {
    beforeEach(async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));
    });

    it('should check for existing attest flows', async () => {
      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(
        mockSupplierService.getAttestWorkFlowHeadFromInvoiceIds
      ).toHaveBeenCalledWith([1, 2]);
    });

    it('should show confirmation dialog when existing flows found', async () => {
      const existingFlows = [
        { attestWorkFlowHeadId: 100 } as IAttestWorkFlowHeadDTO,
        null,
      ];
      mockSupplierService.getAttestWorkFlowHeadFromInvoiceIds.mockReturnValue(
        of(existingFlows)
      );

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockMessageboxService.question).toHaveBeenCalled();
    });

    it('should proceed to save when no existing flows found', async () => {
      const saveAttestFlowSpy = vi.spyOn(component as any, 'saveAttestFlow');
      mockSupplierService.getAttestWorkFlowHeadFromInvoiceIds.mockReturnValue(
        of([])
      );

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(saveAttestFlowSpy).toHaveBeenCalled();
    });

    it('should reset buttonOKClicked when user cancels confirmation', async () => {
      const existingFlows = [
        { attestWorkFlowHeadId: 100 } as IAttestWorkFlowHeadDTO,
      ];
      mockSupplierService.getAttestWorkFlowHeadFromInvoiceIds.mockReturnValue(
        of(existingFlows)
      );
      mockMessageboxService.question.mockReturnValue({
        afterClosed: vi.fn().mockReturnValue(of({ result: false })),
      });

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['buttonOKClicked']()).toBe(false);
    });
  });

  describe('saveAttestFlow', () => {
    beforeEach(async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));
      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));
    });

    it('should validate that rows are not empty', () => {
      // Mock empty user selectors
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();

      expect(mockMessageboxService.error).toHaveBeenCalled();
      expect(component['buttonOKClicked']()).toBe(false);
    });

    it('should validate required user is selected when threshold exceeded', () => {
      component['requiredUserId'] = 999;
      component['totalAmountWhenUserRequired'] = 4000;
      component['highestAmount'] = 5000;

      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [
            {
              userId: 123,
              attestTransitionId: 100,
            },
          ],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();

      expect(component['userRequiredMessageType']()).toBe('error');
      expect(component['buttonOKClicked']()).toBe(false);
    });

    it('should proceed to save when required user is selected', async () => {
      component['requiredUserId'] = 999;
      component['totalAmountWhenUserRequired'] = 4000;
      component['highestAmount'] = 5000;

      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [
            {
              userId: 999,
              attestTransitionId: 100,
            },
          ],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(
        mockAttestationService.saveAttestWorkFlowMultiple
      ).toHaveBeenCalled();
    });

    it('should add registration row with current user', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [
            {
              userId: 123,
              attestTransitionId: 100,
            },
          ],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      const callArgs =
        mockAttestationService.saveAttestWorkFlowMultiple.mock.calls[0];
      const savedHead = callArgs[0];
      const regRow = savedHead.rows.find(
        (r: IAttestWorkFlowRowDTO) =>
          r.processType === TermGroup_AttestWorkFlowRowProcessType.Registered
      );

      expect(regRow).toBeTruthy();
      expect(regRow.answer).toBe(true);
    });

    it('should set correct processType for first level', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [
            {
              userId: 123,
              attestTransitionId: 100,
            },
          ],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      const callArgs =
        mockAttestationService.saveAttestWorkFlowMultiple.mock.calls[0];
      const savedHead = callArgs[0];
      const userRow = savedHead.rows.find(
        (r: any) =>
          r.processType ===
          TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess
      );

      expect(userRow).toBeTruthy();
    });

    it('should set LevelNotReached for subsequent levels', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [{ userId: 123, attestTransitionId: 100 }],
          getAttestTransitionId: () => 100,
        },
        {
          getRowsToSave: () => [{ userId: 456, attestTransitionId: 200 }],
          getAttestTransitionId: () => 200,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      const callArgs =
        mockAttestationService.saveAttestWorkFlowMultiple.mock.calls[0];
      const savedHead = callArgs[0];
      const levelNotReachedRows = savedHead.rows.filter(
        (r: any) =>
          r.processType ===
          TermGroup_AttestWorkFlowRowProcessType.LevelNotReached
      );

      expect(levelNotReachedRows.length).toBeGreaterThan(0);
    });

    it('should close dialog with success result', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [{ userId: 123, attestTransitionId: 100 }],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockDialogRef.close).toHaveBeenCalledWith({
        success: true,
        affectedInvoiceCount: 2,
      });
    });

    it('should close dialog with failure result when save fails', async () => {
      mockAttestationService.saveAttestWorkFlowMultiple.mockReturnValue(
        of({
          success: false,
        } as BackendResponse)
      );

      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [{ userId: 123, attestTransitionId: 100 }],
          getAttestTransitionId: () => 100,
        },
      ]);

      component['saveAttestFlow']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockDialogRef.close).toHaveBeenCalledWith({ success: false });
    });
  });

  describe('Form Value Changes', () => {
    beforeEach(async () => {
      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));
    });

    it('should call groupChanged when attestWorkFlowHeadId changes', async () => {
      const groupChangedSpy = vi.spyOn(component as any, 'groupChanged');

      component['form'].attestWorkFlowHeadId.setValue(2);
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(groupChangedSpy).toHaveBeenCalledWith(2);
    });

    it('should call loadCompanyTemplateRows when attestWorkFlowTemplateHeadId changes', async () => {
      const templateChangedSpy = vi.spyOn(
        component as any,
        'loadCompanyTemplateRows'
      );

      component['form'].attestWorkFlowTemplateHeadId.setValue(20);
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(templateChangedSpy).toHaveBeenCalledWith(20);
    });

    it('should ignore non-number values in attestWorkFlowHeadId changes', async () => {
      const groupChangedSpy = vi.spyOn(component as any, 'groupChanged');

      component['form'].attestWorkFlowHeadId.setValue('' as any);
      await new Promise(resolve => setTimeout(resolve, 10));

      expect(groupChangedSpy).not.toHaveBeenCalled();
    });
  });

  describe('Integration Tests - Complete Flows', () => {
    it('should handle complete flow: load → select group → select template → save', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [{ userId: 123, attestTransitionId: 100 }],
          getAttestTransitionId: () => 100,
        },
      ]);

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(component['isLoaded']()).toBe(true);

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowTemplateHeadId.setValue(10);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockDialogRef.close).toHaveBeenCalledWith({
        success: true,
        affectedInvoiceCount: 2,
      });
    });

    it('should handle complete flow: with existing flows requiring confirmation', async () => {
      const existingFlows = [
        { attestWorkFlowHeadId: 100 } as IAttestWorkFlowHeadDTO,
      ];
      mockSupplierService.getAttestWorkFlowHeadFromInvoiceIds.mockReturnValue(
        of(existingFlows)
      );
      mockMessageboxService.question.mockReturnValue({
        afterClosed: vi.fn().mockReturnValue(of({ result: true })),
      });

      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [{ userId: 123, attestTransitionId: 100 }],
          getAttestTransitionId: () => 100,
        },
      ]);

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 100));

      expect(mockMessageboxService.question).toHaveBeenCalled();
      expect(mockDialogRef.close).toHaveBeenCalledWith({
        success: true,
        affectedInvoiceCount: 2,
      });
    });

    it('should handle validation failure: no users selected', async () => {
      vi.spyOn(component, 'userSelectors' as any).mockReturnValue([
        {
          getRowsToSave: () => [],
          getAttestTransitionId: () => 100,
        },
      ]);

      component.ngOnInit();
      await new Promise(resolve => setTimeout(resolve, 50));

      component['form'].attestWorkFlowHeadId.setValue(1);
      await new Promise(resolve => setTimeout(resolve, 50));

      component['buttonOkClick']();
      await new Promise(resolve => setTimeout(resolve, 50));

      expect(mockMessageboxService.error).toHaveBeenCalled();
      expect(mockDialogRef.close).not.toHaveBeenCalled();
    });
  });
});
