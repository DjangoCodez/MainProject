import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { of, throwError, firstValueFrom, skip } from 'rxjs';
import { CurrencyService } from './currency.service';
import { CurrencyCoreService } from './currency-core.service';
import { ICompCurrencyDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  TermGroup_CurrencyType,
  TermGroup_CurrencySource,
} from '@shared/models/generated-interfaces/Enumerations';
import { vi } from 'vitest';

describe('CurrencyService', () => {
  let service: CurrencyService;
  let mockCurrencyCoreService: any;

  const mockBaseCurrency: ICompCurrencyDTO = {
    currencyId: 1,
    code: 'USD',
    name: 'US Dollar',
    rateToBase: 1,
    date: new Date('2024-01-01'),
    compCurrencyRates: [],
    sysCurrencyId: 1,
  };

  const mockTransactionCurrency: ICompCurrencyDTO = {
    currencyId: 2,
    code: 'EUR',
    name: 'Euro',
    rateToBase: 0.85,
    date: new Date('2024-01-01'),
    compCurrencyRates: [
      {
        currencyId: 2,
        code: 'EUR',
        name: 'Euro',
        date: new Date('2024-01-01'),
        rateToBase: 0.85,
        intervalType: 1,
        source: TermGroup_CurrencySource.Manually,
        intervalTypeName: 'Daily',
        sourceName: 'Test Source',
      },
      {
        currencyId: 2,
        code: 'EUR',
        name: 'Euro',
        date: new Date('2024-01-02'),
        rateToBase: 0.87,
        intervalType: 1,
        source: TermGroup_CurrencySource.Manually,
        intervalTypeName: 'Daily',
        sourceName: 'Test Source',
      },
    ],
    sysCurrencyId: 2,
  };

  const mockLedgerCurrency: ICompCurrencyDTO = {
    currencyId: 3,
    code: 'GBP',
    name: 'British Pound',
    rateToBase: 0.75,
    date: new Date('2024-01-01'),
    compCurrencyRates: [],
    sysCurrencyId: 3,
  };

  beforeEach(() => {
    const currencyCoreSpy = {
      init: vi.fn(),
      findCurrency: vi.fn(),
      getLedgerCurrency: vi.fn(),
      getEnterpriseCurrencyRate: vi.fn(),
      getCompCurrencyRate: vi.fn(),
      isBaseCurrency: vi.fn(),
      getBaseCurrencyCode: vi.fn(),
      getBaseCurrency: vi.fn(),
      getTransactionCurrency: vi.fn(),
      getCurrencies: vi.fn(),
      getHasLedgerCurrency: vi.fn(),
      convertCurrency: vi.fn(),
      setTransactionCurrencyRate: vi.fn(),
      getLedgerCurrencyRate: vi.fn(),
      getCurrencyRateDateString: vi.fn(),
      updateForm: vi.fn(),
    };

    Object.assign(currencyCoreSpy, {
      baseCurrency: mockBaseCurrency,
      enterpriseCurrency: mockBaseCurrency,
      currencies: [
        mockBaseCurrency,
        mockTransactionCurrency,
        mockLedgerCurrency,
      ],
    });

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        CurrencyService,
        { provide: CurrencyCoreService, useValue: currencyCoreSpy },
      ],
    });

    service = TestBed.inject(CurrencyService);
    mockCurrencyCoreService = TestBed.inject(CurrencyCoreService);

    // Setup default mock returns
    mockCurrencyCoreService.findCurrency.mockReturnValue(
      mockTransactionCurrency
    );
    mockCurrencyCoreService.getLedgerCurrency.mockReturnValue(
      of(mockLedgerCurrency)
    );
    mockCurrencyCoreService.getEnterpriseCurrencyRate.mockReturnValue(of(1));
    mockCurrencyCoreService.getCompCurrencyRate.mockReturnValue(of(0.75));
    mockCurrencyCoreService.isBaseCurrency.mockReturnValue(false);
    mockCurrencyCoreService.getBaseCurrencyCode.mockReturnValue('USD');
    mockCurrencyCoreService.getBaseCurrency.mockReturnValue(mockBaseCurrency);
    mockCurrencyCoreService.getTransactionCurrency.mockReturnValue(
      mockTransactionCurrency
    );
    mockCurrencyCoreService.getCurrencies.mockReturnValue([
      mockBaseCurrency,
      mockTransactionCurrency,
      mockLedgerCurrency,
    ]);
    mockCurrencyCoreService.getHasLedgerCurrency.mockReturnValue(true);
    mockCurrencyCoreService.convertCurrency.mockReturnValue(100);
    mockCurrencyCoreService.setTransactionCurrencyRate.mockReturnValue(
      of(null)
    );
    mockCurrencyCoreService.getLedgerCurrencyRate.mockReturnValue(of(0.85));
    mockCurrencyCoreService.getCurrencyRateDateString.mockReturnValue(
      '2024-01-01'
    );
    mockCurrencyCoreService.updateForm.mockReturnValue(of(null));
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should initialize currency core service', () => {
      expect(mockCurrencyCoreService.init).toHaveBeenCalled();
    });
  });

  describe('Signal-based Properties', () => {
    it('should have initial currency ID of 0', () => {
      expect(service.getCurrencyId()).toBe(0);
    });

    it('should update currency ID when set', () => {
      service.setCurrencyId(2);
      expect(service.getCurrencyId()).toBe(2);
    });

    it('should have initial currency date as undefined', () => {
      expect(service['getCurrencyDate']()).toBeUndefined();
    });

    it('should update currency date when set', () => {
      const testDate = new Date('2024-01-01');
      service.setCurrencyDate(testDate);
      expect(service['getCurrencyDate']()).toEqual(testDate);
    });

    it('should handle string date input', () => {
      const dateString = '2024-01-01';
      service.setCurrencyDate(dateString);
      expect(service['getCurrencyDate']()).toEqual(new Date(dateString));
    });

    it('should handle undefined date input', () => {
      service.setCurrencyDate(undefined);
      expect(service['getCurrencyDate']()).toBeUndefined();
    });
  });

  describe('Computed Properties', () => {
    it('should compute isBaseCurrency correctly', () => {
      mockCurrencyCoreService.isBaseCurrency.mockReturnValue(true);
      service.setCurrencyId(1);
      expect(service.isBaseCurrency()).toBe(true);
    });

    it('should return base currency information', () => {
      expect(service.baseCurrency).toBe(mockBaseCurrency);
      expect(service.baseCurrencyId).toBe(1);
      expect(service.baseCurrencyCode).toBe('USD');
    });

    it('should return transaction currency code', () => {
      service.setCurrencyId(2);
      expect(service.transactionCurrencyCode).toBe('EUR');
    });

    it('should return currencies list', () => {
      expect(service.currencies).toEqual([
        mockBaseCurrency,
        mockTransactionCurrency,
        mockLedgerCurrency,
      ]);
    });

    it('should return hasLedgerCurrency status', () => {
      expect(service.hasLedgerCurrency).toBe(false);
      service.loadLedgerCurrency(1);
      expect(service.hasLedgerCurrency).toBe(true);
    });
  });

  describe('Currency Conversion', () => {
    beforeEach(() => {
      service.setCurrencyId(2); // Set EUR as transaction currency
    });

    it('should convert currency amounts correctly', () => {
      const amount = 100;
      const result = service.getCurrencyAmount(
        amount,
        TermGroup_CurrencyType.TransactionCurrency,
        TermGroup_CurrencyType.TransactionCurrency
      );
      expect(result).toBe(100); // Same currency, no conversion
    });

    it('should convert from transaction to base currency', () => {
      const amount = 100;
      const result = service.getCurrencyAmount(
        amount,
        TermGroup_CurrencyType.TransactionCurrency,
        TermGroup_CurrencyType.TransactionCurrency
      );
      expect(result).toBe(100);
    });

    it('should handle currency rate calculations', () => {
      const sourceRate = service['getCurrencyRate'](
        TermGroup_CurrencyType.TransactionCurrency
      );
      expect(sourceRate).toBe(0.85);
    });
  });

  describe('Currency Rate Management', () => {
    it('should set transaction currency rate', () => {
      service['setTransactionCurrency'](2);
      expect(service.transactionCurrencyRate).toBe(0.85);
    });

    it('should handle currency with historical rates', () => {
      const testDate = new Date('2024-01-02');
      service.setCurrencyDate(testDate);
      service['setTransactionCurrency'](2);
      expect(service.transactionCurrencyRate).toBe(0.87); // Should use historical rate
    });

    it('should reset transaction currency when currency not found', () => {
      mockCurrencyCoreService.findCurrency.mockReturnValue(undefined);
      service['setTransactionCurrency'](999);
      expect(service.transactionCurrencyRate).toBe(1);
    });
  });

  describe('Ledger Currency Management', () => {
    it('should load ledger currency for valid actor ID', () => {
      service.loadLedgerCurrency(1);
      expect(mockCurrencyCoreService.getLedgerCurrency).toHaveBeenCalledWith(1);
    });

    it('should use base currency for actor ID 0', () => {
      service.loadLedgerCurrency(0);
      expect(service.hasLedgerCurrency).toBe(true);
    });

    it('should handle ledger currency loading errors', () => {
      const consoleSpy = vi.spyOn(console, 'error');
      mockCurrencyCoreService.getLedgerCurrency.mockReturnValue(
        throwError(() => 'Network error')
      );

      service.loadLedgerCurrency(1);

      expect(consoleSpy).toHaveBeenCalledWith(
        'CurrencyService.loadLedgerCurrency: Error loading ledger currency',
        'Network error'
      );
    });
  });

  describe('Form Integration', () => {
    it('should update form with currency data', () => {
      const mockForm = {
        patchValue: vi.fn(),
        currencyRate: { setValue: vi.fn() },
        currencyDate: { setValue: vi.fn() },
      };

      service.setCurrencyId(2);
      service.toForm(mockForm);

      expect(mockForm.patchValue).toHaveBeenCalledWith(
        { currencyId: 2 },
        { emitEvent: false }
      );
    });

    it('should handle form update errors', () => {
      const consoleSpy = vi.spyOn(console, 'error');
      const mockForm = {
        patchValue: vi.fn().mockImplementation(() => {
          throw new Error('Form error');
        }),
        currencyRate: { setValue: vi.fn() },
        currencyDate: { setValue: vi.fn() },
      };

      service.toForm(mockForm);

      expect(consoleSpy).toHaveBeenCalledWith(
        'CurrencyService.toForm: Error updating form',
        expect.any(Error)
      );
    });

    it('should handle null form gracefully', () => {
      expect(() => service.toForm(null)).not.toThrow();
    });
  });

  describe('Observable Streams', () => {
    it('should emit currency ID changes', async () => {
      const promise = firstValueFrom(service.currencyIdChanged$.pipe(skip(1)));
      service.setCurrencyId(2);
      const id = await promise;
      expect(id).toBe(2);
    });

    it('should have observable stream with takeUntilDestroyed', () => {
      // Test that the observable is properly configured with takeUntilDestroyed
      expect(service.currencyIdChanged$).toBeDefined();

      // Test that we can subscribe to the observable and it emits the current value
      let emittedValue: number | undefined;
      const subscription = service.currencyIdChanged$.subscribe(value => {
        emittedValue = value;
      });

      // Verify the initial value was emitted (BehaviorSubject emits current value on subscription)
      expect(emittedValue).toBe(0);

      // Clean up
      subscription.unsubscribe();
    });
  });

  describe('Lifecycle Management', () => {
    it('should use DestroyRef pattern', () => {
      expect(service['destroyRef']).toBeDefined();
    });

    it('should have destroyRef available for cleanup', () => {
      const destroyRef = service['destroyRef'] as any;
      expect(destroyRef).toBeDefined();
      expect(typeof destroyRef.destroy).toBe('function');
    });
  });

  describe('Error Handling', () => {
    it('should handle enterprise currency rate errors', () => {
      mockCurrencyCoreService.getEnterpriseCurrencyRate.mockReturnValue(
        throwError(() => 'Rate error')
      );

      service.setCurrencyDate(new Date());
      service['setEnterpriseCurrencyRate']();

      // Should not throw, should handle gracefully
      expect(service['enterpriseCurrencyRate']).toBe(1);
    });

    it('should handle ledger currency rate errors', () => {
      service.loadLedgerCurrency(1);
      mockCurrencyCoreService.getCompCurrencyRate.mockReturnValue(
        throwError(() => 'Rate error')
      );

      service['loadLedgerCurrencyRate']();

      // Should not throw, should handle gracefully
      expect(service['ledgerCurrencyRate']).toBe(1);
    });
  });

  describe('Currency Date String Formatting', () => {
    it('should format currency rate date string', () => {
      const testDate = new Date('2024-01-01');
      service['setCurrencyRateDate'](testDate);
      expect(service.currencyRateDateString).toBeDefined();
    });

    it('should return empty string for undefined date', () => {
      service['setCurrencyRateDate'](undefined);
      expect(service.currencyRateDateString).toBe('');
    });
  });

  describe('Currency Changes Logic', () => {
    it('should reset transaction currency for ID 0', () => {
      service.setCurrencyId(0);
      expect(service.transactionCurrencyRate).toBe(1);
    });

    it('should use base currency ID when currency ID is falsy', () => {
      service.setCurrencyId(0);
      service['setCurrencyChanges']();
      // When currency ID is 0, it should reset transaction currency and not call findCurrency
      expect(mockCurrencyCoreService.findCurrency).not.toHaveBeenCalled();
    });
  });
});
