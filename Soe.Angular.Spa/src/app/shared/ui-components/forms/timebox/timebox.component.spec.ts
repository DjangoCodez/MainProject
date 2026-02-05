import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  ComponentFixture,
  fakeAsync,
  TestBed,
  tick,
} from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ComponentRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { TimeboxComponent } from './timebox.component';

describe('TimeboxComponent', () => {
  let component: TimeboxComponent;
  let fixture: ComponentFixture<TimeboxComponent>;
  let componentRef: ComponentRef<TimeboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TimeboxComponent,
        ReactiveFormsModule,
        FormsModule,
        SoftOneTestBed,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TimeboxComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Setup', () => {
    it('should initalize with default values', () => {
      expect(component.inputId()).toBeTruthy();
      expect(component.labelKey()).toBe('');
      expect(component.labelLowercase()).toBe(false);
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.secondaryLabelLowercase()).toBe(false);
      expect(component.placeholderKey()).toBe(
        'core.time.placeholder.hoursminutes'
      );
      expect(component.inline()).toBe(false);
      expect(component.alignInline()).toBe(false);
      expect(component.width()).toBe(0);
      expect(component.isDuration()).toBe(false);
      expect(component.leadingZero()).toBe(false);
      expect(component.timeSpan).toBe('');
    });
  });

  describe('Methods', () => {
    describe('ngOnInit', () => {
      it('should call updateVisibleValue and formatValue', () => {
        vi.spyOn(component, 'updateVisibleValue');
        vi.spyOn(component, 'formatValue');
        component.ngOnInit();
        expect(component.updateVisibleValue).toHaveBeenCalled();
        expect(component.formatValue).toHaveBeenCalled();
      });
    });
    describe('ngAfterViewInit', () => {});
    describe('updateVisibleValue', () => {
      describe('Date values', () => {
        it('should parse date 05:30 to 05:30', () => {
          const testdate = new Date('2022-02-02 05:30:00');
          component.updateVisibleValue(testdate);
          expect(component.timeSpan).toEqual('05:30');
        });
        it('should parse date 05:30 to 05:30', () => {
          const testdate = new Date('2026-01-01 01:00:00');
          component.updateVisibleValue(testdate);
          expect(component.timeSpan).toEqual('01:00');
        });
      });
      describe('String values', () => {
        it('should accept string values as is', () => {
          const teststring = '10:01';
          component.updateVisibleValue(teststring);
          expect(component.timeSpan).toEqual('10:01');
        });
      });
      describe('Number values', () => {
        describe('isDuration is false', () => {
          it('should parse number 10 to 10', () => {
            const testnumber = 10;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('10');
          });
          it('should parse number 1 to 1', () => {
            const testnumber = 1;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('1');
          });
          it('should parse number 8.5 to 8.5', () => {
            const testnumber = 8.5;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('8.5');
          });
        });
        describe('isDuration is true', () => {
          beforeEach(() => {
            componentRef.setInput('isDuration', true);
            fixture.detectChanges();
          });
          it('should parse number 10 to 0:10', () => {
            const testnumber = 10;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('0:10');
          });
          it('should parse number 1 to 0:01', () => {
            const testnumber = 1;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('0:01');
          });
          it('should parse number 8.5 to 0:08', () => {
            const testnumber = 8.5;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('0:09');
          });
          it('should parse number 100 to 1:40', () => {
            const testnumber = 100;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('1:40');
          });
          it('should parse 360 to 6:00', () => {
            const testnumber = 360;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('6:00');
          });
          it('should parse 0 to 0:00', () => {
            const testnumber = 0;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('0:00');
          });
          it('should parse 60 to 1:00', () => {
            const testnumber = 60;
            component.updateVisibleValue(testnumber);
            expect(component.timeSpan).toEqual('1:00');
          });
        });
      });
    });
    describe('formatValue', () => {
      describe('Parse values', () => {
        // TODO: Tests when isDuration is true
        describe('Positive cases', () => {
          describe('Using dots', () => {
            it('should parse 1.5 to 01:30', fakeAsync(() => {
              component.control.patchValue('1.5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(
                  new Date(1900, 0, 1).setHours(1, 30, 0, 0)
                  // .setFullYear(1900, 1, 1).setHours(1, 30, 0, 0)
                )
              );
            }));
            it('should parse 4.5 to 04:30', fakeAsync(() => {
              component.control.patchValue('4.5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(4, 30, 0, 0))
              );
            }));
            it('should parse 0.5 to 00:30', fakeAsync(() => {
              component.control.patchValue('0.5');
              component.formatValue(component.control);
              tick();
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 30, 0, 0))
              );
            }));
            it('should parse 0.55 to 00:33', fakeAsync(() => {
              component.control.patchValue('0.55');
              component.formatValue(component.control);
              tick();
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 33, 0, 0))
              );
            }));
            it('should parse 0.05 to 00:03', fakeAsync(() => {
              component.control.patchValue('0.05');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 3, 0, 0))
              );
            }));
            it('should parse 25.00 to 01:00', fakeAsync(() => {
              component.control.patchValue('25:00');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(1, 0, 0, 0))
              );
            }));
          });
          describe('Using commas', () => {
            it('should parse 1,5 to 01:30', fakeAsync(() => {
              component.control.patchValue('1,5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(1, 30, 0, 0))
              );
            }));
            it('should parse 4,5 to 04:30', fakeAsync(() => {
              component.control.patchValue('4,5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(4, 30, 0, 0))
              );
            }));
            it('should parse 0,5 to 00:30', fakeAsync(() => {
              component.control.patchValue('0,5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 30, 0, 0))
              );
            }));
            it('should parse 0,55 to 00:55', fakeAsync(() => {
              component.control.patchValue('0,55');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 33, 0, 0))
              );
            }));
            it('should parse 0,05 to 00:03', fakeAsync(() => {
              component.control.patchValue('0,05');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 3, 0, 0))
              );
            }));
          });
          describe('Using one char', () => {
            it('should parse 5 to 05:00', fakeAsync(() => {
              component.control.patchValue('5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(5, 0, 0, 0))
              );
            }));
            it('should parse 9 to 09:00', fakeAsync(() => {
              component.control.patchValue('9');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(9, 0, 0, 0))
              );
            }));
          });
          describe('Using two chars', () => {
            it('should parse 12 to 12:00', fakeAsync(() => {
              component.control.patchValue('12');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(12, 0, 0, 0))
              );
            }));
          });
          describe('Using three chars', () => {
            it('should parse 230 to 02:30', fakeAsync(() => {
              component.control.patchValue('230');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(2, 30, 0, 0))
              );
            }));
            it('should parse 2:3 to 02:03', fakeAsync(() => {
              component.control.patchValue('2:3');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(2, 3, 0, 0))
              );
            }));
          });
          describe('Using four chars', () => {
            it('should parse 0430 to 04:30', fakeAsync(() => {
              component.control.patchValue('5');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(5, 0, 0, 0))
              );
            }));
            it('should parse 0499 to 05:39', fakeAsync(() => {
              component.control.patchValue('0499');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(5, 39, 0, 0))
              );
            }));
            it('should parse 0560 to 06:00', fakeAsync(() => {
              component.control.patchValue('0560');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(6, 0, 0, 0))
              );
            }));
          });
          describe('Using five chars (including colon)', () => {
            it('should parse 05:60 to 06:00', fakeAsync(() => {
              component.control.patchValue('05:60');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(6, 0, 0, 0))
              );
            }));
            it('should parse 25:00 to 01:00', fakeAsync(() => {
              component.control.patchValue('25:00');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(1, 0, 0, 0))
              );
            }));
            it('should parse 48:00 to 00:00', fakeAsync(() => {
              component.control.patchValue('48:00');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(0, 0, 0, 0))
              );
            }));
            it('should parse 50:00 to 02:00', fakeAsync(() => {
              component.control.patchValue('50:00');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual(
                new Date(new Date(1900, 0, 1).setHours(2, 0, 0, 0))
              );
            }));
          });
        });
        describe('Negative cases', () => {
          it('should not change the valid value 05:30', fakeAsync(() => {
            component.control.patchValue('05:30');
            component.formatValue(component.control);
            tick(30);
            expect(component.control.value).toEqual(
              new Date(new Date(1900, 0, 1).setHours(5, 30, 0, 0))
            );
          }));
          it('should remove seconds from the value 05:30:00', fakeAsync(() => {
            component.control.patchValue('05:30:00');
            component.formatValue(component.control);
            tick(30);
            expect(component.control.value).toEqual(
              new Date(new Date(1900, 0, 1).setHours(5, 30, 0, 0))
            );
          }));
          it('should remove seconds from the value 05:30:60', fakeAsync(() => {
            component.control.patchValue('05:30:60');
            component.formatValue(component.control);
            tick(30);
            expect(component.control.value).toEqual(
              new Date(new Date(1900, 0, 1).setHours(5, 30, 0, 0))
            );
          }));
        });
        describe('isDuration', () => {
          beforeEach(() => {
            componentRef.setInput('isDuration', true);
            fixture.detectChanges();
          });
          describe('Positive cases', () => {
            describe('Using dots', () => {
              it('should parse 1.5 to 1:30', fakeAsync(() => {
                component.control.patchValue('1.5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('1:30');
              }));
              it('should parse 4.5 to 4:30', fakeAsync(() => {
                component.control.patchValue('4.5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('4:30');
              }));
              it('should parse 0.5 to 0:30', fakeAsync(() => {
                component.control.patchValue('0.5');
                component.formatValue(component.control);
                tick();
                expect(component.control.value).toEqual('0:30');
              }));
              it('should parse 0.55 to 0:33', fakeAsync(() => {
                component.control.patchValue('0.55');
                component.formatValue(component.control);
                tick();
                expect(component.control.value).toEqual('0:33');
              }));
              it('should parse 0.05 to 0:03', fakeAsync(() => {
                component.control.patchValue('0.05');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('0:03');
              }));
              it('should parse 25.00 to 25:00', fakeAsync(() => {
                component.control.patchValue('25:00');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('25:00');
              }));
            });
            describe('Using commas', () => {
              it('should parse 1,5 to 1:30', fakeAsync(() => {
                component.control.patchValue('1,5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('1:30');
              }));
              it('should parse 4,5 to 4:30', fakeAsync(() => {
                component.control.patchValue('4,5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('4:30');
              }));
              it('should parse 0,5 to 0:30', fakeAsync(() => {
                component.control.patchValue('0,5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('0:30');
              }));
              it('should parse 0,55 to 0:55', fakeAsync(() => {
                component.control.patchValue('0,55');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('0:33');
              }));
              it('should parse 0,05 to 0:03', fakeAsync(() => {
                component.control.patchValue('0,05');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('0:03');
              }));
              it('should parse 24,5 to 24:30', fakeAsync(() => {
                component.control.patchValue('24,5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('24:30');
              }));
            });
            describe('Using one char', () => {
              it('should parse 5 to 5:00', fakeAsync(() => {
                component.control.patchValue('5');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('5:00');
              }));
              it('should parse 9 to 9:00', fakeAsync(() => {
                component.control.patchValue('9');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('9:00');
              }));
            });
            describe('Using two chars', () => {
              it('should parse 12 to 12:00', fakeAsync(() => {
                component.control.patchValue('12');
                component.formatValue(component.control);
                tick(30);
                expect(component.control.value).toEqual('12:00');
              }));
            });
          });
          describe('Negative cases', () => {
            it('should not change the valid value 05:30', fakeAsync(() => {
              component.control.patchValue('05:30');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual('5:30');
            }));
            it('should remove seconds from the value 05:30:00', fakeAsync(() => {
              component.control.patchValue('05:30:00');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual('5:30');
            }));
            it('should remove seconds from the value 05:30:60', fakeAsync(() => {
              component.control.patchValue('05:30:60');
              component.formatValue(component.control);
              tick(30);
              expect(component.control.value).toEqual('5:30');
            }));
          });
        });
      });
    });
  });
  describe('DOM', () => {
    describe('div', () => {
      let divElement: HTMLDivElement;
      beforeEach(() => {
        divElement = fixture.debugElement.query(By.css('div')).nativeElement;
      });
      it('should render with default values', () => {
        expect(divElement).toBeTruthy();
        expect(divElement.classList.contains('mt-2')).toBe(true);
        expect(divElement.classList.contains('d-flex')).toBe(false);
        expect(divElement.classList.contains('flex-nowrap')).toBe(false);
        expect(
          divElement.classList.contains('form-label-inline-top-alignment')
        ).toBe(false);
        expect(divElement.style.width).toBe('');
      });
      it('should apply different classes when inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(divElement.classList.contains('mt-2')).toBe(false);
        expect(divElement.classList.contains('d-flex')).toBe(true);
        expect(divElement.classList.contains('flex-nowrap')).toBe(true);
        expect(
          divElement.classList.contains('form-label-inline-top-alignment')
        ).toBe(false);
      });
      it('should apply different classes when alignInline is true', () => {
        componentRef.setInput('alignInline', true);
        fixture.detectChanges();
        expect(divElement.classList.contains('mt-2')).toBe(true);
        expect(divElement.classList.contains('d-flex')).toBe(false);
        expect(divElement.classList.contains('flex-nowrap')).toBe(false);
        expect(
          divElement.classList.contains('form-label-inline-top-alignment')
        ).toBe(true);
      });
      it('should apply width when width is set', () => {
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(divElement.style.width).toBe('100px');
      });
    });
    describe('form-label-container', () => {
      let formLabelContainerElement: HTMLDivElement;
      beforeEach(() => {
        componentRef.setInput('gridMode', false);
        componentRef.setInput('labelKey', 'testLabel');
        fixture.detectChanges();
        formLabelContainerElement = fixture.debugElement.query(
          By.css('.form-label-container.d-flex.align-items-center')
        ).nativeElement;
      });
      it('should render with default values', () => {
        expect(formLabelContainerElement).toBeTruthy();
        expect(formLabelContainerElement.classList.contains('me-2')).toBe(
          false
        );
      });
      it('should apply me-2 if inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(formLabelContainerElement.classList.contains('me-2')).toBe(true);
      });
    });
    describe('soe-label', () => {
      let soeLabelDebugElement: DebugElement;
      let soeLabelElement: HTMLElement;
      let soeLabelComponentInstance: any;
      beforeEach(() => {
        componentRef.setInput('gridMode', false);
        componentRef.setInput('labelKey', 'testLabel');
        fixture.detectChanges();
        soeLabelDebugElement = fixture.debugElement.query(By.css('soe-label'));
        soeLabelElement = soeLabelDebugElement.nativeElement;
        soeLabelComponentInstance = soeLabelDebugElement.componentInstance;
      });
      it('should initialize with default values', () => {
        expect(soeLabelComponentInstance.labelKey()).toBe(component.labelKey());
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          component.secondaryLabelKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(
          component.secondaryLabelBold()
        );
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          component.secondaryLabelParantheses()
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          component.secondaryLabelPrefixKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          component.secondaryLabelPostfixKey()
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(
          component.isRequired()
        );
      });
      it('should pass the correct values', () => {
        componentRef.setInput('labelKey', 'testLabel');
        componentRef.setInput('secondaryLabelKey', 'testSecondaryLabel');
        componentRef.setInput('secondaryLabelBold', true);
        componentRef.setInput('secondaryLabelParantheses', false);
        componentRef.setInput('secondaryLabelPrefixKey', 'prefix');
        componentRef.setInput('secondaryLabelPostfixKey', 'postfix');
        component.isRequired.set(true);

        fixture.detectChanges();

        expect(soeLabelComponentInstance.labelKey()).toBe('testLabel');
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          'testSecondaryLabel'
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(true);
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          false
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          'prefix'
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          'postfix'
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(true);
      });
    });
    describe('input-group', () => {
      let inputGroupElement: HTMLDivElement;
      beforeEach(() => {
        inputGroupElement = fixture.debugElement.query(
          By.css('.input-group')
        ).nativeElement;
      });
      it('should render with default values', () => {
        expect(inputGroupElement).toBeTruthy();
        expect(inputGroupElement.style.width).toBe('');
      });
      it('should apply width when width is set and inline is true', () => {
        componentRef.setInput('width', 100);
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(inputGroupElement.style.width).toBe('100px');
      });
    });
    describe('input', () => {
      let inputDebugElement: DebugElement;
      let inputElement: HTMLInputElement;
      let inputInstance: any;
      beforeEach(() => {
        inputDebugElement = fixture.debugElement.query(
          By.css('.form-control.input-sm.text-right')
        );
        inputElement = fixture.debugElement.query(
          By.css('.form-control.input-sm.text-right')
        ).nativeElement;
        inputInstance = inputDebugElement.componentInstance;
      });
      it('should render with default values', () => {
        expect(inputElement).toBeTruthy();
        expect(inputElement.placeholder).toBe(component.placeholderKey());
        expect(inputElement.value).toBe(component.timeSpan);
        expect(inputElement.autofocus).toBe(component.autoFocus());
        expect(inputElement.disabled).toBe(component.control.disabled);
        expect(inputElement.style.width).toBe('');
      });
      it('should apply no-border-right-radius class when hasContent is true', () => {
        component.hasContent.set(true);
        fixture.detectChanges();
        expect(inputElement.classList.contains('no-border-right-radius')).toBe(
          true
        );
      });
      it('should apply width when width is set', () => {
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(inputElement.style.width).toBe('100px');
      });
      it('should bind the disabled attribute correctly', () => {
        component.control.disable(); // Disable the form control
        fixture.detectChanges();

        expect(inputElement.hasAttribute('disabled')).toBe(true);
      });
      it('should bind the value correctly', () => {
        component.timeSpan = '12:00';
        fixture.detectChanges();
        expect(inputElement.value).toBe('12:00');
      });
      it('should call formatValue on blur', () => {
        vi.spyOn(component, 'formatValue');
        inputElement.dispatchEvent(new Event('blur'));
        expect(component.formatValue).toHaveBeenCalled();
      });
    });
  });

  describe('Date values', () => {
    it('should read in the timeSpan from a date', fakeAsync(() => {
      component.ngOnInit();
      const date = new Date();
      date.setHours(4, 4, 0, 0);
      component.control.patchValue(date);
      component.updateVisibleValue(date);
      tick(100);
      expect(component.timeSpan).toEqual('04:04');
      expect(component.control.value).toEqual(date);
    }));
  });
});
