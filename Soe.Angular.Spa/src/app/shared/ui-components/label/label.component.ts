
import { Component, input } from '@angular/core';
import { faAsterisk } from '@fortawesome/pro-light-svg-icons';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';

@Component({
  selector: 'soe-label',
  imports: [IconModule, TranslatePipe],
  templateUrl: './label.component.html',
  styleUrls: ['./label.component.scss'],
})
export class LabelComponent {
  labelKey = input('');
  labelLowercase = input(false);
  labelCentered = input(false);
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  secondaryLabelLowercase = input(false);
  inline = input(false);
  labelClass = input('');
  labelValue = input('');
  tooltipKey = input('');
  isRequired = input(false);
  forRef = input('');
  applyMinHeight = input(false);

  readonly faAsterisk = faAsterisk;
}
