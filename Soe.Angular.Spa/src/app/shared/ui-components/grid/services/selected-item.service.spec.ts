import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SelectedItemService } from './selected-item.service';
import { Constants } from '@shared/util/client-constants';
import { vi } from 'vitest';

describe('SelectedItemService', () => {
  let service: SelectedItemService<any, any>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [SelectedItemService],
    }).compileComponents();

    service = TestBed.inject(SelectedItemService);
  });
  it('should create', () => {
    expect(service).toBeTruthy();
  });
  describe('methods', () => {
    describe('changedItems', () => {
      it('should return the changed items', () => {
        service.items['1'] = { value: 'test', row: { id: 1, name: 'test' } };
        expect(service.changedItems).toEqual(['1']);
      });
    });
    describe('toggle', () => {
      it('should add an item on toggle', () => {
        const row = { id: '1' };
        const value = 'item1';
        service.toggle(row, 'id', value);

        expect(service.items['1']).toEqual({ value, row });
      });
      it('should toggle the item', () => {
        const row = { id: 1, name: 'test' };
        service.toggle(row, 'id', 'test');
        expect(service.items['1']).toBeTruthy();
        service.toggle(row, 'id', 'test');
        expect(service.items['1']).toBeFalsy();
      });
      it('should not add item if idField is undefined', () => {
        const row = { id: 1, name: 'test' };
        service.toggle(row, undefined, 'test');
        expect(service.items['1']).toBeFalsy();
      });
    });
    describe('toDict', () => {
      it('should return the dict', () => {
        const row = { id: 1, name: 'test' };
        service.items['1'] = { value: 'test', row };
        expect(service.toDict()).toEqual({ dict: { '1': 'test' } });
      });
    });
  });
});
