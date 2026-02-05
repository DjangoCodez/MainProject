import { StringUtility } from "../../Util/StringUtility";

export class FormatHtmlFilter {

    private static filter(value: string) {
        // Replace \n with <br/>
        return StringUtility.ToBr(value);
    }

    public static create() {
        return FormatHtmlFilter.filter;
    }
}
