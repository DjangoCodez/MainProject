import { Page } from "@playwright/test";



export class ManageAPI {

    readonly page: Page;
    readonly dominaUrl: string;

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url + "/api/";
    }


    async getCheckLists() {
        const checkLists = await this.page.request.get(this.dominaUrl + "Manage/Registry/Checklists/ChecklistHeads/");
        return checkLists;
    }

    async createCheckList(jsonData:any) {
        const response = await this.page.request.post(this.dominaUrl + "Manage/Registry/Checklists/ChecklistHead/", { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create checklist : ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

}