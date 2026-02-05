import { computed, inject, Injectable, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SoeAccountDistributionType } from '@shared/models/generated-interfaces/Enumerations';

@Injectable()
export class AccountDistributionUrlParamsService {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  type = signal(
    String(
      this.router.routerState.snapshot.root.queryParams['type'] ??
        this.route.snapshot.queryParams['type']
    ).toLowerCase()
  );
  isPeriod = computed(() => this.type() === 'period');
  isAuto = computed(() => this.type() === 'auto');
  isWriteOff = computed(() => !this.isPeriod() && !this.isAuto());

  typeId = computed(() => {
    switch (this.type()) {
      case 'period':
        return SoeAccountDistributionType.Period;
      case 'auto':
        return SoeAccountDistributionType.Auto;
      default:
        return SoeAccountDistributionType.Inventory_WriteOff;
    }
  });
}
