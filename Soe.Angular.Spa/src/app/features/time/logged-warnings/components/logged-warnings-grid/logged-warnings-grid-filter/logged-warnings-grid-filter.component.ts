import { Component, OnInit, inject, model } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-logged-warnings-grid-filter',
  templateUrl: './logged-warnings-grid-filter.component.html',
  standalone: false,
})
export class LoggedWarningsGridFilterComponent implements OnInit {
  selectedDateRange = model.required<number>();

  coreService = inject(CoreService);
  dateSelectionOptions: SmallGenericType[] = [];

  ngOnInit() {
    this.loadDateSelectionOptions();
  }

  loadDateSelectionOptions() {
    this.coreService
      .getTermGroupContent(
        TermGroup.ChangeStatusGridAllItemsSelection,
        false,
        false
      )
      .subscribe((options: SmallGenericType[]) => {
        this.dateSelectionOptions = options.sort((a, b) => a.id - b.id);
      });
  }

  onDateRangeChange(value: number) {
    this.selectedDateRange.set(value);
  }
}
