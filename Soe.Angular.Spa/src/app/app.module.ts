import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';
import {
  HttpClient,
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { TranslateLoader, provideTranslateService } from '@ngx-translate/core';
import { createCommonTranslateLoader } from '@shared/localization/term-loader';
import { APP_BASE_HREF } from '@angular/common';
import { AppComponent } from './app.component';
import { AuthInterceptor } from '@core/interceptors/auth.interceptor';
import { CoreModule } from '@core/core.module';
import { AppRoutingModule } from './app-routing.module';
import { ToastrModule } from 'ngx-toastr';

/** Global Prototype Utils */
import '@core/utils/prototype-array-extensions';
import '@core/utils/prototype-date-extensions';
import '@core/utils/prototype-number-extensions';
import '@core/utils/prototype-string-extensions';

import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSliderModule } from '@angular/material/slider';
import { MatTabsModule } from '@angular/material/tabs';
import {
  MAT_TOOLTIP_DEFAULT_OPTIONS,
  MatTooltipDefaultOptions,
  MatTooltipModule,
} from '@angular/material/tooltip';

import { DateTransformInterceptor } from '@core/interceptors/date-transform.interceptor';
import { RightMenuModule } from './features/right-menu/right-menu.module';
import {
  AngularJsLegacyType,
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { StringTransformInterceptor } from '@core/interceptors/string-transform.interceptor';
import { LocaleService } from '@shared/services/locale.service';
import { RouteReuseStrategy } from '@angular/router';
import { NoReuseRoutingStrategy } from '@core/strategies/no-reuse-routing.strategy';
import { provideAnimations } from '@angular/platform-browser/animations';
import { HttpErrorInterceptor } from '@core/interceptors/http-error.interceptor';

// Custom options to configure the Material tooltip
const tooltipDefaults: MatTooltipDefaultOptions = {
  showDelay: 1000,
  hideDelay: 0,
  touchendHideDelay: 0,
  position: 'above',
  positionAtOrigin: true,
  disableTooltipInteractivity: true,
};

@NgModule({
  declarations: [AppComponent],
  bootstrap: [AppComponent],
  imports: [
    CoreModule,
    BrowserModule,
    AppRoutingModule,
    RightMenuModule,
    MatDialogModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSliderModule,
    MatTabsModule,
    MatTooltipModule,
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right',
      enableHtml: true,
      preventDuplicates: true,
      countDuplicates: true,
      resetTimeoutOnDuplicate: true,
      includeTitleDuplicates: true,
    }),
  ],
  providers: [
    provideTranslateService({
      loader: {
        provide: TranslateLoader,
        useFactory: createCommonTranslateLoader,
        deps: [HttpClient],
      },
      fallbackLang: 'sv-SE',
    }),
    { provide: APP_BASE_HREF, useValue: '/' },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: StringTransformInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: DateTransformInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpErrorInterceptor,
      multi: true,
    },
    {
      provide: LOCALE_ID,
      deps: [LocaleService],
      useFactory: (LocaleService: { locale: string }) => LocaleService.locale,
    },
    { provide: MAT_TOOLTIP_DEFAULT_OPTIONS, useValue: tooltipDefaults },
    provideHttpClient(withInterceptorsFromDi()),
    { provide: RouteReuseStrategy, useClass: NoReuseRoutingStrategy },
    // TODO: ngx-toaster does not support Angular 20 animations yet
    provideAnimations(),
  ],
})
export class AppModule {}

declare global {
  interface Window {
    softOneSpa: {
      isChromeless: boolean;
      feature: Feature;
      module: SoeModule;
    };
    ajsLegacy: {
      type: AngularJsLegacyType;
    };
  }
}
