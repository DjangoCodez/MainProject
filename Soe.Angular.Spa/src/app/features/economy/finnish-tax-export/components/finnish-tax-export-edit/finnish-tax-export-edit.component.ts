import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DownloadUtility } from '@shared/util/download-util';
import { Observable, take, tap } from 'rxjs';
import { IFinnishTaxExporFiletDTO } from '../../../../../shared/models/generated-interfaces/FinishTaxExportDTO';
import { FinnishTaxExportForm } from '../../models/finnish-tax-export-form.model';
import { FinnishTaxExportDTO } from '../../models/finnish-tax-export.model';
import { FinnishTaxExportService } from '../../services/finnish-tax-export.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-finnish-tax-export-edit',
  templateUrl: './finnish-tax-export-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class FinnishTaxExportEditComponent
  extends EditBaseDirective<
    FinnishTaxExportDTO,
    FinnishTaxExportService,
    FinnishTaxExportForm
  >
  implements OnInit
{
  readonly service = inject(FinnishTaxExportService);
  private readonly coreService = inject(CoreService);

  protected periodLengths: SmallGenericType[] = [];
  protected correctionReasons: SmallGenericType[] = [];

  ngOnInit(): void {
    this.startFlow(Feature.Economy_Export_Finnish_Tax, {
      lookups: [this.loadPeriodLengths(), this.loadCorrectionReasons()],
    });
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.export.finnishtax.periodtaxreturn.taxperiod.month.error',
      'economy.export.finnishtax.periodtaxreturn.taxperiod.quarter.error',
      'economy.export.finnishtax.periodtaxreturn.taxperiodyear.length.error',
      'economy.export.finnishtax.periodtaxreturn.taxperiodyear.toofarinpast.error',
      'economy.export.finnishtax.periodtaxreturn.exportsuccess',
      'economy.export.finnishtax.periodtaxreturn.exporterror',
    ]);
  }

  private loadPeriodLengths(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.FinnishTaxReturnExportTaxPeriodLength,
          false,
          true
        )
        .pipe(tap(x => (this.periodLengths = x)))
    );
  }

  private loadCorrectionReasons(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.FinnishTaxReturnExportCause, true, true)
        .pipe(tap(x => (this.correctionReasons = x)))
    );
  }

  override onFinished(): void {
    this.form?.setCustomValidators(
      this.terms[
        'economy.export.finnishtax.periodtaxreturn.taxperiod.month.error'
      ],
      this.terms[
        'economy.export.finnishtax.periodtaxreturn.taxperiod.quarter.error'
      ],
      this.terms[
        'economy.export.finnishtax.periodtaxreturn.taxperiodyear.length.error'
      ],
      this.terms[
        'economy.export.finnishtax.periodtaxreturn.taxperiodyear.toofarinpast.error'
      ]
    );
  }

  protected export(): void {
    if (!this.form || this.form.invalid || !this.service) return;

    this.performAction.crud(
      CrudActionTypeEnum.Work,
      this.service.exportVatFile(
        this.form?.getRawValue() as FinnishTaxExportDTO
      ),
      this.exportSuccess.bind(this),
      undefined,
      {
        failIfNoObjectsAffected: false,
        showDialog: false,
        showToastOnComplete: false,
      }
    );
  }

  private exportSuccess(backendResponse: BackendResponse): void {
    if (backendResponse.success) {
      const file = ResponseUtil.getValueObject(
        backendResponse
      ) as IFinnishTaxExporFiletDTO;
      DownloadUtility.downloadFile(file.name, file.extension, file.data);
      this.progressService.workComplete({
        showToastOnComplete: true,
        message:
          this.terms['economy.export.finnishtax.periodtaxreturn.exportsuccess'],
      });
    } else {
      this.progressService.workError({
        showToastOnComplete: true,
        message:
          this.terms['economy.export.finnishtax.periodtaxreturn.exporterror'],
      });
    }
  }
}
