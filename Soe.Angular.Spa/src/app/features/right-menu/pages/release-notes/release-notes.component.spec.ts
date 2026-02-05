import { ActivatedRoute } from '@angular/router';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ReleaseNotesComponent } from './release-notes.component';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

describe('ReleaseNotesComponent', () => {
  let component: ReleaseNotesComponent;
  let fixture: ComponentFixture<ReleaseNotesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [ReleaseNotesComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParams: {
                c: 'someValue',
                module: 'someModule',
                feature: 'someFeature',
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ReleaseNotesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
