# MasterScanner

## Overview
**MasterScanner** is a simple yet powerful **multi-process text file word counting system** built with **C#**. It demonstrates how multiple scanner applications can process text files in parallel, send their results via **named pipes**, and have a master application collect, merge, and display a combined **word count index**.

## Why I Created This
I built this project to explore **inter-process communication** and **concurrency** in .NET, specifically using **named pipes** and **concurrent collections**. The goals were to:

- Understand how to divide work between multiple scanner agents scanning different folders simultaneously.
- Learn how to safely merge data from multiple sources into a central store without conflicts or data loss.
- Experiment with **processor affinity** to improve performance by binding processes to specific CPU cores.
- Practice building **scalable, modular console applications** with clean separation of concerns.

## How It Works
- **ScannerA** and **ScannerB** are two independent console applications that scan their respective directories for `.txt` files.
- Each scanner reads the text files, counts occurrences of each word, and sends this data through a **named pipe** to the **MasterScanner**.
- **MasterScanner** listens on multiple named pipes concurrently, collecting word counts from both scanners.
- It merges all incoming data into a **thread-safe dictionary**, summing counts of the same words across different files and scanners.
- When all data is received, **MasterScanner** outputs the complete word index, showing each word’s frequency per file.

## Key Features
- **Concurrent scanning** of files with two separate scanners.
- **Inter-process communication** via **named pipes**.
- **Thread-safe data aggregation** using `ConcurrentDictionary`.
- Optional **processor affinity** settings to optimize CPU core usage.
- Clear **console output** for easy review of results.

## How to Use
1. Build all three projects (**ScannerA**, **ScannerB**, **MasterScanner**).
2. Run **MasterScanner** with two pipe names as arguments, for example:
3. Run **ScannerA** and **ScannerB** with the directory to scan and their corresponding pipe name.
4. Watch the master app aggregate and display word counts from both scanners
