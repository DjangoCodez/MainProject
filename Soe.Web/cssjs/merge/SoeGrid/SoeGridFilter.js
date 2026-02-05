var SoeGridFilter = {

    filters: {},
    tableInfo: {},
    oddClass: 'odd',
    evenClass: 'even',
    quickPageLinksNumInRange: 9,
    currentPageClass: 'currentPage',
    nextLinkText: 'Nästa', //TODO: Hard-coded (language not handled because it is not visible at the moment)
    previousLinkText: 'Föregående', //TODO: Hard-coded (language not handled because it is not visible at the moment)
    lengthSetter: null,

    init: function () {
        $('.SoeGrid').each(function () {
           
            var soeGrid = $(this);
            var table = document.getElementsByTagName('table')[0];
        
            if (table.className.match(/pageable/)) {

                var tbody = table.tBodies[0];
                var numRows = tbody.rows.length;
                if (numRows == 1 && tbody.previousSibling == null)
                    numRows = 0;


                var pp = $(soeGrid).find('.Header');
                var tblIfo = {
                    numTotalRows: numRows,
                    numVisibleRows: numRows,
                    pageLength: 1000,
                    prevLink: null,
                    nextLink: null,
                    quickPageLinksContainer: null,
                    table: table,
                    legend: pp[0],
                    lengthSetter: $(pp[1]),
                    box: null,
                    boxOriginalHeight: null,
                    firstRow: 0
                };

                    
                var filter = $(soeGrid).find('.SoeGridFilter');
                if (filter && filter.length) {
                    //alert($(this).html());
                    tblIfo.box = $(filter[0]);
                    SoeGridFilter.initFilter(tblIfo);
                }

                SoeGridFilter.toggleRows(tblIfo);
                SoeGridFilter.addFooter(tblIfo);

                SoeGridFilter.tableInfo[table.id] = tblIfo;
                SoeGridFilter.updateLegend(tblIfo, tblIfo.numVisibleRows);
            }
        });
    },

    supressKeysNum: function (e) {
        var key = window.event ? window.event.keyCode : e.which;
        var allowFirst = ['<', '>'];
        return Forms.supressNonNumericKeysSub(this, key, allowFirst);
    },

    initFilter: function (tblIfo) {

        $(tblIfo.box).find(':input').each(function () {
            
            var fn;
            /* 2010-09-23: Check className before hasClass. Bug-fix for Chrome that doesnt recognize hasClass method */
            if (this.className == 'startswith') {
                fn = SoeGridFilter.filterStartswith;
            }
            else if (this.className == 'contains') {
                fn = SoeGridFilter.filterContains;
            }
            else if (this.className == 'match') {
                fn = SoeGridFilter.filterMatch;
            }
            else if (this.className == 'numeric') {
                fn = SoeGridFilter.filterNumeric;
                this.onkeypress = SoeGridFilter.supressKeysNum;
            }

            this.onkeyup = SoeGridFilter.filterUpdated;

            if (!SoeGridFilter.filters[tblIfo.table.id])
                SoeGridFilter.filters[tblIfo.table.id] = {};
            SoeGridFilter.filters[tblIfo.table.id][this.id.split('-')[1]] = { input: this, fn: fn };
           
        });
        SoeGridFilter.initPageLengthSelector(tblIfo);
    },

    addFooter: function (tblIfo) {
        var table = tblIfo.table;
        var tfoots = $('tfoot');
        if (!tfoots.length)
            return;
        var tfoot = $(tfoots[0]);
        var td = tfoot.find('td')[0];

        var prevLink = SoeGridFilter.createLink(SoeGridFilter.previousLinkText, table.id, function (e) {
            e = e || window.event;
            var tblId = this.className;
            var tblInfo = SoeGridFilter.tableInfo[tblId];
            SoeGridFilter.goToRow(tblId, tblInfo.firstRow - tblInfo.pageLength);
            return fdTableSort.stopEvent(e);
        });
        tblIfo.prevLink = prevLink;
        td.appendChild(prevLink);

        var quickPageLinksContainer = document.createElement('span');
        tblIfo.quickPageLinksContainer = quickPageLinksContainer;
        td.appendChild(quickPageLinksContainer);

        SoeGridFilter.redrawQuickPageLinks(tblIfo);

        var nextLink = SoeGridFilter.createLink(SoeGridFilter.nextLinkText, table.id, function (e) {
            e = e || window.event;
            var tblId = this.className;
            var tblInfo = SoeGridFilter.tableInfo[tblId];
            SoeGridFilter.goToRow(tblId, tblInfo.firstRow + tblInfo.pageLength);
            return fdTableSort.stopEvent(e);
        });
        tblIfo.nextLink = nextLink;
        td.appendChild(nextLink);

        SoeGridFilter.tableInfo[table.id] = tblIfo;
        SoeGridFilter.toggleLinks(table.id);
    },

    initPageLengthSelector: function (tblIfo) {
        $(tblIfo).find('a').each(function () {
            this.onclick = SoeGridFilter.setPageLength;
        });
    },

    goToRow: function (tblId, rowNo) {
        var tblIfo = SoeGridFilter.tableInfo[tblId];
        if (tblIfo) {
            if (rowNo < 0) rowNo = 0;
            tblIfo.firstRow = rowNo;
            SoeGridFilter.toggleRows(tblIfo);
            SoeGridFilter.toggleLinks(tblId);
            SoeGridFilter.redrawQuickPageLinks(tblIfo);
        }
        return false;
    },

    setPageLength: function () {
        var s = this.className.split(',');
        var tblIfo = SoeGridFilter.tableInfo[s[0]];
        tblIfo.pageLength = parseInt(s[1]);
        while (tblIfo.firstRow % tblIfo.pageLength > 0)
            tblIfo.firstRow--;
        SoeGridFilter.toggleRows(tblIfo);
        SoeGridFilter.toggleLinks(tblIfo.table.id);
        SoeGridFilter.redrawQuickPageLinks(tblIfo);
    },

    createLink: function (text, tableId, clickFn) {
        var a = document.createElement('a');
        a.href = '#';
        a.className = tableId;
        a.onclick = clickFn;
        a.appendChild(document.createTextNode(text));
        return a;
    },

    redrawQuickPageLinks: function (tblIfo) {
        var e = tblIfo.quickPageLinksContainer;
        if (!e)
            return;

        var lastPage = Math.ceil(tblIfo.numVisibleRows / tblIfo.pageLength);

        var currentPage;
        if (tblIfo.firstRow == 0)
            currentPage = 1;
        else
            currentPage = Math.floor(tblIfo.firstRow / tblIfo.pageLength) + 1;

        var rangeStart = currentPage - Math.floor(SoeGridFilter.quickPageLinksNumInRange / 2);
        if (rangeStart < 0)
            rangeStart = 1;

        var rangeEnd = rangeStart + SoeGridFilter.quickPageLinksNumInRange - 1;
        if (rangeEnd > lastPage) {
            rangeEnd = lastPage;
            rangeStart = rangeEnd - SoeGridFilter.quickPageLinksNumInRange;
        }

        while (e.childNodes.length > 0)
            e.removeChild(e.childNodes[0]);

        for (var p = 1; p <= lastPage; p++) {
            if (p == currentPage) {
                var span = document.createElement('span');
                span.appendChild(document.createTextNode(tblIfo.numVisibleRows + '(' + tblIfo.numTotalRows + ')')); //NI 2013-08-07 Show nr of rows instead of current page nr (wich always is 1 since we disabled paging)
                if (SoeGridFilter.currentPageClass)
                    span.className = SoeGridFilter.currentPageClass;
                e.appendChild(span);
            } else if (p >= rangeStart && p <= rangeEnd) {
                e.appendChild(SoeGridFilter.createLink(p, tblIfo.table.id, function (e) {
                    e = e || window.event;
                    var tableId = this.className;
                    var targetRow = (parseInt(this.childNodes[0].nodeValue) - 1) * SoeGridFilter.tableInfo[tableId].pageLength;
                    SoeGridFilter.goToRow(tableId, targetRow);
                    return fdTableSort.stopEvent(e);
                }));
            }
        }
    },

    filterUpdated: function (e) {
        e = e || window.event;
        var kc = e.keyCode != null ? e.keyCode : e.charCode;
        if (kc == Kbd.enter) return;
        var tableId = this.id.split('-')[0];
        if (!(tableId in fdTableSort.tableCache)) {
            fdTableSort.prepareTableData($$(tableId));
        }
        SoeGridFilter.filter(tableId);
    },

    filter: function (tableId) {
        var tblIfo = SoeGridFilter.tableInfo[tableId];
        var rowCount = 0;
        var data = fdTableSort.tableCache[tableId].data;

        for (var row = 0; row < data.length; row++) {
            var rowLen = data[row].length - 1;
            var tr = data[row][rowLen];
            rowCount++;

            tr.style.display = '';
            fdTableSort.removeClass(tr, 'SoeGridFilter.removed');

            for (var col = 0; col < rowLen; col++) {
                var filterInfo = SoeGridFilter.filters[tableId][col];
                if (filterInfo) {
                    var colData = data[row][col];
                    var colFilter = filterInfo.input.value;
                    if (colFilter) {
                        if (!filterInfo.fn(colData, colFilter)) {
                            tr.style.display = 'none';
                            fdTableSort.addClass(tr, 'SoeGridFilter.removed');
                            fdTableSort.removeClass(tr, 'tablepaging-removed');
                            rowCount--;
                            break;
                        }
                    }
                }
            }
        }

        tblIfo.numVisibleRows = rowCount;
        SoeGridFilter.updateLegend(tblIfo, tblIfo.numVisibleRows);
        SoeGridFilter.tableInfo[tableId] = tblIfo;
        SoeGridFilter.goToRow(tableId, 0);
    },

    updateLegend: function (tblIfo, text) {
        var legend = tblIfo.legend;
        if (legend == null || legend.childNodes)
            return;
        if (legend.childNodes.length) legend.removeChild(legend.childNodes[0]);
        legend.appendChild(document.createTextNode(text));
    },

    toggleRows: function (tblIfo) {
        var allRows = tblIfo.table.tBodies[0].rows;
        var lastRow = tblIfo.firstRow + tblIfo.pageLength;
        if (tblIfo.firstRow < 0) {
            tblIfo.firstRow = 0;
            lastRow = tblIfo.pageLength;
        }
        var rowCount = 0;
        for (var i = 0; i < allRows.length; i++) {
            var tr = allRows[i];
            if (!tr.className.match(/SoeGridFilter.removed/)) {
                if (rowCount < tblIfo.firstRow || rowCount >= lastRow) {
                    tr.style.display = 'none';
                    fdTableSort.addClass(tr, 'tablepaging-removed');
                } else {
                    fdTableSort.removeClass(tr, 'tablepaging-removed');
                    tr.style.display = '';
                    if (rowCount % 2) {
                        if (SoeGridFilter.oddClass) fdTableSort.removeClass(tr, SoeGridFilter.oddClass);
                        if (SoeGridFilter.evenClass) fdTableSort.addClass(tr, SoeGridFilter.evenClass);
                    } else {
                        if (SoeGridFilter.oddClass) fdTableSort.addClass(tr, SoeGridFilter.oddClass);
                        if (SoeGridFilter.evenClass) fdTableSort.removeClass(tr, SoeGridFilter.evenClass);
                    }
                }
                rowCount++;
            }
        }
        return tblIfo;
    },

    toggleLinks: function (tblId) {
        var tblInfo = SoeGridFilter.tableInfo[tblId];
        if (tblInfo.prevLink) tblInfo.prevLink.style.display = tblInfo.firstRow == 0 ? 'none' : '';
        if (tblInfo.nextLink) tblInfo.nextLink.style.display = tblInfo.numVisibleRows <= tblInfo.firstRow + tblInfo.pageLength ? 'none' : '';
    },

    filterStartswith: function (data, filter) {
        if (data.length < filter.length)
            return false;
        return data.toString().substring(0, filter.length) == filter.toLowerCase();
    },

    filterContains: function (data, filter) {
        var data1 = data.toString().toLowerCase();
        return data1.indexOf(filter.toLowerCase()) >= 0;
    },

    filterNumeric: function (data, filter) {
        if (!filter)
            return true;
        var ndata = parseFloat(String(data).replace(' ', '').replace(',', '.'));
        var c = filter.substring(0, 1);
        if (c == '>') {
            if (filter.length == 1)
                return true;
            return ndata > parseFloat(String(filter).substring(1));
        }
        if (c == '<') {
            if (filter.length == 1)
                return true;
            return ndata < parseFloat(String(filter).substring(1));
        }
        return ndata == parseFloat(filter);
    },

    filterMatch: function (data, filter) {
        return data == filter.toLowerCase();
    }
};

var sortCompleteCallback = function () {
    var tables = document.getElementsByTagName('table');
    for (var i = 0; i < tables.length; i++)
        if (tables[i].className.match(/pageable/))
            SoeGridFilter.goToRow(tables[i].id, 0);
};

$(window).bind('load', SoeGridFilter.init);