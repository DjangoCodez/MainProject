import xlsx from "node-xlsx";
import { readFileSync } from "fs";
import decompress from "decompress";

export class FileUtils {
  parseXlsx(filePath: string) {
    return new Promise((resolve, reject) => {
      try {
        const jsonData = xlsx.parse(readFileSync(filePath));
        resolve(jsonData);
      } catch (e) {
        reject(e);
      }
    });
  }

  decompress(input: string, output: string = undefined! , opts: () => void = undefined!) {
    return decompress(input, output, opts);
  }
}
