import { Injectable } from '@angular/core';
import { ProjectCentralSummaryDTO } from '../models/project-central.model';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProjectCentralDataService {
  private _projectCentralData = new BehaviorSubject<ProjectCentralSummaryDTO>(
    new ProjectCentralSummaryDTO()
  );

  get projectCentralData$() {
    return this._projectCentralData.asObservable();
  }

  setData(data: ProjectCentralSummaryDTO): void {
    this._projectCentralData.next(data);
  }
}
