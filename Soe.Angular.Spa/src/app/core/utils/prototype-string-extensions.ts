export default class PrototypeStringExtensions {}

declare global {
  interface String {
    right: (length: number) => string;
    left: (length: number) => string;
    startsWithCaseInsensitive: (
      searchString: string,
      position?: number
    ) => boolean;
    endsWithCaseInsensitive: (searchString: string) => boolean;
    removeWhitespaces: () => string;
    replaceCommaWithDot: () => string;
    replaceDashWithMinus: () => string;
    toSnakeCase: (toLower?: boolean) => string;
    toUpperCaseFirstLetter: () => string;
    format: (...args: string[]) => string;
  }
}

String.prototype.right = function (length: number): string {
  return this.substring(this.length - length);
};

String.prototype.left = function (length: number): string {
  return this.substring(0, length);
};

String.prototype.startsWithCaseInsensitive = function (
  searchString: string,
  position?: number
): boolean {
  if (!searchString) return true;

  position = position || 0;
  return (
    this.substring(position, searchString.length).toLowerCase() ===
    searchString.toLowerCase()
  );
};

String.prototype.endsWithCaseInsensitive = function (
  searchString: string
): boolean {
  if (!searchString) return true;

  return (
    this.toLowerCase().indexOf(
      searchString.toLowerCase(),
      this.length - searchString.length
    ) !== -1
  );
};

String.prototype.removeWhitespaces = function (): string {
  return this.replace(/\s+/g, '');
};

String.prototype.replaceCommaWithDot = function (): string {
  return this.replace(',', '.');
};

String.prototype.replaceDashWithMinus = function (): string {
  return this.replace('−', '-');
};

String.prototype.toSnakeCase = function (toLower = false): string {
  let str = this.replace(/[\. ,:-]+/g, '_');
  if (toLower) str = str.toLocaleLowerCase();

  return str;
};

String.prototype.toUpperCaseFirstLetter = function (): string {
  return this.left(1).toUpperCase() + this.substring(1).toLowerCase();
};

String.prototype.format = function (...args: string[]): string {
  let formatted = this.toString();
  for (let i = 0; i < arguments.length; i++) {
    if (arguments[i])
      formatted = formatted.replace(
        RegExp('\\{' + i + '\\}', 'g'),
        arguments[i].toString()
      );
  }

  // Replace remaing placeholders with empty string.
  // (In case original string contains more placeholders than arguments).
  formatted = formatted.replace(/{\d+}/g, '');

  return formatted;
};
