/*
 * Funktionen för hantering av webläsarens display.
 */

var Display = {

	/*
	 * Returnerar pixelbredden på displayen eller 0 om webbläsarkompatiblitet ej kan detekteras.
	 */
	getWidth: function() {
		if (window.innerWidth)return window.innerWidth;
		if (document.documentElement && document.documentElement.clientWidth) return document.documentElement.clientWidth;
		if (document.body) return document.body.clientWidth;
		return 0;		
	},

	/*
	 * Returnerar pixelhöjden på displayen eller 0 om webbläsarkompatiblitet ej kan detekteras.
	 */
	getHeight: function() {
		if (window.innerHeight) return window.innerHeight;
		if (document.documentElement && document.documentElement.clientHeight) return document.documentElement.clientHeight;
		if (document.body) return document.body.clientHeight;
		return 0;	
	},
	
	/*
	 * Returnerar displayens pixelförskjutning av rullning i sidled eller 0 om webbläsarkompatiblitet ej kan detekteras.
	 */
	getLeft: function() {
		if (window.pageXOffset) return window.pageXOffset;
		if (document.documentElement && document.documentElement.scrollLeft) return document.documentElement.scrollLeft;
		if (document.body) return document.body.scrollLeft;
		return 0;		
	},
	
	/*
	 * Returnerar displayens pixelförskjutning av rullning i höjdled eller 0 om webbläsarkompatiblitet ej kan detekteras.
	 */
	getTop: function() {
		if (self.pageYOffset) return self.pageYOffset;
		if (document.documentElement && document.documentElement.scrollTop) return document.documentElement.scrollTop;
		if (document.body) return document.body.scrollTop;
		return 0;	
	},
	
	/*
	 * Döljer rullningslister.
	 */
	hideScrollBars: function () {
	    //doc.elmsByTag('html')[0].style.overflow = 'hidden';
	    $(doc).find('html')[0].style.overflow = 'hidden';
	},
	
	/*
	 * Visar rullningslister.
	 */
	showScrollBars: function() {
	   // doc.elmsByTag('html')[0].style.overflow = 'auto';
	    $(doc).find('html')[0].style.overflow = 'auto';
	}	
}
