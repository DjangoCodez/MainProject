var SideBar = {
	fullWidth: 300,
	duration: 500,
	area: null,
	currentContent: null,
	isOpen: null,
	tmpContentElement: null,
	tmpContentName: null,
	
	createArea: function() {
	SideBar.area = $$('content').create('div', { id: 'SideBar' }, true);
//	SideBar.area = $$('nonFooter').create('div', { id: 'SideBar' }, true);
	},
	
	toggle: function(contentElement, contentName, fn) {
		if (!SideBar.area) { // First time - create, fill and show
			SideBar.createArea();
			SideBar.area.appendChild(contentElement);
			SideBar.show(contentName, fn);
		} else if (SideBar.isOpen) {
			if (SideBar.currentContent == contentName) { // Open with same content - hide
				SideBar.hide();
			} else { // Open with other content - hide, remove, fill, show
				SideBar.tmpContentElement = contentElement;
				SideBar.tmpContentName = contentName;
				SideBar.hide();
			}
		} else {
			if (SideBar.currentContent == contentName) { // Closed with same content - show
				SideBar.show(contentName, fn);
			} else { // Closed with other content - remove, fill, show
				SideBar.area.replaceContent('');
				SideBar.area.appendChild(contentElement);
				SideBar.show(contentName, fn);
			}			
		} 
	},
	
	show: function(contentName, fn) {
		SideBar.area.style.display = 'block';
		Animation.width(SideBar.area, SideBar.fullWidth, SideBar.duration);
		SideBar.currentContent = contentName;
		SideBar.isOpen = true;
		if (fn)
			fn();		
	},
	
	hide: function() {
		Animation.width(SideBar.area, 0, SideBar.duration, SideBar.afterHide);
		SideBar.isOpen = false;
	},
	
	afterHide: function() {
		if (!SideBar.isOpen) {
			if (SideBar.tmpContentElement) {
				SideBar.area.replaceContent('');
				SideBar.area.appendChild(sideBar.tmpContentElement);
				SideBar.show(sideBar.tmpContentName);
				SideBar.tmpContentElement = '';
				SideBar.tmpContentName = '';		
			} else {
				SideBar.area.style.display = 'none';
			}
		}
	}
}
