import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToolbarComponent } from './toolbar.component';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';
import {
  ComponentRef,
  DebugElement,
  NO_ERRORS_SCHEMA,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { By } from '@angular/platform-browser';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCoffee } from '@fortawesome/pro-regular-svg-icons';
import { vi } from 'vitest';
import { ToolbarService } from './services/toolbar.service';

describe('ToolbarComponent', () => {
  let component: ToolbarComponent;
  let componentRef: ComponentRef<ToolbarComponent>;
  let fixture: ComponentFixture<ToolbarComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ToolbarComponent],
      providers: [ToolbarService],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    });
    library.add(faCoffee);
    fixture = TestBed.createComponent(ToolbarComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set up with default values', () => {
      expect(component.toolbarGroups()).toEqual([]);
      expect(component.noPadding()).toBe(false);
      expect(component.noMargin()).toBe(false);
      expect(component.noBorder()).toBe(false);
      expect(component.recordConfig()).toEqual(new NavigatorRecordConfig());
    });
  });
  describe('methods', () => {
    describe('recordChanged', () => {
      it('should emit navigatorRecordChanged event', () => {
        const record = { id: 1, name: 'test' };
        vi.spyOn(component.navigatorRecordChanged, 'emit');
        component.recordChanged(record);
        expect(component.navigatorRecordChanged.emit).toHaveBeenCalledWith(
          record
        );
      });
    });
  });
  describe('DOM', () => {
    let mockForm: any;
    let mockRecordConfig: any;
    beforeEach(() => {
      mockForm = {
        getIdFieldName: vi.fn().mockReturnValue('id'),
        value: { id: 1 },
        id: 'mockId',
        dirty: false,
      };

      mockRecordConfig = {
        hideRecordNavigator: false,
      };

      componentRef.setInput('recordConfig', mockRecordConfig);
      componentRef.setInput('form', mockForm);
      fixture.detectChanges();
    });
    describe('toolbar-container', () => {
      let toolbarDebugElement: DebugElement;
      let toolbarElement: HTMLDivElement;
      beforeEach(() => {
        toolbarDebugElement = fixture.debugElement.query(
          By.css('.toolbar-container')
        );
        toolbarElement = toolbarDebugElement.nativeElement;
      });
      it('should render', () => {
        expect(toolbarElement).toBeTruthy();
      });
      it('should not render when conditions are not met', () => {
        componentRef.setInput('form', undefined);
        component.hasLeftContent.set(false);
        component.hasRightContent.set(false);
        fixture.detectChanges();
        expect(
          fixture.debugElement.query(By.css('.toolbar-container'))
        ).toBeNull();
      });
      it('should render with correct classes', () => {
        expect(toolbarElement.classList).toContain('toolbar-container');
        expect(toolbarElement.classList).not.toContain('no-padding');
        expect(toolbarElement.classList).not.toContain('no-margin');
        expect(toolbarElement.classList).not.toContain('no-border');

        componentRef.setInput('noPadding', true);
        componentRef.setInput('noMargin', true);
        componentRef.setInput('noBorder', true);

        fixture.detectChanges();

        expect(toolbarElement.classList).toContain('toolbar-container');
        expect(toolbarElement.classList).toContain('no-padding');
        expect(toolbarElement.classList).toContain('no-margin');
        expect(toolbarElement.classList).toContain('no-border');
      });
    });
    describe('record-navigator', () => {
      let recordNavigatorDebugElement: DebugElement | null;
      let recordNavigatorElement: HTMLElement;
      let recordNavigatorInstance: any;
      beforeEach(() => {
        recordNavigatorDebugElement = fixture.debugElement.query(
          By.css('soe-record-navigator')
        );
        if (!recordNavigatorDebugElement) return;
        recordNavigatorElement = recordNavigatorDebugElement.nativeElement;
        recordNavigatorInstance = recordNavigatorDebugElement.componentInstance;
      });
      it('should render', () => {
        expect(recordNavigatorDebugElement).toBeTruthy();
        expect(recordNavigatorElement).toBeTruthy();
      });
      it('should render with correct inputs', () => {
        if (!recordNavigatorInstance) return;
        expect(recordNavigatorInstance.hideIfEmpty()).toBe(
          component.recordConfig().hideIfEmpty
        );
        expect(recordNavigatorInstance.hidePosition()).toBe(
          component.recordConfig().hidePosition
        );
        expect(recordNavigatorInstance.showRecordName()).toBe(
          component.recordConfig().showRecordName
        );
        expect(recordNavigatorInstance.hideDropdown()).toBe(
          component.recordConfig().hideDropdown
        );
        expect(recordNavigatorInstance.dropdownTextProperty()).toBe(
          component.recordConfig().dropdownTextProperty
        );
        expect(recordNavigatorInstance.isDate()).toBe(
          component.recordConfig().isDate
        );
        expect(recordNavigatorInstance.records()).toEqual(
          component.form()?.records || []
        );
        expect(recordNavigatorInstance.selectedId()).toBe(
          component.form()?.value[component.form()!.getIdFieldName()]
        );
        expect(recordNavigatorInstance.formDirty()).toBe(
          component.form()?.dirty
        );
      });
      it('should trigger recordChanged on recordChanges', () => {
        if (!recordNavigatorInstance) return;
        vi.spyOn(component, 'recordChanged');
        recordNavigatorInstance.recordChanged.emit({ id: 1, name: 'test' });
        expect(component.recordChanged).toHaveBeenCalledWith({
          id: 1,
          name: 'test',
        });
      });
    });
    describe('toolbar-groups', () => {
      beforeEach(() => {
        // Reset component to clean state without form
        componentRef.setInput('form', undefined);
        componentRef.setInput('recordConfig', { hideRecordNavigator: true });
        component.hasLeftContent.set(false);
        component.hasRightContent.set(false);
        fixture.detectChanges();
      });
      it('should render the correct number of toolbar groups and buttons', () => {
        const mockToolbarGroups = [
          {
            alignmentRight: false,
            buttons: [
              {
                label: 'Button 1',
                disabled: vi.fn().mockReturnValue(false),
                hidden: vi.fn().mockReturnValue(false),
                onClick: vi.fn(),
              },
              {
                label: 'Button 2',
                disabled: vi.fn().mockReturnValue(true),
                hidden: vi.fn().mockReturnValue(false),
                onClick: vi.fn(),
              },
            ],
          },
          {
            alignmentRight: true,
            buttons: [
              {
                label: 'Button 3',
                disabled: vi.fn().mockReturnValue(false),
                hidden: vi.fn().mockReturnValue(false),
                onClick: vi.fn(),
              },
            ],
          },
        ];

        componentRef.setInput('toolbarGroups', mockToolbarGroups);

        fixture.detectChanges();

        const buttonElements = fixture.debugElement.queryAll(By.css('button'));

        expect(buttonElements.length).toBe(3);
      });
      it('should apply correct alignment class based on alignmentRight property', () => {
        const mockToolbarGroups = [
          { alignmentRight: false, buttons: [] }, // Left-aligned group
          { alignmentRight: true, buttons: [] }, // Right-aligned group
        ];

        componentRef.setInput('toolbarGroups', mockToolbarGroups);

        fixture.detectChanges();

        const toolbarGroupElements = fixture.debugElement.queryAll(
          By.css('.btn-group')
        );

        expect(toolbarGroupElements.length).toBe(2);
        expect(toolbarGroupElements[0].nativeElement.className).toContain(
          'float-start'
        );
        expect(toolbarGroupElements[1].nativeElement.className).toContain(
          'float-end'
        );
      });
      it('should correctly bind button disabled and hidden states', () => {
        const mockButtons = [
          {
            label: 'Button 1',
            disabled: vi.fn().mockReturnValue(false),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
          {
            label: 'Button 2',
            disabled: vi.fn().mockReturnValue(true),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
          {
            label: 'Button 3',
            disabled: vi.fn().mockReturnValue(false),
            hidden: vi.fn().mockReturnValue(true),
            onClick: vi.fn(),
          },
        ];

        componentRef.setInput('toolbarGroups', [
          { alignmentRight: false, buttons: mockButtons },
        ]);

        fixture.detectChanges();

        const buttonElements = fixture.debugElement.queryAll(By.css('button'));

        expect(
          buttonElements[0].nativeElement.classList.contains('is-disabled')
        ).toBe(false); // Button 1 not disabled
        expect(
          buttonElements[1].nativeElement.classList.contains('is-disabled')
        ).toBe(true); // Button 2 disabled

        expect(buttonElements[2].nativeElement.hidden).toBe(true); // Button 3 should be hidden
      });
      it('should call button onClick handler when clicked and not disabled', () => {
        const mockButtons = [
          {
            label: 'Button 1',
            disabled: vi.fn().mockReturnValue(false),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
          {
            label: 'Button 2',
            disabled: vi.fn().mockReturnValue(true),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
        ];

        componentRef.setInput('toolbarGroups', [
          { alignmentRight: false, buttons: mockButtons },
        ]);

        fixture.detectChanges();

        const buttonElements = fixture.debugElement.queryAll(By.css('button'));

        // Simulate a click on Button 1 (which is not disabled)
        buttonElements[0].nativeElement.click();

        // Ensure onClick handler for Button 1 was called
        expect(mockButtons[0].onClick).toHaveBeenCalled();

        // Simulate a click on Button 2 (which is disabled)
        buttonElements[1].nativeElement.click();

        // Ensure onClick handler for Button 2 was not called
        expect(mockButtons[1].onClick).not.toHaveBeenCalled();
      });
      it('should conditionally render icon and label', () => {
        const mockButtons = [
          {
            label: 'Button 1',
            icon: 'coffee',
            disabled: vi.fn().mockReturnValue(false),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
          {
            label: 'Button 2',
            icon: null,
            disabled: vi.fn().mockReturnValue(false),
            hidden: vi.fn().mockReturnValue(false),
            onClick: vi.fn(),
          },
        ];

        componentRef.setInput('toolbarGroups', [
          { alignmentRight: false, buttons: mockButtons },
        ]);

        fixture.detectChanges();

        const buttonElements = fixture.debugElement.queryAll(By.css('button'));

        const firstButtonIcon = buttonElements[0].query(By.css('fa-icon'));
        const secondButtonIcon = buttonElements[1].query(By.css('fa-icon'));

        expect(firstButtonIcon).toBeTruthy();
        expect(secondButtonIcon).toBeNull();

        expect(buttonElements[0].nativeElement.textContent).toContain(
          'Button 1'
        );
        expect(buttonElements[1].nativeElement.textContent).toContain(
          'Button 2'
        );
      });
    });
  });
});
