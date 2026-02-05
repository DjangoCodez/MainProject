import { ISaveUserGridStateModel } from './generated-interfaces/CoreModels';

export class SaveUserGridStateModel implements ISaveUserGridStateModel {
  grid: string;
  gridState: string;

  constructor(grid: string, gridState: string) {
    this.grid = grid;
    this.gridState = gridState;
  }
}
