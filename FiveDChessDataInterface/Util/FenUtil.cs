using System.Collections.Generic;

namespace FiveDChessDataInterface.Util
{
    public static class FenUtil
    {
        /// <summary>
        /// Expands notations such as individual numbers, such as 8 to 8*1, and expands N*X to a repetition of X N times
        /// </summary>
        /// <param name="compressedFen"></param>
        /// <returns></returns>
        public static string ExpandFen(string compressedFen)
        {
            var result = new List<string>();

            var lines = compressedFen.Split('/');
            foreach (var line in lines)
            {
                string processedLine = line;

                // expand {n*x} to {new string(x,n)}
                int asteriskIndex;
                while ((asteriskIndex = processedLine.IndexOf('*')) != -1) // while there is an *
                {
                    // replace 2*x by xx and 4*y by yyyy

                    var coefficient = int.Parse(processedLine.Substring(asteriskIndex - 1, 1));
                    var toCopy = processedLine[asteriskIndex + 1];
                    processedLine = processedLine.Replace($"{ coefficient}*{toCopy}", new string(toCopy, coefficient));
                }

                for (int i = 2; i <= 9; i++)
                    processedLine = processedLine.Replace(i.ToString(), new string('1', i));

                result.Add(processedLine);
            }


            return string.Join("/", result);
        }
    }
}
