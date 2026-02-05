(function () {
    function loadLocales(localeFile, momentLocaleFile) {
        var p = [];
        p.push(System.import(localeFile));
        if (momentLocaleFile)
            p.push(System.import(momentLocaleFile));
        return Promise.all(p);
    }

    function loadAppBundle(path) {
        var filename = path.replace(/\./g, '/') + '/Bundle';
        System.import(filename).then(function () {
            angular.bootstrap(document, [document.getElementById("ng-app-bootstrap-element").getAttribute("data-soe-app")]);
        });
    }

    function getDependencies(app, path, prefix, suffix, settings) {

        var promises = [];

        promises.push(System.import(prefix + 'Soe.Libs' + suffix));
        promises.push(System.import(prefix + 'Soe.Core' + suffix));
        promises.push(System.import(prefix + 'Soe.Util' + suffix));
        promises.push(System.import(prefix + 'Soe.' + path + suffix));

        if (settings.deps[app] && settings.deps[app].length) {
            for (var i = 0; i < settings.deps[app].length; i++) {
                promises.push(System.import(prefix + settings.deps[app][i] + suffix));
            }
        }
        return promises;
    }

    function getSettings(baseUrl) {
        var systemjsconfig = {
            baseURL: baseUrl,
            packages: {
                "": {
                    defaultExtension: 'js'
                }
            },
            paths: {
                "moment": "/Angular/node_modules/moment/min/moment.min.js",
                "moment-timezone": "/Angular/node_modules/moment-timezone/builds/moment-timezone-with-data.js",
                "moment-range": "/Angular/node_modules/moment-range/dist/moment-range.js",
                "lodash": "/Angular/node_modules/lodash/lodash.min.js",
                "angular": "/Angular/node_modules/angular/angular.js",
                "ag-grid-community": "/Angular/node_modules/@ag-grid-community/all-modules/dist/ag-grid-community.js",
                "ag-grid-enterprise": "/Angular/node_modules/@ag-grid-enterprise/all-modules/dist/ag-grid-enterprise.js",
                "ag-charts-community": "/Angular/node_modules/ag-charts-community/dist/ag-charts-community.js",
                "angular-translate": "/Angular/node_modules/angular-translate/dist/angular-translate.js",
                "angular-translate-loader-partial": "/Angular/node_modules/angular-translate/dist/angular-translate-loader-partial/angular-translate-loader-partial.js",
                "angular-sanitize": "/Angular/node_modules/angular-sanitize/angular-sanitize.js",
                "angular-ui-bootstrap": "/Angular/node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js",
                "angular-ui-router": "/Angular/node_modules/angular-ui-router/release/angular-ui-router.js",
                "angular-ui-sortable": "/Angular/node_modules/angular-ui-sortable/dist/sortable.js",
                "angular-ui-indeterminate": "/Angular/node_modules/angular-ui-indeterminate/dist/indeterminate.js",
                "angular-ui-grid": "/Angular/node_modules/angular-ui-grid/ui-grid.js",
                "ui-select": "/Angular/node_modules/ui-select/dist/select.js",
                "angular-file-upload": "/Angular/node_modules/angular-file-upload/dist/angular-file-upload.js",
                "angular-moment": "/Angular/node_modules/angular-moment/angular-moment.js",
                "angular-animate": "/Angular/node_modules/angular-animate/angular-animate.js",
                "angular-simple-logger": "/Angular/node_modules/angular-simple-logger/dist/angular-simple-logger.light.js",
                "angular-google-maps": "/Angular/node_modules/angular-google-maps/dist/angular-google-maps.js",
                "oclazyload": "/Angular/node_modules/oclazyload/dist/ocLazyLoad.js",
                "tinymce": "/Angular/node_modules/tinymce/tinymce.min.js",
                "angular-ui-tinymce": "/Angular/node_modules/angular-ui-tinymce/dist/tinymce.min.js",
                "tinymce-custom-plugins": '/Angular/js/HelpMenuPlugin.js',
                "angular-minicolors": "/Angular/node_modules/angular-minicolors/angular-minicolors.js",
                "jquery-minicolors": "/Angular/node_modules/jquery-minicolors/jquery.minicolors.js",
                "jquery": "/Angular/build/Libs/Adapters/jQueryAdapter.js",
                "jquery-ui-dist": "/Angular/node_modules/jquery-ui-dist/jquery-ui.js",
                "bootstrap3-typeahead": "/Angular/node_modules/bootstrap-3-typeahead/bootstrap3-typeahead.js",
                "postal": "/Angular/node_modules/postal/lib/postal.js",
                "angular-hotkeys": "/Angular/node_modules/angular-hotkeys/build/hotkeys.min.js",
                "angularjs-gauge": "/Angular/node_modules/angularjs-gauge/dist/angularjs-gauge.js",
                "angular-bootstrap-contextmenu": "/Angular/node_modules/angular-bootstrap-contextmenu/contextMenu.js",
                "nvd3": "/Angular/node_modules/nvd3/build/nv.d3.js",
                "d3": "/Angular/node_modules/d3/d3.js",
                "angular-nvd3": "/Angular/node_modules/angular-nvd3/dist/angular-nvd3.js",
                "file-saver": '/Angular/node_modules/file-saver/FileSaver.js',
                "xlsx": '/Angular/node_modules/xlsx/dist/xlsx.core.min.js',
                "angularjs-dropdown-multiselect": "/Angular/js/angularjs-dropdown-multiselect.js",
                "pdfmake": '/angular/node_modules/pdfmake/build/pdfmake.min.js',
                "vfs_fonts": '/angular/node_modules/pdfmake/build/vfs_fonts.js',
                //"pdfjs_Compatibility": '/angular/node_modules/pdfjs-dist/lib/shared/compatibility.js',
                "pdfjs-dist": '/angular/js/pdfjs-dist/build/pdf.js',
                "angular-in-viewport": "/Angular/node_modules/angular-in-viewport/dist/in-viewport.js"
            },
            meta: {
                "/Angular/node_modules/*": {
                    format: 'global'
                },
                'angular-moment': {
                    format: 'amd'
                },
                'angular-minicolors': {
                    format: 'amd'
                },
                "jquery-minicolors": {
                    format: 'amd'
                },
                /*'ag-grid-community': {
                     format: 'global',
                     exports: 'agGridCommunity'
                 },*/
                'ag-grid-enterprise': {
                    format: 'global',
                    exports: 'agGrid'
                },
                'ag-charts-community': {
                    format: 'global',
                    exports: 'agCharts'
                },
                "pdfjs-dist": {
                    format: 'global'
                }
            }
        };

        var settings = {
            defaultExt: "js",
            deps: {
            },
            systemjsconfig: systemjsconfig
        };

        return settings;
    }

    function bootSoftOneDev(appBundlePath, prefix, localeFile, momentLocaleFile) {
        System.import("angular").then(function (x) {
            System.import(prefix + "Util/Bundle").then(function () {
                loadLocales(localeFile, momentLocaleFile).then(function () { loadAppBundle(appBundlePath); });
            });
        });
    }

    function bootSoftOneProd(app, appBundlePath, prefix, suffix, localeFile, momentLocaleFile, settings) {
        Promise.all(getDependencies(app, appBundlePath, prefix, suffix, settings)).then(function () {
            System.import('Util/Bundle').then(function () {
                System.import('Core/Bundle').then(function () { // Loads angular and all core modules
                    loadLocales(localeFile, momentLocaleFile).then(function () { loadAppBundle(appBundlePath); });
                });
            });
        });
    }

    window.bootSoftOne = function (app, appBundlePath, prefix, suffix, localeFile, momentLocaleFile, version) {
        var settings = getSettings(prefix);
        System.config(settings.systemjsconfig);
        
        if (!suffix) {
            bootSoftOneDev(appBundlePath, prefix, localeFile, momentLocaleFile);
        } else {
            bootSoftOneProd(app, appBundlePath, prefix, suffix, localeFile, momentLocaleFile, settings);
        }
    };
})();

