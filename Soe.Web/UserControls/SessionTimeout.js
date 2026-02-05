var warningUrl = '/modalforms/SessionLogoutWarning.aspx';
var timeoutUrl = '/logout.aspx?timeout=1';
var lastSessionModified;
var sessionCookieName;
var timeoutTime;
var warnMin;
var warnSec;
var checkWarningId;
var checkTimeoutId;
var updateClockId;
var doWarnId;
var doLogoutId;

function init() {
    refreshSession();
}

//Do not re-name. Is called from other projects, for example SL
function refreshSession(fromAngular) {
    console.log("Refreshing session in SessionTimeout.js");
}

function renewSoftoneOnline() {
    console.log("SessionTimeout.js is renewing SoftOne Online session...");
}

    function checkWarning() {
        checkSession(true, false);
    }

    function checkTimeout() {
        checkSession(false, true);
    }

    function checkSession(checkWarn, checkLogout) {
    }

    function setTimeouts(warningMS, timeoutMS) {
        checkWarningId = setTimeout('checkWarning()', warningMS);
        checkTimeoutId = setTimeout('checkTimeout()', timeoutMS);
    }

    function clearTimeouts() {
        if (checkWarningId != null) {
            clearTimeout(checkWarningId);
        }
        if (checkTimeoutId != null) {
            clearTimeout(checkTimeoutId);
        }
        if (updateClockId != null) {
            clearTimeout(updateClockId);
        }
        if (doWarnId != null) {
            clearTimeout(doWarnId);
        }
        if (doLogoutId != null) {
            clearTimeout(doLogoutId);
        }
    }

    function reset() {
        setClockHidden();
        clearTimeouts();
        closeWarning();
    }

    function start(obj) {
        reset();
        setTimeouts(obj.WarningMS, obj.TimeoutMS);
        timeoutTime = addTime(new Date(), obj.TimeoutMin, obj.TimeoutSec);
        setClockVisible();
        //updateClock();
    }

    // CLOCK

    function updateClock() {
        var distance = timeoutTime - new Date();
        if (distance < 0 || isSessionRefreshed()) {
            reset();
            checkSession(true, true);
        }
        else {
            var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((distance % (1000 * 60)) / 1000);
            var clock = checkTime(hours) + ":" + checkTime(minutes) + ":" + checkTime(seconds);
            var clockElement = getClockElement();
            if (clockElement) {
                clockElement.innerHTML = clock;
            }
            updateClockId = setTimeout("updateClock()", 500);
        }
    }

    function checkTime(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    }

    function setClockVisible() {
        var sessionClockElement = document.getElementById("sessionClock");
        if (sessionClockElement)
            sessionClockElement.style.display = "inline";
    }

    function setClockHidden() {
        var sessionClockElement = document.getElementById("sessionClock");
        if (sessionClockElement)
            sessionClockElement.style.display = "none";
    }

    // HELP-FUNCTIONS

    function isSessionRefreshed(name) {
            return true;
    }

    function getSessionCookie() {
        var cookies = document.cookie.split(';');
        for (var i = 0; i < cookies.length; ++i) {
            var pair = cookies[i].trim().split('=');
            if (pair[0] == sessionCookieName)
                return pair[1];
        }
        return null;
    }

    function getClockElement() {
        return document.getElementById('sessionClock');
    }

    function getWarningElement() {
        var warningContent = document.getElementById("sessionLogoutWarning");
        if (warningContent && warningContent.style.display != 'none')
            return warningContent;
        return null;
    }

    function addTime(date, minutes, seconds) {
        var time = addMinutes(date, minutes);
        time = addSeconds(time, seconds);
        return new Date(time);
    }

    function addMinutes(date, minutes) {
        return new Date(date.getTime() + minutes * 60000);
    }

    function addSeconds(date, seconds) {
        return new Date(date.getTime() + seconds * 1000);
    }

    // ACTIONS

    function closeWarning() {
        var warningElement = getWarningElement();
        if (warningElement) {
            var warningDialogs = document.getElementsByClassName("ds1 Popup");
            var warningDialogOverlays = document.getElementsByClassName("Popup_overlay");
            if (warningDialogs && warningDialogOverlays) {
                for (var i = 0, ilen = warningDialogs.length; i < ilen; i++) {
                    warningDialogs[i].style.display = 'none';
                }
                for (var o = 0, olen = warningDialogOverlays.length; o < olen; o++) {
                    warningDialogOverlays[o].style.display = 'none';
                }
                warningElement.parentElement.removeChild(warningElement);
            }
        }
    }

    function doWarn() {
        console.log("SessionTimeout.js - Session is expiring in 2 minutes");
        closeWarning();
        doWarnId = setTimeout('PopLink.modalWindowShow(warningUrl);', 200);
    }

    function doLogout() {
        console.log("SessionTimeout.js - Session has expired");
        closeWarning();
        doLogoutId = setTimeout(function () { document.location.href = timeoutUrl }, 200);
    }

    $(window).bind('load', init);