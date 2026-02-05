import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CoreService } from '@shared/services/core.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { Feature } from './shared/models/generated-interfaces/Enumerations';
import { take } from 'rxjs';
import { SpaNavigationService } from '@shared/services/spa-navigation.service';

@Component({
  selector: 'soe-root',
  templateUrl: './app.component.html',
  standalone: false,
})
export class AppComponent {
  private readonly coreService = inject(CoreService);
  private readonly navService = inject(SpaNavigationService);

  title = 'SoftOne GO';
  isChromeless = false;
  showMessageMenu = false;
  informationMenuPosition = 1;
  helpMenuPosition = 2;
  academyMenuPosition = 3;
  messageMenuPosition = 4;
  reportMenuPosition = 5;
  documentMenuPosition = 6;
  hasLoadedTranslations = false;

  constructor(translate: TranslateService) {
    this.isChromeless = window.softOneSpa?.isChromeless || false;

    // Language
    const lang = SoeConfigUtil.language;
    translate.setFallbackLang(lang);
    translate.use(lang);

    this.loadReadOnlyPermissions();
    translate.onLangChange.subscribe(() => {
      this.hasLoadedTranslations = true;
    });
  }

  loadReadOnlyPermissions() {
    const features: Feature[] = [Feature.Communication_XEmail];
    this.coreService
      .hasReadOnlyPermissions(features)
      .pipe(take(1))
      .subscribe(permission => {
        this.showMessageMenu = permission[Feature.Communication_XEmail];
        this.setRightMenuPositions();
      });
  }

  setRightMenuPositions() {
    if (!this.showMessageMenu) {
      this.reportMenuPosition = 4;
      this.documentMenuPosition = 5;
    }
  }
}
