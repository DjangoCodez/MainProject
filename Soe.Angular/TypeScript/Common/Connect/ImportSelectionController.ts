import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { FilesLookupDTO } from "../Models/FilesLookupDTO";
import { Feature, SoeModule } from "../../Util/CommonEnumerations";
import { ICoreService } from "../../Core/Services/CoreService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IConnectService } from "./ConnectService";
import { EmbeddedGridController } from "../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../Core/Handlers/gridhandlerfactory";
import { ImportDTO } from "../Models/ImportDTO";
import { ImportSelectionGridRowDTO } from "../Models/ImportSelectionGridRowDTO";
import { TypeAheadColumnOptions, TypeAheadOptionsAg } from "../../Util/SoeGridOptionsAg";
import { TypeAheadOptions } from "../../Util/SoeGridOptions";
import { SmallGenericType } from "../Models/SmallGenericType";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../Util/Enumerations";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { EditController } from "./EditController";
import { Constants } from "../../Util/Constants";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../Core/Handlers/ProgressHandler";

export class ImportSelectionController {
	private title: string;
	private gridHandler: EmbeddedGridController;
	private terms: TermsType;
	private importLookupSimple: SmallGenericType[];
	private importLookup: ImportDTO[];
	private data: ImportSelectionGridRowDTO[];
	private tabs: ITabHandler;
	private progressHandler: IProgressHandler;

	//@ngInject
	constructor(
		$timeout: ng.ITimeoutService,
		
		private $uibModalInstance,
		private gridHandlerFactory: IGridHandlerFactory,
		tabHandlerFactory: ITabHandlerFactory,

		private translationService: ITranslationService,
		private connectService: IConnectService,
		private urlHelperService: IUrlHelperService,
		private notificationService: INotificationService,
		private progressHandlerFactory: IProgressHandlerFactory,

		private $q: ng.IQService,
		private $scope: ng.IScope,
		private fileImports: FilesLookupDTO,

		protected coreService: ICoreService
	) {
		this.tabs = tabHandlerFactory.create();
		this.progressHandler = this.progressHandlerFactory.create();
	}

	public $onInit() {
		this.progressHandler.startLoadingProgress([() => {
			this.gridHandler = new EmbeddedGridController(this.gridHandlerFactory, "common.connect.new_import");
			this.gridHandler.gridAg.options.enableFiltering = true;
			this.gridHandler.gridAg.options.enableRowSelection = false;
			this.gridHandler.gridAg.options.autoHeight = true;
			this.gridHandler.gridAg.options.ignoreResizeToFit = false;

			return this.$q.all([
				this.loadLookups(),
				this.loadTerms()]
			)
				.then(() => this.setupGridColumns())
				.then(() => this.loadData())
				.catch(error => this.handleError(error));
		}]);
	}

	private loadLookups(): ng.IPromise<void> {
		return this.connectService.getImports(SoeModule.Economy).then(data => {
			this.importLookupSimple = data.map(d => new SmallGenericType(d.importId, d.name));
			this.importLookup = data.map(row => {
				return new ImportDTO({
					actorCompanyId: row.actorCompanyId,
					created: row.created,
					createdBy: row.createdBy,
					guid: row.guid,
					headName: row.headName,
					importDefinitionId: row.importDefinitionId,
					importHeadType: row.importHeadType,
					importId: row.importId,
					isStandard: row.isStandard,
					module: row.module,
					name: row.name,
					specialFunctionality: row.specialFunctionality,
					state: row.state,
					type: row.type,
					typeText: row.typeText,
					updateExistingInvoice: row.updateExistingInvoice,
					useAccountDimensions: row.useAccountDimension,
					useAccountDistribution: row.useAccountDistribution
				});
			});
		});
	}

	private loadTerms(): ng.IPromise<void> {
		const keys: string[] = [
			"common.connect.import",
			"common.filename",
			"common.message",
			"common.connect.modalTitle",
			"core.import"
		];
		return this.translationService.translateMany(keys).then(terms => {
			this.terms = terms;
			this.title = terms["common.connect.modalTitle"];
		});
	}

	private handleImportChange(input: ImportChangeType): void {
		const newValueAsNumber = Number(input.newValue);
		if (isNaN(newValueAsNumber))
			return;

		const member = this.importLookupSimple.find(i => i.id == newValueAsNumber);
		if (!member)
			return;
		
		const changeObj = this.data.find(i => i.dataStorageId == input.data.dataStorageId);
		if (!changeObj)
			return;

		changeObj.importId = member.id;
		changeObj.importName = member.name;
		changeObj.import = this.importLookup.find(i => i.importId == newValueAsNumber);
		if (changeObj.disableImport) {
			changeObj.disableImport = false;
			this.gridHandler.gridAg.options.refreshGrid();
		}
	}

	private setupGridColumns(): ng.IPromise<void> {
		const importOptions: TypeAheadOptionsAg = {
			source: () => this.importLookupSimple,
			displayField: "name",
			dataField: "id",
			minLength: 0,
			delay: 0,
			useScroll: true,
			updater: null,
			allowNavigationFromTypeAhead: () => true
		}

		this.gridHandler.gridAg.addColumnText("fileName", this.terms["common.filename"], 300);
		this.gridHandler.gridAg.addColumnTypeAhead("importName", this.terms["common.connect.import"], 250, {
			displayField: "name",
			editable: true,
			onChanged: data => this.handleImportChange(data),
			typeAheadOptions: importOptions
		});
		this.gridHandler.gridAg.addColumnText("message", this.terms["common.message"], 200);
		this.gridHandler.gridAg.addColumnBool(
			"doImport", //field
			this.terms["core.import"], //title
			60, //width
			true, //enable cell edit
			null, //on changed
			"disableImport" //disabled field
		);

		this.gridHandler.gridAg.finalizeInitGrid("", false);
		return this.$q.resolve();
	}

	private loadData(): ng.IPromise<void> {
		return this.connectService.getImportSelectionGrid(this.fileImports).then(data => {
			this.data = data.map((d, i) => ({
				id: i+1,
				...d,
				doImport: d.message == "",
				disableImport: d.import == undefined
			}));
			this.gridHandler.gridAg.options.setData(this.data);
		});
	}

	private handleError(error: any) {
		console.log("An error occured: ", error);
	}

	private ok() {
		this.$uibModalInstance.close({data: this.data});
	}

	private cancel() {
		this.$uibModalInstance.close({data: undefined});
	}
}
type TermsType = { [index: string]: string };
type ImportChangeType = { newValue: number; data: ImportSelectionGridRowDTO }
