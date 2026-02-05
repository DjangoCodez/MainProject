export class ExportUtil {
  static Export(item: any, filename?: string, contentType?: string) {
    if (!item) return;

    if (typeof item === 'object') {
      item = JSON.stringify(item, undefined, 4);
    }

    const blob = new Blob([item], {
      type: contentType ? contentType : 'text/json',
    });
    const e = document.createEvent('MouseEvents');
    const a = document.createElement('a');

    a.download = !filename ? 'export.json' : filename;
    a.href = window.URL.createObjectURL(blob);
    a.dataset.downloadurl = [
      contentType ? contentType : 'text/json',
      a.download,
      a.href,
    ].join(':');
    e.initMouseEvent(
      'click',
      true,
      false,
      window,
      0,
      0,
      0,
      0,
      0,
      false,
      false,
      false,
      false,
      0,
      null
    );
    a.dispatchEvent(e);
  }

  static ExportToCSV(content: string, filename: string) {
    if (!content) return;

    const blob = new Blob([content], { type: 'text/csv' });
    const mouseEvent = document.createEvent('MouseEvents');
    const tag = document.createElement('a');

    tag.download = filename;
    tag.href = window.URL.createObjectURL(blob);
    tag.dataset.downloadurl = ['text/csv', tag.download, tag.href].join(':');
    mouseEvent.initMouseEvent(
      'click',
      true,
      false,
      window,
      0,
      0,
      0,
      0,
      0,
      false,
      false,
      false,
      false,
      0,
      null
    );
    tag.dispatchEvent(mouseEvent);
  }
}
