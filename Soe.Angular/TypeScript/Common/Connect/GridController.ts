import { ITranslationService } from "../../Core/Services/TranslationService";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { IConnectService } from "./ConnectService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { CoreUtility } from "../../Util/CoreUtility";
import { ToolBarUtility, ToolBarButton } from "../../Util/ToolBarUtility";
import { IconLibrary } from "../../Util/Enumerations";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../Core/Handlers/GridHandler"
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { Constants } from "../../Util/Constants";
import { SoeEntityType } from "../../Util/CommonEnumerations";
import { FilesLookupDTO } from "../Models/FilesLookupDTO";
import { ImportFileDTO } from "../Models/ImportFileDTO";
import { ImportSelectionController } from "./ImportSelectionController";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

	//modal
	private modalInstance: any;

	private module: number;
	private uploadResponse: any;
	private $q: ng.IQService;

	//@ngInject
	constructor(
		private translationService: ITranslationService,
		private connectService: IConnectService,
		private notificationService: INotificationService,
		private $timeout: ng.ITimeoutService,
		private $filter: ng.IFilterService,
		$uibModal,
		controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
		progressHandlerFactory: IProgressHandlerFactory,
		messagingHandlerFactory: IMessagingHandlerFactory,
		gridHandlerFactory: IGridHandlerFactory,
		private urlHelperService: IUrlHelperService,
		private messagingService: IMessagingService,
		$q: ng.IQService
	) {
		super(gridHandlerFactory, "Common.Connect", progressHandlerFactory, messagingHandlerFactory);

		this.modalInstance = $uibModal;
		this.$q = $q;

		this.flowHandler = controllerFlowHandlerFactory.createForGrid()
			.onPermissionsLoaded((feature, readOnly, modify) => {
				this.readPermission = readOnly;
				this.modifyPermission = modify;
				if (this.modifyPermission) {
					// Send messages to TabsController
					this.messagingHandler.publishActivateAddTab();
				}
			})
			.onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
			.onSetUpGrid(() => this.setupToolbar())
			.onSetUpGrid(() => this.setupGrid())
			.onLoadGridData(() => this.loadGridData(false));
	}

	// SETUP

	onInit(parameters: any) {
		this.parameters = parameters;
		this.isHomeTab = !!parameters.isHomeTab;
		this.module = soeConfig.module;

		if (this.isHomeTab) {
			this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
		}

		this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });
	}

	private setupGrid() {

		var keys: string[] = [
			"common.name",
			"common.connect.import",
			"common.connect.imports",
			"common.standard",
			"common.connect.importtype",
            "core.edit"
		];

		return this.translationService.translateMany(keys).then(terms => {
			//this.gridAg.options.clearColumnDefs();

			this.gridAg.addColumnText("name", terms["common.name"], 25);
			this.gridAg.addColumnText("headName", terms["common.connect.import"], 25);
			this.gridAg.addColumnText("typeText", terms["common.connect.importtype"], 15);
            this.gridAg.addColumnBool("isStandard", terms["common.standard"],5)
			this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

			this.gridAg.finalizeInitGrid("common.connect.imports", true);
		});
	}

	private onCreateToolbar(toolbarFactory: IToolbarFactory) {
		this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
	}

	private setupToolbar() {
		this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton(
			"common.connect.uploadfiles",
			"common.connect.uploadfiles",
			IconLibrary.FontAwesome,
			"fa-upload",
			() => { this.startUploading(); })));
	}

	// SERVICE CALLS   

	public loadGridData(useCache: boolean) {
		this.progress.startLoadingProgress([() => {
			return this.connectService.getImports(this.module).then((x) => {
				this.setData(x);
			});
		}]);
	}

	private reloadData() {
		this.loadGridData(false);
	}

	// EVENTS   

	edit(row) {
		// Send message to TabsController        
		if (this.readPermission || this.modifyPermission)
			this.messagingHandler.publishEditRow(row);
	}

	// METHODS
	private startUploading() {
		this.uploadFiles()
			.then(files => this.uploadSelection(files))
			.then(files => this.groupFiles(files))
			.then(groupedFiles => this.openEdit(groupedFiles));
	}

	private uploadFiles(): ng.IPromise<FilesLookupDTO> {
		const url = `${CoreUtility.apiPrefix}${Constants.WEBAPI_CORE_FILES_UPLOAD}${SoeEntityType.XEConnectImport}/0/0?extractZip=true`;
		return this.notificationService.showFileUpload(
			url,
			this.translationService.translateInstant("core.fileupload.choosefiletoimport"), //title
			true, //show drop zone
			true, //show queue
			true, //allow multiple files
			true  //noMaxSize
		).result.then(
			success => {
				this.uploadResponse = success.result;
				const uploadedFiles = new FilesLookupDTO(
					SoeEntityType.XEConnectImport,
					success.result.map(file => new ImportFileDTO(file.integerValue2, file.stringValue))
				);
				return uploadedFiles;
			},
			error => {
				return this.handleError(error);
			}
		)
	}

	private uploadSelection(uploadedFiles: FilesLookupDTO): ng.IPromise<any> {
		const options: angular.ui.bootstrap.IModalSettings = {
			templateUrl: this.urlHelperService.getViewUrl("importSelection.html"),
			controller: ImportSelectionController,
			controllerAs: "ctrl",
			bindToController: true,
			size: 'lg',
			windowClass: '',
			resolve: {
				fileImports: () => { return uploadedFiles }
			}
		}
		return this.modalInstance.open(options).result.then(
			success => { // Removes files from this.uploadResponse that haven't been selected for import by user

				const filesToImportObject = success.data?.filter(d => d.doImport);
				if (!filesToImportObject)
					return this.$q.resolve();

				const filesToImport: string[] = filesToImportObject.map(i => i.dataStorageId);
				if (filesToImport.length == 0)
					return this.$q.resolve();

				this.uploadResponse = this.uploadResponse.filter(file => filesToImport.includes(file.integerValue2));

				return filesToImportObject;
			},
			error => {
				return this.handleError(error);
			}
		);
	}

	private groupFiles(filesToImportObject: any): ng.IPromise<any> {
		if (!filesToImportObject)
			return this.handleError("No files to group");

		const groupedData = filesToImportObject.reduce((fileArray, file) => {
			const { importId, dataStorageId } = file;
			if (!fileArray[importId]) {
				fileArray[importId] = [];
			}
			fileArray[importId].push(dataStorageId);
			return fileArray;
		}, {} as Record<number, number[]>);
		const fileGroups = Object.keys(groupedData).map(importId => ({
			importId: Number(importId),
			dataStorageIds: groupedData[Number(importId)]
		}));

		const importsArray = filesToImportObject.reduce((importArray, file) => {
			const name = file.import.name;
			const { importId, fileType } = file;
			if (!importArray[importId]) {
				importArray[importId] = [];
			}
			importArray[importId] = { fileType: fileType, name };
			return importArray;
		}, {} as Record<number, ImportsArrayValue>);

		const tabControllerData: TabControllerData[] = fileGroups.map(group => ({
			importId: group.importId,
			import: importsArray[group.importId],
			files: this.uploadResponse.filter(r => group.dataStorageIds.includes(r.integerValue2))
		}));

		return this.$q.resolve(tabControllerData);
	}

	private handleError(error: any): ng.IPromise<void> {
		console.log("Error occured", error);
		return this.$q.reject(error);
	}

	private openEdit(data) {
		data.forEach(row => {
			const title = row.import?.fileType != null ? `${row.import.name} - ${row.import.fileType}` : row.import.name;

			this.messagingService.publish(Constants.EVENT_OPEN_IMPORT, {
				data: row,
				title: title
			});
		});
	}
};
type ImportsArrayValue = { fileType: string, name: string }
type TabControllerData = { importId: number, import: ImportsArrayValue, files: any }