${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;
    // This custom function will take the type currently being processed and
    // append a ? next to any type that is nullable
    Template(Settings settings)
    {
        settings
            .IncludeProject("Soe.Common")
            .IncludeProject("Soe.WebApi")
            .DisableStrictNullGeneration();
    }
    static string debug = "";

    static HashSet<string> locals = new HashSet<string>();
    static HashSet<string> classImports = new HashSet<string>();

    static string currentFile = "";

    string GetFileName(File f){
        return f.Name.Replace(".ts", "");
    }

    void AppendLog(string log) {
        debug += "\n//" + log;
    }

    string Debug(File f) {
        return debug;
    }

    string FormatInterface(string typeName) {
        return "I" + typeName;
    }

    string OnNewFile(File f) {
        locals = new HashSet<string>();
        classImports = new HashSet<string>();
        currentFile = GetFileName(f);
        debug = "";
        return "";
    }

    void RecursiveGetClasses(Class c) {
        string className = FormatInterface(c.Name);
        if (locals.Any(l => l == className)) {
            return;
        }
        locals.Add(className);
        foreach (var nested in c.NestedClasses) {
            RecursiveGetClasses(nested);
        }
        foreach (var nested in c.NestedEnums) {
            locals.Add(nested.Name);
        }
    }

    void GetClassesOfFile(File f) {
        foreach (var c in f.Classes) {
            RecursiveGetClasses(c);
        }
        foreach (var e in f.Enums) {
            locals.Add(e.Name);
        }
    }

    string FindReferences(File f) {
        var filemap = new Dictionary<string, List<string>>();
        List<string> references = classImports.ToList();

        string sourceFilePth = f.FullName;
        string newSource="NewSource";
        int idx = sourceFilePth.IndexOf(newSource);		
        string rootPath = sourceFilePth.Substring(0, idx + newSource.Length);

        string path = rootPath + "\\Soe.Angular.Spa\\src\\app\\shared\\models\\generated-interfaces";
        var files = System.IO.Directory.GetFiles(path);

        foreach (var file in files) {
            if (!references.Any()) break;
            foreach(var line in System.IO.File.ReadLines(file)) {
                if (!line.Contains("export ")) continue;
                if (!references.Any()) break;

                for (int i = 0; i < references.Count; i++) {
                    if (line.Contains(" " + references[i] + " ") || line.Contains(" I" + references[i] + " ")) {
                        if (filemap.TryGetValue(file, out List<string> classes)) {
                            classes.Add(references[i]);
                        } else {
                            filemap.Add(file, new List<string>() { references[i] });
                        }

                        references.RemoveAt(i);
                        break;
                    }
                }

            }
        }

        string imports = "";
        foreach (var item in filemap.Keys) {
            var objNames = string.Join(", ", filemap[item]);
            imports += "\nimport { " + objNames + " } from \"" + GetRelativePath(item) + "\";";
        }
        return imports;
    }

    string GetRelativePath(string fullPath) {
        //FROM: "./C:\Repos\Main\NewSource\Soe.Angular.SpaNew\ImportedModels\PurchaseDTOs.ts";
        //TO: "../ImportedModels/PurchaseDTOs";

        return "../" + fullPath.Substring(fullPath.IndexOf("generated-interfaces")).Replace(".ts", "").Replace("\\", "/");
    }

    string TypeFormatted(Property property)
    {
        var type = property.Type;

        if (type.IsNullable)
        {
            return  $"?";
        }
        else
        {
            return  $"";
        }
    }

    string HandleComplexType(Property property) {
        string type = property.Type.Name;

        void HandleType(Type argType) {
            bool isEnum = argType.IsEnum;
            string adjName = "";
            bool replaceSeparator = false;;

            if (isEnum) {
                adjName = argType.Name;
                if (!locals.Contains(argType.Name)) {
                    classImports.Add(argType.Name);
                }
            }
            else if (argType.IsPrimitive) {
                adjName = argType.Name;
            }
            else if (argType.Name.Equals("any")) {
                adjName = argType.Name;
            }
            else if (argType.IsEnumerable) {
                adjName = argType.Name;
            }
            else {
                adjName = FormatInterface(argType.Name);
                if (!locals.Contains(FormatInterface(argType.Name))) {
                    classImports.Add(adjName);
                }
            }

            string toBeReplaced = replaceSeparator ? ": " + argType.Name : argType.Name;

            type = type.Replace(toBeReplaced, adjName);
        }

        if (property.Type.TypeArguments.Any()) {
            foreach (var arg in property.Type.TypeArguments) {
                if (arg.TypeArguments.Any()) {
                    foreach (var argType in arg.TypeArguments) {
                        HandleType(argType);
                    }
                }
                else {
                    HandleType(arg);
                }
            }
        }
        else {
            HandleType(property.Type);
        }
        return type;
    }

    string ParsePropForInterface(Property property) {
        return $"{property.name}{TypeFormatted(property)}: {HandleComplexType(property)};";
    }

    bool IncludeClass(Class c) {
        if (!c.Attributes.Any(a => a.Name.Equals("TSInclude"))) {
            return false;
        }
        if (c.TypeParameters.Any() && (c.Name.Equals("GenericType"))) {
            return false;
        }
        if (c.Name.Equals("DynamicEntity")) {
            return false;
        }
        return true;
    }

    bool IncludeProperty(Property property) {
        if (property.Attributes.Any(a => a.Name.Equals("TSIgnore"))) {
            return false;
        }
        return true;
    }
}
// This file was generated by TypeWriter. See TypeWriter.Common.tst for reference.
$OnNewFile
$GetClassesOfFile
$Classes($IncludeClass)[
    export interface I$Name$TypeParameters {$Properties($IncludeProperty)[
        $ParsePropForInterface]
    }

    $NestedClasses()[
    export interface I$Name$TypeParameters {$Properties[
        $ParsePropForInterface]
    }]

    $NestedEnums()[
    export enum $Name {$Values[
        $Name = $Value][,]
    }]
]


$Enums(*)[
export enum $Name { $Values[
    $Name = $Value][,]
}
]

$FindReferences
$Debug