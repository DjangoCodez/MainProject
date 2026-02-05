import { type Page } from '@playwright/test';
import { TimeAPI } from '../TimeAPI';


export class ScheduleUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly timeAPI: TimeAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.timeAPI = new TimeAPI(page, url);
    }

    async removeSchedule(shiftIds: number[]) {
        const fs = require('fs');
        const filePath = this.basePathJsons + 'delete-shifts.json';
        const rawData = fs.readFileSync(filePath);
        const jsonData = JSON.parse(rawData);
        jsonData.shiftIds = shiftIds;
        console.log('Removing schedule with shift IDs:', jsonData.shiftIds);
        await this.timeAPI.removeSchedule(jsonData);
    }

}