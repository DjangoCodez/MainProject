export class Checkable<T> {
  public entity: T;
  public checked: boolean;

  constructor(entity: T) {
    this.entity = entity;
    this.checked = false;
  }
}
