import '../../Core/Module';

import { PdfViewerDirective } from "./pdfViewerDirective";

angular.module("Soe.Common.PdfViewer", ['Soe.Core'])
    .directive("pdfViewer", PdfViewerDirective);
