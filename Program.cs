using System;
using System.IO;
using System.Diagnostics;

namespace PythonAI
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            string[] Input = new string[0];
            string Output = string.Empty;
            string Expect = string.Empty;

            // Process arguments
            if(!processArguments(args, ref Input, ref Output, ref Expect))
            {
                Console.WriteLine("Fatal Error: Invalid arguments!");
                return;
            }

            // Read all input files
            string[] linesTemp = new string[1024];
            int linesLength = 0;

            foreach(string _Input in Input)
            {
                using(StreamReader sr = new StreamReader(File.OpenRead(_Input)))
                {
                    while((linesTemp[linesLength] = sr.ReadLine()) != null)
                    {
                        linesLength++;
                        
                        if(linesLength >= linesTemp.Length)
                        {
                            Console.WriteLine("Fatal Error: Too much input!");
                            return;
                        }
                    }
                }
            }

            // Remove empty lines
            string[] lines = new string[linesLength];
            for(int i = 0; i < linesLength; i++)
            {
                lines[i] = linesTemp[i];
            }

            if(lines.Length <= 0)
            {
                Console.WriteLine("Fatal Error: Not enough input!");
                return;
            }

            // Generate code
            int attempts = 0;
            int maxArugments = 8;
            string[] commands = new string[0];
            int[,] commandOptions = new int[0,0];
            string pythonOutput = string.Empty;

            do
            {
                for (int i1 = 0; i1 < commands.Length; i1++)
                {
                    commands[i1] = lines[commandOptions[i1, 0]];

                    for (int i2 = 0; i2 < maxArugments; i2++)
                            commands[i1] += (commandOptions[i1, i2 + 1] >= 0 ? " " + lines[commandOptions[i1, i2 + 1]] : string.Empty);
                }

                WriteIntoFile(commands, Output);
                pythonOutput = GetOutput(Output);
                
                attempts++;
                
                if(!Increase(ref commandOptions, 0, 0, commands.Length - 1, maxArugments, lines.Length - 1))
                {
                    commands = new string[commands.Length + 1];
                    commandOptions = new int[commands.Length, maxArugments + 1];

                    for (int i1 = 0; i1 < commands.Length; i1++)
                    {
                        commandOptions[i1, 0] = 0;

                        for (int i2 = 0; i2 < maxArugments; i2++)
                            commandOptions[i1, i2 + 1] = -1;
                    }
                }
            }
            while(pythonOutput != Expect + "\n");

            Console.WriteLine("Done!");
            Console.WriteLine("Attempts: " + attempts.ToString());
        }

        private static bool processArguments(string[] args, ref string[] _Input, ref string _Output, ref string _Expect)
        {
            if(args.Length == 0)
                return false;

            bool parsingInput = false;
            bool parsingOutput = false;
            bool parsingExpect = false;

            string[] inputTemp = new string[args.Length];
            int inputLength = 0;

            _Output = "output.py";
            _Expect = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "--input":
                        parsingInput = true;
                        parsingOutput = false;
                        parsingExpect = false;
                        break;

                    case "--output":
                        parsingInput = false;
                        parsingOutput = true;
                        parsingExpect = false;
                        break;

                    case "--expect":
                        parsingInput = false;
                        parsingOutput = false;
                        parsingExpect = true;
                        break;

                    default:
                        if(parsingInput)
                        {
                            inputTemp[inputLength] = args[i];
                            inputLength++;
                        }
                        else if(parsingOutput)
                            _Output = args[i];
                        else if(parsingExpect)
                            _Expect = args[i];
                        break;
                }
            }

            _Input = new string[inputLength];
            for(int i = 0; i < inputLength; i++)
            {
                _Input[i] = inputTemp[i];
            }

            if(inputLength == 0)
                return false;
            else
                return true;
        }

        private static string GetOutput(string _file)
        {
            string pythonOutput = string.Empty;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "python";
            startInfo.Arguments = _file;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            Process python = new Process();
            python.StartInfo = startInfo;
            python.Start();

            StreamReader reader = python.StandardOutput;
            pythonOutput += reader.ReadToEnd();

            python.WaitForExit();
            python.Dispose();

            return pythonOutput;
        }

        private static void WriteIntoFile(string[] commands, string path)
        {
            File.Delete(path);

            using(StreamWriter sw = new StreamWriter(File.OpenWrite(path)))
            {
                foreach(string command in commands)
                {
                    sw.WriteLine(command);
                }
            }
        }

        private static bool Increase(ref int[,] cmdOptions, int i1, int i2, int i1Max, int i2Max, int valueMax)
        {
            if(i1 > i1Max || i2 > i2Max)
                return false;

            if(++cmdOptions[i1, i2] > valueMax)
            {
                cmdOptions[i1, i2] = (i2 == 0 ? 0 : -1);
                
                if(i2 + 1 > i2Max)
                    if(i1 + 1 > i1Max)
                        return false;
                    else
                        return Increase(ref cmdOptions, i1 + 1, 0, i1Max, i2Max, valueMax);
                else
                    return Increase(ref cmdOptions, i1, i2 + 1, i1Max, i2Max, valueMax);
            }

            return true;
        }
    }
}
