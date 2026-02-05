function Left(str, n) {
    if (n <= 0)
        return "";
    else if (n > String(str).length)
        return str;
    else
        return String(str).substring(0, n);
}
function Right(str, n) {
    if (n <= 0)
        return "";
    else if (n > String(str).length)
        return str;
    else {
        var iLen = String(str).length;
        return String(str).substring(iLen, iLen - n);
    }
}
function FormatDate(date) {
        var m = date.getMonth() + 1;
        var d = date.getDate();
        if (m < 10) m = '0' + m;
        if (d < 10) d = '0' + d;
        return date.getFullYear() + '-' + m + '-' + d;
}
