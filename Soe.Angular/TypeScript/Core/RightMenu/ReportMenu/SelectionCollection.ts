import { SelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";

type Selection = (SelectionDTO | SelectionDTO[]);

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

        const mapIterator = this.selections.entries();
        let current = mapIterator.next();
        while (!current.done) {
            const [key, entry] = current.value;
            current = mapIterator.next();

            if (entry instanceof Array) {
                const selections = <SelectionDTO[]>entry;
                for (var i = 0; i < selections.length; i++) {
                    output.push(this.prepareSelection("{0}_{1}".format(key, i.toString()), selections[i]));
                }
            } else {
                output.push(this.prepareSelection(key, <SelectionDTO>entry));
            }
        }

        return output;
    }

    private prepareSelection(key: string, selection: SelectionDTO): SelectionDTO {
        if (selection)
            selection.key = key;
        return selection;
    }
}