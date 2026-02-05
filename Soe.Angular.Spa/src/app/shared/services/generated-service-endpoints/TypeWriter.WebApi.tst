${
    using Typewriter.Extensions.WebApi;

    Template(Settings settings)
    {
        settings.IncludeProject("Soe.WebApi");
        settings.OutputFilenameFactory = file => 
        {
            // Get parent folder name (Billing, Economy, Manage or Time)
            string folderName = "";
            string[] folders = file.FullName.Split('\\');
            if (folders.Length > 1) {
                folderName = folders[folders.Length - 2].ToLower();
            }

            string finalFileName = file.Name.Replace("Controller", "");
            finalFileName = finalFileName.Replace(".cs", "");
            
            // Add folder
            if (folderName.Length > 0)
                finalFileName = folderName + "/" + finalFileName;

            return $"{finalFileName}.endpoints.ts";
        };
        settings.DisableStrictNullGeneration();
    }

    static string debug = "";
    static HashSet<string> methodNames = new HashSet<string>();
    static List<string> skipParams = new List<string>() {"HttpRequestMessage"};
    static List<string> replaceParams = new List<string>() {"number", "number[]", "string", "boolean"};

    void AppendLog(string log) {
        debug += "\n//" + log;
    }
    
    string Debug(File f) {
        return debug;
    }

    void OnNewFile(File f) {
        methodNames = new HashSet<string>();
    }

    string ServiceName(Class c) => c.Name.Replace("Controller", "Service");

    string GetUrlMethod(Method m) {
        string methodName = m.name;
        int i = 0;
        while (methodNames.Contains(methodName)) {
            methodName += i;
        }
        methodNames.Add(methodName);
        return $"export const {methodName} = ({FormatParameters(m.Parameters, m)}) => {GetFormattedEndpoint(m.Url(), m.Parameters)};";
    }

    string FormatParameters(IParameterCollection pc, Method m) {
        if (m.HttpMethod() == "post") {
            return GetParametersInUrl(m.Url(), pc);
        }
        return $"{GetFormattedParameters(pc)}";
    }

    string GetParametersInUrl(string url, IParameterCollection pc) {
        List<string> parameters = new List<string>();
        foreach (var p in pc) {
            if (url.Contains(p.name)) {
                string type = p.Type.Name;
                if (!replaceParams.Contains(type))
                    type = "number";
                parameters.Add($"{p.name}: {type}");
            }
        }
        return string.Join(", ", parameters);
    }

    string GetFormattedParameters(IParameterCollection pc) {
        List<string> parameters = new List<string>();
        foreach (var p in pc) {
            if (!skipParams.Contains(p.Type)) {
                string type = p.Type.Name;
                string optionalFlag = p.Type.FullName.EndsWith("?") ? "?" : "";
                if (!replaceParams.Contains(type))
                    type = "number";
                parameters.Add($"{p.name}{optionalFlag}: {type}");
            }
        }
        return string.Join(", ", parameters);
    }

    string GetFormattedEndpoint(string url, IParameterCollection pc) {
        string[] qsParts = url.Split('?'); // Has optional parameters as a querystring
        foreach (var p in pc) {
            if (qsParts.Length == 2) {
                if (p.Type.FullName.EndsWith("?") && qsParts[1].Contains(p.name)) {
                    // Do not add any null-check value for this parameter.
                    continue;
                }
            }
            if (p.Type.FullName.EndsWith("?") && url.Contains(p.name)) { 
                url = url.Replace(p.name, p.name + " || ''");
            }
        }
        return $"`{url}`";
    }

    string GetComment(Method m) {
        string httpMethod = m.HttpMethod();
        if (httpMethod == "post") {
            return $"{httpMethod}, takes args: ({GetFormattedParameters(m.Parameters)})";
        }
        return $"{httpMethod}";
    }

    bool IncludeController(Class c) {
        return c.Namespace.StartsWith("Soe.WebApi.V2");
    }

    bool IncludeMethod(Method m) {
        return true;
    }
}
$OnNewFile
$Classes($IncludeController)[
//Available methods for $Name
$Methods($IncludeMethod)[
//$GetComment
$GetUrlMethod
]
]
$Debug