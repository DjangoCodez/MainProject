import { TimeProjectDTO } from "../../../../Common/Models/ProjectDTO";
import { BudgetHeadDTO, BudgetRowDTO } from "../../../../Common/Models/BudgetDTOs";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { IBudgetHeadDTO } from "../../../../Scripts/TypeLite.Net4";
import { ProjectCentralBudgetRowType } from "../../../../Util/CommonEnumerations";

export class ProjectBudgetHelper {

    public overheadCostAsFixedAmount: boolean = false;
    public overheadCostAsAmountPerHour: boolean = false;

    private hoursTotalRow: BudgetRowDTO;
    private incomePersonelRow: BudgetRowDTO;
    private incomeMaterialRow: BudgetRowDTO;
    private costPersonalRow: BudgetRowDTO;
    private costMaterialRow: BudgetRowDTO;
    private budgetCostRow: BudgetRowDTO;
    private budgetOverheadPerHourRow: BudgetRowDTO;
    private budgetOverheadRow: BudgetRowDTO;

    private _hoursTotal: number = 0;
    get hoursTotal() {
        return this._hoursTotal;
    }
    set hoursTotal(item: any) {
        this._hoursTotal = item.toString().replace(",", ".");
        this.calculateBudgetTotals();
    }

    private _hoursTotalIB: number = 0;
    get hoursTotalIB() {
        return this._hoursTotalIB;
    }
    set hoursTotalIB(item: any) {
        this._hoursTotalIB = item.toString().replace(",", ".");
        this.calculateBudgetTotals();
    }

    private _incomePersonel: number = 0;
    get incomePersonel() {
        return this._incomePersonel;
    }
    set incomePersonel(item: any) {
        this._incomePersonel = item;
        this.calculateBudgetTotals();
    }

    private _incomePersonelIB: number = 0;
    get incomePersonelIB() {
        return this._incomePersonelIB;
    }
    set incomePersonelIB(item: any) {
        this._incomePersonelIB = item;
        this.calculateBudgetTotals();
    }

    private _incomeMaterial: number = 0;
    get incomeMaterial() {
        return this._incomeMaterial;
    }
    set incomeMaterial(item: any) {
        this._incomeMaterial = item;
        this.calculateBudgetTotals();
    }

    private _incomeMaterialIB: number = 0;
    get incomeMaterialIB() {
        return this._incomeMaterialIB;
    }
    set incomeMaterialIB(item: any) {
        this._incomeMaterialIB = item;
        this.calculateBudgetTotals();
    }

    private _costPersonal: number = 0;
    get costPersonal() {
        return this._costPersonal;
    }
    set costPersonal(item: any) {
        this._costPersonal = item;
        this.calculateBudgetTotals();
    }

    private _costPersonalIB: number = 0;
    get costPersonalIB() {
        return this._costPersonalIB;
    }
    set costPersonalIB(item: any) {
        this._costPersonalIB = item;
        this.calculateBudgetTotals();
    }

    private _costMaterial: number = 0;
    get costMaterial() {
        return this._costMaterial;
    }
    set costMaterial(item: any) {
        this._costMaterial = item;
        this.calculateBudgetTotals();
    }

    private _costMaterialIB: number = 0;
    get costMaterialIB() {
        return this._costMaterialIB;
    }
    set costMaterialIB(item: any) {
        this._costMaterialIB = item;
        this.calculateBudgetTotals();
    }

    private _budgetCost: number = 0;
    get budgetCost() {
        return this._budgetCost;
    }
    set budgetCost(item: any) {
        this._budgetCost = item;
        this.calculateBudgetTotals();
    }

    private _budgetCostIB: number = 0;
    get budgetCostIB() {
        return this._budgetCostIB;
    }
    set budgetCostIB(item: any) {
        this._budgetCostIB = item;
        this.calculateBudgetTotals();
    }

    private _budgetOverheadPerHour: number = 0;
    get budgetOverheadPerHour() {
        return this._budgetOverheadPerHour;
    }
    set budgetOverheadPerHour(item: any) {
        this._budgetOverheadPerHour = item;
        this.calculateBudgetTotals();
    }

    private _budgetOverheadPerHourIB: number = 0;
    get budgetOverheadPerHourIB() {
        return this._budgetOverheadPerHourIB;
    }
    set budgetOverheadPerHourIB(item: any) {
        this._budgetOverheadPerHourIB = item;
        this.calculateBudgetTotals();
    }

    private _budgetOverhead: number = 0;
    get budgetOverhead() {
        return this._budgetOverhead;
    }
    set budgetOverhead(item: any) {
        this._budgetOverhead = item;
        this.calculateBudgetTotals();
    }

    private _budgetOverheadIB: number = 0;
    get budgetOverheadIB() {
        return this._budgetOverheadIB;
    }
    set budgetOverheadIB(item: any) {
        this._budgetOverheadIB = item;
        this.calculateBudgetTotals();
    }

    incomeTotal: number = 0;
    costTotal: number = 0;

    incomeTotalIB: number = 0;
    costTotalIB: number = 0;

    private loading = false;

    private calculateBudgetTotals() {
        var hours = this.hoursTotal; //(Util.CalendarUtility.timeSpanToMinutes(this.hoursTotal) / 60);
        var hoursIB = this.hoursTotalIB; // (Util.CalendarUtility.timeSpanToMinutes(this.hoursTotalIB) / 60);

        this.incomeTotal = this.incomePersonel + this.incomeMaterial;

        if (this.overheadCostAsAmountPerHour) {
            this._budgetOverhead = hours && this.budgetOverheadPerHour ? hours * this.budgetOverheadPerHour : 0;
            this._budgetOverheadIB = hoursIB && this.budgetOverheadPerHourIB ? hoursIB * this.budgetOverheadPerHourIB : 0;
        }

        this.incomeTotalIB = this.incomePersonelIB + this.incomeMaterialIB;
     
        this.costTotal = this.costPersonal + this.costMaterial + this.budgetCost + this.budgetOverhead;
        this.costTotalIB = this.costPersonalIB + this.costMaterialIB + this.budgetCostIB + this.budgetOverheadIB;
    }

    public setupBudgetValues(project: TimeProjectDTO, loading: boolean) {
        return
        this.loading = loading;

        if (project.budgetHead && project.budgetHead.rows) {
            _.forEach(project.budgetHead.rows, (row) => {
                if (row.type === ProjectCentralBudgetRowType.BillableMinutesInvoiced) {
                    this.hoursTotalRow = row;
                    this._hoursTotal = row.totalAmount / 60; //Util.CalendarUtility.minutesToTimeSpan(row.totalAmount);
                }
                if (row.type === ProjectCentralBudgetRowType.IncomePersonellTotal) {
                    this.incomePersonelRow = row;
                    this._incomePersonel = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.IncomeMaterialTotal) {
                    this.incomeMaterialRow = row;
                    this._incomeMaterial = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostPersonell) {
                    this.costPersonalRow = row;
                    this._costPersonal = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostMaterial) {
                    this.costMaterialRow = row;
                    this._costMaterial = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostExpense) {
                    this.budgetCostRow = row;
                    this._budgetCost = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHour) {
                    this.budgetOverheadPerHourRow = row;
                    this._budgetOverheadPerHour = row.totalAmount;
                }

                if (row.type === ProjectCentralBudgetRowType.OverheadCost) {
                    this.budgetOverheadRow = row;
                    this.budgetOverhead = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.IncomeTotal) {
                    this.incomeTotal = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostTotal) {
                    this.costTotal = row.totalAmount;
                }

                //IB
                if (row.type === ProjectCentralBudgetRowType.BillableMinutesInvoicedIB) {
                    this._hoursTotalIB = row.totalAmount / 60; //Util.CalendarUtility.minutesToTimeSpan(row.totalAmount);
                }
                if (row.type === ProjectCentralBudgetRowType.IncomePersonellTotalIB) {
                    this._incomePersonelIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.IncomeMaterialTotalIB) {
                    this._incomeMaterialIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostPersonellIB) {
                    this._costPersonalIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostMaterialIB) {
                    this._costMaterialIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostExpenseIB) {
                    this._budgetCostIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHourIB) {
                    this._budgetOverheadPerHourIB = row.totalAmount;
                }

                if (row.type === ProjectCentralBudgetRowType.OverheadCostIB) {
                    this.budgetOverheadIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.IncomeTotalIB) {
                    this.incomeTotalIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.CostTotalIB) {
                    this.costTotalIB = row.totalAmount;
                }
                if (row.type === ProjectCentralBudgetRowType.OverheadCostIB) {
                    row.totalAmount = this.budgetOverheadIB;
                    row.isModified = true;
                }

            });

            this.calculateBudgetTotals();
            this.checkIBRowExistens(project.budgetHead);
        }
        else {
            if (!project.budgetHead) {
                project.budgetHead = new BudgetHeadDTO();
                project.budgetHead.actorCompanyId = CoreUtility.actorCompanyId;
                project.budgetHead.name = "Proj";
            }

            var rows: BudgetRowDTO[] = [];
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.BillableMinutesInvoiced));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomePersonellTotal));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomeMaterialTotal));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomeTotal));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostPersonell));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostMaterial));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostExpense));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.OverheadCostPerHour));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.OverheadCost));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostTotal));

            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.BillableMinutesInvoicedIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomePersonellTotalIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomeMaterialTotalIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.IncomeTotalIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostPersonellIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostMaterialIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostExpenseIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.OverheadCostPerHourIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.OverheadCostIB));
            rows.push(this.createBudgetRow(ProjectCentralBudgetRowType.CostTotalIB));

            project.budgetHead.rows = rows;
        }

        this.loading = false;
    }

    public saveCurrenValuesToRows(budgetHead: IBudgetHeadDTO) {
        _.forEach(budgetHead.rows, (row: any) => {
            const hasTimeCode = row.timeCodeId && row.timeCodeId > 0;
            if (row.type === ProjectCentralBudgetRowType.BillableMinutesInvoiced) {
                var amount: number = this.hoursTotal * 60;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount; //Util.CalendarUtility.timeSpanToMinutes(this.hoursTotal);
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.BillableMinutesTotal) {
                var amount: number = this.hoursTotal * 60;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount; //Util.CalendarUtility.timeSpanToMinutes(this.hoursTotal);
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomePersonellTotal) {
                if (row.totalAmount !== this.incomePersonel) {
                    row.totalAmount = this.incomePersonel;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomeMaterialTotal) {
                if (row.totalAmount !== this.incomeMaterial) {
                    row.totalAmount = this.incomeMaterial;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomeTotal) {
                var amount: number = this.incomePersonel + this.incomeMaterial;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostPersonell && !hasTimeCode) {
                if (row.totalAmount !== this.costPersonal) {
                    row.totalAmount = this.costPersonal;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostMaterial && !hasTimeCode) {
                if (row.totalAmount !== this.costMaterial) {
                    row.totalAmount = this.costMaterial;
                    row.isModified = true;
                }
            }
            if (hasTimeCode) {
                row.isModified = true;
            }
            if (row.type === ProjectCentralBudgetRowType.CostExpense) {
                if (row.totalAmount !== this.budgetCost) {
                    row.totalAmount = this.budgetCost;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHour) {
                if (row.totalAmount !== this.budgetOverheadPerHour) {
                    row.totalAmount = this.budgetOverheadPerHour;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.OverheadCost) {
                if (row.totalAmount !== this.budgetOverhead) {
                    row.totalAmount = this.budgetOverhead;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostTotal) {
                var amount: number = this.costPersonal + this.costMaterial + this.budgetCost;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount;
                    row.isModified = true;
                }
            }

            //IB
            if (row.type === ProjectCentralBudgetRowType.BillableMinutesInvoicedIB) {
                var amount: number = this.hoursTotalIB * 60;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount; // Util.CalendarUtility.timeSpanToMinutes(this.hoursTotalIB);
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.BillableMinutesTotalIB) {
                var amount: number = this.hoursTotalIB * 60;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount;  //Util.CalendarUtility.timeSpanToMinutes(this.hoursTotalIB);
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomePersonellTotalIB) {
                if (row.totalAmount !== this.incomePersonelIB) {
                    row.totalAmount = this.incomePersonelIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomeMaterialTotalIB) {
                if (row.totalAmount !== this.incomeMaterialIB) {
                    row.totalAmount = this.incomeMaterialIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.IncomeTotalIB) {
                var amount: number = this.incomePersonelIB + this.incomeMaterialIB;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostPersonellIB) {
                if (row.totalAmount !== this.costPersonalIB) {
                    row.totalAmount = this.costPersonalIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostMaterialIB) {
                if (row.totalAmount !== this.costMaterialIB) {
                    row.totalAmount = this.costMaterialIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostExpenseIB) {
                if (row.totalAmount !== this.budgetCostIB) {
                    row.totalAmount = this.budgetCostIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHourIB) {
                if (row.totalAmount !== this.budgetOverheadPerHourIB) {
                    row.totalAmount = this.budgetOverheadPerHourIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.OverheadCostIB) {
                if (row.totalAmount !== this.budgetOverheadIB) {
                    row.totalAmount = this.budgetOverheadIB;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.CostTotalIB) {
                var amount: number = this.costPersonalIB + this.costMaterialIB + this.budgetCostIB;
                if (row.totalAmount !== amount) {
                    row.totalAmount = amount;
                    row.isModified = true;
                }
            }
            if (row.type === ProjectCentralBudgetRowType.OverheadCostIB) {
                if (row.totalAmount !== this.budgetOverheadIB) {
                    row.totalAmount = this.budgetOverheadIB;
                    row.isModified = true;
                }
            }
        });
    }

    public checkIBRowExistens(budgetHead: IBudgetHeadDTO) {
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.BillableMinutesInvoicedIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.IncomePersonellTotalIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.IncomeMaterialTotalIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.IncomeTotalIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.CostPersonellIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.CostMaterialIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.CostExpenseIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.OverheadCostPerHourIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.OverheadCostIB);
        this.addRowIfNotExisting(budgetHead, ProjectCentralBudgetRowType.CostTotalIB);
    }

    private createBudgetRow(rowType: ProjectCentralBudgetRowType) {
        var row = new BudgetRowDTO();
        row.type = rowType;
        return row;
    }

    private addRowIfNotExisting(budgetHead: IBudgetHeadDTO, type: ProjectCentralBudgetRowType) {
        if (!budgetHead.rows.some(x => x.type === type))
            budgetHead.rows.push(this.createBudgetRow(type));
    }
}