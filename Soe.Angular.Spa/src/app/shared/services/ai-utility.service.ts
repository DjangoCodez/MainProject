import { computed, inject, Injectable, signal } from '@angular/core';
import { SoeHttpClient } from './http.service';
import { Observable } from 'rxjs';
import { ProgressService } from './progress';
import {
  getProfessionalizedText,
  getTranslationSuggestions,
} from './generated-service-endpoints/core/AIUtility.endpoints';
import { CoreService } from './core.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Perform } from '@shared/util/perform.class';

@Injectable({
  providedIn: 'root',
})
export class AIUtilityService {
  private readonly http = inject(SoeHttpClient);
  private readonly coreService = inject(CoreService);
  private readonly progressService = inject(ProgressService);

  private readonly perform = new Perform<any>(this.progressService);

  public hasPermission = signal(false);
  public isDoingWork = computed(() => this.perform.inProgress());

  constructor() {
    this.coreService
      .hasModifyPermissions([Feature.Common_AI])
      .subscribe(result => {
        this.hasPermission.set(!!result[Feature.Common_AI]);
      });
  }

  doLoad<T>(obs: Observable<T>) {
    return this.perform.load$(obs) as Observable<T>;
  }

  getTranslationSuggestions(originalText: string, languages: number[]) {
    return this.doLoad(
      this.http.get<{ [key: number]: string }>(
        getTranslationSuggestions(originalText, languages.toString())
      )
    );
  }

  professionalizeText(text: string) {
    return this.doLoad(this.http.get<string>(getProfessionalizedText(text)));
  }
}
