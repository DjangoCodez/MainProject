import { ChangeDetectorRef, ViewContainerRef } from '@angular/core';
import { EditBaseDirective } from './edit-base.directive';
import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

describe('EditBaseDirective', (): void => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        { provide: ToolbarService, useValue: {} },
        [ViewContainerRef],
        [ChangeDetectorRef],
      ],
    });
  });

  it('should create an instance', (): void => {
    TestBed.runInInjectionContext((): void => {
      const directive: EditBaseDirective<any, any, any> = new EditBaseDirective<
        any,
        any,
        any
      >();
      expect(directive).toBeTruthy();
    });
  });
});
