import "pdfmake";

import getDocDefinition from "./GetDocDefinition";

declare const pdfMake;

function PrintPdfDoc(gridOptions: any, fileName: string) {
    console.log("Exporting to PDF...");
    const docDefinition = getDocDefinition(
        {
            PDF_PAGE_ORITENTATION: "landscape",
            PDF_HEADER_HEIGHT: 50,
            PDF_WITH_FOOTER_PAGE_COUNT: true
        },
        gridOptions.api,
        gridOptions.columnApi
    );
    pdfMake.createPdf(docDefinition).download(fileName);
}

export default PrintPdfDoc;