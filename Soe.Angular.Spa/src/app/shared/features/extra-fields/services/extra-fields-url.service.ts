import { DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { tap } from 'rxjs';

@Injectable()
export class ExtraFieldsUrlParamsService {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  entityType = signal<number>(this.route.snapshot.queryParams['entity'] || 0);

  readonly params$ = this.route.queryParamMap
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(params => {
        this.entityType.set(Number(params.get('entity')));
      })
    )
    .subscribe();
}
