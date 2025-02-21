using System.Text;
using CLIHelper;

namespace Base64
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
            if (!Arguments.IsArgumentSet("encode") && !Arguments.IsArgumentSet("decode"))
            {
                Console.Error.WriteLine("Either -e or -d is required");
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }

            if (Arguments.IsArgumentSet("encode") && Arguments.IsArgumentSet("decode"))
            {
                Console.Error.WriteLine("Cannot use both -e and -d");
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
                Console.Write("Input> ");
                input = Console.ReadLine()??"";
            }

            if (Arguments.IsArgumentSet("encode"))
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
                var output = Encode(inputData);
                if (width != -1)
                {
                    var builder = new StringBuilder();
                    for (int i = 0; i < output.Length; i += width)
                    {
                        builder.AppendLine(output.Substring(i, Math.Min(width, output.Length - i)));
                    }
                    output = builder.ToString();
                }
                if (Arguments.IsArgumentSet("outputfile"))
                {
                    WriteToFile(Encoding.UTF8.GetBytes(output));
                }
                else
                {
                    Console.WriteLine(output);
                }
            }
            else
            {
                try
                {
                    if (file)
                    {
                        input = File.ReadAllText(input);
                    }
                    var output = Decode(input);
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

        private static string Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
        private static byte[] Decode(string data)
        {
            return Convert.FromBase64String(data);
        }


        static void RegisterArg()
        {
            Config.FullName = "Base64";
            Config.Version = "0.0.1";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "Base64 Encoder/Decoder";
            Arguments.RegisterArgument("encode", new ArgumentDefinition(ArgumentType.Flag, "encode", "e", "Encode the input"));
            Arguments.RegisterArgument("decode", new ArgumentDefinition(ArgumentType.Flag, "decode", "d", "Decode the input"));
            Arguments.RegisterArgument("input", new ArgumentDefinition(ArgumentType.String, "input", "i", "String to encode or decode", "Input Text"));
            Arguments.RegisterArgument("inputfile", new ArgumentDefinition(ArgumentType.String, "inputfile", "if", "Selects input file (cannot be used with -i)", "File Name"));
            Arguments.RegisterArgument("outputfile", new ArgumentDefinition(ArgumentType.String, "outputfile", "of", "Selects Output File", "File Name"));
            Arguments.RegisterArgument("overwrite", new ArgumentDefinition(ArgumentType.Flag, "overwrite", "o", "Overwrite output file if exists"));
            Arguments.RegisterArgument("width", new ArgumentDefinition(ArgumentType.Integer, "width", "w", "Width of output (only usable with encoding)", "Width"));
            Arguments.RegisterArgument("help", new ArgumentDefinition(ArgumentType.Flag, "help", "h", "Shows Help"));
            Arguments.RegisterArgument("version", new ArgumentDefinition(ArgumentType.Flag, "version", "v", "Shows Version"));
            Config.HelpExample = "-i \"Hello World\" -e -of \"output.txt\"";
        }
    }
}
