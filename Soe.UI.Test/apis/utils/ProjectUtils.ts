import { type Page, expect } from '@playwright/test';
import * as allure from "allure-js-commons";
import { BillingAPI } from '../BillingAPI';


export class ProjectUtils {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly billingAPI: BillingAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.billingAPI = new BillingAPI(page, url);
    }

    private highestInLowestSet(arr: number[]): number {
        if (arr.length === 0) throw new Error('Empty array');
        let current = arr[0];
        for (let i = 1; i < arr.length; i++) {
            if (arr[i] === current + 1) {
                current = arr[i];
            } else {
                break;
            }
        }
        return current;
    }


    async addProject(projectName: string, uniqueId: number) {
        await allure.step('API:Add project', async () => {

            const response = await this.billingAPI.getProjects();
            expect(response.ok()).toBeTruthy();
            await this.page.waitForTimeout(1000);
            let project = await response.json();
            let maxNumber: number = 0;
            if (project.length > 0) {
                let ids = [...project.map((p: any) => Number(p.number))];
                ids = ids.sort((a, b) => a - b);
                maxNumber = this.highestInLowestSet(ids);
                console.log('Max project number:', maxNumber);
            }
            let newProjectNumber = maxNumber + 1 + uniqueId;
            const fs = require('fs');
            const filePath = this.basePathJsons + 'project.json';
            const rawData = fs.readFileSync(filePath);
            const jsonData = JSON.parse(rawData);
            jsonData.invoiceProject.name = projectName;
            jsonData.invoiceProject.number = newProjectNumber;
            const responseProject = await this.billingAPI.createProject(jsonData);
            expect(responseProject.ok()).toBeTruthy();
        });
    }

    async getProjectNumberByName(projectName: string): Promise<number | null> {
        return await allure.step(`API:Get project number for ${projectName}`, async () => {
            const response = await this.billingAPI.getProjects();
            expect(response.ok()).toBeTruthy();
            const projects = await response.json();
            const found = projects.find((p: any) => p.name === projectName);
            if (!found) {
                console.warn(`Project "${projectName}" not found`);
                return null;
            }
            console.log(`Project "${projectName}" number:`, found.number);
            return Number(found.number);
        });
    }

}