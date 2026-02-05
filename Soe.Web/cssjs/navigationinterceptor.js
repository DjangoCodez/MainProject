let navHelper;

(function () {
    navHelper = {
        // This is set in the spaNavigationService in AngularSpa project.
        spaRouter: null,
        migratedModules: new Set(), 
        intercept: function (onClickEvent, feature, url, label) {
            const currentPath = this._getSoePath(window.location.href);
            const goToPath = this._getSoePath(url);
            const isMigrated = feature === -1 || this.migratedModules?.has(feature);
            if (!this.spaRouter || !goToPath || !isMigrated) return;

            // If we get here, we are in a SPA page with valid destination.
            onClickEvent?.preventDefault();
            
            // Start navigating using SPA router.
            const $nav = this.spaRouter.navigateByUrl(goToPath);
            // While navigating, update menu to make it appear as instant change.
            this._updateLeftMenuAndTitle(currentPath, goToPath, label, url);
            $nav.catch(e => {
                // Fallback to normal navigation if SPA navigation fails.
                console.error(e);
                window.location.href = url;
            });
        },
        _updatePageTitle: function (pageLabel) {
            const existingTitle = document?.title ?? "";
            const companyName = existingTitle.includes(":") ?
                existingTitle.split(":")[0] : 
                undefined 

            if (!companyName) document.title = pageLabel;
            else document.title = `${companyName}: ${pageLabel}`;
        },
        // Menu update functions
        _updateLeftMenuAndTitle: function (pathFrom, pathTo, label, fallbackUrl) {
            try {
                this._updateMenu(pathFrom, false);
                const a = this._updateMenu(pathTo, true);
                if (a && !label) {
                    label = this._extractLabelFromAnchor(a);
                }
                this._updatePageTitle(label);
            } catch (ex) {
                // Fallback to normal navigation...
                console.error(ex);
                window.location.href = fallbackUrl;
            }
        },
        _updateMenu: function (url, makeActive) {
            const a = this._findMenuAnchor(url);
            if (!a) return;
            this._updateSelectedItem(a, makeActive);
            this._updateExpandedMenu(a, makeActive);
            this._updatePane(a, makeActive);
            return a;
        },
        _updateSelectedItem: function (a, makeActive) {
            const li = a.closest("li");
            if (li) {
                this._updateActiveClass(li, makeActive);
            }
        },
        _updatePane: function (a, makeActive) {
            const pane = a.closest(".tab-pane");
            if (pane) {
                this._updateActiveClass(pane, makeActive);
                if (!makeActive) {
                    pane.classList.remove("hover-menu");
                }
                const mainItem = pane.closest("li");
                if (mainItem) {
                    this._updateActiveClass(mainItem, makeActive);
                    const moduleId = mainItem.id ?? "";
                    // Get the name of the main module... Personal/Sales/Finance etc.
                    const mainModuleName = moduleId.split("_")[0] ?? "";
                    this._updateMainModule(mainModuleName.toLowerCase());
                }
            }
        },
        _updateExpandedMenu: function (anchor, doActive) {
            const collapser = anchor.closest(".panel-collapse");
            if (!collapser) return;
            this._conditionalChangeClasslist(collapser, "in", doActive);

            const heading = anchor.closest(".panel-heading");
            if (!heading) return;
            this._conditionalChangeClasslist(heading, "collapsed", doActive);

            const arrow = heading.closest(".fa-chevron-down");
            if (!arrow) return;

            this._conditionalChangeClasslist(arrow, "fa-chevron-up", !doActive);
            this._conditionalChangeClasslist(arrow, "fa-chevron-down", doActive);
        },
        _updateMainModule: function (activeModuleName) {
            const modules = document.querySelectorAll("#moduleSelector > li");
            if (!modules || modules.length === 0) return;

            modules.forEach((el) => {
                const name = (el.id ?? "").toLowerCase();
                const activateModule = name === activeModuleName;
                this._conditionalChangeClasslist(el, "active", activateModule);
                this._conditionalChangeClasslist(el, "module-active", activateModule);
                this._conditionalChangeClasslist(el, "module-inactive", !activateModule);
            });

        },
        //Helper functions.
        _getSoePath: function (url) {
            const index = url.indexOf("/soe");
            if (index === -1) return undefined;

            url = url
                .toLowerCase()
                .replace("&spa=true", "");

            return url.substring(index);
        },
        _updateActiveClass: function (el, isActive) {
            this._conditionalChangeClasslist(el, "active", isActive);
        },
        _conditionalChangeClasslist: function (el, className, doAdd) {
            if (!el) return;
            if (doAdd) {
                el.classList.add(className);
            } else {
                el.classList.remove(className);
            }
        },
        _findMenuAnchor: function (path) {
            return document.querySelector(`a[href^="${path}"]`);
        },
        _extractLabelFromAnchor: function (a) {
            if (a.onclick) {
                const funcStr = a.onclick.toString();
                const match = funcStr.match(/'([^']+)'(?=\s*\)\s*;)/);
                const lastArgument = match ? match[1] : undefined;
                return lastArgument;
            }
        }
    }
})()