import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';
import { AngularJsLegacyType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: 'report-menu',
    templateUrl: './report-menu.component.html',
    styleUrls: ['./report-menu.component.scss'],
    standalone: false
})
export class ReportMenuComponent extends RightMenuBaseComponent {
  constructor(protected translate: TranslateService) {
    super(translate);
    this.setToggleTooltip('core.reportmenu.title');
  }

  openLegacyMenu(): void {
    super.openLegacyMenu(AngularJsLegacyType.RightMenu_Report);
  }
}
