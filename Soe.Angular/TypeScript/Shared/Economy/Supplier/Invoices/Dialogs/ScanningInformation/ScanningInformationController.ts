import { IInvoiceInterpretationDTO } from "../../../../../../Common/Models/InvoiceDTO";

export class ScanningInformationController {
    private rawResponseFormatted: string;
    private arrivalTimeFormatted: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private interpretation: IInvoiceInterpretationDTO 
    ) {
        if (interpretation.metadata.arrivalTime) {
            const date = new Date(interpretation.metadata.arrivalTime);
            this.arrivalTimeFormatted = date.toLocaleString();
        }
        if (interpretation.metadata.provider === "ReadSoft") {
            this.rawResponseFormatted = this.prettyPrintXML(interpretation.metadata.rawResponse);
        } else if (interpretation.metadata.provider === "AzoraOne") {
            this.rawResponseFormatted = this.prettyPrintJSON(interpretation.metadata.rawResponse);
        }
    }

    prettyPrintXML(xmlString: string): string {
        let formatted = "";
        let indent = 0;
        const padding = "  "; // Indentation level (2 spaces)

        // Add newlines between tags for better readability
        xmlString = xmlString.replace(/>\s*</g, ">\n<");

        xmlString.split("\n").forEach((line) => {
            if (line.match(/^<\/\w/)) {
                // Closing tag: Decrease indentation
                indent--;
            }

            formatted += `${padding.repeat(indent)}${line.trim()}\n`;

            if (line.match(/^<\w([^>/]*)?>$/)) {
                // Opening tag: Increase indentation (if it's not self-closing)
                indent++;
            }
        });

        return formatted.trim();
    }


    private prettyPrintJSON(json: string): string {
        try {
            const obj = JSON.parse(json); 
            return JSON.stringify(obj, null, 2); 
        } catch (error) {
            return json;
        }
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}