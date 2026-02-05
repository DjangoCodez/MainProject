/*
 * URI handling functions
 */

var Uri = {

	/*
	 * Extracts the directory part from a uri.
	 * Example: Directory part in http://www.example.com/folder/file.html?a=1&b=2 is /folder/
	 * If no uri is provided, current uri is used.
	 */
	getDir: function(uri) {
		uri = uri || window.location;
		var uri = Uri.getPathAndQuery(uri);
		return uri.substring(0, uri.lastIndexOf('/')) + '/';	
	},

	/*
	 * Extracts the directory, file and query part from a uri.
	 * Exempel: Directory, file and query part in http://www.example.com/folder/file.html?a=1&b=2
	 * is /folder/file.html?a=1&b=2
	 * If no uri is provided, current uri is used.
	 */
	getPathAndQuery: function(uri) {
		uri = uri || window.location;
		uri = String(uri).substring(8);	
		return uri.substring(uri.indexOf('/'));
	}

	/*
	 * Extrahera ordlista med parametrarna ur parametersträngen s.
	 * Om s ej tillhandahålls används parameterdelen från aktuell url.
	 */	
	//getQuery: function(s) {
	//	s = s || window.location;
	//	var dict = new Object;
	//	var ss = String.split(s, '?');		
	//	if (ss.length == 2) {
	//		var sss = String.split(ss[1], '&');
	//		for (i = 0; i < sss.length; i++) {
	//			var qs = String.split(sss[i], '=');
	//			dict[qs[0]] = qs[1];
	//		}
	//	}
	//	return dict;
	//}	
}
