/*
 * Funktioner för animering av element.
 */

var Animation = {

	/*
	 * Ändra bredden på elementet el till width under duration millisekunder.
	 * Efter avslut anropas runAfter, om tillhandahållen.
	 */
	width: function(el, width, duration, runAfter) {
		var startWidth = Element.getWidth(el);
		var diff = width - startWidth;
		var frequency = 1 / duration;
		var startTime = new Date().getTime();
		var tmr = setInterval(function() {
			var wi = width;
			var elapsedTime = new Date().getTime() - startTime;
			if (elapsedTime < duration)
				wi = elapsedTime * frequency * diff + startWidth;
			else
				clearInterval(tmr);
			wi = Math.round(wi)
			el.style.width = wi + 'px';
			if (wi == width && runAfter)
				runAfter();	
		}, 10);
	},
	
	/*
	 * Ändra höjden på elementet el till height under duration millisekunder.
	 * Efter avslut anropas runAfter, om tillhandahållen.
	 */
	height: function(el, height, duration, runAfter) {
		var startHeight = Element.getHeight(el);
		var diff = height - startHeight;
		var frequency = 1 / duration;
		var startTime = new Date().getTime();
		var tmr = setInterval(function() {
			var he = height;
			var elapsedTime = new Date().getTime() - startTime;
			if (elapsedTime < duration)
				he = elapsedTime * frequency * diff + startHeight;
			else
				clearInterval(tmr);
			he = Math.round(he)
			el.style.height = he + 'px';
			if (he == height && runAfter)
				runAfter();	
		}, 10);
	},	
	
	leftPos: function(el, leftPos, duration, runAfter) {
		var startLeftPos = Element.getLeftPos(el);
		var diff = leftPos - startLeftPos;
		var frequency = 1 / duration;
		var startTime = new Date().getTime();
		var tmr = setInterval(function() {
			var lp = leftPos;
			var elapsedTime = new Date().getTime() - startTime;
			if (elapsedTime < duration)
				lp = elapsedTime * frequency * diff + startLeftPos;
			else
				clearInterval(tmr);
			lp = Math.round(lp)
			el.style.left = lp + 'px';
			if (lp == leftPos && runAfter)
				runAfter();	
		}, 10);
	},
	
	leftMargin: function(el, leftMargin, duration, runAfter) {
		var startLeftMargin = el.style.marginLeft ? parseInt(el.style.marginLeft.replace('px', '')) : 0;
		var diff = leftMargin - startLeftMargin;
		var frequency = 1 / duration;
		var startTime = new Date().getTime();
		var tmr = setInterval(function() {
			var lp = leftMargin;
			var elapsedTime = new Date().getTime() - startTime;
			if (elapsedTime < duration)
				lp = elapsedTime * frequency * diff + startLeftMargin;
			else
				clearInterval(tmr);
			lp = Math.round(lp)
			el.style.marginLeft = lp + 'px';
			if (lp == leftMargin && runAfter)
				runAfter();	
		}, 10);
	}	
}
