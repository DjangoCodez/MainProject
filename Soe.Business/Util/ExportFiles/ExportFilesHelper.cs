using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles.Common
{
    public static class ExportFilesHelper
    {
        public static string FillWithEmptyBeginning(int targetSize, string originValue)
        {
            return FillWithChar(" ", targetSize, originValue);
        }

        public static string FillWithEmptyEnd(int targetSize, string originValue)
        {
            return FillWithChar(" ", targetSize, originValue, reverse: true);
        }

        public static string FillWithZerosBeginning(int targetSize, string originValue)
        {
            return FillWithChar("0", targetSize, originValue);
        }

        public static string FillWithZerosEnd(int targetSize, string originValue)
        {
            return FillWithChar("0", targetSize, originValue, reverse: true);
        }


        public static string FillWithChar(string character, int targetSize, string originValue, bool truncate = true, bool reverse = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                StringBuilder zeros = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros.Append(character);
                }
                return !reverse ? (zeros.ToString() + originValue) : (originValue + zeros.ToString());
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }
    }
}


