import { SortDatesByKeyPipe, SortDatesPipe } from './sort-dates.pipe';

describe('SortDatesPipe', () => {
  it('create an instance', () => {
    const pipe = new SortDatesPipe();
    expect(pipe).toBeTruthy();
  });
});

describe('SortDatesByKeyPipe', () => {
  it('create an instance', () => {
    const pipe = new SortDatesByKeyPipe();
    expect(pipe).toBeTruthy();
  });
});
