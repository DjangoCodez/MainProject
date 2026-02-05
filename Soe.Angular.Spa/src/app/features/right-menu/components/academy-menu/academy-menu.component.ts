import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { RightMenuBaseComponent } from '../right-menu-base/right-menu-base.component';

@Component({
    // eslint-disable-next-line @angular-eslint/component-selector
    selector: 'academy-menu',
    templateUrl: './academy-menu.component.html',
    styleUrls: ['./academy-menu.component.scss'],
    standalone: false
})
export class AcademyMenuComponent extends RightMenuBaseComponent {
  constructor(protected translate: TranslateService) {
    super(translate);
    this.setToggleTooltip('core.academymenu.title');
  }

  openSoftOneAcademy() {
    const url = this.translate.instant('core.help.softoneacademy.url');
    window.open(url, '_blank');
  }
}
