import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { SignatoryContractComponent } from './signatory-contract.component';
import { SignatoryContractGridComponent } from '../signatory-contract-grid/signatory-contract-grid.component';
import { SignatoryContractEditComponent } from '../signatory-contract-edit/signatory-contract-edit.component';
import { SignatoryContractForm } from '../../models/signatory-contract-form.model';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SignatoryContractService } from '../../services/signatory-contract.service';
import { of } from 'rxjs';

describe('SignatoryContractComponent', () => {
  let component: SignatoryContractComponent;
  let fixture: ComponentFixture<SignatoryContractComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SignatoryContractComponent],
      imports: [MultiTabWrapperComponent, SoftOneTestBed],
             providers: [
               {
                 provide: SignatoryContractService,
                 useValue: {
                   get: vi.fn(),
                   post: vi.fn(),
                   put: vi.fn(),
                   delete: vi.fn(),
                   getGrid: vi.fn().mockReturnValue(of([]))
                 }
               }
             ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignatoryContractComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('Config Property', () => {
    it('should have correct MultiTabConfig structure with all required properties', () => {
      expect(component.config.length).toBe(1);
      
      const config = component.config[0];
      expect(config.gridComponent).toBe(SignatoryContractGridComponent);
      expect(config.editComponent).toBe(SignatoryContractEditComponent);
      expect(config.FormClass).toBe(SignatoryContractForm);
      expect(config.gridTabLabel).toBe('manage.registry.signatorycontract.signatorycontract');
      expect(config.editTabLabel).toBe('manage.registry.signatorycontract.signatorycontract');
      expect(config.createTabLabel).toBe('manage.registry.signatorycontract.new_signatorycontract');
      expect(config.exportFilenameKey).toBe('manage.registry.signatorycontract.signatorycontract');
    });
  });

  describe('addPermission Getter', () => {
    it('should return true when supportUserId exists', () => {
      vi.spyOn(SoeConfigUtil, 'supportUserId', 'get').mockReturnValue(123);
      
      fixture = TestBed.createComponent(SignatoryContractComponent);
      component = fixture.componentInstance;
      
      expect((component as any).addPermission).toBe(true);
    });

    it('should return false when supportUserId is falsy', () => {
      vi.spyOn(SoeConfigUtil, 'supportUserId', 'get').mockReturnValue(null as any);
      
      fixture = TestBed.createComponent(SignatoryContractComponent);
      component = fixture.componentInstance;
      
      expect((component as any).addPermission).toBe(false);
    });
  });

});
