import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FilePreviewDialogData, FileViewerComponent } from "./file-viewer.component";
import { before } from "lodash";

describe('FileViewerComponent', () => {
  let component: FileViewerComponent;
  let fixture: ComponentFixture<FileViewerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: const [SoftOneTestBed],
      declarations: [FileViewerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(FileViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set dialog parameters correctly when data is provided', () => {
      const dialogData = new FilePreviewDialogData('example.pdf', 'base64string', 'pdf');
      component.data = dialogData;
  
      component.setDialogParam();
  
      expect(component.base64Data).toBe(dialogData.base64Data);
      expect(component.fileExtension).toBe(dialogData.fileExtension);
    });
    it('should not set dialog parameters if data is not provided', () => {
      component.data = undefined as any;
  
      component.setDialogParam();
  
      expect(component.base64Data).toBeUndefined();
      expect(component.fileExtension).toBeUndefined();
    });
  });
  describe('methods', () => {
    describe('setDialogParam', () => {
      it('should set dialog parameters correctly when data is provided', () => {
        const dialogData = new FilePreviewDialogData('example.pdf', 'base64string', 'pdf');
        component.data = dialogData;
    
        component.setDialogParam();
    
        expect(component.base64Data).toBe(dialogData.base64Data);
        expect(component.fileExtension).toBe(dialogData.fileExtension);
      });
      it('should not set dialog parameters if data is not provided', () => {
        component.data = undefined as any;
    
        component.setDialogParam();
    
        expect(component.base64Data).toBeUndefined();
        expect(component.fileExtension).toBeUndefined();
      });
    });
  });
  describe('DOM', () => {
    describe('soe-pdf-viewer', () => {
      let pdfViewer: any;
      beforeEach(() => {
        component.fileExtension = '.pdf';
        fixture.detectChanges();
        pdfViewer = fixture.nativeElement.querySelector('soe-pdf-viewer');
      });
      it('should render soe-pdf-viewer when fileExtension is pdf', () => {
        expect(pdfViewer).toBeTruthy();
      });
      it('should not render soe-pdf-viewer when fileExtension is not pdf', () => {
        component.fileExtension = '.png';
        fixture.detectChanges();
        pdfViewer = fixture.nativeElement.querySelector('soe-pdf-viewer');
        expect(pdfViewer).toBeFalsy();
      });
      it('should have pdfSrc set to base64Data', () => {
        component.base64Data = 'base64string';
        fixture.detectChanges();
        expect(pdfViewer.pdfSrc).toBe(component.base64Data);
      });
    });
    describe('content-border', () => {
      it('should render content-border if file extension is .png', () => {
        component.fileExtension = '.png';
        fixture.detectChanges();
        const contentBorder = fixture.nativeElement.querySelector('.content-border');
        expect(contentBorder).toBeTruthy();
      });
      it('should render content-border if file extension is .jpg', () => {
        component.fileExtension = '.jpg';
        fixture.detectChanges();
        const contentBorder = fixture.nativeElement.querySelector('.content-border');
        expect(contentBorder).toBeTruthy();
      });
      it('should render content-border if file extension is .jpeg', () => {
        component.fileExtension = '.jpeg';
        fixture.detectChanges();
        const contentBorder = fixture.nativeElement.querySelector('.content-border');
        expect(contentBorder).toBeTruthy();
      });
      it('should render content-border if file extension is .gif', () => {
        component.fileExtension = '.gif';
        fixture.detectChanges();
        const contentBorder = fixture.nativeElement.querySelector('.content-border');
        expect(contentBorder).toBeTruthy();
      });
      it('should not render content-border if file extension is not .png, .jpg, .jpeg, .gif', () => {
        component.fileExtension = '.pdf';
        fixture.detectChanges();
        const contentBorder = fixture.nativeElement.querySelector('.content-border');
        expect(contentBorder).toBeFalsy();
      });
      it('should render src with base64Data', () => {
        component.fileExtension = '.png';
        component.base64Data = 'base64string';
        fixture.detectChanges();
        const image = fixture.nativeElement.querySelector('.content-border img');
        expect(image.src).toBe('data:image/jpg;base64,' + component.base64Data);
      });
    });
  }); 
});
describe('FilePreviewDialogData', () => {
  it('should initialize with given filename, base64Data, and fileExtension', () => {
    const filename = 'example.pdf';
    const base64Data = 'base64string';
    const fileExtension = '.pdf';
    const dialogData = new FilePreviewDialogData(filename, base64Data, fileExtension);

    expect(dialogData.title).toBe(filename);
    expect(dialogData.base64Data).toBe(base64Data);
    expect(dialogData.fileExtension).toBe(fileExtension);
    expect(dialogData.size).toBe('lg');
  });

  it('should return true for previewable file extensions', () => {
    expect(FilePreviewDialogData.canBePreviewed('pdf')).toBe(true);
    expect(FilePreviewDialogData.canBePreviewed('.png')).toBe(true);
    expect(FilePreviewDialogData.canBePreviewed('jpg')).toBe(true);
    expect(FilePreviewDialogData.canBePreviewed('.jpeg')).toBe(true);
    expect(FilePreviewDialogData.canBePreviewed('gif')).toBe(true);
  });

  it('should return false for non-previewable file extensions', () => {
    expect(FilePreviewDialogData.canBePreviewed('docx')).toBe(false);
    expect(FilePreviewDialogData.canBePreviewed('.xls')).toBe(false);
    expect(FilePreviewDialogData.canBePreviewed('mp4')).toBe(false);
  });
});
