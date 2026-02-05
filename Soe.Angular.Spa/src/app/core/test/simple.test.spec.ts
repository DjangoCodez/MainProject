import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';

function sum(a: number, b: number): number {
  return a + b;
}

test('adds 1 + 2 to equal 3', () => {
  TestBed.configureTestingModule({
    imports: [ToolbarComponent, SoftOneTestBed],
  })
    .compileComponents()
    .then(() => {
      const fixture: ComponentFixture<ToolbarComponent> =
        TestBed.createComponent(ToolbarComponent);
      fixture.detectChanges();

      expect(sum(1, 2)).toBe(3);
    });
});
