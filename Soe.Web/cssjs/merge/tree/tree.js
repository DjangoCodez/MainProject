/*var tree = {
	init: function() {
		foreach (getElementsByClassName('tree', 'ul', document), function(ul) {
			foreach (ul.childNodes, function(li) {
				if (li.nodeType == element.ELEMENT_NODE) {
					if (li.tagName == 'LI') {
						var a = element.getChild(li, 'a');
						if (a)
							a.onclick = tree.toggleNode;
						var ipt = element.getChild(li, 'input');
						ipt.onclick = tree.toggleSubNodes;
					}
				}
			});
		});
	},
	
	toggleNode: function() {
		var li = this.parentNode;
		var img = element.getChild(this, 'img');
		var isOpen = element.hasClass(li, 'open');
		if (isOpen) {
			img.src = '/StyleAndBehaviuor/tree/tree-open.gif';
			element.removeClass(li, 'open');
			element.addClass(li, 'closed');
		} else {
			img.src = '/StyleAndBehaviuor/tree/tree-close.gif';
			element.removeClass(li, 'closed');
			element.addClass(li, 'open');
		}
		return false;		
	},
	
	toggleSubNodes: function() {
		var chk = this;
		var ul = element.getChild(chk.parentNode.parentNode, 'ul');
		foreach (ul.childNodes, function(li) {
			if (li.nodeType == element.ELEMENT_NODE) {
				if (li.tagName == 'LI') {
					var subChk = element.getChild(li, 'input');
					subChk.disabled = !chk.checked;
					subChk.checked = false;
				}
			}
		});		
	}	
}

win.addEvent('load', tree.init);
});*/
