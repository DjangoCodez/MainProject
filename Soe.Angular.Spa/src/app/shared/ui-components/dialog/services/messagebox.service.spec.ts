import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { MessageboxService } from './messagebox.service';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MessageboxComponent } from '../messagebox/messagebox.component';
import { MessageboxData } from '../models/messagebox';
import { vi } from 'vitest';

describe('MessageboxService', () => {
  let service: MessageboxService;
  let matDialogMock: MatDialog;

  beforeEach(() => {
    // Mock MatDialog
    matDialogMock = {
      open: vi.fn(),
    } as unknown as MatDialog;
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        MessageboxService,
        { provide: MatDialog, useValue: matDialogMock },
      ],
    });
    service = TestBed.inject(MessageboxService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
  describe('information', () => {
    it('should open the messagebox with type "information"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Info Title';
      const text = 'Information message';
      const result = service.information(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'information',
          buttons: 'ok',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('warning', () => {
    it('should open the messagebox with type "warning"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Warning Title';
      const text = 'Warning message';
      const result = service.warning(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'warning',
          size: 'lg',
          buttons: 'okCancel',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('error', () => {
    it('should open the messagebox with type "error"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Error Title';
      const text = 'Error message';
      const result = service.error(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'error',
          buttons: 'ok',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('success', () => {
    it('should open the messagebox with type "success"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Success Title';
      const text = 'Success message';
      const result = service.success(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'success',
          buttons: 'ok',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('progress', () => {
    it('should open the messagebox with type "progress"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Progress Title';
      const text = 'Progress message';
      const result = service.progress(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'progress',
          buttons: 'none',
          hideCloseButton: true,
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('question', () => {
    it('should open the messagebox with type "question"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Question Title';
      const text = 'Question message';
      const result = service.question(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'question',
          buttons: 'yesNo',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('questionAbort', () => {
    it('should open the messagebox with type "questionAbort"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Question Abort Title';
      const text = 'Abort question message';
      const result = service.questionAbort(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'questionAbort',
          buttons: 'yesNoCancel',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });

  describe('show', () => {
    it('should open the messagebox with type "custom"', () => {
      const dialogRefMock: MatDialogRef<MessageboxComponent<MessageboxData>> =
        {} as MatDialogRef<MessageboxComponent<MessageboxData>>;
      matDialogMock.open = vi.fn().mockReturnValue(dialogRefMock);

      const title = 'Custom Title';
      const text = 'Custom message';
      const result = service.show(title, text);

      expect(matDialogMock.open).toHaveBeenCalledWith(MessageboxComponent, {
        data: {
          title,
          text,
          type: 'custom',
          buttons: 'ok',
        },
      });

      expect(result).toBe(dialogRefMock);
    });
  });
});
