var SoeGridExport = {

    init: function () {
        $(doc).find('SoeGrid').each(function () {
            var soeGrid = $(this);
            SoeGridExport.initExportLink(soeGrid);
        });
    },

    initExportLink: function (soeGrid) {
        var table = $(soeGrid.elmsByTag('table')[0]);
        var exportLink = soeGrid.elmsByClass('exportLink');
        if (exportLink && exportLink.length) {
            var a = $(exportLink[0]);
            a.className = table.id;
            a.onclick = SoeGridExport.exportData;
            a.style.display = '';
        }
    },

    exportData: function () {
        var table = document.getElementById(this.className);
        var form = document.createElement('form');
        form.method = 'post';
        form.action = '/exportdata.aspx';
        form.id = 'excelexport';
        form.style.display = 'none';
        var s = String(this);
        var div = document.createElement('div');
        var textarea = document.createElement('textarea');
        textarea.name = 'data';
        var v = '';

        // Head
        var firstCell = true;
        $(table.rows[1].cells).each(function () {
            if (firstCell)
                firstCell = false;
            else
                v += ';';
            v += getObjInnerText(this).replace(/^\s*|\s*$|\t|\n|;/g, ''); // Trim and remove any tabs, newlines or semicolons
        });
        v += '\n';

        // Body
        $(table.tBodies[0].rows).each(function () {
            if (!this.className.match(/tablefilter-removed/)) {
                firstCell = true;
                $(this.cells).each(function () {
                    if (firstCell)
                        firstCell = false;
                    else
                        v += ';';

                    var text = null;
                    if (this.className == 'num')
                        text = getObjInnerText(this).replace(/\s/g, ''); // Remove any whitespace
                    else
                        text = getObjInnerText(this).replace(/^\s*|\s*$|\t|\n|;/g, ''); // Trim and remove any tabs, newlines or semicolons

                    if (text) {
                        if (text != "[Redigera]")
                            v += text;
                    }
                });

                v += '\n';
            }
        });

        textarea.appendChild(document.createTextNode(v));
        div.appendChild(textarea);
        form.appendChild(div);
        document.body.appendChild(form);
        form.submit();
        document.body.removeChild(form);
        return false;
    }
}

function getObjInnerText(obj)
{
    var text = '';

    if (document.all)
    {
         // IE
        if (obj.innerText != '')
            text = obj.innerText;
    }
    else
    {
        // Firefox
        if (obj.innerText != '')
            text = obj.textContent;
    }

    return text;
}

$(window).bind('load', SoeGridExport.init);