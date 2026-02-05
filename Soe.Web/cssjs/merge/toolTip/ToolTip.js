var ToolTip = {
	bubble: null,
	tips: {},

	add: function(element, content) {
		ToolTip.tips[SOE.getID(element)] = content;
		
		element.addEvent('mousemove', ToolTip.move) 
		element.addEvent('mouseout', ToolTip.close) 
	},
	
	create: function ()
	{
	    var el = $$(document.body).create('div', { id: 'ToolTip' }, true);
		el.style.display = 'none';
		el.style.position = 'absolute';
		return el;
	},

	move: function(e) {
		e = e || window.event;
		
		if (!ToolTip.bubble)
		    ToolTip.bubble = ToolTip.create();
		
		ToolTip.bubble.innerHTML = ToolTip.tips[this.id];
		ToolTip.bubble.style.display = 'block';
		ToolTip.bubble.style.top = e.clientY + 17 + 'px';
		ToolTip.bubble.style.left = e.clientX + 12 + 'px';
	},

	close: function() {
		ToolTip.bubble.style.display = 'none';
	}
}
