import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import {
  SoeEntityType,
  TermGroup_ExtraFieldType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ExtraFieldsService } from '../../services/extra-fields.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { take, tap } from 'rxjs';
import { ValidationHandler } from '@shared/handlers';
import { ExtraFieldsInputForm } from '../../models/extra-fields-input-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TranslateService } from '@ngx-translate/core';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';

@Component({
  selector: 'soe-extra-fields-input',
  templateUrl: './extra-fields-input.component.html',
  styleUrls: ['./extra-fields-input.component.scss'],
  standalone: false,
})
export class ExtraFieldsInputComponent implements OnInit, OnChanges {
  @Input({ required: true }) recordId!: number;
  @Input({ required: true }) entity!: SoeEntityType;
  @Input({ required: true }) readOnly!: boolean;
  @Input() initialLoad: boolean = true;
  @Input() connectedEntity: number | undefined;
  @Input() connectedRecordId: number | undefined;
  @Input() copiedExtraFieldRecords: IExtraFieldRecordDTO[] = [];

  @Output() recordsChange = new EventEmitter<IExtraFieldRecordDTO[]>();

  service = inject(ExtraFieldsService);
  translate = inject(TranslateService);
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  performLoad = new Perform<IExtraFieldRecordDTO[]>(this.progressService);

  form: ExtraFieldsInputForm | undefined;

  yesNoDict: SmallGenericType[] = [];
  extraFieldType = TermGroup_ExtraFieldType;

  ngOnInit(): void {
    this.translate
      .get(['core.yes', 'core.no'])
      .pipe(take(1))
      .subscribe(terms => {
        this.yesNoDict = [
          { id: 0, name: '' },
          { id: 1, name: terms['core.yes'] },
          { id: 2, name: terms['core.no'] },
        ];
      });

    if (this.initialLoad) {
      this.performLoad.load(
        this.service
          .getExtraFieldWithRecords(
            this.recordId,
            this.entity,
            SoeConfigUtil.languageId,
            this.connectedEntity || 0,
            this.connectedRecordId || 0
          )
          .pipe(
            tap(records => {
              this.createForm(records);
            })
          )
      );
    } else {
      if (this.copiedExtraFieldRecords.length > 0) {
        this.createForm(this.copiedExtraFieldRecords);
      }
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.readOnly) {
      this.form?.disableControlsByReadOnly(changes.readOnly.currentValue);
    }
  }

  private createForm(records: IExtraFieldRecordDTO[]) {
    this.form = new ExtraFieldsInputForm({
      validationHandler: this.validationHandler,
      elements: records,
      readOnly: this.readOnly,
    });

    this.form.rows.valueChanges.subscribe(() => {
      if (this.form?.dirty) {
        this.recordsChange.emit(this.form!.rows.value);
      }
    });
  }
}
