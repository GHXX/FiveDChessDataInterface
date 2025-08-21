using System;
using System.Text.RegularExpressions;

namespace DataInterfaceConsole;

internal static class Extensions {
    public static string ToSanitizedString(this Exception e) => Regex.Replace(e.ToString(), @"([A-z]:[\\\/]Users[\\\/])(.*?)([\\\/])", "$1USERNAME$3");
}
