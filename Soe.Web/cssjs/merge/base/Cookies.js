/*
 * Funktioner för hantering av kakor.
 */

var Cookies = {

	/*
	 * Skapa kaka med namn name och värde value. Om days tillhandahålls sparas kakan hos klienten i days dagar,
	 * annars endast under pågående session.
	 */
	create: function(name, value, days) {
		var expires = '';
		if (days) {
			var date = new Date();
			date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
			expires = '; expires=' + date.toGMTString();
		}
		document.cookie = name + "=" + value + expires + '; path=/';
	},

	/*
	 * Läs värde från kaka med namn name. Om kaka saknas returneras null.
	 */
	read: function(name) {
		var nameEQ = name + '=';
		var ca = document.cookie.split(';');
		for(var i = 0;i < ca.length;i++) {
			var c = ca[i];
			while (c.charAt(0) == ' ') c = c.substring(1, c.length);
			if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
		}
		return null;
	},
	
	/*
	 * Radera kaka.
	 */
	erase: function(name) {
		Cookies.create(name, '', -1);
	}
}

