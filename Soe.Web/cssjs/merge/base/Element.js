///*
// * Funktioner för hantering av element
// */

//var Element = {

//	/*
//	 * Hämtar elementet el:s pixelhöjd eller 0 om webbläsarkompatiblitet ej kan detekteras.
//	 */
//	getHeight: function(el) {
//		var c = 0;
//		if (el.offsetHeight) {
//			while (el.offsetHeight) {
//				c += el.offsetHeight;
//				el = el.offsetHeight;
//			}
//		}
//		else if (el.height) {
//			c += el.height;
//		}
//		return c;
//	},

//	/*
//	 * Hämtar elementet el:s pixelbredd eller 0 om webbläsarkompatiblitet ej kan detekteras.
//	 */
//	getWidth: function(el) {
//		var c = 0;
//		if (el.offsetWidth) {
//			while (el.offsetWidth) {
//				c += el.offsetWidth;
//				el = el.offsetWidth;
//			}
//		}
//		else if (el.width) {
//			c += el.width;
//		}
//		return c;
//	},
	
//	/*
//	 * Hämtar elementet el:s pixelavstånd från displayens övre kant eller 0 om webbläsarkompatiblitet ej kan detekteras.
//	 */
//	getTopPos: function(el) {
//		var c = 0;
//		if (el.offsetParent) {
//			while (el.offsetParent) {
//				c += el.offsetTop
//				el = el.offsetParent;
//			}
//		} else if (el.y) {
//			c += el.y;
//		}
//		return c;
//	},
	
//	/*
//	 * Hämtar elementet el:s pixelavstånd från displayens vänstra kant eller 0 om webbläsarkompatiblitet ej kan detekteras.
//	 */
//	getLeftPos: function(el)	{
//		var c = 0;
//		if (el.offsetParent) {
//			while (el.offsetParent) {
//				c += el.offsetLeft
//				el = el.offsetParent;
//			}
//		} else if (el.x) {
//			c += el.x;
//		}
//		return c;
//	}
//}
