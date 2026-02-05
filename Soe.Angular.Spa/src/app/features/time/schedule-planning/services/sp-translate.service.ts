import { inject, Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root',
})
export class SpTranslateService {
  translateService = inject(TranslateService);

  shiftDefined = signal('');
  shiftUndefined = signal('');
  shiftsDefined = signal('');
  shiftsUndefined = signal('');
  // bookingDefined = signal('');
  // bookingUndefined = signal('');
  // bookingsDefined = signal('');
  // bookingsUndefined = signal('');

  constructor() {
    this.loadTerms();
  }

  private loadTerms(): void {
    this.translateService
      .get([
        'time.schedule.planning.shiftdefined',
        'time.schedule.planning.shiftundefined',
        'time.schedule.planning.shiftsdefined',
        'time.schedule.planning.shiftsundefined',
      ])
      .subscribe(terms => {
        this.shiftDefined.set(terms['time.schedule.planning.shiftdefined']);
        this.shiftUndefined.set(terms['time.schedule.planning.shiftundefined']);
        this.shiftsDefined.set(terms['time.schedule.planning.shiftsdefined']);
        this.shiftsUndefined.set(
          terms['time.schedule.planning.shiftsundefined']
        );
      });
  }
}
