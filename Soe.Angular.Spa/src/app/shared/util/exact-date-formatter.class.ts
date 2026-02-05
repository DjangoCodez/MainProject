import { DateUtil } from './date-util';

export class ExactDateFormatter {
  private date: Date;

  constructor(inputDate: Date | string) {
    if (inputDate instanceof Date) {
      this.date = inputDate;
    } else if (typeof inputDate === 'string') {
      // Parse the input string as a date
      this.date = new Date(inputDate);
    } else {
      throw new Error(
        'Invalid input. Please provide a Date object or a valid date string.'
      );
    }
  }

  format(formatString: string): string {
    return DateUtil.format(this.date, formatString);
  }
}
