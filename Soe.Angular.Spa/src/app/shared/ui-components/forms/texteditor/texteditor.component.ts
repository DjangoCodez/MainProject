// https://github.com/KillerCodeMonkey/ngx-quill

// TODO: Translations, look here for a solution maybe?
// https://dsebastien.medium.com/dynamically-customizing-quill-ngx-quill-editors-in-an-angular-application-b81c75bc4a6

import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  OnInit,
  ViewEncapsulation,
  computed,
  input,
  output,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { LabelComponent } from '@ui/label/label.component';
import { QuillModule as NgxQuillModule } from 'ngx-quill';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'soe-texteditor',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgxQuillModule,
    LabelComponent,
    TranslatePipe,
  ],
  templateUrl: './texteditor.component.html',
  styleUrls: ['./texteditor.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: TexteditorComponent,
    },
  ],
  encapsulation: ViewEncapsulation.None,
})
export class TexteditorComponent
  extends ValueAccessorDirective<string>
  implements OnInit, AfterViewInit
{
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  hasLabel = computed(() => {
    return (
      this.labelKey() ||
      this.secondaryLabelKey() ||
      this.secondaryLabelPrefixKey() ||
      this.secondaryLabelPostfixKey()
    );
  });
  placeholderKey = input('');
  width = input(0);
  minHeight = input(100);
  maxHeight = input(500);

  // Toolbar
  showFontFormat = input(true);
  showBlock = input(true);
  showSubSuper = input(true);
  showBullet = input(true);
  showIndent = input(true);
  showAlign = input(true);
  showFont = input(true);
  showFontSize = input(true);
  showHeading = input(true);
  showColor = input(true);
  showClean = input(true);
  showLink = input(true);

  valueChange = output<string>();

  modules: any = {
    toolbar: [],
  };

  onValueChange = (value: string) => this.valueChange.emit(value);

  ngOnInit(): void {
    super.ngOnInit();

    this.buildToolbar();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      const editor = document.querySelector('.ql-editor') as HTMLElement;
      if (editor) {
        editor.style.minHeight = this.minHeight() + 'px';
        editor.style.maxHeight = this.maxHeight() + 'px';
      }
    }, 200);
  }

  private buildToolbar() {
    if (this.showFontFormat())
      this.modules.toolbar.push(['bold', 'italic', 'underline', 'strike']); // Toggled buttons
    if (this.showBlock())
      this.modules.toolbar.push(['blockquote', 'code-block']);
    if (this.showSubSuper())
      this.modules.toolbar.push([{ script: 'sub' }, { script: 'super' }]); // Superscript/subscript
    if (this.showBullet())
      this.modules.toolbar.push([{ list: 'bullet' }, { list: 'ordered' }]);
    if (this.showIndent())
      this.modules.toolbar.push([{ indent: '+1' }, { indent: '-1' }]); // Indent/outdent
    if (this.showAlign()) this.modules.toolbar.push([{ align: [] }]);
    if (this.showFont()) this.modules.toolbar.push([{ font: [] }]);
    if (this.showFontSize())
      this.modules.toolbar.push([{ size: ['small', false, 'large', 'huge'] }]); // Custom dropdown
    if (this.showHeading())
      this.modules.toolbar.push([{ header: [1, 2, 3, 4, 5, 6, false] }]);
    if (this.showColor())
      this.modules.toolbar.push([{ color: [] }, { background: [] }]); // Dropdown with defaults from theme
    if (this.showClean()) this.modules.toolbar.push(['clean']); // Remove formatting button
    if (this.showLink()) this.modules.toolbar.push(['link', 'image']); // Link and image
  }
}
