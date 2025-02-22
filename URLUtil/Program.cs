using CLIHelper;
using System.Text;
using System.Text.Json;

namespace URLUtil
{
    public class Program
    {
        private static readonly JsonSerializerOptions options1 = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions options2 = new() { WriteIndented = false };
        static public void Main(string[] args)
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
            int json = 0;
            if (Arguments.IsArgumentSet("infoj")) json = 1;
            if (Arguments.IsArgumentSet("infojm")) json = 2;
            bool info = Arguments.IsArgumentSet("info") || (json > 0);
            if (!Arguments.IsArgumentSet("encode") && !Arguments.IsArgumentSet("decode") && !info)
            {
                Console.Error.WriteLine("Either -e, -d or -x is required");
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }

            if (Arguments.IsArgumentSet("encode") && Arguments.IsArgumentSet("decode"))
            {
                Console.Error.WriteLine("Cannot use both -e and -d");
                Console.WriteLine(Generator.GenerateHelp());
                return;
            }

            string input;
            bool file = Arguments.IsArgumentSet("inputfile");
            if (!file && Arguments.IsArgumentSet("input"))
            {
                input = (string)Arguments.GetArgumentData("input");
            }
            else if (!file && Arguments.ExtraArguments.Count != 0)
            {
                input = string.Join(' ', Arguments.ExtraArguments);
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
                if (Arguments.IsArgumentSet("outputfile"))
                {
                    if (info)
                    {
                        output = Encoding.UTF8.GetBytes(GetInfo(Encoding.UTF8.GetString(output), json));
                    }
                    WriteToFile(output);
                }
                else
                {
                    if (info || Arguments.IsArgumentSet("infoj"))
                    {
                        Console.WriteLine(GetInfo(Encoding.UTF8.GetString(output), json));
                    }
                    else
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(output)); 
                    }
                }
            }
            else if (Arguments.IsArgumentSet("decode"))
            {
                try
                {
                    byte[] inputdata = Encoding.UTF8.GetBytes(input);
                    if (file)
                    {
                        inputdata = File.ReadAllBytes(input);
                    }
                    var output = Decode(inputdata);
                    if (Arguments.IsArgumentSet("outputfile"))
                    {
                        if (info)
                        {
                            output = Encoding.UTF8.GetBytes(GetInfo(Encoding.UTF8.GetString(output), json));
                        }
                        WriteToFile(output);
                    }
                    else
                    {
                        if (info)
                        {
                            Console.WriteLine(GetInfo(Encoding.UTF8.GetString(output), json));
                        }
                        else
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(output)); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "(" + input + ")");
                }
            }
            else
            {
                try
                {
                    byte[] inputdata = Encoding.UTF8.GetBytes(input);
                    if (file)
                    {
                        inputdata = File.ReadAllBytes(input);
                    }
                    var output = inputdata;
                    if (Arguments.IsArgumentSet("outputfile"))
                    {
                        if (info)
                        {
                            output = Encoding.UTF8.GetBytes(GetInfo(Encoding.UTF8.GetString(output), json));
                        }
                        WriteToFile(output);
                    }
                    else
                    {
                        if (info)
                        {
                            Console.WriteLine(GetInfo(Encoding.UTF8.GetString(output), json));
                        }
                        else
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(output));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "(" + input + ")");
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

        private static string GetInfo(string address, int json)
        {
            var uri = new Uri(address);
            var scheme = uri.Scheme;
            var host = uri.Host;
            var port = uri.Port;
            var fragment = uri.Fragment;
            var path = uri.AbsolutePath;
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (json == 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"FullUrl: {address}");
                builder.AppendLine($"Scheme: {scheme}");
                builder.AppendLine($"Host: {host}");
                builder.AppendLine($"Port: {port}");
                builder.AppendLine($"Path: {path}");
                builder.AppendLine($"Fragment: {fragment}");
                builder.AppendLine($"Queries:");
                foreach (var key in query.Keys)
                {
                    if (key is not string keyStr) continue;
                    if (query[keyStr] is not string value)
                    {
                        builder.AppendLine("\t" + keyStr);
                    }
                    else
                    {
                        builder.AppendLine($"\t{keyStr}: {value}");
                    }
                }
                return builder.ToString();
            }
            else
            {
                UriInfo info = new()
                {
                    FullUrl = address,
                    Scheme = scheme,
                    Host = host,
                    Port = port.ToString(),
                    Path = path,
                    Fragment = fragment
                };
                foreach (var key in query.Keys)
                {
                    if (key is not string keyStr) continue;
                    if (query[keyStr] is not string value)
                    {
                        info.Queries.Add(keyStr, "");
                    }
                    else
                    {
                        info.Queries.Add(keyStr, value);
                    }
                }
                return System.Text.Json.JsonSerializer.Serialize(info, ((json == 1) ? options1 : options2));
            }
        }

        private static byte[] Encode(byte[] data)
        {
            return System.Web.HttpUtility.UrlEncodeToBytes(data);
        }
        private static byte[] Decode(byte[] data)
        {
            return System.Web.HttpUtility.UrlDecodeToBytes(data);
        }


        static void RegisterArg()
        {
            Config.FullName = "URLUtiil";
            Config.Version = "0.5.0";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "URL Encoder/Decoder";
            Config.ErrorOnUnkownArguments = false;
            Arguments.RegisterArgument("encode", new ArgumentDefinition(ArgumentType.Flag, "encode", "e", "Encode the URL"));
            Arguments.RegisterArgument("decode", new ArgumentDefinition(ArgumentType.Flag, "decode", "d", "Decode the URL"));
            Arguments.RegisterArgument("multiline", new ArgumentDefinition(ArgumentType.Flag, "multiline", "m", "Allow Multiline input when input is not specifed as parameter"));
            Arguments.RegisterArgument("info", new ArgumentDefinition(ArgumentType.Flag, "extract", "x", "Extracts information from URL"));
            Arguments.RegisterArgument("infoj", new ArgumentDefinition(ArgumentType.Flag, "json", "j", "Extracts information from URL and format it as json"));
            Arguments.RegisterArgument("infojm", new ArgumentDefinition(ArgumentType.Flag, "compactjson", "cj", "Extracts information from URL and format it as compact json"));
            Arguments.RegisterArgument("input", new ArgumentDefinition(ArgumentType.String, "input", "i", "String to encode or decode", "Input Text"));
            Arguments.RegisterArgument("inputfile", new ArgumentDefinition(ArgumentType.String, "inputfile", "if", "Selects input file (cannot be used with -i)", "File Name"));
            Arguments.RegisterArgument("outputfile", new ArgumentDefinition(ArgumentType.String, "outputfile", "of", "Selects Output File", "File Name"));
            Arguments.RegisterArgument("overwrite", new ArgumentDefinition(ArgumentType.Flag, "overwrite", "o", "Overwrite output file if exists"));
            Arguments.RegisterArgument("help", new ArgumentDefinition(ArgumentType.Flag, "help", "h", "Shows Help"));
            Arguments.RegisterArgument("version", new ArgumentDefinition(ArgumentType.Flag, "version", "v", "Shows Version"));
            Config.HelpExample = "-i \"Hello World\" -e -of \"output.txt\"";
        }
    }
}
