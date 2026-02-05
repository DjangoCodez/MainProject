import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { IntrastatExportGridHeaderDTO } from '../../models/intrastat-export.model';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { take, tap } from 'rxjs';
import { IntrastatReportingType } from '@shared/models/generated-interfaces/Enumerations';
import { TranslateService } from '@ngx-translate/core';
import { IntrastatExportGridHeaderForm } from '../../models/intrastat-export-grid-header-form.model';

@Component({
    selector: 'soe-intrastat-export-grid-header',
    templateUrl: './intrastat-export-grid-header.component.html',
    standalone: false
})
export class IntrastatExportGridHeaderComponent implements OnInit {
  @Output() searchClick = new EventEmitter<IntrastatExportGridHeaderDTO>();
  @Output() canCreateFile = new EventEmitter<boolean>();

  translate = inject(TranslateService);
  validationHandler = inject(ValidationHandler);

  types: SmallGenericType[] = [];
  formHeader: IntrastatExportGridHeaderForm = new IntrastatExportGridHeaderForm(
    {
      validationHandler: this.validationHandler,
      element: new IntrastatExportGridHeaderDTO(),
    }
  );

  ngOnInit(): void {
    this.getTypes().subscribe();
  }

  search() {
    this.searchClick.emit({
      fromDate: this.formHeader.fromDate.value,
      endDate: this.formHeader.endDate.value,
      reportingType: this.formHeader.reportingType.value,
    });
  }

  getTypes() {
    return this.translate
      .get([
        'common.intrastat.import',
        'common.intrastat.export',
        'common.intrastat.both',
      ])
      .pipe(
        take(1),
        tap(terms => {
          this.types = [
            {
              id: IntrastatReportingType.Import,
              name: terms['common.intrastat.import'],
            },
            {
              id: IntrastatReportingType.Export,
              name: terms['common.intrastat.export'],
            },
            {
              id: IntrastatReportingType.Both,
              name: terms['common.intrastat.both'],
            },
          ];
        })
      );
  }

  reportTypeChanged() {
    this.canCreateFile.emit(
      this.formHeader.reportingType.value === IntrastatReportingType.Both
    );
  }
}
