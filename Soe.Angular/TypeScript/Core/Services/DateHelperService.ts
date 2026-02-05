export class DateHelperService {

    //@ngInject
    constructor(private $locale) {
    }

    public getShortDateFormat(): string {
        return this.$locale.DATETIME_FORMATS.shortDate;
    }

    public getDateSeparator(): string {
        var separators = this.getShortDateFormat().replace(/[yYmMdD]/g, '');
        return separators && separators.length > 0 ? separators.charAt(0) : '-';
    }
}
