using System;
using System.Globalization;
using System.Threading;

namespace DataInterfaceConsole
{
    class Program
    {
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Console.Title = "5D Data Interface Console";



            Console.WriteLine("Hello World!");
        }
    }
}
