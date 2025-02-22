using CLIHelper;
using System.Text;

namespace HexDump
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RegisterArg();

            try
            {
                Arguments.ParseArguments(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }
            if (Arguments.IsArgumentSet("help"))
            {
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }
            if (Arguments.IsArgumentSet("version"))
            {
                Console.WriteLine(Generator.GenerateVersion());
                return;
            }
            if (Arguments.IsArgumentSet("input") && Arguments.IsArgumentSet("inputfile"))
            {
                Console.Error.WriteLine("Cannot use both -i and -if");
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }

            int width = -1;
            if (Arguments.IsArgumentSet("width"))
            {
                width = (int)Arguments.GetArgumentData("width");
            }

            string input;
            bool file = Arguments.IsArgumentSet("inputfile");
            if (!file && Arguments.IsArgumentSet("input"))
            {
                input = (string)Arguments.GetArgumentData("input");
            }
            else if (file)
            {
                input = (string)Arguments.GetArgumentData("inputfile");
            }
            else
            {
                if (Console.IsInputRedirected || (bool)Arguments.IsArgumentSet("multiline"))
                {
                    input = ConsoleExtras.ReadLinesUntilEOF(!Console.IsInputRedirected);
                }
                else
                {
                    Console.Write("Input> ");
                    input = Console.ReadLine() ?? "";
                }
            }

            var view = ViewType.InvalidView;
            if (Arguments.IsArgumentSet("fullview"))
            {
                view = ViewType.FullView;
            }
            else if (Arguments.IsArgumentSet("compact"))
            {
                view = ViewType.CompactView;
            }
            else if (Arguments.IsArgumentSet("asciionly"))
            {
                view = ViewType.AsciiView;
            }
            else if (Arguments.IsArgumentSet("hexonly"))
            {
                view = ViewType.HexView;
            }

            if (Arguments.IsArgumentSet("fromhex"))
            {
                try
                {
                    if (file)
                    {
                        input = File.ReadAllText(input);
                    }
                    var output = Decoder.Decode(input, view);
                    if (Arguments.IsArgumentSet("outputfile"))
                    {
                        WriteToFile(output);
                    }
                    else
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(output));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
            else
            {
                byte[] inputData;
                if (!file)
                {
                    inputData = Encoding.UTF8.GetBytes(input);
                }
                else
                {
                    inputData = File.ReadAllBytes(input);
                }
                var output = Encoder.Encode(inputData, view, width);
                if (Arguments.IsArgumentSet("outputfile"))
                {
                    WriteToFile(Encoding.UTF8.GetBytes(output));
                }
                else
                {
                    Console.WriteLine(output);
                }
            }
        }

        private static void WriteToFile(byte[] data)
        {
            var path = (string)Arguments.GetArgumentData("outputfile");
            if (File.Exists(path) && !Arguments.IsArgumentSet("overwrite"))
            {
                Console.WriteLine($"File {path} already exists. Do you want to override? Y/[N]");
                var key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y)
                {
                    return;
                }
            }
            File.WriteAllBytes(path, data);
        }

        static void RegisterArg()
        {
            Config.FullName = "HexDump";
            Config.Version = "0.0.1";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "Hex Dump Utility";
            Arguments.RegisterArgument("fullview", new ArgumentDefinition(ArgumentType.Flag, "full", "f", "Show full view of hex dump (Default for normal operation)"));
            Arguments.RegisterArgument("compact", new ArgumentDefinition(ArgumentType.Flag, "compact", "c", "Show compact view of hex dump"));
            Arguments.RegisterArgument("asciionly", new ArgumentDefinition(ArgumentType.Flag, "ascii", "a", "Show only ASCII characters (Not usable with reverse)"));
            Arguments.RegisterArgument("hexonly", new ArgumentDefinition(ArgumentType.Flag, "hex", "x", "Show only Hex result"));
            Arguments.RegisterArgument("fromhex", new ArgumentDefinition(ArgumentType.Flag, "reverse", "r", "Reverse the operation"));
            Arguments.RegisterArgument("multiline", new ArgumentDefinition(ArgumentType.Flag, "multiline", "m", "Allow Multiline input when input is not specifed as parameter")); 
            Arguments.RegisterArgument("input", new ArgumentDefinition(ArgumentType.String, "input", "i", "String to encode or decode", "Input Text"));
            Arguments.RegisterArgument("inputfile", new ArgumentDefinition(ArgumentType.String, "inputfile", "if", "Selects input file (cannot be used with -i)", "File Name"));
            Arguments.RegisterArgument("outputfile", new ArgumentDefinition(ArgumentType.String, "outputfile", "of", "Selects Output File", "File Name"));
            Arguments.RegisterArgument("overwrite", new ArgumentDefinition(ArgumentType.Flag, "overwrite", "o", "Overwrite output file if exists"));
            Arguments.RegisterArgument("width", new ArgumentDefinition(ArgumentType.Integer, "width", "w", "Width of output", "Width"));
            Arguments.RegisterArgument("help", new ArgumentDefinition(ArgumentType.Flag, "help", "h", "Shows This Information"));
            Arguments.RegisterArgument("version", new ArgumentDefinition(ArgumentType.Flag, "version", "v", "Shows Version"));
            Config.HelpExample = "-i \"Hello World\" -e -of \"output.txt\"";
        }
    }
}
