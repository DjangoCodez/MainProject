import { type Page } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import { defaultContractGroupDTO } from '../models/AgreementGroupDefault';
import { TimeAPI } from 'apis/TimeAPI';
import fs from 'fs';
import { setJsonValues } from 'utils/CommonUtil';


export class AgreementUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly billingAPI: BillingAPI;
    readonly timeAPI: TimeAPI;
    readonly basePathJsons: string = './apis/jsons/';


    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.billingAPI = new BillingAPI(page, url);
        this.timeAPI = new TimeAPI(page, url);
    }

    async getAgreementGroups() {
        return await this.billingAPI.getAgreementsGroups();;
    }

    async createAgreement(contractGroupId: string, actorId: string, productId: string, internalText: string) {
        const filePath = this.basePathJsons + 'agreement.json';
        const rawData = fs.readFileSync(filePath, 'utf-8');
        const data = JSON.parse(rawData);
        data.modifiedFields.actorid = actorId;
        data.modifiedFields.contractgroupid = contractGroupId;
        data.modifiedFields.customerinvoicerows[0].productId = productId;
        data.modifiedFields.origindescription = internalText;
        data.modifiedFields.nextcontractperioddate = new Date().toISOString();
        const response = await this.billingAPI.createAgreement(data);
        return response;
    }

    async getAgreementGroupsPriceManagement() {
        let agreementsGroups = await this.billingAPI.getAgreementsGroups();
        return agreementsGroups;
    }

    async createAgreementGroup(name: string, period: string, dayInMonth: number, interval: number) {
        const newAgreementGroup = { ...defaultContractGroupDTO };
        newAgreementGroup.description = `Test Description`;
        newAgreementGroup.name = name;
        const periods = await this.billingAPI.getAgreementsGroupsPeriod();
        newAgreementGroup.period = periods.find((p: { name: string; }) => p.name === period)?.id || periods[0].id;
        const priceManagementOptions = await this.billingAPI.getAgreementsGroupsPriceManagement();
        newAgreementGroup.priceManagement = priceManagementOptions[0].id;
        newAgreementGroup.dayInMonth = dayInMonth;
        newAgreementGroup.interval = interval;
        const agreementGroupResponse = await this.billingAPI.createAgreementGroup(newAgreementGroup);
        console.log('Agreement group is created new (API)');
        return agreementGroupResponse;
    }

    async getTimeAgreements() {
        interface TimeAgreement {
            employeeGroupId: number;
            name: string;
            timeDeviationCausesNames: string;
            dayTypesNames: string;
            timeReportType: number;
            timeReportTypeName: string;
            state: number;
        }
        const agreements = await this.timeAPI.getTimeAgreements();
        return agreements as TimeAgreement[];
    }

    private async getTimeAgreementByName(name: string) {
        const agreements = await this.getTimeAgreements();
        return agreements.find((agreement: { name: string; }) => agreement.name === name);
    }

    async createTimeAgreement(timeData: any = {}) {
        const keys = Object.keys(timeData)
        const existingAgreement = await this.getTimeAgreementByName(timeData.name)
        if (existingAgreement == undefined) {
            const filePath = this.basePathJsons + 'time-agreement.json';
            const rawData = fs.readFileSync(filePath, 'utf-8');
            const data = JSON.parse(rawData);
            for (let key of keys) {
                setJsonValues(data, key, timeData[key])
            }
            const { integerValue, success } = await this.timeAPI.createTimeAgreement(data)
            console.log(`Time agreement created with ID: ${integerValue},${success}`);
            return { timeAgreementId: integerValue, timeAgreement: data }
        } else {
            console.log(`Time agreement with name ${timeData.name} already exists`)
            return { timeAgreementId: existingAgreement.employeeGroupId, timeAgreement: existingAgreement }
        }
    }

}