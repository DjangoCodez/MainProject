import { FieldOrEvaluator, FieldOrPredicate } from "./SoeGridOptionsAg";

export class ObjectFieldHelper {
    public static getFieldFrom<T>(obj: any, field: FieldOrEvaluator<T>): T {
        if (field && obj) {
            return typeof field === "function" ? field(obj) : (obj[field] as T);
        }

        return null;
    }

    public static IsEvaluatedTrue(obj: any, field: FieldOrPredicate): boolean {
        if (field) {
            return typeof field === "function" ? field(obj) : (obj[field] as boolean);
        }

        return null;
    }
}
