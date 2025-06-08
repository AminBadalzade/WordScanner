using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace ScannerBApp
{
    class ScannerB
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ScannerB <directoryPath> <pipeName>");
                return;
            }

            try
            {
                // Pin this process to CPU Core 1
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: Could not set processor affinity. " + ex.Message);
            }

            string directoryPath = args[0];
            string pipeName = args[1];

            Dictionary<string, Dictionary<string, int>> wordIndex = ScanTextFiles(directoryPath);
            SendWordCountsToPipe(wordIndex, pipeName);
        }

        static Dictionary<string, Dictionary<string, int>> ScanTextFiles(string directoryPath)
        {
            Dictionary<string, Dictionary<string, int>> fileWordMap = new Dictionary<string, Dictionary<string, int>>();

            string[] files = Directory.GetFiles(directoryPath, "*.txt");
            foreach (string filePath in files)
            {
                Dictionary<string, int> wordCount = new Dictionary<string, int>();
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    foreach (string word in words)
                    {
                        string cleanedWord = word.ToLower().Trim('.', ',', ';', ':', '!', '?');
                        if (string.IsNullOrWhiteSpace(cleanedWord)) continue;

                        if (wordCount.ContainsKey(cleanedWord))
                            wordCount[cleanedWord]++;
                        else
                            wordCount[cleanedWord] = 1;
                    }
                }

                string filename = Path.GetFileName(filePath);
                fileWordMap[filename] = wordCount;
            }

            return fileWordMap;
        }

        static void SendWordCountsToPipe(Dictionary<string, Dictionary<string, int>> data, string pipeName)
        {
            using NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            pipe.Connect();

            using StreamWriter writer = new StreamWriter(pipe) { AutoFlush = true };

            foreach (KeyValuePair<string, Dictionary<string, int>> fileEntry in data)
            {
                foreach (KeyValuePair<string, int> wordEntry in fileEntry.Value)
                {
                    writer.WriteLine($"{fileEntry.Key};{wordEntry.Key};{wordEntry.Value}");
                }
            }
        }
    }
}
