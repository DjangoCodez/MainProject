var searchComponent=
{
    open: false,
    items: {},
    initContainer: "",
    activeIndex: -1,
    maxLength: 0,
    indexPrefix: "",
    containerClass: 'searchContainer',
    containerId: 'searchContainerAccounts', //deprecated
    onKeyMethodName: '',
    setValueMethodName: '',
    getHeight: function (el) {
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
    getTopPos: function (el) {
        var c = 0;
        if (el.offsetParent) {
            while (el.offsetParent) {
                c += el.offsetTop
                el = el.offsetParent;
            }
        } else if (el.y) {
            c += el.y;
        }
        return c;
    },

    getLeftPos: function (el) {
        var c = 0;
        if (el.offsetParent) {
            while (el.offsetParent) {
                c += el.offsetLeft
                el = el.offsetParent;
            }
        } else if (el.x) {
            c += el.x;
        }
        return c;
    },

    init: function(items,content,timeout,searchInitFrom,indexPrefix,onKeyMethodName,setValueMethodName,maxLengthNum,maxLengthName) {
        searchComponent.indexPrefix=indexPrefix;
        searchComponent.open=false;
        searchComponent.activeIndex=0;
        searchComponent.items=items;
        searchComponent.initContainer=searchInitFrom;
        searchComponent.render(content,maxLengthNum,maxLengthName);
        searchComponent.maxLength=items.length-1;
        searchComponent.onKeyMethodName=onKeyMethodName;
        searchComponent.setValueMethodName=setValueMethodName;
    },
    render: function(content,maxLengthNum,maxLengthName) {
        var op=$$(searchComponent.containerId);
        if(op!=null)
            document.body.removeChild(op);
        searchComponent.open=true;
        content.style.display="block";
        var input=$$(searchComponent.initContainer);
        //input.style.width=((maxLengthNum*8)+(maxLengthName*8))+"px";
        document.getElementById(searchComponent.initContainer).onkeydown=searchComponent.keyDown;
        var list=document.createElement('div');
        list.id=searchComponent.containerId;
        list.setAttribute('class',searchComponent.containerClass);
        list.setAttribute('className',searchComponent.containerClass);
        list.innerHTML=content.innerHTML;
        list.style.position='absolute';
        list.style.top = String(searchComponent.getHeight(input) + searchComponent.getTopPos(input)-1)+'px';
        list.style.left = String(searchComponent.getLeftPos(input)-1)+'px';
        list.scrollTop=0;
        document.body.appendChild(list);
        for(var i=0;i<searchComponent.items.length;i++) {
            var numDiv=document.getElementById("extendNumWidth_"+i);
            if(numDiv!=null)
                numDiv.style.width=(maxLengthNum*7)+"px";
            var nameDiv=document.getElementById("extendNameWidth_"+i);
            if(nameDiv!=null)
                nameDiv.style.width=((maxLengthName*7))+"px";
        }
    },
    dispose: function() {
        var op=$$(searchComponent.containerId);
        if(op!=null)
            document.body.removeChild(op);
        searchComponent.open=false;
    },
    next: function() {
        searchComponent.unselect();
        if(searchComponent.activeIndex++ <searchComponent.maxLength)
            searchComponent.activeIndex=searchComponent.activeIndex++;
        else
            searchComponent.activeIndex=0;
        searchComponent.selectByIndex();

    },
    previous: function() {
        searchComponent.unselect();
        if(searchComponent.activeIndex-- >0)
            searchComponent.activeIndex=searchComponent.activeIndex--;
        else
            searchComponent.activeIndex=searchComponent.maxLength;
        searchComponent.selectByIndex();
    },
    select: function() {
        if(!e) {
            var e=window.event;
            var selectIndex;
            var parent;
            if(e.toElement&&e.toElement.parentNode.id.match(searchComponent.indexPrefix)!=null) {
                parent=$(e.toElement.parentNode);
                selectIndex=e.toElement.parentNode.id.replace(searchComponent.indexPrefix,'');
            }
            else if(e.fromElement&&e.fromElement.parentElement.id.match(searchComponent.indexPrefix)!=null) {
                parent=$(e.fromElement.parentElement);
                selectIndex=e.fromElement.parentElement.id.replace(searchComponent.indexPrefix,'');
            }
            if(parent&&parent.hasClass&&!parent.hasClass('selected')) {
                parent.addClass('selected');
                searchComponent.unselect();
                searchComponent.activeIndex=selectIndex;
            }
        }
    },
    unselect: function() {
        if(searchComponent.activeIndex> -1) {
            var target=document.getElementById(searchComponent.indexPrefix+searchComponent.activeIndex);
            var t=$(target);
            if(t&&t.hasClass('selected'))
                t.removeClass('selected');
        }
    },
    selectByIndex: function() {
        if(searchComponent.activeIndex> -1) {
            var target=document.getElementById(searchComponent.indexPrefix+searchComponent.activeIndex);
            var t=$(target);
            if(t&&!t.hasClass('selected'))
                t.addClass('selected');
        }
    },
    choose: function() {
        eval(searchComponent.setValueMethodName+'(searchComponent.items[searchComponent.activeIndex]);');
        searchComponent.dispose();
    },
    keyDown: function (e) {
        var evtobj=window.event?event:e
        var key=window.event?window.event.keyCode:e.which;
        switch(key) {
            case Kbd.enter:
                if(searchComponent.open)
                    searchComponent.choose();
                return false;
                break;
            case Kbd.tab:
                if(!searchComponent.open)
                    return true;
                else
                    searchComponent.choose();
                return true;
                break;
            case Kbd.arrowUp:
            case Kbd.arrowLeft:
                searchComponent.previous();
                return false;
                break;
            case Kbd.arrowDown:
            case Kbd.arrowRight:
                searchComponent.next();
                return false;
                break;
            case Kbd.escape:
                searchComponent.dispose();
                return false;
                break;
            case 8:
            case 46:
                eval(searchComponent.onKeyMethodName+'();');
                break;
        }
        if((key>47&&key<91)||(key>95&&key<106))
            eval(searchComponent.onKeyMethodName+'();');
        return true;
    }
}
