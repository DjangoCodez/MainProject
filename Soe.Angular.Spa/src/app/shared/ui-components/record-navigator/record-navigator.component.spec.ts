import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { RecordNavigatorComponent } from './record-navigator.component';
import { of } from 'rxjs';
import { By } from '@angular/platform-browser';
import { ComponentRef, DebugElement } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

describe('RecordNavigatorComponent', () => {
  let component: RecordNavigatorComponent;
  let componentRef: ComponentRef<RecordNavigatorComponent>;
  let fixture: ComponentFixture<RecordNavigatorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, RecordNavigatorComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(RecordNavigatorComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set up with default values', () => {
      expect(component.records()).toEqual([]);
      expect(component.selectedId()).toEqual(0);
      expect(component.hideIfEmpty()).toBe(false);
      expect(component.hidePosition()).toBe(false);
      expect(component.showRecordName()).toBe(false);
      expect(component.hideDropdown()).toBe(false);
      expect(component.dropdownTextProperty()).toEqual('name');
      expect(component.isDate()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('ngOnInit', () => {
      it('should set selectedRecord and index according to value of selectedId when isDate is false', () => {
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        componentRef.setInput('selectedId', 2);
        componentRef.setInput('isDate', false);
        component.ngOnInit();
        expect(component.selectedRecord).toEqual({ id: 2 });
        expect(component.index).toEqual(1);
      });
      it('should set selectedRecord and index according to index of selectedId when isDate is true', () => {
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        componentRef.setInput('selectedId', 2);
        componentRef.setInput('isDate', true);
        component.ngOnInit();
        expect(component.selectedRecord).toEqual({ id: 3 });
        expect(component.index).toEqual(2);
      });
    });
    describe('setIndex', () => {
      it('should set index to the index of selectedRecord in records and emit recordChanged event', () => {
        vi.spyOn(component.recordChanged, 'emit');
        component.index = 0;
        const records = [{ id: 1 }, { id: 2 }, { id: 3 }];
        componentRef.setInput('records', records);
        component.selectedRecord = records[1];
        component['setIndex']();
        expect(component.index).toEqual(1);
        expect(component.recordChanged.emit).toHaveBeenCalledWith({ id: 2 });
      });
    });
    describe('moveFirst', () => {
      it('should call selectRecord with first record if index is greater than 0', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 0;
        component.moveFirst();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
    });
    describe('movePrev', () => {
      it('should call selectRecord with previous index if index is greater than 0', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 2;
        component.movePrev();
        expect(component.selectRecord).toHaveBeenCalledWith({ id: 2 });
      });
      it('should not call selectRecord if index is 0', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 0;
        component.movePrev();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
    });
    describe('moveNext', () => {
      it('should call selectRecord with next index if index is less than records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 0;
        component.moveNext();
        expect(component.selectRecord).toHaveBeenCalledWith({ id: 2 });
      });
      it('should not call selectRecord if index is equal to records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 2;
        component.moveNext();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
      it('should not call selectRecord if index is greater than records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 4;
        component.moveNext();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
    });
    describe('moveLast', () => {
      it('should call selectRecord with last record if index is less than records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 0;
        component.moveLast();
        expect(component.selectRecord).toHaveBeenCalledWith({ id: 3 });
      });
      it('should not call selectRecord if index is equal to records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 2;
        component.moveLast();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
      it('should not call selectRecord if index is greater than records length', () => {
        vi.spyOn(component, 'selectRecord');
        componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
        component.index = 4;
        component.moveLast();
        expect(component.selectRecord).not.toHaveBeenCalled();
      });
    });
    describe('selectRecord', () => {
      it('should call validateMove and set selectedRecord and index if response is true', () => {
        vi.spyOn(component, 'validateMove' as any).mockReturnValue({
          subscribe: vi.fn().mockImplementation(cb => cb(true)),
        });
        vi.spyOn(component, 'setIndex' as any);
        component.selectRecord({
          id: 2,
          name: '',
        });
        expect(component.selectedRecord).toEqual({
          id: 2,
          name: '',
        });
        expect(component['setIndex']).toHaveBeenCalled();
      });
    });
    describe('validateMove', () => {
      it('should return an observable that emits true if formDirty is false', () => {
        componentRef.setInput('formDirty', false);
        const response = component['validateMove']();
        response.subscribe(res => {
          expect(res).toBe(true);
        });
      });
      it('should return an observable that emits true if formDirty is true', () => {
        componentRef.setInput('formDirty', true);

        const mockDialogRef = {
          afterClosed: () => of({ result: true }),
        } as MatDialogRef<any>;

        vi.spyOn(component['messageboxService'], 'warning').mockReturnValue(
          mockDialogRef
        );

        const response = component['validateMove']();

        response.subscribe(res => {
          expect(res).toBe(true);
        });
      });
    });
  });
  describe('DOM', () => {
    beforeEach(() => {
      componentRef.setInput('records', [{ id: 1 }, { id: 2 }, { id: 3 }]);
      fixture.detectChanges();
    });
    describe('Buttons', () => {
      describe('first button', () => {
        let firstButtonDebug: DebugElement;
        let firstButton: HTMLButtonElement;
        beforeEach(() => {
          firstButtonDebug = fixture.debugElement.query(
            By.css('button[title="core.navigatefirst"]')
          );
          firstButton = firstButtonDebug.nativeElement;
        });
        it('should render', () => {
          expect(firstButton).toBeTruthy();
        });
        it('should be disabled if index is 0', () => {
          component.index = 0;
          fixture.detectChanges();
          expect(firstButton.disabled).toBe(true);
        });
        it('should not be disabled if index is greater than 0', () => {
          component.index = 1;
          fixture.detectChanges();
          expect(firstButton.disabled).toBe(false);
        });
        it('should call moveFirst when clicked', () => {
          vi.spyOn(component, 'moveFirst');
          firstButtonDebug.triggerEventHandler('click', null);
          expect(component.moveFirst).toHaveBeenCalled();
        });
      });
      describe('left button', () => {
        let leftButtonDebug: DebugElement;
        let leftButton: HTMLButtonElement;
        beforeEach(() => {
          leftButtonDebug = fixture.debugElement.query(
            By.css('button[title="core.navigateleft"]')
          );
          leftButton = leftButtonDebug.nativeElement;
        });
        it('should render', () => {
          expect(leftButton).toBeTruthy();
        });
        it('should be disabled if index is 0', () => {
          component.index = 0;
          fixture.detectChanges();
          expect(leftButton.disabled).toBe(true);
        });
        it('should not be disabled if index is greater than 0', () => {
          component.index = 1;
          fixture.detectChanges();
          expect(leftButton.disabled).toBe(false);
        });
        it('should call movePrev when clicked', () => {
          vi.spyOn(component, 'movePrev');
          leftButtonDebug.triggerEventHandler('click', null);
          expect(component.movePrev).toHaveBeenCalled();
        });
      });
      describe('right button', () => {
        let rightButtonDebug: DebugElement;
        let rightButton: HTMLButtonElement;
        beforeEach(() => {
          rightButtonDebug = fixture.debugElement.query(
            By.css('button[title="core.navigateright"]')
          );
          rightButton = rightButtonDebug.nativeElement;
        });
        it('should render', () => {
          expect(rightButton).toBeTruthy();
        });
        it('should be disabled if index is equal to one less than records length', () => {
          component.index = 2;
          fixture.detectChanges();
          expect(rightButton.disabled).toBe(true);
        });
        it('should not be disabled if index is less than records length minus one', () => {
          component.index = 1;
          fixture.detectChanges();
          expect(rightButton.disabled).toBe(false);
        });
        it('should call moveNext when clicked', () => {
          vi.spyOn(component, 'moveNext');
          rightButtonDebug.triggerEventHandler('click', null);
          expect(component.moveNext).toHaveBeenCalled();
        });
      });
      describe('last button', () => {
        let lastButtonDebug: DebugElement;
        let lastButton: HTMLButtonElement;
        beforeEach(() => {
          lastButtonDebug = fixture.debugElement.query(
            By.css('button[title="core.navigatelast"]')
          );
          lastButton = lastButtonDebug.nativeElement;
        });
        it('should render', () => {
          expect(lastButton).toBeTruthy();
        });
        it('should be disabled if index is equal to records length minus 1', () => {
          component.index = 2;
          fixture.detectChanges();
          expect(lastButton.disabled).toBe(true);
        });
        it('should not be disabled if index is less than records length minus 1', () => {
          component.index = 1;
          fixture.detectChanges();
          expect(lastButton.disabled).toBe(false);
        });
        it('should call moveLast when clicked', () => {
          vi.spyOn(component, 'moveLast');
          lastButtonDebug.triggerEventHandler('click', null);
          expect(component.moveLast).toHaveBeenCalled();
        });
      });
    });
    describe('Conditional content rendering', () => {
      it('should not render buttons when hideIfEmpty is true and records length is 0', () => {
        componentRef.setInput('hideIfEmpty', true);
        componentRef.setInput('records', []);
        fixture.detectChanges();

        const buttonGroup = fixture.debugElement.query(By.css('.btn-group'));
        expect(buttonGroup).toBeNull();
      });

      it('should render position section when hidePosition is false', () => {
        componentRef.setInput('hidePosition', false);
        fixture.detectChanges();

        const positionDiv = fixture.debugElement.query(By.css('.btn-position'));
        expect(positionDiv).not.toBeNull();
      });

      it('should not render position section when hidePosition is true', () => {
        componentRef.setInput('hidePosition', true);
        fixture.detectChanges();

        const positionDiv = fixture.debugElement.query(
          By.css('.btn-position.no-dropdown')
        );
        expect(positionDiv).toBeNull();
      });

      it('should render record name when showRecordName is true and isDate is false', () => {
        componentRef.setInput('hidePosition', false);
        componentRef.setInput('hideDropdown', true);
        componentRef.setInput('showRecordName', true);
        componentRef.setInput('isDate', false);
        componentRef.setInput('selectedRecord', { name: 'Record 1' });
        component.selectedRecord = { name: 'Record 1' }; // Mock record
        componentRef.setInput('dropdownTextProperty', 'name');
        fixture.detectChanges();

        const recordNameSpan = fixture.debugElement.query(
          By.css('.btn-position span')
        );
        expect(recordNameSpan.nativeElement.textContent.trim()).toBe(
          'Record 1'
        );
      });

      it('should render formatted date when showRecordName and isDate are true', () => {
        componentRef.setInput('hidePosition', false);
        componentRef.setInput('hideDropdown', true);
        componentRef.setInput('showRecordName', true);
        componentRef.setInput('isDate', true);
        component.selectedRecord = {
          toFormattedDate: () => '2024-01-01',
        };
        fixture.detectChanges();

        const dateSpan = fixture.debugElement.query(
          By.css('.btn-position span')
        );
        expect(dateSpan.nativeElement.textContent.trim()).toBe('2024-01-01');
      });
    });
  });
});
