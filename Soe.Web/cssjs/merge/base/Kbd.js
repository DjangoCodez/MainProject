var Kbd = {
	backspace: 8,
	tab: 9,
	enter: 13,
	escape: 27,
	arrowLeft: 37,
	arrowUp: 38,
	arrowRight: 39,
	arrowDown: 40,
	
	setKeydownListener: function(el, key, fn, remove) {
		el.onkeydown = function(e) {
			var k = window.event ? window.event.keyCode : e.which;
			if (k == key) {
				fn();
				if (remove)
					el.onkeydown = null;
			}
		}
	}
}