import { Component, input, model, OnInit } from '@angular/core';
import { NgStyle } from '@angular/common';
import { AnimationProp } from '@fortawesome/angular-fontawesome';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { LinebreakPipe } from '@shared/pipes';
import { IconUtil } from '@shared/util/icon-util';
import { IconModule } from '@ui/icon/icon.module';

export type InstructionType =
  | 'success'
  | 'info'
  | 'warning'
  | 'error'
  | 'plain';

@Component({
  selector: 'soe-instruction',
  imports: [IconModule, LinebreakPipe, TranslatePipe, NgStyle],
  templateUrl: './instruction.component.html',
  styleUrls: ['./instruction.component.scss'],
})
export class InstructionComponent implements OnInit {
  type = input<InstructionType>('info');
  text = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = model<IconName | undefined>(undefined);
  iconClass = input('');
  iconSpin = input(false);
  showIcon = input(false);
  showClose = input(false);
  inline = input(false);
  fitToContent = input(false);
  textStyles = input<Record<string, string>>({});

  icon: IconProp | undefined;
  animation: AnimationProp | undefined;

  ngOnInit() {
    if (!this.iconName()) this.setDefaultIconName();

    if (this.iconName())
      this.icon = IconUtil.createIcon(this.iconPrefix(), this.iconName());

    if (this.iconSpin()) this.animation = 'spin';
  }

  private setDefaultIconName() {
    switch (this.type()) {
      case 'success':
        this.iconName.set('check-circle');
        break;
      case 'info':
        this.iconName.set('info-circle');
        break;
      case 'warning':
        this.iconName.set('exclamation-circle');
        break;
      case 'error':
        this.iconName.set('exclamation-triangle');
        break;
    }
  }
}
