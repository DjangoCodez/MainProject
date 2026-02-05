import { StringUtility } from "../../Util/StringUtility";
const isNumeric = StringUtility.isNumeric;

export class OrderAccountsByStartsWithFirst {
  private static transform(items: any, input: string) {
    if (!items || !input) {
      return items;
    }

    function compare(a: Account, b: Account) {
      const aString = a.accountNr;
      const bString = b.accountNr;
      const aStartsWithInput = aString.startsWith(input);
      const bStartsWithInput = bString.startsWith(input);

      if (aStartsWithInput && bStartsWithInput) 
        return compareStrings(aString, bString);
      if (aStartsWithInput)
        return -1; // a should come before b
      if (bStartsWithInput)
        return 1; // b should come before a

      const aIncludesInput = aString.includes(input);
      const bIncludesInput = bString.includes(input);
      
      if (aIncludesInput == bIncludesInput) // Either both or neither account number include input.
        return compareStrings(aString, bString);
      if (aIncludesInput)
        return -1; // a should come before b
      
      return 1; // b should come before a
    }

    function compareStrings(a: string, b: string) {
      if (a < b)
        return -1;
      if (a > b)
        return 1;

      return 0;
    }

    return items.sort(compare);
  }

  public static create(){
    return OrderAccountsByStartsWithFirst.transform;
  }
}
type Account = {
  accountNr: string;
}