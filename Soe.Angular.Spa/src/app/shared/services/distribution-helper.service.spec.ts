import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { TranslateService } from '@ngx-translate/core';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { AccountDistributionService } from '@features/economy/account-distribution/services/account-distribution.service';
import { MessagingService } from './messaging.service';

import { DistributionHelperService } from './distribution-helper.service';
import { vi } from 'vitest';

describe('DistributionHelperService', () => {
  let service: DistributionHelperService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        {
          provide: DistributionHelperService,
          useFactory: () =>
            new DistributionHelperService(
              [], // accountDistributions
              [], // accountDistributionsForImport
              false, // useAutomaticAccountDistribution
              {} as any, // container
              () => {}, // allChangesDone
              () => {}, // deleteRow
              () => ({ row: {} as any, rowIndex: 0 }) // addRow
            ),
        },
        {
          provide: TranslateService,
          useValue: { get: vi.fn(), instant: vi.fn() },
        },
        { provide: MessageboxService, useValue: {} },
        { provide: DialogService, useValue: {} },
        {
          provide: AccountDistributionService,
          useValue: { accountDistributionHeadName$: { subscribe: vi.fn() } },
        },
        { provide: MessagingService, useValue: {} },
      ],
    });
    service = TestBed.inject(DistributionHelperService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
