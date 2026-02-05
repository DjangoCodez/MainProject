export class FileUtility {
  public static base64StringToByteArray(base64String: string) {
    // Decode the Base64 string to a byte array
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    return new Uint8Array(byteNumbers);
  }

  public static createFormData(
    filename: string,
    extension: string,
    base64String: string,
    binaryContent: Uint8Array
  ) {
    const type = this.getTypeFromExtension(extension);

    let blob: Blob;
    if (base64String) {
      const byteArray = this.base64StringToByteArray(base64String);
      blob = new Blob([byteArray], {
        type: type,
      });
    } else {
      // Ensure binaryContent is a valid BlobPart (Uint8Array backed by ArrayBuffer)
      const uint8 = new Uint8Array(binaryContent);
      blob = new Blob([uint8], {
        type: type,
      });
    }

    const file = new File([blob], filename, {
      type: type,
    });

    const formData = new FormData();
    formData.append('file', file, file.name);

    return formData;
  }

  public static getTypeFromExtension(extension: string) {
    let mime = '';
    switch (extension.replace('.', '')) {
      case 'png':
        mime = 'image/png';
        break;
      case 'jpeg':
      case 'jpg':
        mime = 'image/jpeg';
        break;
      case 'gif':
        mime = 'image/gif';
        break;
      case 'pdf':
        mime = 'application/pdf';
        break;
      case 'txt':
        mime = 'text/plain';
        break;
      case 'doc':
      case 'docx':
        mime = 'application/msword';
        break;
      case 'xls':
      case 'xlsx':
        mime = 'application/excel';
        break;
      case 'xml':
        mime = 'application/xml';
        break;
      case 'zip':
        mime = 'application/x-compressed';
        break;
      default:
        mime = 'application/unknown';
        break;
    }
    return mime;
  }
}
