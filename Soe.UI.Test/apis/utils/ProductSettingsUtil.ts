import { type Page } from "@playwright/test";
import { BillingAPI } from "../BillingAPI";
import { defaultProductGroupDTO } from "../models/productGroupDefault";
import { defaultProductUnitDTO, defaultProductUnitModel } from "../models/productUnitDefault";

export class ProductSettingsUtil {
  readonly page: Page;
  readonly dominaUrl: string;
  readonly billingAPI: BillingAPI;
  readonly basePathJsons: string = "./apis/jsons/";

  constructor(page: Page, url: string) {
    this.page = page;
    this.dominaUrl = url;
    this.billingAPI = new BillingAPI(page, url);
  }

  async getProductGroups() {
    let productGroups = await this.billingAPI.getProductGroups();
    return productGroups;
  }

  async createProductGroup(name: string, code: string) {
    const newProductGroup = { ...defaultProductGroupDTO };
    newProductGroup.code = code;
    newProductGroup.name = name;
    await this.billingAPI.createProductGroup(
      newProductGroup
    );
  }

  async getProductUnits() {
    let productunits = await this.billingAPI.getProductUnits();
    return productunits;
  }

  async createProductUnit(name: string, code: string) {
    const newProductUnit = { ...defaultProductUnitDTO };
    newProductUnit.code = code;
    newProductUnit.name = name;
    const newProductUnitInput = { ...defaultProductUnitModel };
    newProductUnitInput.productUnit = newProductUnit;
    newProductUnitInput.translations = [];
    await this.billingAPI.createProductUnit(
      newProductUnitInput
    );
  }
}
