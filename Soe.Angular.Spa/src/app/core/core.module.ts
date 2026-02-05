import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './components/header/header.component';
import { HomeComponent } from './components/home/home.component';

@NgModule({
  declarations: [HeaderComponent, HomeComponent],
  exports: [HeaderComponent, HomeComponent],
  imports: [CommonModule],
})
export class CoreModule {}
