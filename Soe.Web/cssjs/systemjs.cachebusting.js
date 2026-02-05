//Try some of the other solutions in https://github.com/systemjs/systemjs/issues/1616 when you updated systemjs. 
//This file can be replaced by the 4 lines of code below in future versions..
//const systemResolve = System.resolve;
//System.resolve = async function (id, parent) {
//    const resolved = await systemResolve.call(this, id, parent);
//    return resolved + '?cache=' + Date.now();
//};

System.enableCacheBusting = function(checksums) {
    //SystemJSLoader$1 -> RegisterLoader$1 -> Loader
    var Loader = System.__proto__.__proto__.__proto__.constructor;
    var loaderResolve = Loader.prototype.resolve;

    function augment(key) {
        if (!key.toLowerCase().match(".+/soe\..+\.(min|bundle).js") || key.indexOf("cs=") > 0) {
            // Not a bundle, do not add checksum
            // or, already has checksum, do not add again
            return key;
        }

        var fn = key.substring(key.lastIndexOf("/") + 1);
        var f = fn.substring(0, fn.lastIndexOf("."));
        f = f.substring(0, f.lastIndexOf("."));
        
        var checksum = checksums[f];
        if (!checksum) {
            // checksum not available
            return key;
        }

        var hI = key.indexOf("#"), qI = key.lastIndexOf("?", hI < 0 ? undefined : hI);
        if (qI > 0) {
            if (hI > 0) {
                //? and # found: put build between ? and #
                key = key.slice(0, hI) + "&cs=" + checksum + key.slice(hI);
            } else {
                //? found: put build at end
                key += "&cs=" + checksum;
            }
        } else {
            //no ? nor #
            key += "?cs=" + checksum;
        }
        return key;
    }

    var metadataSymbol;
    function getMetadataSymbol(loader, mustFindKey) {
        if (metadataSymbol) {
            return metadataSymbol;
        }
        if (loader["@@metadata"]) {
            //some browsers dont support Symbol()
            return metadataSymbol = "@@metadata";
        }
        var symbols = Object.getOwnPropertySymbols(loader);
        for (var i = 0; i < symbols.length; i++) {
            var s = symbols[i], lov = loader[s];
            if (lov && typeof lov === 'object' && lov[mustFindKey]) {
                return metadataSymbol = s;
            }
        }
        throw new Error("getMetadataSymbol unsuccessful");
    }

    Loader.prototype.resolve = function hackedLoaderResolve() {
        var loader = this;
        return Promise
            .resolve(loaderResolve.apply(loader, arguments))
            .then(function (key) {
                var newKey = augment(key);
                if (newKey !== key) {
                    var metaSymbol = getMetadataSymbol(loader, key);
                    var metadata = loader[metaSymbol];
                    var moveMe = metadata[key];

                    if (moveMe) {
                        delete metadata[key];
                        metadata[newKey] = moveMe;
                    }
                }
                return newKey;
            });
    };
}
