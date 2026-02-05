import { IReportDataSelectionDTO } from './generated-interfaces/ReportDataDTO';

type Selection = SelectionDTO | SelectionDTO[];

export abstract class SelectionDTO implements IReportDataSelectionDTO {
  key: string = '';
  typeName: string = '';
}

export class SelectionCollection {
  private selections: Map<string, Selection> = new Map<string, Selection>();

  public get length(): number {
    return this.selections.size;
  }

  public upsert(key: string, selection: Selection) {
    this.selections.set(key, selection);
  }

  public materialize(): Array<SelectionDTO> {
    const output: Array<SelectionDTO> = [];

    for (const [key, entry] of this.selections) {
      if (Array.isArray(entry)) {
        for (let i = 0; i < entry.length; i++) {
          output.push(
            this.prepareSelection(`${key}_${i.toString()}`, entry[i])
          );
        }
      } else {
        output.push(this.prepareSelection(key, entry));
      }
    }

    return output;
  }

  private prepareSelection(key: string, selection: SelectionDTO): SelectionDTO {
    if (selection) selection.key = key;
    return selection;
  }
}
