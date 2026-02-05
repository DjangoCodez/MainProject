import { AdjustTimeStampsForm } from './adjust-time-stamps-form.model';

describe('AdjustTimeStampsForm', () => {
  it('should create an instance', () => {
    expect(new AdjustTimeStampsForm({ validationHandler: {} as any, element: {} as any })).toBeTruthy();
  });
});
