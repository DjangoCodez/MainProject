import { SoeEntityState } from "../../../Util/CommonEnumerations";

export class TestCaseSettingDTO {
    testCaseSettingId: number;
    testCaseId: number;
    testCaseGroupId: number;
    type: TestCaseSettingType;
    stringValue: string;
    jsonValue: string;
    intValue: number;
    boolValue: boolean;
    decimalValue: number;
    state: SoeEntityState;
    created: Date;
    modified: Date;
    settingType: TestCaseSettingTypeDTO;

    constructor(testCaseGroupId?: number, testCaseId?: number, type?: TestCaseSettingType) {
        this.testCaseSettingId = 0;
        this.testCaseGroupId = testCaseGroupId ? testCaseGroupId : undefined;
        this.testCaseId = testCaseId ? testCaseId : undefined;
        this.type = type ? type : 0;
        this.state = 0;
    }
}

export class TestCaseGroupMappingDTO {
    testCaseGroupMappingId: number;
    sort: number;
    testCaseId: number;
    testCaseGroupId: number;
    state: SoeEntityState;
    testCaseDTO: TestCaseDTO;
    testCaseGroupDTO: TestCaseGroupDTO;

    constructor(testCaseGroupId?: number, testCaseId?: number) {
        this.testCaseGroupMappingId = 0;
        this.sort = 0;
        this.testCaseId = testCaseId ? testCaseId : 0;
        this.testCaseGroupId = testCaseGroupId ? testCaseGroupId : 0;
        this.state = SoeEntityState.Active;
    }
}

export class TestCaseGroupDTO {
    testCaseGroupId: number;
    name: string;
    description: string;
    cron: string;
    state: SoeEntityState;
    executeTime: Date;
    testCaseSettingDTOs: TestCaseSettingDTO[];
    testCaseGroupMappingDTOs: TestCaseGroupMappingDTO[];

    constructor() {
        this.testCaseGroupId = undefined;
        this.name = "";
        this.description = "";
        this.executeTime = undefined;
        this.state = SoeEntityState.Active;
        this.testCaseGroupMappingDTOs = [];
        this.testCaseSettingDTOs = [];
    }
}

export class TestCaseGroupResultDTO {
    testCaseResults: any[];
}

export class TestCaseDTO {
    testCaseId: number;
    name: string;
    description: string;
    testCaseType: TestCaseType;
    seleniumType: SeleniumType;
    seleniumBrowsers: number[];
    testCaseSettingDTOs: TestCaseSettingDTO[];
    testCaseGroupMappingDTOs: TestCaseGroupMappingDTO[];
    state: SoeEntityState;

    constructor() {
        this.testCaseId = undefined;
        this.name = "";
        this.description = "";
        this.testCaseType = 0;
        this.testCaseSettingDTOs = [];
        this.testCaseGroupMappingDTOs = [];
        this.state = SoeEntityState.Active;
    }
}

export class TestCaseSettingTypeDTO {
    type: number
    name: string;
    isInt: boolean;
    isBool: boolean;
    isSelect: boolean;
    isString: boolean;
    isJson: boolean;
    alternatives: TestCaseSettingAlternativeDTO[];
}

export class TestCaseSettingAlternativeDTO {
    id: number;
    name: string;
}

export class TestCaseResultDTO {

}

export class TestCaseStepResultDTO {

}

export enum TestCaseSettingType {
    Unknown = 0,
    SeleniumType = 1,
    SeleniumBrowser = 2,
    StartUrl = 3,
    Domain = 4,
    Password = 5,
    Username = 6,
    Capabilities = 7,
    RequestBody = 8,
    ProjectName = 9,
    ScenarioName = 10,
    RunInOneSession = 11,
    AppiumType = 12,
    AppiumPlatform = 13,
}

export enum TestCaseType {
    Unknown = 0,
    Selenium = 1,
    Unit = 2,
    LoadNinja = 3,
    Appium = 4,
}

//1 - 3000 HR
//3001 - 6000 Sales
//6001 - 9000 Economy
export enum SeleniumType {
    Unknown = 0,

    //HR
    Login = 1,
    OpenPlanning = 2,
    OpenTimeAttest = 3,

    //Sales
    OrderList = 3001,
    ProductGroup = 3002,
    CreateOrder = 3003,
    EditOrder = 3004,
    OrderRevTax = 3005,
    OrderChecklist = 3006,
    OrderToInvoiceLyft = 3007,
    OrderCopy = 3008,
    OrderFixedPriceLyft = 3009,
    InvoiceCreateCredit = 3010,


    //Economy

    //SoftOne Online
    OnlineLogin = 12001
}

export enum SeleniumBrowser {
    UnSpecified = 0,
    Chrome = 1,
    Firefox = 2,
    BrowserStack = 3,
}