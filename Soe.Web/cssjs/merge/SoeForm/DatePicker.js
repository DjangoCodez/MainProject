var DatePicker = {
    input: null,
    datePicker: null,
    year: null,
    month: null,
    tdIdPrefix: 'DatePicker',
    valueChanged: false,
    focus: function () {
        DatePicker.input = this;

        //Reset DatePicker
        DatePicker.year = new Date().getFullYear();
        DatePicker.month = new Date().getMonth();

        DatePicker.showPicker();

        Kbd.setKeydownListener(document, Kbd.escape, DatePicker.hidePicker);
        Kbd.setKeydownListener(this, Kbd.tab, DatePicker.hidePicker);

        document.onmousedown = function () {
        DatePicker.hidePicker();
        }
    },
    
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

    getWidth: function (el) {
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
 
    /*blur: function() {
    if (DatePicker.datePicker) {
    var v = this.value;
    if (v.length == 4) {
    var date = new Date();
    this.value = date.year + '-' + v.substring(2, 4) + '-' + v.substring(4, 6);
    } if (v.length == 6) {
    this.value = '20' + v.substring(0, 2) + '-' + v.substring(2, 4) + '-' + v.substring(4, 6);
    } else if (v.length == 8) {
    this.value = v.substring(0, 4) + '-' + v.substring(4, 6) + '-' + v.substring(6, 8);
    }
    DatePicker.hidePicker();
    }
    },*/

    showPicker: function () {
        var firstDay = new Date();
        var url = '/ajax/getLanguage.aspx';
        DOMAssistant.AJAX.get(url, function (data, status) {
            var languageObj = JSON.parse(data);
            if (DatePicker.input.value) {
                // Override splitter with Finnish,English,Norwegian if found from the date field. Otherwise webbrowser hangs forever when language is not Swedish. 
                var splitter = '';
                if (languageObj.IsSwedish) {
                    splitter = '-';
                }
                else if (languageObj.IsEnglish) {
                    splitter = '/';
                }
                else if (languageObj.IsFinnish || languageObj.IsNorwegian) {
                    splitter = '.';
                }

                if (splitter.length > 0) {
                    var d = DatePicker.input.value.split(splitter);
                    if (languageObj.IsSwedish) {
                        firstDay = new Date(d[0], Number(d[1]) - 1, d[2]);
                    }
                    else if (languageObj.IsEnglish) {
                        firstDay.setFullYear(Number(d[2]), Number(d[0]) - 1, Number(d[1]));
                    }
                    else if (languageObj.IsFinnish || languageObj.isNorwegian) {
                        firstDay.setFullYear(Number(d[2]), Number(d[1]) - 1, Number(d[0]));
                    }

                    if (!DatePicker.valueChanged) {
                        DatePicker.year = firstDay.getFullYear();
                        DatePicker.month = firstDay.getMonth();
                    }
                }
            }

            //Create div
            DatePicker.hidePicker();
            DatePicker.datePicker = $$(document.createElement('div'));
            DatePicker.datePicker.id = 'datePicker';

            //Get selected year and month
            if (!DatePicker.year) {
                DatePicker.year = firstDay.getFullYear();
                DatePicker.month = firstDay.getMonth();
            }

            //Determine first day of selected month	
            //Important to select the date first.
            firstDay.setDate(1);
            if (DatePicker.valueChanged) {
                firstDay.setFullYear(DatePicker.year);
                firstDay.setMonth(DatePicker.month);
                DatePicker.valueChanged = false;
            }

            //Calculate offset
            var fd = firstDay.getDay() - 1;
            if (fd < 0)
                fd = 6;
            var startDayOffset = -(fd);

            //Create year and month switchers
            var p = $$(document.createElement('p'));
            p.appendChild(DatePicker.getImgLink('/cssjs/merge/SoeForm/arrow-left.png', 6, 9, function () {
                DatePicker.year--;
                //DatePicker.hidePicker();
                DatePicker.valueChanged = true;
                DatePicker.showPicker();
            }));
            p.appendChild(document.createTextNode(firstDay.getFullYear()));
            p.appendChild(DatePicker.getImgLink('/cssjs/merge/SoeForm/arrow-right.png', 6, 9, function () {
                DatePicker.year++;
                DatePicker.valueChanged = true;
                //DatePicker.hidePicker();
                DatePicker.showPicker();
            }));
            p.appendChild(DatePicker.getImgLink('/cssjs/merge/SoeForm/arrow-left.png', 6, 9, function () {
                DatePicker.month--;
                if (DatePicker.month < 0) {
                    DatePicker.month = 11;
                    DatePicker.year--;
                }
                //DatePicker.hidePicker();
                DatePicker.valueChanged = true;
                DatePicker.showPicker();
            }));
            //Months
            p.appendChild(document.createTextNode(SOE.monthNames[firstDay.getMonth()]));
            p.appendChild(DatePicker.getImgLink('/cssjs/merge/SoeForm/arrow-right.png', 6, 9, function () {
                DatePicker.month++;
                if (DatePicker.month == 12) {
                    DatePicker.month = 0;
                    DatePicker.year++;
                }
                //DatePicker.hidePicker();
                DatePicker.valueChanged = true;
                DatePicker.showPicker();
            }));

            DatePicker.datePicker.appendChild(p);

            //Create table and headers
            var table = document.createElement('table');
            var thead = document.createElement('thead');
            var tr = document.createElement('tr');
            for (var i = 0; i < SOE.weekDayNames.length; i++) {
                var td = document.createElement('td');
                // The weekDayNames array starts with sunday, we want to begin with monday
                $$(td).appendChild(document.createTextNode(SOE.weekDayNames[i == 6 ? 0 : i + 1]));
                $$(tr).appendChild(td);
            }
            thead.appendChild(tr);
            table.appendChild(thead);

            //Create table body
            var tbody = document.createElement('tbody');
            var end = false;
            var row = 0;
            var emptyRow = false;
            while (!end) {
                tr = document.createElement('tr');
                row++;
                for (var col = 0; col < 7; col++) {
                    td = $$(document.createElement('td'));
                    var date = new Date(firstDay.getTime() + ((startDayOffset++) * 24 * 60 * 60 * 1000));
                    td.appendChild(document.createTextNode(date.getDate()));
                    //td.onmousedown = eval('function(){DatePicker.input.value = "' + DatePicker.format(date) + '";}');
                    td.id = DatePicker.tdIdPrefix + DatePicker.format(date, languageObj);
                    td.onmousedown = DatePicker.select;
                    if (startDayOffset <= 0) {
                        td.addClass('offmonth');
                    } else if (row >= 4 && date.getDate() <= 7) {
                        end = true;
                        td.addClass('offmonth');
                    }
                    tr.appendChild(td);
                    if (col == 0 && end) emptyRow = true;
                }
                if (!emptyRow) tbody.appendChild(tr);
            }
            table.appendChild(tbody);
            DatePicker.datePicker.appendChild(table);

            // Display and position
            DatePicker.datePicker.style.display = 'block';
            DatePicker.datePicker.style.top = DatePicker.getTopPos(DatePicker.input) + DatePicker.getHeight(DatePicker.input) + 'px';
            DatePicker.datePicker.style.left = DatePicker.getLeftPos(DatePicker.input) + 'px';
            document.body.appendChild(DatePicker.datePicker);
        });
    },

    getImgLink: function (src, width, height, fn) {
        var img = document.createElement('img');
        img.src = src;
        img.width = width;
        img.hight = height;
        img.onmousedown = fn;
        return img;
    },

    select: function () {
        DatePicker.input.value = this.id.substring(DatePicker.tdIdPrefix.length);
        if (DatePicker.input.onchange)
            DatePicker.input.onchange();
        DatePicker.hidePicker();
    },

    format: function (date, languageObj) {
        var m = date.getMonth() + 1;
        var d = date.getDate();
        var split = '-';
        if (m < 10)
            m = '0' + m;
        if (d < 10)
            d = '0' + d;

        if (languageObj.IsSwedish) {
            return date.getFullYear() + split + m + split + d;
        }
        else if (languageObj.IsEnglish) {
            split = '/';
            return m + split + d + split + date.getFullYear();
        }
        else if (languageObj.IsFinnish || languageObj.IsNorwegian) {
            split = '.';
            return d + split + m + split + date.getFullYear();
        }
    },

    hidePicker: function () {
        if (DatePicker.datePicker) {
            document.onmousedown = null;
            DatePicker.datePicker.remove();
            DatePicker.datePicker = null;
        }
    },
}

