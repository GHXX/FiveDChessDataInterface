using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FiveDChessDataInterface.Util
{
    public static class FenUtil
    {
        /// <summary>
        /// Expands notations such as individual numbers, such as 8 to 8*1, and expands N*X to a repetition of X N times
        /// </summary>
        /// <param name="compressedFen"></param>
        /// <param name="removeExtraAsterisks">Whether or not to ignore asterisks which refer to if a piece has been moved previously.</param>
        /// <returns></returns>
        public static string ExpandFen(string compressedFen, bool removeExtraAsterisks = true) // TODO implement loading pieces that are considered to have moved 
        {
            var result = new List<string>();

            if (removeExtraAsterisks)
                compressedFen = Regex.Replace(compressedFen, @"(\D)\*", @"$1"); // turns [A-z]\* into [A-z] e.g. P* -> P

            var lines = compressedFen.Split('/');
            foreach (var line in lines)
            {
                string processedLine = line;

                // expand {n*x} to {new string(x,n)}
                int asteriskIndex;
                while ((asteriskIndex = processedLine.IndexOf('*')) != -1) // while there is an *
                {
                    // replace 2*x by xx and 4*y by yyyy

                    bool ok = int.TryParse(processedLine.Substring(asteriskIndex - 1, 1), out int coefficient);
                    if (!ok)
                        if (removeExtraAsterisks)
                        {
                            throw new Exception("Fen parsing failed even though extra asterisks should be ignored. Please report this.");
                        }
                        else
                        {
                            throw new FormatException($"Unable to expand asterisk expression at line {processedLine}, " +
                                $"at position index {asteriskIndex}, because there is no number at position index {asteriskIndex - 1}");
                        }

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
