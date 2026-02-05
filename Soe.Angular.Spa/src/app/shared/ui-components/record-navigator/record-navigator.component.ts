
import { Component, OnInit, inject, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox'
import { IconModule } from '@ui/icon/icon.module'
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { first, indexOf, last } from 'lodash';
import { Observable } from 'rxjs';

export class NavigatorRecordConfig {
  hideIfEmpty = true;
  hidePosition = false;
  showRecordName = false;
  hideDropdown = false;
  dropdownTextProperty = 'name';
  hideRecordNavigator = false;
  isDate = false;
  refetchDataOnRecordChange = true;
}

@Component({
  selector: 'soe-record-navigator',
  imports: [IconModule, TranslatePipe],
  templateUrl: './record-navigator.component.html',
  styleUrls: ['./record-navigator.component.scss'],
})
export class RecordNavigatorComponent implements OnInit {
  records = input<any[]>([]);
  selectedId = input(0);
  hideIfEmpty = input(false);
  hidePosition = input(false);
  showRecordName = input(false);
  hideDropdown = input(false);
  dropdownTextProperty = input('name');
  isDate = input(false);
  formDirty = input(false);
  recordChanged = output<SmallGenericType>();

  private readonly messageboxService = inject(MessageboxService);

  index = 0;
  selectedRecord: any = undefined;

  ngOnInit() {
    if (this.records().length > 0) {
      this.selectedRecord = this.isDate()
        ? this.records()[this.selectedId()]
        : this.records().find(r => r.id === this.selectedId());
      this.setIndex();
    }
  }

  private setIndex() {
    const prevIndex = this.index;
    this.index = indexOf(this.records(), this.selectedRecord);
    if (prevIndex !== this.index && this.selectedRecord)
      this.recordChanged.emit(this.selectedRecord);
  }

  moveFirst() {
    if (this.index > 0) this.selectRecord(first(this.records()));
  }

  movePrev() {
    if (this.index > 0) this.selectRecord(this.records()[this.index - 1]);
  }

  moveNext() {
    if (this.index < this.records().length - 1)
      this.selectRecord(this.records()[this.index + 1]);
  }

  moveLast() {
    if (this.index < this.records().length - 1)
      this.selectRecord(last(this.records()));
  }

  selectRecord(record: SmallGenericType) {
    this.validateMove().subscribe(x => {
      if (x) {
        this.selectedRecord = record;
        this.setIndex();
      }
    });
  }

  private validateMove(): Observable<boolean> {
    const proceed$ = new Observable<boolean>(observer => {
      if (this.formDirty()) {
        const mb = this.messageboxService.warning(
          'core.warning',
          'core.confirmonexit'
        );
        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          observer.next(response?.result || false);
        });
      } else {
        observer.next(true);
      }
    });
    return proceed$;
  }
}
