import { Component, input } from '@angular/core';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { CreatedModified } from './models/created-modified';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'created-modified',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './created-modified.component.html',
  styleUrls: ['./created-modified.component.scss'],
})
export class CreatedModifiedComponent {
  readonly currentLanguage = SoeConfigUtil.language;
  model = input.required<CreatedModified | any>();
}
