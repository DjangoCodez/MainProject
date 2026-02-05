import { ISmallGenericType, IIntDateType, IIntKeyValue } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class SmallGenericType implements ISmallGenericType {
    id: number;
    name: string;

    constructor(id: number, name: string) {
        this.id = id;
        this.name = name;
    }
}

export class IntDateType implements IIntDateType {
    date: Date;
    number: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class IntKeyValue implements IIntKeyValue {
    key: number;
    value: number;

    constructor(key: number, value: number) {
        this.key = key;
        this.value = value;
    }

    get hashedString(): string {
        return `${this.key}#${this.value}`;
    }
}
