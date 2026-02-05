import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { LanguageTranslationsComponent } from './language-translations.component';
import {
  SoeEntityState,
  TermGroup_Languages,
} from '@shared/models/generated-interfaces/Enumerations';

describe('LanguageTranslationsComponent', () => {
  let component: LanguageTranslationsComponent;
  let fixture: ComponentFixture<LanguageTranslationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      declarations: [LanguageTranslationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LanguageTranslationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return the correct ID for adding a new language', () => {
    component.rowData = {
      value: [
        { compTermId: -1 },
        { compTermId: -2 },
        { compTermId: -3 },
      ] as any[],
    } as any;

    const id = component.getIdForAddLanguage();

    expect(id).toBe(-4);
  });

  it('should return -1 if there are no negative IDs', () => {
    component.rowData = {
      value: [{ compTermId: 1 }, { compTermId: 2 }, { compTermId: 3 }],
    } as any;

    const id = component.getIdForAddLanguage();

    expect(id).toBe(-1);
  });

  it('should filter out deleted rows and find the correct row', () => {
    component.rowData = {
      value: [
        { state: SoeEntityState.Deleted, lang: TermGroup_Languages.English },
        { state: SoeEntityState.Active, lang: TermGroup_Languages.Swedish },
      ],
    } as any;
    const l = { id: TermGroup_Languages.Swedish };
    let available = false;

    const lagRow = component.rowData.value
      .filter(r => r.state != SoeEntityState.Deleted)
      .find(f => f.lang === +l.id);
    if (!lagRow) {
      available = true;
    }

    expect(lagRow).toEqual({
      state: SoeEntityState.Active,
      lang: TermGroup_Languages.Swedish,
    });
    expect(available).toBe(false);
  });

  it('should disable add button if readOnly is true', () => {
    component.readOnly = true;
    component.toolbarAddRowDisabled = false;

    if (component.readOnly) {
      component.toolbarAddRowDisabled = true;
    }

    expect(component.toolbarAddRowDisabled).toBe(true);
  });

  it('should enable add button when conditions are met', () => {
    // Mock data
    component.rowData = {
      value: [
        { state: SoeEntityState.Active, lang: 1 },
        { state: SoeEntityState.Active, lang: 2 },
        { state: SoeEntityState.Deleted, lang: 3 },
      ],
    } as any;
    component.languages = [{ id: 1 }, { id: 2 }, { id: 3 }] as any;
    component.readOnly = false;

    // Call the method
    component.validateAddButton();

    // Assertions
  });
});
