var MENU_COLLAPSED = 'SESSION_MENU_COLLAPSED'; //Must conform with Constants.SESSION_MENU_COLLAPSED in Constants.cs

function whichTransitionEvent() { // find which transitionEvent to use, different browsers support different events, and Chrome supports more than one
    var t,
        el = document.createElement("fakeelement");

    var transitions = {
        "WebkitTransition": "webkitTransitionEnd",
        "transition": "transitionend",
        "OTransition": "oTransitionEnd",
        "MozTransition": "transitionend"

    };

    for (t in transitions) {
        if (el.style[t] !== undefined) {
            return transitions[t];
        }
    }
}

var ignoreEvent = false;
var events = whichTransitionEvent();

//Occurres when a module is clicked
$("[data-collapse-group='moduleDivs']").click(function (e) {

    var $this = $(this);

    //Prevent nav-tabs from disappearing when clicking on an active module
    if ($this.hasClass("module-active")) {
        return false;
    }

    var activeHeader = $("#ActiveHeader");
    var activeHeaderName = $this[0].attributes["data-value"].value;
    activeHeader.text(activeHeaderName);

    //Set module as active
    $($(this).data("target")).addClass("module-active");
    $($(this).data("target")).removeClass("collapse").removeClass("module-inactive");
    toggleMenu();

    //Ensures that only tabs for the active menu shows
    $("[data-collapse-group='moduleDivs']:not([data-target='" + $this.data("target") + "'])").each(function () {
        $($(this).data("target")).removeClass("in").removeClass("show").addClass('collapse').addClass("module-inactive");
        $(this).removeClass("module-active");
    });

    var icon = $(this).find(".module-icon")[0];
    if (icon) {
        var classes = icon.classList;
        var iconName = classes[classes.length - 1];
        setModuleIcon(iconName);
    }
});

//Occurres when active module is clicked in collapsed mode
$("ul.module-selector-active").click(function (e) {
    e.preventDefault();
    $("#wrapper").toggleClass("toggled");
});

//Occurres when "hamburger menu" is clicked
$(".menu-toggle").click(function (e) {
    e.preventDefault();
    toggleMenu();
});

function toggleMenu() {
    $("#wrapper").toggleClass("toggled");
    $(".hamburger-menu").toggleClass("fa-chevron-down");
    $(".hamburger-menu").toggleClass("fa-chevron-up");
    $(window).trigger('resize');
}

//Occures when clicking on the activeheader text
$("#ActiveHeader").click(function (e) {
    e.preventDefault();
    toggleMenu();
});

$(".hover-trigger").hover(function (e) {
    e.preventDefault();
    clearHover();

    if ($(".collapse-menu").length === 0 && $(e.currentTarget.parentElement).hasClass("active"))
        return;

    var parentElement = e.currentTarget.parentElement;
    var menuToHover = getElementToHover(parentElement);
    menuToHover.classList.add("hover-menu");

    var topPosition = parentElement.getBoundingClientRect().top;
    handleHoverMenu(menuToHover, topPosition);
});

function getElementToHover(parentElement) {
    var element = null;
    for (var i = 0; i < parentElement.childNodes.length; i++) { // to support IE
        if (parentElement.childNodes[i].classList.contains("tab-pane")) {
            element = parentElement.childNodes[i];
        }
    }
    return element;
}

function handleHoverMenu(menuToHover, menuPosition) {
    var currentHeight = menuToHover.getBoundingClientRect().height;

    var bottomPosition = document.documentElement.clientHeight - (menuPosition + currentHeight); // calculate bottom position of hover menu
    if (bottomPosition >= 0) { // bottom of hover menu is above bottom of page, open downwards
        menuToHover.style.top = menuPosition - 3 + "px";
        menuToHover.style.bottom = "";
    }
    else {
        var menuHeight = menuToHover.getBoundingClientRect().height;
        var maxMenuHeight = document.documentElement.clientHeight - menuPosition;
        if (menuHeight > maxMenuHeight) {
            menuToHover.style.bottom = "5px";
            menuToHover.style.top = "";
        } else {
            menuToHover.style.top = menuPosition + "px";
            menuToHover.style.bottom = "";
            menuToHover.style.maxHeight = maxMenuHeight + "px";
        }
        menuToHover.style.overflowY = "auto";
    }
}

$("#MenuBottom").mouseenter(function (e) {
    e.preventDefault();
    clearHover();
});

$(".main-content").mouseenter(function (e) {
    e.preventDefault();
    if (ignoreEvent) {
        ignoreEvent = false;
        return;
    }
    clearHover();
});

$("#TopLevel").mouseenter(function (e) {
    e.preventDefault();
    clearHover();
});

//When moving up and down in menu, when moving into and expanded menu hover-menu needs to be cleared.
$(".nav-tabs > li").mouseenter(function (e) {
    e.preventDefault();
    if ($(".collapse-menu").length === 0 && $(this).hasClass("active")) {
        clearHover();
    }
});

function clearHover() {
    var hoverMenu = document.querySelectorAll(".hover-menu")[0];
    if (hoverMenu) {
        hoverMenu.style.top = "";
        hoverMenu.style.bottom = "";
        hoverMenu.style.maxHeight = "";
        hoverMenu.classList.remove("hover-menu");
    }
}

//Occures when a nav-tab is clicked
$("a.tablink").click(function (e) {
    e.preventDefault();
    if (!$("#wrapper").hasClass("toggled")) {
        toggleMenu();
        clearHover();
    }

});

//Occurres when the area left of the modules is clicked
$("li.active-module").click(function (e) {
    if (!$("#wrapper").hasClass("toggled")) {
        $("#wrapper").toggleClass("toggled");
        $(window).trigger('resize');
    }
});

//Occurres when the work-area outside the menu is clicked
$("div.main-content").click(function (e) {
    if (!$("#wrapper").hasClass("toggled")) {
        toggleMenu();
        $(window).trigger('resize');
    }
    clearHover();
});

//Occurres when active sub-menu is expanded/collapsed
$("div.panel-heading").click(function (e) {
    var $this = $(this);

    //Check if current span was expanded
    var expanded = !$this.hasClass("collapsed");
    ignoreEvent = true;
    $this.parent().parent().parent().one(events, // rezising done
        function (event) {
            var hoverMenu = document.querySelectorAll(".hover-menu")[0];
            if (hoverMenu) {
                var parentElement = hoverMenu.parentElement;
                if (parentElement) {
                    var menuPosition = parentElement.getBoundingClientRect().top;
                    handleHoverMenu(hoverMenu, menuPosition);
                }
            }
            setTimeout(function () {
                ignoreEvent = false;
            }, 10);
        });
    //Set all spans to collapsed
    $(".left-menu .panel-heading span").removeClass("fal fa-chevron-right");    // fa-chevron-right is bootstrap default
    $(".left-menu .panel-heading span").removeClass("fal fa-chevron-up");
    $(".left-menu .panel-heading span").addClass("fal fa-chevron-down");

    //Change current span expanded if it was collapsed
    if (!expanded) {
        var span = $this.find("span");
        if (span) {
            span.removeClass("fal fa-chevron-right");   // fa-chevron-right is bootstrap default
            span.removeClass("fal fa-chevron-down");
            span.addClass("fal fa-chevron-up");
        }
    }

});

function handlePageStates() {
    var activeDiv = $("a.module-active .module-text");
    var text = activeDiv.length === 1 ? activeDiv[0].innerText : "";
    //var url = activeDiv.length === 1 ? activeDiv[0].src : "";
    var activeHeader = $("#ActiveHeader");
    activeHeader.text(text);

    var icon = $("a.module-active .module-icon")[0];
    if (icon) {
        var classes = icon.classList;
        var iconName = classes[classes.length - 1];
        setModuleIcon(iconName);
    }
    setTimeout(handleActiveMenuItem, 1);

    var url = '/ajax/getUserCompanySetting.aspx?date=' + Date().toString() + '&setting=' + MENU_COLLAPSED;
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);

        var collapseMenu = obj && obj.Found && obj.Value;
        if (collapseMenu) {
            var menu = $("#wrapper");
            menu.addClass("collapse-menu");
            toggleCollapseButton();
        }
        setTimeout(function () {
            var mainElement = document.getElementById("mainView");
            mainElement.style.display = "block";
        }, 15);
    });
}

function setModuleIcon(url) {
    var iconElement = $("#ActiveIcon")[0];
    if (iconElement) {
        iconElement.className = "active-icon fa-fw fal " + url;

        showModuleIcon();
    }
}

function showModuleIcon() {
    var iconElement = $("#ActiveIcon")[0];
    if (iconElement) {
        iconElement.style.display = $(".collapse-menu").length === 0 ? "none" : "block";
    }
}

function handleCollapseMenuDone(event) {
    var mainContent = $(".main-content");
    mainContent.show();
}

//Occures when collapsing/expanding left menu
$("#CollapseMenu").click(function (e) {
    var menu = $("#wrapper");
    menu.one(events, // rezising done, ie transitioned to thin or thick menu
        handleCollapseMenuDone
    );
    var mainContent = $(".main-content");
    mainContent.hide(); // hide maincontent before transition since some of the grids are heavy to animate
    menu.toggleClass("collapse-menu");
    toggleCollapseButton();

    var url = '/ajax/updateUserCompanySetting.aspx?date=' + Date().toString() + '&setting=' + MENU_COLLAPSED + '&value=' + menu.hasClass("collapse-menu");
    DOMAssistant.AJAX.get(url, function (data, status) {
        JSON.parse(data);

        clearHover();
        $(window).trigger('resize');
    });
});

function storeSessionVariables(variables) {
    if (variables !== null) { // change to set value on server instead
        for (var prop in variables) {
            if (Object.prototype.hasOwnProperty.call(variables, prop)) {
                sessionStorage.setItem(prop, variables[prop]);
            }
        }
    }
}

function getSessionVariable(variableName) {
    return sessionStorage.getItem(variableName); // change to fetch value from server
}

function getBoolean(boolValue) {
    if (boolValue === null || boolValue === "false") {
        return false;
    }
    return true;
}

function toggleCollapseButton() {
    $("#CollapseMenu").toggleClass("fa-angle-double-right");
    $("#CollapseMenu").toggleClass("fa-angle-double-left");
    showModuleIcon();
}


//this should be changed to check url to be able to handle old /edit paths
function handleActiveMenuItem() {
    if ($(".tab-pane.active").length === 0) { // when page is totally reloaded, need to set active tab

        var element = $("div.module-active li[id*='Dashboard']"); // always dashboard active under menues
        if (element) {
            element.addClass("active");
            var dashboardElement = $("div.module-active li[id*='Dashboard']")[0];
            if (dashboardElement) {
                var id = dashboardElement.id;
                element = $("#" + id + " .tab-pane");
                if (element) {
                    element.addClass("active");
                    element = $("#" + id + " li:first");
                    if (element) {
                        element.addClass("active");
                    }
                }
            }
        }
    }
}

function init() {
    setTimeout('handlePageStates()', 1);
}

$(window).bind('load', init);