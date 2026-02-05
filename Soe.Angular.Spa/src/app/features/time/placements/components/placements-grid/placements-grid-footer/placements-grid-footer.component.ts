import {
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import {
  createHasInitialAttestStateValidator,
  createSelectedRowValidator,
  createStopdateAfterStartdateValidator,
  createStopdateMaxTwoYearsValidator,
  PlacementsFooterForm,
} from '@features/time/placements/models/placements-footer-form.model';
import { PlacementsService } from '@features/time/placements/services/placements.service';
import { TranslateService } from '@ngx-translate/core';
import {
  IPlacementsControlDialogData,
  PlacementsControlDialogComponent,
} from '@shared/components/time/placements-control-dialog/placements-control-dialog.component';
import {
  IPlacementsPendingRecalculationDialogData,
  PlacementsPendingRecalculationDialogComponent,
} from '@shared/components/time/placements-pending-recalculation-dialog/placements-pending-recalculation-dialog/placements-pending-recalculation-dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  SoeModule,
  TermGroup,
  TermGroup_AttestEntity,
  TermGroup_TemplateScheduleActivateFunctions,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IActivateScheduleControlDTO,
  IActivateScheduleGridDTO,
  ITimeScheduleTemplateHeadSmallDTO,
  ITimeScheduleTemplatePeriodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DayOfWeek } from '@shared/util/Enumerations';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { Observable, take, tap } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';
import { DateUtil } from '@shared/util/date-util';
import { LabelComponent } from '@ui/label/label.component';
import { Perform } from '@shared/util/perform.class';
import { ProgressService } from '@shared/services/progress';

export interface IActivationResult {
  activationSuccessful: boolean;
}
@Component({
  selector: 'soe-placements-grid-footer',
  templateUrl: './placements-grid-footer.component.html',
  styleUrls: ['./placements-grid-footer.component.scss'],
  providers: [FlowHandlerService],
  imports: [
    SelectComponent,
    DatepickerComponent,
    CheckboxComponent,
    SaveButtonComponent,
    ReactiveFormsModule,
  ],
})
export class PlacementsGridFooterComponent implements OnInit {
  // Services
  private readonly service = inject(PlacementsService);
  private readonly coreService = inject(CoreService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly dialogService = inject(DialogService);
  private readonly translateService = inject(TranslateService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly progressService = inject(ProgressService);
  private readonly performLoadData = new Perform<any>(this.progressService);
  private readonly destroyRef = inject(DestroyRef);

  // Data
  public functions: SmallGenericType[] = [];
  public templateHeads: SmallGenericType[] = [];
  public templatePeriods: SmallGenericType[] = [];
  private terms: TermCollection = {};

  // Flags
  public showPreliminary = signal(false);
  public defaultPreliminary = signal(false);
  public progressMessage = signal('');
  public activationInProgress = signal(false);
  public hasInitialAttestState = signal(false);

  // Props
  public selectedRows = input.required<IActivateScheduleGridDTO[]>();

  // Output
  public activatingFinished = output<IActivationResult>();

  public form!: PlacementsFooterForm;

  constructor() {
    effect(() => {
      this.selectedRows();
      this.form?.updateValueAndValidity({ onlySelf: true, emitEvent: false });
    });
  }

  ngOnInit() {
    this.form = this.createForm();
    this.setDisabled();
    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(x => {
        this.setDisabled();
      });

    this.loadCompanySettings().subscribe();
    this.loadFunctions().subscribe();
    this.loadTemplateHeads().subscribe();
    this.loadHasInitialAttestState().subscribe();
    this.loadTerms().subscribe();
    this.loadTemplatePeriods(0).subscribe();
  }

  public get saveIsDisabled() {
    return (
      !this.form?.dirty || this.form?.invalid || this.activationInProgress()
    );
  }

  private addFormValidators() {
    this.form?.addValidators([
      createSelectedRowValidator(
        this.terms['time.schedule.activate.noemployeeselected'],
        this.selectedRows
      ),
      createStopdateAfterStartdateValidator(
        this.terms['time.schedule.activate.stopdatebeforestartdate']
      ),
      createStopdateMaxTwoYearsValidator(
        this.terms['time.schedule.activate.stopdatemaxtwoyears']
      ),
      createHasInitialAttestStateValidator(
        this.hasInitialAttestState,
        this.terms['time.schedule.activate.missinginitialatteststate']
      ),
    ]);
    this.form?.updateValueAndValidity();
  }

  // LOAD DATA

  private loadCompanySettings() {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.TimePlacementHidePreliminary,
        CompanySettingType.TimePlacementDefaultPreliminary,
      ])
      .pipe(
        tap(x => {
          this.showPreliminary.set(
            !SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimePlacementHidePreliminary
            )
          );
          this.defaultPreliminary.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimePlacementDefaultPreliminary
            )
          );
        }),
        tap(() => {
          this.form?.patchValue(
            { isPreliminary: this.defaultPreliminary() },
            { emitEvent: false }
          );
        })
      );
  }

  private loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return this.translateService
      .get([
        'time.schedule.activate.searchtemplate',
        'time.schedule.activate.noemployeeselected',
        'time.schedule.activate.stopdatebeforestartdate',
        'time.schedule.activate.stopdatemaxtwoyears',
        'time.schedule.activate.missinginitialatteststate',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
          this.addFormValidators();
        })
      );
  }

  private loadFunctions(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TemplateScheduleActivateFunctions,
        false,
        true,
        true
      )
      .pipe(
        tap(x => {
          this.functions = x;
        })
      );
  }

  private loadTemplateHeads(): Observable<ITimeScheduleTemplateHeadSmallDTO[]> {
    return this.service.getTimeScheduleTemplateHeadsForActivate().pipe(
      tap(templates => {
        this.templateHeads = [
          ...templates.map(template => {
            return new SmallGenericType(
              template.timeScheduleTemplateHeadId,
              template.name
            );
          }),
          new SmallGenericType(
            0,
            this.terms['time.schedule.activate.searchtemplate']
          ),
        ];
      })
    );
  }

  private loadTemplatePeriods(
    templateHeadId: number
  ): Observable<ITimeScheduleTemplatePeriodDTO[]> {
    return this.service
      .getTimeScheduleTemplatePeriodsForActivate(templateHeadId)
      .pipe(
        tap(periods => {
          this.templatePeriods =
            periods && periods.length > 0
              ? periods.map(
                  period =>
                    new SmallGenericType(
                      period.timeScheduleTemplatePeriodId,
                      period.dayNumber.toString()
                    )
                )
              : [new SmallGenericType(0, ' ')];
        })
      );
  }

  private loadHasInitialAttestState(): Observable<boolean> {
    return this.service
      .getHasInitialAttestState(TermGroup_AttestEntity.Unknown, SoeModule.Time)
      .pipe(
        tap(x => {
          this.hasInitialAttestState.set(x);
        })
      );
  }

  // EVENTS
  public templateHeadChanged(templateHeadId: number) {
    this.loadTemplatePeriods(this.form.controls.templateHeadId.value)
      .pipe(
        tap(() => {
          // templateHeadId = 0 means searching basic schedule > no periods
          if (templateHeadId !== 0) this.setPeriod();
        })
      )
      .subscribe();
  }

  public onActivate(): void {
    this.translateService
      .get([
        'time.recalculatetimestatus.activateschedulecontrol',
        'time.schedule.activate.to',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.progressMessage.set(
          terms['time.recalculatetimestatus.activateschedulecontrol']
        );

        this.performControlActivations(
          this.selectedRows(),
          this.form?.controls.startDate.value,
          this.form?.controls.stopDate.value
        ).subscribe(control => {
          if (!control.hasWarnings) {
            this.performActivate(control);
          } else {
            const dialogData: IPlacementsControlDialogData = {
              size: 'fullscreen',
              title:
                terms['time.schedule.activate.to'] +
                ' ' +
                DateUtil.localeDateFormat(this.form?.controls.stopDate.value),
              control: control,
              disableClose: true,
            };
            this.dialogService
              .open(PlacementsControlDialogComponent, dialogData)
              .afterClosed()
              .subscribe(result => {
                if (result) {
                  this.performActivate(control);
                }
              });
          }
        });
      });
  }

  // HELPER METHODS
  private performActivate(control: IActivateScheduleControlDTO) {
    this.activationInProgress.set(true);
    this.translateService
      .get([
        'time.recalculatetimestatus.validate.gethead',
        'time.schedule.activate.to',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.progressMessage.set(
          terms['time.recalculatetimestatus.validate.gethead']
        );
        const dialogData: IPlacementsPendingRecalculationDialogData = {
          size: 'md',
          title:
            terms['time.schedule.activate.to'] +
            ' ' +
            DateUtil.localeDateFormat(this.form.stopDate.value),
          control: control,
          rows: this.selectedRows(),
          functionId: this.form.functionId.value,
          templateHeadId: this.form.templateHeadId.value,
          templatePeriodId: this.form.templatePeriodId.value,
          startDate: this.form.startDate.value,
          stopDate: this.form.stopDate.value,
          isPreliminary: this.form.isPreliminary.value,
          disableClose: true,
          hideCloseButton: true,
        };
        this.dialogService
          .open(PlacementsPendingRecalculationDialogComponent, dialogData)
          .afterClosed()
          .subscribe(result => {
            this.onActivationFinished(result);
          });
      });
  }

  private createForm(): PlacementsFooterForm {
    return new PlacementsFooterForm({
      validationHandler: this.validationHandler,
      element: {
        functionId: TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate,
        templateHeadId: 0,
        templatePeriodId: 0,
        startDate: null,
        stopDate: null,
        isPreliminary: false,
      },
    });
  }

  private onActivationFinished(result: IActivationResult) {
    this.activationInProgress.set(false);
    this.activatingFinished.emit(result);
  }

  private setDisabled() {
    const functionId: number = this.form?.controls.functionId.value;
    const templateHeadId: number = this.form?.controls.templateHeadId.value;
    this.updateTemplateHeadControlState(functionId);
    this.updateTemplatePeriodControlState(templateHeadId, functionId);
    this.updateStartDateControlState(functionId);
  }

  private updateTemplateHeadControlState(functionId: number) {
    if (
      functionId ===
        TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate ||
      this.templateHeads.length <= 0
    ) {
      this.form?.controls.templateHeadId.disable({ emitEvent: false });
      this.form?.patchValue(
        {
          templateHeadId: 0,
        },
        { emitEvent: false }
      );
    } else if (
      functionId === TermGroup_TemplateScheduleActivateFunctions.NewPlacement
    ) {
      this.form?.controls.templateHeadId.enable({ emitEvent: false });
    }
  }

  private updateTemplatePeriodControlState(
    templateHeadId: number,
    functionId: number
  ) {
    if (
      templateHeadId === 0 ||
      this.templateHeads.length <= 0 ||
      functionId === TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate
    ) {
      this.form?.controls.templatePeriodId.disable({ emitEvent: false });
      this.resetTemplatePeriodValue();
    } else if (
      templateHeadId > 0 &&
      functionId === TermGroup_TemplateScheduleActivateFunctions.NewPlacement
    ) {
      this.form?.controls.templatePeriodId.enable({ emitEvent: false });
    }
  }

  private resetTemplatePeriodValue() {
    this.templatePeriods = [];
    this.templatePeriods.push(new SmallGenericType(0, ' '));

    this.form?.patchValue(
      {
        templatePeriodId: this.templatePeriods[0].id,
      },
      { emitEvent: false }
    );
  }

  private setPeriod() {
    let idx = 0;
    let selectedPeriodId = this.templatePeriods[idx].id;
    let period = this.templatePeriods.find(p => p.id === selectedPeriodId);
    let head = this.templateHeads.find(
      h => h.id === this.form?.templateHeadId.value
    );
    const startDate: Date = this.form?.startDate.value;

    if (!head) return;

    while (
      !!startDate &&
      period &&
      parseInt(period.name) &&
      startDate.addDays(-parseInt(period.name) + 1).dayOfWeek() !=
        DayOfWeek.Monday
    ) {
      if (idx + 1 < this.templatePeriods.length) {
        idx++;
        selectedPeriodId = this.templatePeriods[idx].id;
        period = this.templatePeriods.find(p => p.id === selectedPeriodId);
      } else {
        this.translateService
          .get([
            'time.schedule.activate.cantsetperiod',
            'time.schedule.activate.cantsetperiod.toofewdays',
          ])
          .pipe(
            tap(terms => {
              this.messageboxService.error(
                terms['time.schedule.activate.cantsetperiod'],
                terms['time.schedule.activate.cantsetperiod.toofewdays']
              );
            })
          )
          .subscribe();

        break;
      }
    }

    this.form?.patchValue(
      {
        templatePeriodId: selectedPeriodId,
      },
      { emitEvent: false }
    );
  }

  private updateStartDateControlState(functionId: number) {
    if (
      functionId === TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate
    ) {
      this.form.patchValue({ startDate: null }, { emitEvent: false });
      this.form?.controls.startDate.disable({ emitEvent: false });
    } else if (
      functionId === TermGroup_TemplateScheduleActivateFunctions.NewPlacement
    ) {
      this.form?.controls.startDate.enable({ emitEvent: false });
    }
  }

  private performControlActivations(
    rows: IActivateScheduleGridDTO[],
    startDate?: Date,
    stopDate?: Date,
    isDelete?: boolean
  ): Observable<IActivateScheduleControlDTO> {
    return this.performLoadData.load$(
      this.service.controlActivations(rows, startDate, stopDate, isDelete)
    );
  }
}
