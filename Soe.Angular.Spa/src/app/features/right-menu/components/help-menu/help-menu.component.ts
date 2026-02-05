import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';
import { AngularJsLegacyType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: 'help-menu',
    templateUrl: './help-menu.component.html',
    styleUrls: ['./help-menu.component.scss'],
    standalone: false
})
export class HelpMenuComponent extends RightMenuBaseComponent {
  constructor(protected translate: TranslateService) {
    super(translate);
    this.setToggleTooltip('core.help');
  }

  openLegacyMenu(): void {
    super.openLegacyMenu(AngularJsLegacyType.RightMenu_Help);
  }
}
