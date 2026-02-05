using System.IO;
using System.Linq;

namespace SoftOne.Soe.Common.Util
{
    public static class ImageUtil
    {
        public static string[] ImageBitMapExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
        public static bool IsImageBitMapExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return !string.IsNullOrEmpty(extension) && ImageBitMapExtensions.Any(e => e == extension.ToLower());
        }
        public static string GetMimeType(ImageFormatType formatType)
        {
            string mimeType;

            switch (formatType)
            {
                case ImageFormatType.JPG:
                    mimeType = "image/jpeg";
                    break;
                case ImageFormatType.PNG:
                    mimeType = "image/png";
                    break;
                default: //Should not happen
                    mimeType = "image/png";
                    break;
            }

            return mimeType;
        }

        public static ImageType GetFileImageTypeFromHeader(Stream stream)
        {
            const int mostBytesNeeded = 11;//For JPEG

            if (stream == null || stream.Length < mostBytesNeeded)
            {
                return ImageType.Unknown;
            }

            byte[] headerBytes = new byte[mostBytesNeeded];
            stream.Read(headerBytes, 0, mostBytesNeeded);


            //Sources:
            //http://stackoverflow.com/questions/9354747
            //http://en.wikipedia.org/wiki/Magic_number_%28programming%29#Magic_numbers_in_files
            //http://www.mikekunz.com/image_file_header.html

            //JPEG:
            if (headerBytes[0] == 0xFF &&//FF D8
                headerBytes[1] == 0xD8 &&
                (
                 (headerBytes[6] == 0x4A &&//'JFIF'
                  headerBytes[7] == 0x46 &&
                  headerBytes[8] == 0x49 &&
                  headerBytes[9] == 0x46)
                  ||
                 (headerBytes[6] == 0x45 &&//'EXIF'
                  headerBytes[7] == 0x78 &&
                  headerBytes[8] == 0x69 &&
                  headerBytes[9] == 0x66)
                ) &&
                headerBytes[10] == 00)
            {
                return ImageType.JPEG;
            }
            //PNG 
            if (headerBytes[0] == 0x89 && //89 50 4E 47 0D 0A 1A 0A
                headerBytes[1] == 0x50 &&
                headerBytes[2] == 0x4E &&
                headerBytes[3] == 0x47 &&
                headerBytes[4] == 0x0D &&
                headerBytes[5] == 0x0A &&
                headerBytes[6] == 0x1A &&
                headerBytes[7] == 0x0A)
            {
                return ImageType.PNG;
            }
            //GIF
            if (headerBytes[0] == 0x47 &&//'GIF'
                headerBytes[1] == 0x49 &&
                headerBytes[2] == 0x46)
            {
                return ImageType.GIF;
            }
            //BMP
            if (headerBytes[0] == 0x42 &&//42 4D
                headerBytes[1] == 0x4D)
            {
                return ImageType.BMP;
            }
            //TIFF
            if ((headerBytes[0] == 0x49 &&//49 49 2A 00
                 headerBytes[1] == 0x49 &&
                 headerBytes[2] == 0x2A &&
                 headerBytes[3] == 0x00)
                 ||
                (headerBytes[0] == 0x4D &&//4D 4D 00 2A
                 headerBytes[1] == 0x4D &&
                 headerBytes[2] == 0x00 &&
                 headerBytes[3] == 0x2A))
            {
                return ImageType.TIFF;
            }

            return ImageType.Unknown;
        }

        public enum ImageType
        {
            Unknown,
            JPEG,
            PNG,
            GIF,
            BMP,
            TIFF,
        }
    }
}
