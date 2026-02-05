import {
  IDecimalKeyValue,
  ISmallGenericType,
} from './generated-interfaces/GenericType';

export class SmallGenericType implements ISmallGenericType {
  id: number;
  name: string;

  constructor(id: number, name: string) {
    this.id = id;
    this.name = name;
  }
}

export class SelectableSmallGenericType {
  id: number;
  name: string;
  selected: boolean;

  constructor(id: number, name: string) {
    this.id = id;
    this.name = name;
    this.selected = false;
  }
}

export class DecimalKeyValue implements IDecimalKeyValue {
  key: number;
  value: number;
  constructor(key: number, value: number) {
    this.key = key;
    this.value = value;
  }
}
