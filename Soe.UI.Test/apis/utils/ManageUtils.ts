import { Page } from "@playwright/test";
import { ManageAPI } from "../ManageAPI";
import fs from 'fs'


export class ManageUtils {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly manageApi: ManageAPI
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.manageApi = new ManageAPI(page, url);
    }

    async verifyOrderCheckLists() {
        const response = await this.manageApi.getCheckLists();
        let checkLists = await response.json();
        checkLists = checkLists.filter(
            (cl: { typeName: string }) =>
                cl.typeName === "Order"
        );
        if (!checkLists.every((cl: { defaultInOrder: boolean }) => cl.defaultInOrder === false)) {
            throw new Error("Some checklists have defaultOrder set to true");
        }
    }

    async createCheckList(checkListData: any = {}) {
        const response = await this.manageApi.getCheckLists();
        let checkLists = await response.json() as [{name:string}]
        const checklist = checkLists.find(c => c.name === checkListData.name)
        if (!checklist) {
            const keys = Object.keys(checkListData)
            const filePath = this.basePathJsons + 'checklist.json';
            const rawData = fs.readFileSync(filePath, 'utf-8');
            const data = JSON.parse(rawData);
            for (let key of keys) {
                data[key] = checkListData[key]
            }
            await this.manageApi.createCheckList(data)
        }
    }

}