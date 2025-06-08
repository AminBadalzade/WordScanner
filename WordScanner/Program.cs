using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace ScannerAApp
{
    class ScannerA
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ScannerA <directoryPath> <pipeName>");
                return;
            }



            string directoryPath = args[0];
            string pipeName = args[1];

        }
    }
}