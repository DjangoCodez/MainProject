export class StringUtil {
  // CONVERT
  static camelCaseWord(word: string): string {
    return word
      ? word.substring(0, 1).toLocaleUpperCase() +
          word.substring(1).toLocaleLowerCase()
      : '';
  }

  static newLineToBr(text: string) {
    // Replace \n with <br/>
    if (text) {
      // Difference between writing \n in code and storing it in the database!
      text = text.replace(/(?:\\r\\n|\\r|\\n)/gm, '<br />');
      text = text.replace(/(?:\r\n|\r|\n)/gm, '<br />');
    }

    return text;
  }

  static cloneObject(obj: any) {
    return JSON.parse(JSON.stringify(obj));
  }

  static sortLocale(a: any, b: any) {
    return a.localeCompare(b);
  }

  static uniqueFilter(value: any, index: number, self: any) {
    return self.indexOf(value) === index;
  }

  // VALIDATE
  static isEmpty(str: string): boolean {
    return !str || 0 === str.length;
  }

  static isNumeric(str: string): boolean {
    return !isNaN(Number(str));
  }

  // SPECIAL
  static base64ToByteArray(base64: string) {
    const binaryString = window.atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return Array.from(bytes);
  }

  static getFileExtension(path: string): string {
    const basename = path.split(/[\\/]/).pop() || '', // extract file name from full path ...
      // (supports `\\` and `/` separators)
      pos = basename.lastIndexOf('.'); // get last position of `.`

    if (basename === '' || pos < 1)
      // if file name is empty or ...
      return ''; //  `.` not found (-1) or comes first (0)

    return basename.slice(pos + 1); // extract extension ignoring `.`
  }

  static WildCardToRegEx(wildCard: string) {
    let s = '^';
    const length = (wildCard && wildCard.length) || 0;

    for (let i = 0; i < length; i++) {
      const c = wildCard[i];
      switch (c) {
        case '*':
          s += '.*';
          break;
        case '?':
          s += '.';
          break;
        // Escape special regexp-characters
        case '(':
        case ')':
        case '[':
        case ']':
        case '$':
        case '^':
        case '.':
        case '{':
        case '}':
        case '|':
        case '\\':
          s += '\\';
          s += c;
          break;
        default:
          s += c;
          break;
      }
    }
    s += '$';

    return s;
  }
}

export class Guid {
  static newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0,
          v = c == 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      }
    );
  }
}
