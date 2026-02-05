export class GraphicsUtility {

    public static charToHex(c) {
        var hex = c.toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    }

    public static rgbToHex(r, g, b): string {
        return "#" + this.charToHex(r) + this.charToHex(g) + this.charToHex(b);
    }

    public static hexToRgb(hex) {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    }

    public static rgbStringToRgbObject(rgbString: string): any {
        const rgb = rgbString.match(/\d+/g);
        return { r: parseInt(rgb[0]), g: parseInt(rgb[1]), b: parseInt(rgb[2]) };
    }

    public static foregroundColorByBackgroundBrightness(backgroundColor: string, forceWhite?: boolean): string {
        //http://www.w3.org/TR/AERT#color-contrast

        const black = '#000000';
        const white = '#ffffff';

        if (forceWhite)
            return white;

        if (!backgroundColor)
            return black;

        const rgb = backgroundColor.startsWith("rgb") ? this.rgbStringToRgbObject(backgroundColor) : this.hexToRgb(backgroundColor);
        if (rgb) {
            return ((rgb.r * 299) + (rgb.g * 587) + (rgb.b * 114)) / 1000 > 125 ? black : white;
        } else {
            return black;
        }
    }

    public static removeAlphaValue(color: string, defaultColor: string = "#FFFFFF"): string {
        // Remove alpha values from color
        if (color && color.length === 9)
            color = "#" + color.substring(3);
        else if (!color)
            color = defaultColor;

        return color;
    }

    public static addAlphaValue(color: string, opacity: number): string {
        var rgb = this.hexToRgb(color);
        return rgb ? 'rgba({0},{1},{2},{3})'.format(rgb.r.toString(), rgb.g.toString(), rgb.b.toString(), opacity.toString()) : '';
    }

    public static removeHash(color: string): string {
        if (color.startsWithCaseInsensitive('#'))
            color = color.substr(1);

        return color;
    }

    public static addHash(color: string): string {
        if (!color.startsWithCaseInsensitive('#'))
            color = '#' + color

        return color;
    }

    public static fadeBackground(elem: HTMLElement, opacity: number) {
        let currentColor = elem.style.backgroundColor;
        let insertIndex = currentColor.lastIndexOf(')');
        let newColor = currentColor.slice(0, insertIndex) + ', ' + opacity + ')';
        elem.style.backgroundColor = newColor;
    }
}