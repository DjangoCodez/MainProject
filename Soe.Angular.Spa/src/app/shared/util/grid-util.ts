import { isNil, isNaN } from 'lodash';
import { DateUtil } from './date-util';
import { SoeColumnType } from '@ui/grid/util/column-util';

export class AgGridUtil {
  static groupComparator(nodeA: any, nodeB: any, valueA: any, valueB: any) {
    let colDef;
    let isTimeSpan = false;
    let isDate = false;
    let isNumber = false;

    if (nodeA && nodeA.rowGroupColumn && nodeA.rowGroupColumn.colDef)
      colDef = nodeA.rowGroupColumn.colDef;
    else if (nodeB && nodeB.rowGroupColumn && nodeB.rowGroupColumn.colDef)
      colDef = nodeB.rowGroupColumn.colDef;

    if (colDef && colDef.soeType) {
      if (colDef.soeColumnType === SoeColumnType.TimeSpan) isTimeSpan = true;
      else if (colDef.soeColumnType === SoeColumnType.Date) isDate = true;
      else if (colDef.soeColumnType === SoeColumnType.Number) isNumber = true;
    }

    if (isTimeSpan) {
      // TimeSpan
      const minutesA: number = DateUtil.timeSpanToMinutes(valueA);
      const minutesB: number = DateUtil.timeSpanToMinutes(valueB);
      if (minutesA === minutesB) return 0;
      else if (isNil(minutesA) || isNaN(minutesA)) return 1;
      else if (isNil(minutesB) || isNaN(minutesB)) return -1;
      else return minutesA > minutesB ? 1 : -1;
    } else if (isDate) {
      // Date
      const dateA: Date = DateUtil.parseDate(
        valueA,
        DateUtil.languageDateFormat
      ) as Date;
      const dateB: Date = DateUtil.parseDate(
        valueB,
        DateUtil.languageDateFormat
      ) as Date;

      if (dateA === dateB) return 0;
      else return dateA > dateB ? 1 : -1;
    } else if (isNumber) {
      // Number
      if (valueA === valueB) return 0;
      else if (isNil(valueA) || isNaN(valueA)) return 1;
      else if (isNil(valueB) || isNaN(valueB)) return -1;
      else return parseFloat(valueA) > parseFloat(valueB) ? 1 : -1;
    } else {
      // String
      if (valueA === valueB) return 0;
      else return valueA > valueB ? 1 : -1;
    }
  }
}
