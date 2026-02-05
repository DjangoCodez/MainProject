export class ExportUtility {
    static Export(item: any, filename: string = undefined, contentType: string = undefined) {
        if (!item)
            return;
        
        if (typeof item === "object") {
            item = JSON.stringify(item, undefined, 4);
        }

        const blob = new Blob([item], { type: contentType ? contentType : 'text/json' });
        const e = document.createEvent('MouseEvents');
        const a = document.createElement('a');

        a.download = !filename ? 'export.json' : filename;
        a.href = window.URL.createObjectURL(blob);
        a.dataset.downloadurl = [contentType ? contentType : 'text/json', a.download, a.href].join(':');
        e.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
        a.dispatchEvent(e);
    }

    static ExportToCSV(content: string, filename: string) {
        if (!content)
            return;

        const blob = new Blob([content], { type: 'text/csv'});
        const mouseEvent = document.createEvent('MouseEvents');
        const tag = document.createElement('a');

        tag.download = filename;
        tag.href = window.URL.createObjectURL(blob);
        tag.dataset.downloadurl = ['text/csv', tag.download, tag.href].join(':');
        mouseEvent.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
        tag.dispatchEvent(mouseEvent);
    }

    static DownloadFile(
        dataBase64: string,
        fileName: string,
        fileType: string
    ): void {
        const byteArray = this.base64StringToByteArray(dataBase64);

        const blob = new Blob([byteArray], { type: fileType });
        const link = window.document.createElement('a');
        const url = window.URL.createObjectURL(blob);
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    }

    static OpenFile(
        dataBase64: string,
        fileName: string,
        fileType: string
    ): void {
        const byteArray = this.base64StringToByteArray(dataBase64);
        const file = new Blob([byteArray], { type: fileType });
        var fileURL = URL.createObjectURL(file);
        window.open(fileURL);
    }

    static base64StringToByteArray(base64String: string) {
        // Decode the Base64 string to a byte array
        const byteCharacters = atob(base64String);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        return new Uint8Array(byteNumbers);
    }
}
