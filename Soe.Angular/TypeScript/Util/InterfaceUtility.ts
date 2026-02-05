export class InterfaceUtility {
    public static interfacePropertyToString = (property: (object: any) => void) => {
        var chaine = property.toString();
        var arr = chaine.match(/[\s\S]*{[\s\S]*\.([^\.; ]*)[ ;\n]*}/);
        return arr[1];
    };
}
