import { ISoeGridOptions, SoeGridOptions, GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { SoeGridOptionsEvent, IconLibrary } from "../../../Util/Enumerations";
import { EmployeeVehicleDTO, EmployeeVehicleDeductionDTO, EmployeeVehicleEquipmentDTO, EmployeeVehicleTaxDTO } from "../../../Common/Models/EmployeeVehicleDTO";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IEmployeeService } from "../EmployeeService";
import { TermGroup_VehicleType, TermGroup_SysVehicleFuelType, Feature, TermGroup, TermGroup_SysPayrollPrice } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private employeeVehicleId: number;

    // Data
    private modelCode: string;
    private employeeVehicle: EmployeeVehicleDTO;
    private employeeVehicleAmounts: EmployeeVehicleAmounts;
    private tabTitle: any;

    // Lookups
    private employees: ISmallGenericType[];
    private years: any;
    private vehicleMakes: any;
    private vehicleModels: any;
    private fuelTypes: ISmallGenericType[];

    private vehicleEquipmentReadMoreUrl: string;
    private comparableModelReadMoreUrl: string;

    private typeExpanderInitiallyOpen: boolean = false;
    private modelExpanderInitiallyOpen: boolean = false;
    private sixYearCar: boolean = false;
    // Progress
    private loadingMakes: boolean = false;
    private loadingModels: boolean = false;

    // Subgrids
    protected taxGridOptions: ISoeGridOptions;
    protected taxGridButtonGroups = new Array<ToolBarButtonGroup>();
    protected equipmentGridOptions: ISoeGridOptions;
    protected equipmentGridButtonGroups = new Array<ToolBarButtonGroup>();
    protected netSalaryDeductionGridOptions: ISoeGridOptions;
    protected netSalaryDeductionGridButtonGroups = new Array<ToolBarButtonGroup>();

    // Properties
    get licensePlateNumber() {
        if (this.employeeVehicle && this.employeeVehicle.licensePlateNumber)
            return this.employeeVehicle.licensePlateNumber;
        else
            return "";
    }
    set licensePlateNumber(value: string) {
        if (!this.employeeVehicle)
            return;

        if (value !== undefined && value !== null)
            this.employeeVehicle.licensePlateNumber = value.toUpperCase();
        else
            this.employeeVehicle.licensePlateNumber = value;
    }

    private _selectedEmployee;

    get selectedEmployee(): ISmallGenericType {
        return this._selectedEmployee;
    }
    set selectedEmployee(item: ISmallGenericType) {
        this._selectedEmployee = item;

        this.employeeVehicle.employeeId = this._selectedEmployee ? this._selectedEmployee.id : 0;
    }

    private _selectedYear = new Date().getFullYear();
    get selectedYear(): number {
        return this._selectedYear;
    }
    set selectedYear(value: number) {
        this._selectedYear = value;

        this.employeeVehicle.year = this._selectedYear ? this._selectedYear : new Date().getFullYear();
        this.employeeVehicleAmounts.year = this.employeeVehicle.year;       
    }

    get vehicleType(): TermGroup_VehicleType {
        if (this.employeeVehicle && this.employeeVehicle.type)
            return this.employeeVehicle.type;
        else
            return TermGroup_VehicleType.Car;
    }
    set vehicleType(value: TermGroup_VehicleType) {
        if (!this.employeeVehicle)
            return;

        this.employeeVehicle.type = value;
        this.loadVehicleMakes();
    }

    private get vehicleTypeValue(): string {
        return this.vehicleType.toString();
    }
    private set vehicleTypeValue(value: string) {
        this.vehicleType = parseInt(value);
    }

    get manufacturingYear(): number {
        if (this.employeeVehicle && this.employeeVehicle.manufacturingYear) {
            this.sixYearCar = this.selectedYear - this.employeeVehicle.manufacturingYear >= 6 ?? false;

            return this.employeeVehicle.manufacturingYear;
        }
        else
            return new Date().getFullYear();
    }
    set manufacturingYear(value: number) {
        if (!this.employeeVehicle)
            return;

        this.employeeVehicle.manufacturingYear = value;

        this.loadVehicleMakes();
    }

    get registeredDate(): Date {
        return this.employeeVehicle?.registeredDate;
    }
    set registeredDate(value: Date) {
        this.employeeVehicle.registeredDate = value;
        this.employeeVehicleAmounts.registeredDate = value;
    }

   
    get calculateDate(): Date {
        return this.employeeVehicle?.fromDate ? this.employeeVehicle?.fromDate : CalendarUtility.getDateToday();
    }

    get vehicleMake() {
        if (this.employeeVehicle && this.employeeVehicle.vehicleMake)
            return this.employeeVehicle.vehicleMake;
        else
            return "";
    }
    set vehicleMake(value: string) {
        if (!this.employeeVehicle)
            return;

        this.employeeVehicle.vehicleMake = value;
        this.loadVehicleModels();
    }

    get vehicleModel() {
        if (this.employeeVehicle && this.employeeVehicle.modelCode)
            return this.employeeVehicle.modelCode;
        else
            return "";
    }
    set vehicleModel(value) {
        if (!this.employeeVehicle)
            return;

        // value = model code
        var model = _.find(this.vehicleModels, { id: value });
        if (model != null) {
            this.employeeVehicle.modelCode = value;
            this.employeeVehicle.vehicleModel = model['name'];

            if (this.modelCode !== value) {
                this.modelCode = value;
                this.getVehicleByCode();
            }
        }
    }

    private get fuelTypeText(): string {
        if (this.employeeVehicle && this.employeeVehicle.fuelType && this.fuelTypes) {
            var type = _.find(this.fuelTypes, f => f.id === this.employeeVehicle.fuelType);
            if (type)
                return type.name;
        }

        return '';
    }

    // El- och laddhybridbilar, som kan laddas från elnätet, samt gasbilar (ej gasol) justeras först till en jämförbar bil utan miljöteknik.
    // Därefter sätts förmånsvärdet ner med 40 procent, max 16 000 kronor för inkomstår 2012-2016 och max 10 000 kronor från och med inkomstår 2017.
    // Detta gäller endast om bilen har ett nybilspris som är högre än närmast jämförbara bil.

    // Etanolbilar, elhybridbilar, som inte kan laddas från elnätet, och bilar som kan köras på gasol, rapsmetylester
    // samt övriga typer av miljöanpassade drivmedel justeras enbart ner till jämförbar bil.

    // Reglerna är tidsbegränsade och gäller till och med inkomståret 2020.

    get isEcoCar(): boolean {
        return (this.selectedYear <= 2020 && this.employeeVehicle &&
            (this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.Electricity ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.ElectricHybrid ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.PlugInHybrid ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.Alcohol ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.Gas));
    }

    get isDeductableEcoCar(): boolean {
        return (this.selectedYear <= 2020 && this.employeeVehicle &&
            (this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.Electricity ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.PlugInHybrid ||
                this.employeeVehicle.fuelType === TermGroup_SysVehicleFuelType.Gas));
    }

    get hasEquipment() {
        if (this.employeeVehicle && this.employeeVehicle.hasEquipment)
            return this.employeeVehicle.hasEquipment;
        else
            return false;
    }
    set hasEquipment(value: boolean) {
        if (!this.employeeVehicle)
            return;

        this.employeeVehicle.hasEquipment = value;
        this.calculateEquipmentSum();
    }

    get hasExtensiveDriving() {
        if (this.employeeVehicle && this.employeeVehicle.hasExtensiveDriving)
            return this.employeeVehicle.hasExtensiveDriving;
        else
            return false;
    }
    set hasExtensiveDriving(value: boolean) {
        if (!this.employeeVehicle)
            return;

        this.employeeVehicle.hasExtensiveDriving = value;
        this.employeeVehicleAmounts.hasExtensiveDriving = value;
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private employeeService: IEmployeeService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.employeeVehicleAmounts = new EmployeeVehicleAmounts();
        this.initRowsGrid();

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.setupLookups())
            .onSetUpGUI(() => this.setupGrid())
            .onLoadData(() => this.load())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    private initRowsGrid() {
        // Equipment
        this.equipmentGridOptions = new SoeGridOptions("Time.Employee.Vehicle.Equipment", this.$timeout, this.uiGridConstants);
        this.equipmentGridOptions.enableFiltering = false;
        this.equipmentGridOptions.enableGridMenu = false;
        this.equipmentGridOptions.showGridFooter = false;
        this.equipmentGridOptions.setMinRowsToShow(5);
        var eqEvent: GridEvent = new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (rowEntity, colDef, newValue, oldValue) => {
            if (colDef.field == "price" || colDef.field == 'fromDate' || colDef.field == 'toDate') {
                this.calculateEquipmentSum();
            }
        });
        this.equipmentGridOptions.subscribe([eqEvent]);

        // Deduction
        this.netSalaryDeductionGridOptions = new SoeGridOptions("Time.Employee.Vehicle.NetSalaryDeduction", this.$timeout, this.uiGridConstants);
        this.netSalaryDeductionGridOptions.enableFiltering = false;
        this.netSalaryDeductionGridOptions.enableGridMenu = false;
        this.netSalaryDeductionGridOptions.showGridFooter = false;
        this.netSalaryDeductionGridOptions.setMinRowsToShow(5);
        var netEvent: GridEvent = new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (rowEntity, colDef, newValue, oldValue) => {
            this.calculateDeductionSum();
        });
        this.netSalaryDeductionGridOptions.subscribe([netEvent]);

        // Tax
        this.taxGridOptions = new SoeGridOptions("Time.Employee.Vehicle.Tax", this.$timeout, this.uiGridConstants);
        this.taxGridOptions.enableFiltering = false;
        this.taxGridOptions.enableGridMenu = false;
        this.taxGridOptions.showGridFooter = false;
        this.taxGridOptions.setMinRowsToShow(5);

        var taxEvent: GridEvent = new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (rowEntity, colDef, newValue, oldValue) => {
            this.calculateTax();
        });
        this.taxGridOptions.subscribe([taxEvent]);
    }

    public setupGrid() {
        this.setupSubGrids();
    }

    public onInit(parameters: any) {
        this.employeeVehicleId = parameters.id || 0;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;

        this.flowHandler.start([{ feature: Feature.Time_Employee_Vehicles_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_Vehicles_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Vehicles_Edit].modifyPermission;
    }

    protected setupLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => {
                return this.$q.all([
                    this.loadTerms(),
                    this.loadEmployees(),
                    this.loadYears(),
                    this.loadFuelTypes(),
                    this.getBaseAmount(),
                    this.getGovernmentLoanInterest()]);
            }
        ]);
    }

    private setupToolBar() {
        if (this.modifyPermission) {
            //Equipment grid
            this.equipmentGridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addEquipment();
            })));

            // Net salary deduction grid
            this.netSalaryDeductionGridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addNetSalaryDeduction();
            })));

            // Tax grid
            this.taxGridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addTax();
            })));
        }
    }

    private setupSubGrids() {
        var keys: string[] = [
            "time.employee.vehicle.employeevehicle",
            "common.description",
            "common.fromdate",
            "common.todate",
            "common.amount",
            "core.delete",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            // Equipment
            this.equipmentGridOptions.addColumnText("description", terms['common.description'], null);
            this.equipmentGridOptions.addColumnDate("fromDate", terms["common.fromdate"], "20%");
            this.equipmentGridOptions.addColumnDate("toDate", terms["common.todate"], "20%");
            this.equipmentGridOptions.addColumnNumber("price", terms["common.amount"], "20%", false, 0);
            if (this.modifyPermission)
                this.equipmentGridOptions.addColumnDelete(terms["core.delete"], "deleteEquipment");
            _.forEach(this.equipmentGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                if (colDef['soeType'] !== Constants.GRID_COLUMN_TYPE_ICON) {
                    colDef.enableCellEdit = true;
                }
            });

            // Deduction
            this.netSalaryDeductionGridOptions.addColumnDate("fromDate", terms["common.fromdate"], null);
            this.netSalaryDeductionGridOptions.addColumnNumber("price", terms["common.amount"], null, false, 0);
            if (this.modifyPermission)
                this.netSalaryDeductionGridOptions.addColumnDelete(terms["core.delete"], "deleteNetSalaryDeduction");
            _.forEach(this.netSalaryDeductionGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableCellEdit = true;
            });

            // Tax
            this.taxGridOptions.addColumnDate("fromDate", terms["common.fromdate"], null);
            this.taxGridOptions.addColumnNumber("amount", terms["common.amount"], null, false, 0);
            if (this.modifyPermission)
                this.taxGridOptions.addColumnDelete(terms["core.delete"], "deleteTax");
            _.forEach(this.taxGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableCellEdit = true;
            });

            this.tabTitle = terms["time.employee.vehicle.employeevehicle"];
        });

        this.setupToolBar();
    }
    
    // LOOKUPS
    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.employeeVehicleId > 0) {
            this.employeeService.getEmployeeVehicle(this.employeeVehicleId, true, true, true, true).then(x => {
                this.isNew = false;
                this.employeeVehicle = x;
                this.populate();

                this.messagingHandler.publishSetTabLabel(this.guid, this.tabTitle + ' ' + this.employeeVehicle.licensePlateNumber);

                deferral.resolve();
            });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.employee.vehicle.equipment.readmore.url",
            "time.employee.vehicle.comparablemodel.url"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.vehicleEquipmentReadMoreUrl = terms["time.employee.vehicle.equipment.readmore.url"];
            this.comparableModelReadMoreUrl = terms["time.employee.vehicle.comparablemodel.url"];
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.employeeVehicleId);

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.employeeVehicleId, recordId => {
            if (recordId !== this.employeeVehicleId) {
                this.employeeVehicleId = recordId;
                this.load();
            }
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.employeeService.getEmployeesDict(false, true, false, false).then(x => {
            this.employees = x;
        });
    }

    private loadYears(): ng.IPromise<any> {
        return this.employeeService.getSysVehicleManufacturingYears().then(x => {
            this.years = [];
            _.forEach(x, (year) => {
                this.years.push({ id: year, name: year });
            });
            if (this.years.length > 0) {
                this.manufacturingYear = this.years[0]['id'];
            }
        });
    }

    private loadFuelTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysVehicleFuelType, false, false).then(x => {
            this.fuelTypes = x;
        });
    }

    private loadVehicleMakes() {
        if (this.vehicleType === TermGroup_VehicleType.Unknown || this.manufacturingYear === 0)
            return;

        this.loadingMakes = true;
        this.employeeService.getSysVehicleMakes(<number>this.vehicleType, this.manufacturingYear).then(x => {
            this.vehicleMakes = [];
            _.forEach(x, make => {
                this.vehicleMakes.push({ id: make, name: make });
            });

            if (this.employeeVehicle.vehicleMake)
                this.loadVehicleModels();

            if (this.employeeVehicle && this.employeeVehicle.modelCode)
                this.vehicleModel = this.employeeVehicle.modelCode;

            this.loadingMakes = false;
        });
    }

    private loadVehicleModels() {
        if (this.vehicleType === TermGroup_VehicleType.Unknown || this.manufacturingYear === 0 || this.vehicleMake === "")
            return;

        this.loadingModels = true;
        this.employeeService.getSysVehicleModels(<number>this.vehicleType, this.manufacturingYear, this.vehicleMake).then(x => {
            this.vehicleModels = [];
            _.forEach(x, model => {
                this.vehicleModels.push({ id: model['field1'], name: model['field2'] });
            });

            // If using the model code search, set model after loading it
            if (this.modelCode) {
                let model = _.find(this.vehicleModels, v => v['id'] === this.modelCode);
                if (model)
                    this.vehicleModel = this.modelCode;
            }

            this.loadingModels = false;
        });
    }
    private fromDateChanged() {
        this.$timeout(() => {
            this.calculateTax();
            this.calculateDeductionSum();
            this.calculateEquipmentSum();
        },100);
    }
    private registeredDateChanged() {
        this.$timeout(() => {
            this.getVehicleByCode();
        }, 100);
    }

    private getVehicleByCode() {
        if (!this.modelCode)
            return;

        this.employeeService.getSysVehicleByCode(this.modelCode).then(x => {
            if (x) {
                this.manufacturingYear = x.manufacturingYear;
                this.vehicleMake = x.vehicleMake;
                this.vehicleModel = x.vehicleModel;
                this.employeeVehicle.fuelType = x.fuelType;
                this.employeeVehicleAmounts.isDeductableEcoCar = this.isDeductableEcoCar;
                this.employeeVehicleAmounts.price = x.price;
                this.employeeVehicleAmounts.priceAdjustment = x.priceAdjustment;
                this.employeeVehicleAmounts.priecAfterReduction = x.priceAfterReduction;
                this.employeeVehicle.codeForComparableModel = x.codeForComparableModel;

                if (this.registeredDate && this.registeredDate.isSameOrAfterOnDay(new Date(2022, 6, 1))) {
                    this.employeeVehicleAmounts.priceAdjustment = 0; //No adjustment after 2022-07-01
                    this.employeeVehicleAmounts.comparablePrice = x.price;
                    this.employeeVehicleAmounts.listReduction = this.getReduction(x.fuelType);
                }
                else
                    this.employeeVehicleAmounts.comparablePrice = x.comparableModel ? x.comparableModel.price : 0;

                this.employeeVehicleAmounts.sixYearCar = this.sixYearCar;
            } 
        });
    }

    private getBaseAmount(): ng.IPromise<any> {
        return this.employeeService.getSysPayrollPriceAmount(TermGroup_SysPayrollPrice.SE_BaseAmount, CalendarUtility.getFirstDayOfYear()).then(x => {
            this.employeeVehicleAmounts.baseAmount = x;
        });
    }

    private getGovernmentLoanInterest(): ng.IPromise<any> {
        return this.employeeService.getSysPayrollPriceAmount(TermGroup_SysPayrollPrice.SE_GovernmentLoanInterest, CalendarUtility.getFirstDayOfYear()).then(x => {
            this.employeeVehicleAmounts.governmentLoanInterest = x * 100;
        });
    }

    // EVENTS
    protected loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.setupToolBar()]);
    }

    // ACTIONS
    public save() {
        // Convert properties for save
        this.employeeVehicle.hasExtensiveDriving = this.employeeVehicle.hasExtensiveDriving === true ? true : false;
        this.employeeVehicle.price = this.employeeVehicleAmounts.price;
        this.employeeVehicle.priceAdjustment = this.employeeVehicleAmounts.priceAdjustment;
        this.employeeVehicle.comparablePrice = this.employeeVehicleAmounts.comparablePrice;
        this.employeeVehicle.benefitValueAdjustment = this.employeeVehicleAmounts.benefitValueAdjustment;

        if (this.employeeVehicle.deduction) {
            _.forEach(this.employeeVehicle.deduction, (deduction) => {
                deduction.price = NumberUtility.parseDecimal(deduction.price.toString());
            });
        }

        if (this.employeeVehicle.equipment) {
            _.forEach(this.employeeVehicle.equipment, (equipment) => {
                equipment.price = NumberUtility.parseDecimal(equipment.price.toString());
            });
        }

        if (this.employeeVehicle.tax) {
            _.forEach(this.employeeVehicle.tax, (t) => {
                t.amount = NumberUtility.parseDecimal(t.amount.toString());
            });
        }

        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmployeeVehicle(this.employeeVehicle).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {

                        if (this.employeeVehicleId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, "{0} - {1}".format(this.employeeVehicle.licensePlateNumber, this.selectedEmployee.name)));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.employeeVehicleId = result.integerValue;
                        this.employeeVehicle.employeeVehicleId = result.integerValue;

                        this.toolbar.setSelectedRecord(this.employeeVehicle.employeeVehicleId);
                    }
                    this.messagingHandler.publishSetTabLabel(this.guid, this.tabTitle + ' ' + this.employeeVehicle.licensePlateNumber);
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeeVehicle);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.employeeService.getEmployeeVehicles(true, true, true, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.employeeVehicleId, "{0} - {1}".format(row.licensePlateNumber, row.employeeName)));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.employeeVehicleId) {
                    this.employeeVehicleId = recordId;
                    this.load();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmployeeVehicle(this.employeeVehicleId).then((result) => {
                if (result.success) {
                    completion.completed(this.employeeVehicle, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS
    private new() {
        this.typeExpanderInitiallyOpen = true;
        this.modelExpanderInitiallyOpen = true;
        this.isNew = true;
        this.employeeVehicleId = 0;
        this.employeeVehicle = new EmployeeVehicleDTO();
        this.selectedYear = new Date().getFullYear();
        this.employeeVehicle.deduction = [];
        this.employeeVehicle.equipment = [];
        this.employeeVehicle.tax = [];

        this.vehicleType = this.employeeVehicle.type;
        this.loadVehicleModels();

        this.populateDeduction();
        this.populateEquipment();
        this.populateTax();
    }

    protected copy() {
        super.copy();
        this.isNew = true;
        this.employeeVehicleId = 0;
        this.employeeVehicle.employeeVehicleId = 0;
        this.employeeVehicle.deduction = [];
        this.employeeVehicle.equipment = [];
        this.employeeVehicle.tax = [];

        _.forEach(this.employeeVehicle.deduction, deduction => {
            deduction.employeeVehicleDeductionId = 0;
            deduction.employeeVehicleId = 0;
        });
        _.forEach(this.employeeVehicle.equipment, equipment => {
            equipment.employeeVehicleEquipmentId = 0;
            equipment.employeeVehicleId = 0;
        });
        _.forEach(this.employeeVehicle.tax, tax => {
            tax.employeeVehicleTaxId = 0;
            tax.employeeVehicleId = 0;
        });
    }

    private populate() {
        if (!this.employeeVehicle) {
            this.new();
            return;
        }

        this.employeeVehicleId = this.employeeVehicle.employeeVehicleId;
        this.selectedEmployee = _.find(this.employees, e => e.id == this.employeeVehicle.employeeId);
        this.selectedYear = this.employeeVehicle.year;

        this.vehicleType = this.employeeVehicle.type;
        // Model code starts with manufacturing year (two digits)
        this.employeeVehicle.manufacturingYear = this.employeeVehicle.modelCode ? Number(this.employeeVehicle.modelCode.substr(0, 2)) + 2000 : new Date().getFullYear();
        this.manufacturingYear = this.employeeVehicle.manufacturingYear;
        this.vehicleMake = this.employeeVehicle.vehicleMake;
        this.vehicleModel = this.employeeVehicle.modelCode;
        this.modelCode = this.employeeVehicle.modelCode;
        this.registeredDate = this.employeeVehicle.registeredDate;
        this.hasExtensiveDriving = this.employeeVehicle.hasExtensiveDriving;
        this.employeeVehicleAmounts.benefitValueAdjustment = this.employeeVehicle.benefitValueAdjustment;

        this.employeeVehicleAmounts.isDeductableEcoCar = this.isDeductableEcoCar;
        this.employeeVehicleAmounts.price = this.employeeVehicle.price;
        this.employeeVehicleAmounts.priceAdjustment = this.employeeVehicle.priceAdjustment;
        this.employeeVehicleAmounts.comparablePrice = this.employeeVehicle.comparablePrice;

        this.populateDeduction();
        this.populateEquipment();
        this.populateTax();
    }

    private populateDeduction() {
        if (!this.employeeVehicle)
            return;

        // Populate grid
        this.netSalaryDeductionGridOptions.setData(this.employeeVehicle.deduction);
        this.calculateDeductionSum();
    }

    private calculateDeductionSum() {
        var deductionSum: number = 0;

        if (this.employeeVehicle.deduction && this.employeeVehicle.deduction.length > 0) {
            _.forEach(this.employeeVehicle.deduction, (deduction) => {
                var fromDate: Date = null;
                if (deduction.fromDate)
                    fromDate = CalendarUtility.convertToDate(deduction.fromDate).beginningOfMonth();
                if (!fromDate || fromDate.isSameOrBeforeOnDay(this.calculateDate))
                    deductionSum += Number(deduction.price);
            });
        }

         this.employeeVehicleAmounts.deductionSum = deductionSum;
    }

    private populateEquipment() {
        if (!this.employeeVehicle)
            return;

        // Populate grid
        this.equipmentGridOptions.setData(this.employeeVehicle.equipment);
        this.employeeVehicle.hasEquipment = !!(this.employeeVehicle.equipment && this.employeeVehicle.equipment.length > 0);
        this.calculateEquipmentSum();
    }

    private calculateEquipmentSum() {
        var equipmentSum: number = 0;

        if (this.hasEquipment && this.employeeVehicle.equipment && this.employeeVehicle.equipment.length > 0) {
            _.forEach(this.employeeVehicle.equipment, (equipment) => {
                var fromDate: Date = null;
                if (equipment.fromDate)
                    fromDate = CalendarUtility.convertToDate(equipment.fromDate).beginningOfMonth();
                var toDate: Date;
                if (equipment.toDate)
                    toDate = CalendarUtility.convertToDate(equipment.toDate).endOfMonth();

                if ((!fromDate || fromDate.isSameOrBeforeOnDay(this.calculateDate)) && (!toDate || toDate.isSameOrAfterOnDay(CalendarUtility.getDateToday())))
                    equipmentSum += Number(equipment.price);
            });
        }

        this.employeeVehicleAmounts.equipmentSum = equipmentSum;
    }

    private populateTax() {
        if (!this.employeeVehicle)
            return;

        // Populate grid
        this.taxGridOptions.setData(this.employeeVehicle.tax);
        this.calculateTax();
    }
    
    private calculateTax() {
        var taxAmount: number = 0;

        if (this.employeeVehicle.tax && this.employeeVehicle.tax.length > 0) {
            let taxes = _.filter(this.employeeVehicle.tax, t => !t.fromDate || t.fromDate.isSameOrBeforeOnDay(this.calculateDate));

            if (taxes.length === 1) {
                // Only one tax, use it.
                taxAmount = taxes[0]?.amount ?? 0;
            } else {
                // More than one taxes, only use the ones with dates
                taxes = _.orderBy(_.filter(taxes, t => t.fromDate), t => t.fromDate, 'desc');
                taxAmount = taxes[0]?.amount ?? 0;
            }
        }

        this.employeeVehicleAmounts.taxAmount = Number(taxAmount);
    }

    private addNetSalaryDeduction() {
        var row = new EmployeeVehicleDeductionDTO();
        this.netSalaryDeductionGridOptions.addRow(row, true);
    }

    protected deleteNetSalaryDeduction(row) {
        this.netSalaryDeductionGridOptions.deleteRow(row);
        this.dirtyHandler.setDirty();
        this.calculateDeductionSum();
    }

    private addEquipment() {
        var row = new EmployeeVehicleEquipmentDTO();
        row.employeeVehicleId = this.employeeVehicleId;
        this.equipmentGridOptions.addRow(row, true);
    }

    protected deleteEquipment(row) {
        this.equipmentGridOptions.deleteRow(row);
        this.dirtyHandler.setDirty();
        this.calculateEquipmentSum();
    }

    private addTax() {
        var row = new EmployeeVehicleTaxDTO();
        this.taxGridOptions.addRow(row, true);
    }

    protected deleteTax(row) {
        this.taxGridOptions.deleteRow(row);
        this.dirtyHandler.setDirty();
        this.calculateTax();
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeeVehicle) {
                // Mandatory fields
                if (!this.employeeVehicle.employeeId)
                    mandatoryFieldKeys.push("common.employee");
                if (!this.employeeVehicle.licensePlateNumber)
                    mandatoryFieldKeys.push("time.employee.vehicle.licenseplatenumber");
            }
        });
    }

    private getReduction(type: TermGroup_SysVehicleFuelType) {
        if (this.registeredDate.isSameOrAfterOnDay(new Date(2022, 6, 1))) {
            switch (type) {
                case TermGroup_SysVehicleFuelType.Electricity:
                case TermGroup_SysVehicleFuelType.HydrogenGas:
                    return 350000;
                case TermGroup_SysVehicleFuelType.PlugInHybrid:
                    return 140000;
                case TermGroup_SysVehicleFuelType.Gas:
                    return 100000;

                default:
                    return 0;
            }
        } else {
            return 0;
        }

    }
}

export class EmployeeVehicleAmounts {
    constructor() {
    }

    private yearValue: number = new Date().getFullYear();
    get year(): number {
        return this.yearValue;
    }
    set year(value: number) {
        this.yearValue = value;

        this.calculate();
    }

    private baseAmountValue: number = 0;
    get baseAmount(): number {
        return this.baseAmountValue;
    }
    set baseAmount(value: number) {
        this.baseAmountValue = value;

        this.calculate();
    }

    private governmentLoanInterestValue: number = 0;
    get governmentLoanInterest(): number {
        return this.governmentLoanInterestValue;
    }
    set governmentLoanInterest(value: number) {
        this.governmentLoanInterestValue = value;

        this.calculate();
    }

    private registeredDateValue: Date;
    get registeredDate(): Date {
        return this.registeredDateValue;
    }
    set registeredDate(value: Date) {
        this.registeredDateValue = value;

        this.calculate();
    }

    private priceValue: number = 0;
    get price(): number {
        return this.priceValue;
    }
    set price(value: number) {
        this.priceValue = value;

        this.calculate();
    }

    private comparablePriceValue: number = 0;
    get comparablePrice(): number {
        return this.comparablePriceValue;
    }
    set comparablePrice(value: number) {
        this.comparablePriceValue = value;

        this.calculate();
    }

    private priceAdjustmentValue: number = 0;
    get priceAdjustment(): number {
        return this.priceAdjustmentValue;
    }
    set priceAdjustment(value: number) {
        this.priceAdjustmentValue = value;

        this.calculate();
    }

    private priecAfterReductionValue: number = 0;
    get priecAfterReduction(): number {
        return this.priecAfterReductionValue;
    }
    set priecAfterReduction(value: number) {
        this.priecAfterReductionValue = value;
    }
    

    private isDeductableEcoCarValue: boolean = false;
    get isDeductableEcoCar(): boolean {
        return this.isDeductableEcoCarValue;
    }
    set isDeductableEcoCar(value: boolean) {
        this.isDeductableEcoCarValue = value;

        this.calculate();
    }

    private deductionSumValue: number = 0;
    get deductionSum(): number {
        return this.deductionSumValue;
    }
    set deductionSum(value: number) {
        this.deductionSumValue = value;

        this.calculate();
    }
  
    sixYearCarValue: boolean = false;
    get sixYearCar(): boolean {
        return this.sixYearCarValue;
    }
    set sixYearCar(value: boolean) {
        this.sixYearCarValue = value;
        this.calculate();
    }
    private sixYearsInfo: boolean = false;

    private equipmentSumValue: number = 0;
    get equipmentSum(): number {
        return this.equipmentSumValue;
    }
    set equipmentSum(value: number) {
        this.equipmentSumValue = value;
        
        this.calculate();
    }

    private taxValue: number = 0;
    get taxAmount(): number {
        return this.taxValue;
    }
    set taxAmount(value: number) {
        this.taxValue = value;

        this.calculate();
    }

    private hasExtensiveDrivingValue: boolean = false;
    get hasExtensiveDriving(): boolean {
        return this.hasExtensiveDrivingValue;
    }
    set hasExtensiveDriving(value: boolean) {
        this.hasExtensiveDrivingValue = value;

        this.calculate();
    }

    private totalSumValue: number = 0;
    set totalSum(value: number) {
        this.totalSumValue = value;
    }
    get totalSum(): number {
        return this.totalSumValue;
    }

    private get baseAmountPartPercent(): number {
        return (this.taxAmount || 0) > 0 ? 0.29 : 0.317;
    }

    private baseAmountPartValue: number = 0;
    set baseAmountPart(value: number) {
        this.baseAmountPartValue = value;
    }
    get baseAmountPart(): number {
        return this.baseAmountPartValue;
    }

    private interestPartValue: number = 0;
    set interestPart(value: number) {
        this.interestPartValue = value;
    }
    get interestPart(): number {
        return this.interestPartValue;
    }

    private pricePartValue: number = 0;
    set pricePart(value: number) {
        this.pricePartValue = value;
    }
    get pricePart(): number {
        return this.pricePartValue;
    }

    private pricePart1Value: number = 0;
    set pricePart1(value: number) {
        this.pricePart1Value = value;
    }
    get pricePart1(): number {
        return this.pricePart1Value;
    }

    private pricePart2Value: number = 0;
    set pricePart2(value: number) {
        this.pricePart2Value = value;
    }
    get pricePart2(): number {
        return this.pricePart2Value;
    }

    private extensiveDrivingValue: number = 0;
    set extensiveDriving(value: number) {
        this.extensiveDrivingValue = value;
    }
    get extensiveDriving(): number {
        return this.extensiveDrivingValue;
    }

    private listReductionValue: number = 0;
    set listReduction(value: number) {
        this.listReductionValue = value;
    }
    get listReduction(): number {
        return this.listReductionValue;
    }
   
    get ecoCarDeduction(): number {
        // El- och laddhybridbilar, som kan laddas från elnätet, samt gasbilar (ej gasol) justeras först till en jämförbar bil utan miljöteknik.
        // Därefter sätts förmånsvärdet ner med 40 procent, max 16 000 kronor för inkomstår 2012-2016 och max 10 000 kronor från och med inkomstår 2017.
        // Detta gäller endast om bilen har ett nybilspris som är högre än närmast jämförbara bil.

        // Etanolbilar, elhybridbilar, som inte kan laddas från elnätet, och bilar som kan köras på gasol, rapsmetylester
        // samt övriga typer av miljöanpassade drivmedel justeras enbart ner till jämförbar bil.

        // Reglerna är tidsbegränsade och gäller till och med inkomståret 2020.

        var value: number = 0;

        //if (this.price >= this.comparablePrice && this.isDeductableEcoCar) {
        //    value = this.totalSum * 0.4;
        //    if (value > 10000)
        //        value = 10000;
        //}

        return -value;
    }

    private taxableValuePerYearSumValue: number = 0;
    set taxableValuePerYearSum(value: number) {
        this.taxableValuePerYearSumValue = value;
    }
    get taxableValuePerYearSum(): number {
        return this.taxableValuePerYearSumValue;
    }

    private taxableValuePerMonthValue: number = 0;
    set taxableValuePerMonth(value: number) {
        this.taxableValuePerMonthValue = value;
    }
    get taxableValuePerMonth(): number {
        return this.taxableValuePerMonthValue;
    }

    private netSalaryDeductionPerMonthValue: number = 0;
    set netSalaryDeductionPerMonth(value: number) {
        this.netSalaryDeductionPerMonthValue = value;
        this.netSalaryDeductionPerYear = value * 12;
    }
    get netSalaryDeductionPerMonth(): number {
        return this.netSalaryDeductionPerMonthValue;
    }

    private netSalaryDeductionPerYearValue: number = 0;
    set netSalaryDeductionPerYear(value: number) {
        this.netSalaryDeductionPerYearValue = value;
    }
    get netSalaryDeductionPerYear(): number {
        return this.netSalaryDeductionPerYearValue;
    }

    private totalTaxableValuePerYearValue: number = 0;
    set totalTaxableValuePerYear(value: number) {
        this.totalTaxableValuePerYearValue = value;
        this.totalTaxableValuePerMonth = Math.floor(value / 12);
    }
    get totalTaxableValuePerYear(): number {
        return this.totalTaxableValuePerYearValue;
    }

    private totalTaxableValuePerMonthValue: number = 0;
    set totalTaxableValuePerMonth(value: number) {
        this.totalTaxableValuePerMonthValue = value;
    }
    get totalTaxableValuePerMonth(): number {
        return this.totalTaxableValuePerMonthValue;
    }

    private get useCalculationAfter20210701(): boolean {
        return this.registeredDate && this.registeredDate.isSameOrAfterOnDay(new Date(2021, 6, 1));
    }
    private get useCalculationAfter20220701(): boolean {
        return this.registeredDate && this.registeredDate.isSameOrAfterOnDay(new Date(2022, 6, 1));
    }
    
    private benefitValueAdjustmentValue: number = 100;
    private benefitValueAdjustmentAmount: number = 100;
    private benefitValueAdjustmentAmountNeg: number = 0;
    set benefitValueAdjustment(value: number) {
        if (value == 0 || value > 100) value = 100;
        this.benefitValueAdjustmentAmount = value;
        this.benefitValueAdjustmentAmountNeg = 100 - value;
        this.calculate();
    }
    get benefitValueAdjustment(): number {
        return this.benefitValueAdjustmentAmount;
    }

    public calculate() {
        if (this.useCalculationAfter20220701) {
            if (this.price == this.comparablePrice) {

                if (this.priecAfterReduction && this.priecAfterReduction > 0 && this.price > 0)
                    this.priceAdjustmentValue = Math.floor(this.priecAfterReduction - this.comparablePrice);
            } else if (this.listReduction > 0) {
                if (this.listReduction * 2 <= this.price) {
                    this.priceAdjustmentValue = 0 - this.listReduction;
                } else {
                    this.priceAdjustmentValue = 0 - Math.floor(this.price / 2);
                }
            }
        }
        if (this.sixYearCar) {
            if ((this.comparablePrice !== 0 && !this.useCalculationAfter20220701 ? this.comparablePrice : this.price) + this.equipmentSum <= this.baseAmount * 4) {
                this.sixYearsInfo = true;
                this.priceAdjustmentValue = Math.floor((this.baseAmount * 4) - ((this.comparablePrice !== 0 && !this.useCalculationAfter20220701 ? this.comparablePrice : this.price) + this.equipmentSum).round(2));
            } else {
                this.sixYearsInfo = false;
                this.priceAdjustmentValue = 0;
            }
        }
        this.totalSum = (this.comparablePrice !== 0 && !this.useCalculationAfter20220701 ? this.comparablePrice : this.price) + this.equipmentSum + this.priceAdjustment;
        this.baseAmountPart = Math.floor((this.baseAmount * this.baseAmountPartPercent).round(2));

        if (this.useCalculationAfter20210701) {
            // Cars registered after 2021-07-01
            this.interestPart = Math.floor((this.totalSum * ((0.7 * this.governmentLoanInterest) + 1) / 100).round(2));

            this.pricePart = Math.floor((this.totalSum * 0.13).round(2));
            this.pricePart1 = 0;
            this.pricePart2 = 0;
        } else {
            // Cars registered before 2021-07-01
            this.interestPart = Math.floor((this.totalSum * 0.75 * this.governmentLoanInterest / 100).round(2));

            let baseAmount75 = this.baseAmount * 7.5;
            this.pricePart = 0;
            this.pricePart1 = Math.floor(((this.totalSum > baseAmount75 ? baseAmount75 : this.totalSum) * 0.09).round(2));
            this.pricePart2 = Math.floor((this.totalSum > baseAmount75 ? (this.totalSum - baseAmount75) * 0.2 : 0).round(2));
        }

        this.taxableValuePerYearSum = this.baseAmountPart + this.interestPart + this.pricePart + this.pricePart1 + this.pricePart2 + this.ecoCarDeduction + this.taxAmount;
        this.extensiveDriving = Math.floor((this.hasExtensiveDriving && this.taxableValuePerYearSum > 0 ? (this.taxableValuePerYearSum * 0.25) : 0).round(2));
        this.benefitValueAdjustmentValue = Math.floor((this.taxableValuePerYearSum > 0 ? (this.taxableValuePerYearSum * (100 - this.benefitValueAdjustmentAmount) / 100) : 100).round(2));
        if (this.extensiveDriving > 0)
            this.taxableValuePerYearSum = this.taxableValuePerYearSum - this.extensiveDriving;
        if (this.benefitValueAdjustmentValue > 0)
            this.taxableValuePerYearSum = this.taxableValuePerYearSum - this.benefitValueAdjustmentValue;
        this.taxableValuePerMonth = Math.floor((this.taxableValuePerYearSum / 12).round(2));
        this.netSalaryDeductionPerMonth = this.deductionSum;
        this.totalTaxableValuePerYear = this.taxableValuePerYearSum - this.netSalaryDeductionPerYear;
    }
}
