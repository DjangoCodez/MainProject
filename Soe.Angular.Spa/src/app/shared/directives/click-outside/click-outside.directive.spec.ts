import { ClickOutsideDirective } from './click-outside.directive';
import { ElementRef, Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { vi } from 'vitest';

@Component({
  template: `<div soeClickOutside (clickOutside)="onClickOutside()"></div>`,
})
class TestComponent {
  onClickOutside() {}
}

describe('ClickOutsideDirective', () => {
  let fixture: ComponentFixture<TestComponent>;
  let directive: ClickOutsideDirective;
  let elementRef: ElementRef;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestComponent],
      declarations: [ClickOutsideDirective],
    });

    fixture = TestBed.createComponent(TestComponent);
    elementRef = fixture.debugElement.children[0].injector.get(ElementRef);
    directive = new ClickOutsideDirective(elementRef);
    fixture.detectChanges();
  });

  it('should create an instance', () => {
    expect(directive).toBeTruthy();
  });

  it('should emit clickOutside when clicking outside the element', () => {
    vi.spyOn(directive.clickOutside, 'emit');
    directive.isOpen = true;
    directive.onClick(document.createElement('div'));
    expect(directive.clickOutside.emit).toHaveBeenCalled();
  });

  it('should not emit clickOutside when clicking inside the element', () => {
    vi.spyOn(directive.clickOutside, 'emit');
    directive.isOpen = true;
    directive.onClick(elementRef.nativeElement);
    expect(directive.clickOutside.emit).not.toHaveBeenCalled();
  });

  it('should not emit clickOutside when isOpen is false', () => {
    vi.spyOn(directive.clickOutside, 'emit');
    directive.isOpen = false;
    directive.onClick(document.createElement('div'));
    expect(directive.clickOutside.emit).not.toHaveBeenCalled();
  });
});
