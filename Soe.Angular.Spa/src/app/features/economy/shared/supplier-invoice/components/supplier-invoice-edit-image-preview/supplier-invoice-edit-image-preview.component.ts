import {
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { SupplierInvoiceImageForm } from './models/supplier-invoice-image-form.model';
import { ValidationHandler } from '@shared/handlers';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { Observable, of } from 'rxjs';
import { InvoiceImageFile } from '../../models/supplier-invoice.model';
import { InvoiceIds } from '../../models/utility-models';
import { rxResource } from '@angular/core/rxjs-interop';

/**
 * Known todos:
 *  - Add a loading spinner when loading the image.
 *  - Back and forth communication with the parent component to update the invoice form.
 *  - Some styling for the upload component.
 *  - Add the file previewer instead of just using the PDF viewer.
 *  - Figure out how we want to deal with the form.
 */
@Component({
  selector: 'soe-supplier-invoice-edit-image-preview',
  templateUrl: './supplier-invoice-edit-image-preview.component.html',
  styleUrls: ['./supplier-invoice-edit-image-preview.component.scss'],
  standalone: false,
})
export class SupplierInvoiceEditImagePreviewComponent {
  validationHandler = inject(ValidationHandler);
  service = inject(SupplierInvoiceService);

  invoiceIds = input.required<InvoiceIds>();
  isReadOnly = input(false);

  invoiceImage = signal<InvoiceImageFile | null>(null);
  isLoading = signal(false);
  isInitialized = signal(true);

  image = rxResource({
    params: () => this.invoiceIds(),
    stream: params => this.performLoadInvoiceImage(params.params),
  });

  hasImage = computed(() => {
    return !!this.image.value()?.data;
  });

  form = new SupplierInvoiceImageForm({
    validationHandler: this.validationHandler,
  });

  hasValidIds = computed(() => {
    return (
      this.invoiceIds() &&
      (this.invoiceIds().invoiceId || this.invoiceIds().ediEntryId)
    );
  });

  onAfterFilesAttached(files: AttachedFile[]) {
    const file = files[0];
    if (file.content) {
      this.image.set({
        fileName: file.name ?? '',
        data: file.content,
        extension: file.extension ?? '',
      });
    }
  }

  private performLoadInvoiceImage(
    ids: InvoiceIds
  ): Observable<InvoiceImageFile | null> {
    const { invoiceId, ediEntryId } = ids;
    if (invoiceId) return this.service.loadInvoiceImage(invoiceId);
    if (ediEntryId) return this.service.loadEdiImage(ediEntryId);
    return of(null);
  }
}
