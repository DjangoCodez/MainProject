export class HtmlUtility {

    static blurActiveElement($window) {
        $window.document.activeElement['blur']();
    }

    static openInSameTab($window, url: string) {
        $window.open(url, '_self');
    }

    static openInNewTab($window, url: string) {
        $window.open(url, '_blank');
    }

    static openInNewWindow($window, url: string) {
        $window.open(url, 'newwindow');
    }

    static getQueryParameterByName(location: Location, name: string) {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    }

    static getCaretPosition(elemId): number {
        var elem: HTMLInputElement = <HTMLInputElement>document.getElementById(elemId);
        return elem != null ? elem.selectionStart : 0;
    }

    static downloadUrl(url, filename): void {
        const anchor = window.document.createElement('a');
        anchor.href = url;
        anchor.download = filename;
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(anchor.href);
    }

    static setCaretPosition(elemId, pos) {
        var elem: HTMLInputElement = <HTMLInputElement>document.getElementById(elemId);
        if (elem != null) {
            if (elem.setSelectionRange) {
                elem.focus();
                elem.setSelectionRange(pos, pos);
            }
            else if (elem['createTextRange']) {
                var range = elem['createTextRange']();
                range.collapse(true);
                range.moveEnd('character', pos);
                range.moveStart('character', pos);
                range.select();
            }
        }
    }

    static isElementInViewport(el) {
        // Special bonus for those using jQuery
        if (typeof jQuery === "function" && el instanceof jQuery) {
            el = el[0];
        }

        var rect = el.getBoundingClientRect();

        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) && /*or $(window).height() */
            rect.right <= (window.innerWidth || document.documentElement.clientWidth) /*or $(window).width() */
        );
    }

    static copyTextToClipboard(text: string) {
        var copyElement = document.createElement("textarea");
        copyElement.style.position = 'fixed';
        copyElement.style.opacity = '0';
        copyElement.textContent = text;
        var body = document.getElementsByTagName('body')[0];
        body.appendChild(copyElement);
        copyElement.select();
        document.execCommand('copy');
        body.removeChild(copyElement);
    }
}