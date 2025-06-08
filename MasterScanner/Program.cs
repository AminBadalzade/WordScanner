using System.Collections.Concurrent;
using System.Diagnostics;

namespace MasterScannerApp
{
    class MasterScanner
    {
        static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> index =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MasterScanner <pipeName1> <pipeName2>");
                return;
            }

            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x4; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: Could not set processor affinity: " + ex.Message);
            }

            string pipeName1 = args[0];
            string pipeName2 = args[1];
        }
    }
}