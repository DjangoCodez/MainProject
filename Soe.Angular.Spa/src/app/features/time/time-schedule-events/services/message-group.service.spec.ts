import { TestBed } from '@angular/core/testing';

import { MessageGroupService } from './message-group.service';

describe('MessageGroupService', () => {
  let service: MessageGroupService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MessageGroupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
