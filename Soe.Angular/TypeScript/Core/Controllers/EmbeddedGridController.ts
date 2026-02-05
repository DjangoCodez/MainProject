import { IGridHandlerFactory } from "../Handlers/gridhandlerfactory";
import { IGridHandlerAg } from "../Handlers/GridHandlerAg";

export class EmbeddedGridController {
    public gridAg: IGridHandlerAg;

    constructor(
        gridHandlerFactory: IGridHandlerFactory, gridName: string) {
        this.gridAg = gridHandlerFactory.create(gridName, "agGrid") as IGridHandlerAg;
    }
}