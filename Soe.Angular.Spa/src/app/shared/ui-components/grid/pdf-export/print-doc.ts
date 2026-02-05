import pdfMake from 'pdfmake/build/pdfmake';
import pdfFonts from 'pdfmake/build/vfs_fonts';
import getDocDefinition from './doc-definition';
import { GridApi } from 'ag-grid-community';

export function printPdfDoc(agGridApi: GridApi, fileName: string) {
  const docDefinition: any = getDocDefinition(
    {
      PDF_PAGE_ORITENTATION: 'landscape',
      PDF_HEADER_HEIGHT: 50,
      PDF_WITH_FOOTER_PAGE_COUNT: true,
    },
    agGridApi
  );
  pdfMake
    .createPdf(docDefinition, undefined, undefined, pdfFonts.pdfMake.vfs)
    .download(fileName);
}
