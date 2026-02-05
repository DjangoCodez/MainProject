export class GraphicsUtil {
  public static hexToRgb(hex: string) {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result
      ? {
          r: parseInt(result[1], 16),
          g: parseInt(result[2], 16),
          b: parseInt(result[3], 16),
        }
      : null;
  }

  public static foregroundColorByBackgroundBrightness(
    backgroundColor: string,
    forceWhite?: boolean
  ): string {
    //http://www.w3.org/TR/AERT#color-contrast

    const black = '#000000';
    const white = '#ffffff';

    if (forceWhite) return white;

    if (!backgroundColor) return black;

    const rgb = this.hexToRgb(backgroundColor);
    if (rgb) {
      const o = Math.round((rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000);
      return o > 125 ? black : white;
    } else {
      return black;
    }
  }

  public static removeAlphaValue(
    color: string,
    defaultColor: string = '#ffffff'
  ): string {
    // Remove alpha values from color
    if (color && color.length === 9) color = '#' + color.substring(3);
    else if (!color) color = defaultColor;

    return color;
  }

  public static addAlphaValue(color: string, opacity: number): string {
    const rgb = this.hexToRgb(color);
    return rgb
      ? 'rgba({0},{1},{2},{3})'.format(
          rgb.r.toString(),
          rgb.g.toString(),
          rgb.b.toString(),
          opacity.toString()
        )
      : '';
  }
}
