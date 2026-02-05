import { IStringKeyValue, IStringKeyValueList } from "../../Scripts/TypeLite.Net4";

export class StringKeyValue implements IStringKeyValue {
    key: string;
    value: string;

    constructor(key: string, value: any) {
        this.key = key;
        this.value = value ? value.toString() : null;
    }
}

export class StringKeyValueList implements IStringKeyValueList {
    id: number;
    values: StringKeyValue[];

    constructor(id: number) {
        this.id = id;
        this.values = [];
    }
}
