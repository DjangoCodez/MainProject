export interface IFocusService {
    focusById(id: string);
    focusByName(name);
}

export class FocusService implements IFocusService {

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private $interval: ng.IIntervalService, private $window) {
    }

    public focusById(id: string) {
        // Need an interval loop if GUI is not rendered yet
        var cancel = this.$interval(() => {
            var element = this.$window.document.getElementById(id);
            if (element) {
                this.focus(element);
                this.$interval.cancel(cancel);
            }
        }, 100, 8);
    }

    public focusByName(name) {
        // Need an interval loop if GUI is not rendered yet
        var cancel = this.$interval(() => {
            var elements = this.$window.document.getElementsByName(name);
            if (elements && elements.length) {
                this.focus(elements[elements.length - 1]);
                this.$interval.cancel(cancel);
            }
        }, 100, 8);
    }

    private focus(element) {
        // IE needs a timeout
        this.$timeout(() => {
            element.focus();
        });
    }
}
