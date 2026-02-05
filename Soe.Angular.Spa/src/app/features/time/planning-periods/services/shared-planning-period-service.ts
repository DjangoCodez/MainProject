import { Injectable, signal } from "@angular/core";

@Injectable({providedIn: 'root'})

export class SharedPlanningPeriodService {
  data = signal<any>('init');
  constructor() {}
  
  setData(data: any) {
    this.data.update(() => data);
  }
  
  getData():any {
    return this.data;
  }
  
}   
