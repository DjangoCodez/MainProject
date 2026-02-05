//2025-06-13: NOT USED ANYMORE. Handled by login page.

//Has browser suffix to avoid name collisions when this UserControl is placed inside a page
var browser_intervalId = -1;
var browser_sysTerms = null;
var browser_validations = 0;

function browser_init() {
    browser_initTerms();
}

function browser_initTerms() {
    //create
    browser_sysTerms = new Array();
    browser_sysTerms.push(TermManager.createSysTerm(1, 5546, 'Du använder'));
    browser_sysTerms.push(TermManager.createSysTerm(1, 5547, 'eller senare'));
    browser_sysTerms.push(TermManager.createSysTerm(1, 5548, 'eller kompabillitetsläge'));
    browser_sysTerms.push(TermManager.createSysTerm(1, 5549, 'Du använder en för oss okänd webbläsare'));
    browser_sysTerms.push(TermManager.createSysTerm(1, 5550, 'Den webbläsaren rekommenderas inte för SoftOne, och vi kan inte garantera att systemet visas korrekt'));

    //validate
    browser_intervalId = setInterval(browser_validateTerms, TermManager.delay);
}

function browser_validateTerms()
{
    browser_validations++;
    var valid = TermManager.validateSysTerms(browser_sysTerms, 'browser');
    if (valid || TermManager.reachedMaxValidations(browser_validations))
    {
        clearInterval(browser_intervalId);
        browser_setup();
    }
}

function browser_setup()
{
    //Add initialization here!
    checkBrowser();
}

function checkBrowser()
{
    //Internet Explorer x.x;
    var ie = /MSIE (\d+\.\d+);/.test(navigator.userAgent);
    //Internet Explorer 11 has new userAgent
    var ie11 = !!navigator.userAgent.match(/Trident\/7\./);
    //Firefox/x.x or Firefox x.x (ignoring remaining digits)
    var ff = /Firefox[\/\s](\d+\.\d+)/.test(navigator.userAgent);
    //Opera/x.x or Opera x.x (ignoring remaining decimal places)
    var op = /Opera[\/\s](\d+\.\d+)/.test(navigator.userAgent);
    //Chrome x.x
    var ch = /chrome/.test(navigator.userAgent.toLowerCase());
    //Safari
    var sa = /safari/.test(navigator.userAgent.toLowerCase());

    var browser = '';
    var supported = true;

    if (ie == true) {
        // capture x.x portion and store as a number
        var ieVer = new Number(RegExp.$1);
        if (ieVer >= 8) {
            browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Internet Explorer 8-10 ' + TermManager.getText(browser_sysTerms, 1, 5548) + ' 8-10';
            supported = false
        }
        else if (ieVer >= 7) {
            browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Internet Explorer 7.x ' + TermManager.getText(browser_sysTerms, 1, 5548) + ' 7.x';
            supported = false;
        }
        else if (ieVer >= 6) {
            browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Internet Explorer 6.x ' + TermManager.getText(browser_sysTerms, 1, 5548) + ' 6.x';
            supported = false;
        }
        else if (ieVer >= 5) {
            browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Internet Explorer 5.x ' + TermManager.getText(browser_sysTerms, 1, 5548) + ' 5.x';
            supported = false;
        }
    }
    else if (ie11 == true) {
    }
    else if (ff == true) {
        // capture x.x portion and store as a number
        var ffVer = new Number(RegExp.$1)
        if (ffVer >= 3) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Firefox 3.x ' + TermManager.getText(browser_sysTerms, 1, 5547);
        }
        else if (ffVer >= 2) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Firefox 2.x';
        }
        else if (ffVer >= 1) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Firefox 1.x';
        }
    }
    else if (op == true) {
        // capture x.x portion and store as a number
        var opVer = new Number(RegExp.$1);
        if (opVer >= 10) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Opera 10.x ' + TermManager.getText(browser_sysTerms, 1, 5547);
        }
        else if (opVer >= 9) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Opera 9.x';
        }
        else if (opVer >= 8) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Opera 8.x';
        }
        else if (opVer >= 7) {
            //Do not get term if supported
            //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Opera 7.x';
        }
    }
    else if (ch == true) {
        //Do not get term if supported
        //browser = TermManager.getText(browser_sysTerms, 1, 5546) + ' Google Chrome';
    }
    else if (sa == true) {
        var saVersion = /Version[\/\s](\d+\.\d+)/.test(navigator.userAgent);
        var saVer = new Number(RegExp.$1);

        if (saVersion && saVer < 4) {
            supported = false;
            browser = TermManager.getText(browser_sysTerms, 1, 5549);
        }
    }
    else {
        browser = TermManager.getText(browser_sysTerms, 1, 5549);
        supported = false;
    }

    if (!supported)
        alert(browser + ". " + TermManager.getText(browser_sysTerms, 1, 5550));
}

$(window).bind('load',  browser_init);