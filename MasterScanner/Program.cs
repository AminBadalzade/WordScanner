using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;

namespace MasterScannerApp
{
    class MasterScanner
    {
        // A thread-safe dictionary to hold the final word index
        // Outer key: filename, Inner key: word, Inner value: count
        static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> index =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MasterScanner <pipeName1> <pipeName2>");
                return;
            }

            // Try to assign the process to a specific CPU core
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x4; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: Could not set processor affinity: " + ex.Message);
            }

            //Read pipe names from command-line arguments
            string pipeName1 = args[0];
            string pipeName2 = args[1];

            Console.WriteLine($"Listening on pipes: {pipeName1}, {pipeName2}\n");

            // Start listening to both pipes concurrently
            Task listener1 = Task.Run(() => ListenToPipe(pipeName1));
            Task listener2 = Task.Run(() => ListenToPipe(pipeName2));

            // we will wait for both listeners to complete before proceeding
            Task.WaitAll(listener1, listener2);

            //It is the output that we get at the end
            Console.WriteLine("\n=== Final Word Index ===");
            foreach (var fileEntry in index)
            {
                foreach (var wordEntry in fileEntry.Value)
                {
                    Console.WriteLine($"{fileEntry.Key}:  Word \"{wordEntry.Key}\"  : Count : {wordEntry.Value}");
                }
            }
        }

        // This method listens to a single named pipe and processes incoming data
        static void ListenToPipe(string pipeName)
        {
            // Set up a pipe server to receive data from a Scanner
            using NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Console.WriteLine($"Waiting for connection on pipe: {pipeName}...");

            // Waiting until a client (ScannerA or ScannerB) connects
            pipeServer.WaitForConnection();
            Console.WriteLine($"Connected to pipe: {pipeName}");

            // Read lines sent by the scanner
            using StreamReader reader = new StreamReader(pipeServer);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(';');
                if (parts.Length != 3) continue;

                string filename = parts[0];
                string word = parts[1];
                if (!int.TryParse(parts[2], out int count)) continue;

                ConcurrentDictionary<string, int> fileWords = index.GetOrAdd(filename, _ => new ConcurrentDictionary<string, int>());
                fileWords.AddOrUpdate(word, count, (_, existingCount) => existingCount + count);
            }

            Console.WriteLine($"Finished reading from pipe: {pipeName}");
        }

    }
}