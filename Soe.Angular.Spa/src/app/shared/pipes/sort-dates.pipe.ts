import { Pipe, PipeTransform } from '@angular/core';
import { DateUtil } from '@shared/util/date-util';

@Pipe({
  name: 'sortDates',
  standalone: true,
})
export class SortDatesPipe implements PipeTransform {
  transform(value: Date[], desc = false): any {
    DateUtil.sort(value, desc);
    return value;
  }
}

@Pipe({
  name: 'sortDatesByKey',
  standalone: true,
})
export class SortDatesByKeyPipe implements PipeTransform {
  transform(value: any[], key: any, desc = false): any {
    DateUtil.sortByKey(value, key, desc);
    return value;
  }
}
