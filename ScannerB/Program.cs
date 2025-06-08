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
            // Trying to set the processor affinity to the first CPU core
            try
            {
                
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: Could not set processor affinity. " + ex.Message);
            }

            // Extract arguments: directory to scan and the name of the pipe for communication
            string directoryPath = args[0];
            string pipeName = args[1];

            // Scaning all .txt files in the given directory and count word occurrences
            Dictionary<string, Dictionary<string, int>> wordIndex = ScanTextFiles(directoryPath);
            // Send the results (word counts) to another process via named pipe
            SendWordCountsToPipe(wordIndex, pipeName);
        }

        // This method will read all .txt files in the specified directory and counts words in each file
        static Dictionary<string, Dictionary<string, int>> ScanTextFiles(string directoryPath)
        {
            Dictionary<string, Dictionary<string, int>> fileWordMap = new Dictionary<string, Dictionary<string, int>>();

            // to get all .txt files from the directory
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
                        // Normalize the word: lowercase and strip punctuation
                        string cleanedWord = word.ToLower().Trim('.', ',', ';', ':', '!', '?');
                        if (string.IsNullOrWhiteSpace(cleanedWord)) continue;

                        //Counting process for word
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

        // Sends the word count data to a named pipe so another process can read it
        static void SendWordCountsToPipe(Dictionary<string, Dictionary<string, int>> data, string pipeName)
        {
            using NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            pipe.Connect();

            // Set up a StreamWriter for easy line-by-line writing
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
