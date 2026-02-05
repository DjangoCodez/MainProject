import { DestroyRef, inject, Injectable } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { tap } from 'rxjs';

@Injectable()
export class AccountUrlParamsService {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  public accountDimId: number = 0;
  public isAccountStd: boolean = false;

  readonly params$ = this.route.queryParamMap
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(params => {
        this.accountDimId = Number(params.get('dim'));
        this.isAccountStd = params.get('isaccountstd') === 'true';
      })
    )
    .subscribe();

  public destroy() {
    this.params$.unsubscribe();
  }
}
