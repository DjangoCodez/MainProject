var Popup={
    overlay: null,
    ds1: null,
    getElement: function() {
        return Popup.ds1;
    },
    //Show a popup with html content.
    showInnerHtml: function (htmlString) {
        var container = Popup.createContainer();
        container.innerHTML=htmlString;
        Popup.showIt();
        return container;
    },
    //Positions the popup and displays it.
    showIt: function() {
        if(document.body==null)//prevent crash
          return;

        Popup.ds1.style.left='10000px';
        Popup.ds1.style.display='block';
        document.body.appendChild(Popup.overlay);
        document.body.appendChild(Popup.ds1);
        Popup.ds1.style.top=((Display.getHeight()-Popup.getHeight(Popup.ds1))/3)+'px';
        Popup.ds1.style.left = ((Display.getWidth() - Popup.getWidth(Popup.ds1)) / 2) + 'px';

        $(window).bind('load', function () {
            Popup.positionBackground();
        });
    },
    //Positions the background.
    positionBackground: function() {
        if(Popup.overlay) {
            Popup.overlay.style.width=Display.getWidth()+'px';
            Popup.overlay.style.height=Display.getHeight()+'px';
        }
    },
    //Removes the popup. Default onclick event for buttons.
    hide: function() {
        $(Popup.ds1).remove();
        $(Popup.overlay).remove();
        Display.showScrollBars();
        return false;
    },
    //Creates the popup and returns the element that should be populated with content.
    createContainer: function() {
        // Shade
        Popup.overlay = document.createElement('div');
        Popup.overlay.className = 'Popup_overlay';

        Popup.overlay.style.top = 0;
        Popup.overlay.style.left = 0;

        Display.hideScrollBars();
        Popup.positionBackground();
        // Dropshadow
        Popup.ds1=document.createElement('div');
        var ds2=document.createElement('div');
        var ds3=document.createElement('div');
        var ds4 = document.createElement('div');
       
        Popup.ds1.className='ds1 Popup';
        ds2.className='ds2';
        ds3.className='ds3';
        ds4.className = 'ds4';
       
        ds3.appendChild(ds4);
       
        ds2.appendChild(ds3);
      
        Popup.ds1.appendChild(ds2);

       
        return ds4;
    },
    /*
    	* Hämtar elementet el:s pixelhöjd eller 0 om webbläsarkompatiblitet ej kan detekteras.
    	*/
    getHeight: function(el) {
    	var c = 0;
    	if (el.offsetHeight) {
    		while (el.offsetHeight) {
    			c += el.offsetHeight;
    			el = el.offsetHeight;
    		}
    	}
    	else if (el.height) {
    		c += el.height;
    	}
    	return c;
    },

    /*
    	* Hämtar elementet el:s pixelbredd eller 0 om webbläsarkompatiblitet ej kan detekteras.
    	*/
    getWidth: function(el) {
    	var c = 0;
    	if (el.offsetWidth) {
    		while (el.offsetWidth) {
    			c += el.offsetWidth;
    			el = el.offsetWidth;
    		}
    	}
    	else if (el.width) {
    		c += el.width;
    	}
    	return c;
    },
}
