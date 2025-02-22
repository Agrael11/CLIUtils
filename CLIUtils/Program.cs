using CLIHelper;
using System.Text;

namespace CLIUtils
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

            if (Arguments.IsArgumentSet("b64"))
            {
                var argList = args.ToList();
                argList.Remove("--base64");
                argList.Remove("-b64");
                argList.Remove("/base64");
                argList.Remove("/b64");
                Arguments.Reset();
                Base64.Program.Main([.. argList]);
                return;
            }

            if (Arguments.IsArgumentSet("url"))
            {
                var argList = args.ToList();
                argList.Remove("--urlutil");
                argList.Remove("-url");
                argList.Remove("/urlutil");
                argList.Remove("/url");
                Arguments.Reset();
                URLUtil.Program.Main([.. argList]);
                return;
            }

            if (Arguments.IsArgumentSet("hex"))
            {
                var argList = args.ToList();
                argList.Remove("--hexdump");
                argList.Remove("-hex");
                argList.Remove("/hexdump");
                argList.Remove("/hex");
                Arguments.Reset();
                HexDump.Program.Main([.. argList]);
                return;
            }

            if (Arguments.IsArgumentSet("version"))
            {
                Console.WriteLine(Generator.GenerateVersion());
                return;
            }

            Console.WriteLine(Generator.GenerateHelp());
        }

        static void RegisterArg()
        {
            Config.IgnoreUnknownArguments = true;
            Config.FullName = "CLUtils Collection";
            Config.Version = "0.1.0";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "CLI Util Collection - Collection of various CLI Utilities";
            Arguments.RegisterArgument("b64", new ArgumentDefinition(ArgumentType.Flag, "base64", "b64", "Base64 Encode/Decode"));
            Arguments.RegisterArgument("url", new ArgumentDefinition(ArgumentType.Flag, "urlutil", "url", "URL Encode/Decode"));
            Arguments.RegisterArgument("hex", new ArgumentDefinition(ArgumentType.Flag, "hexdump", "hex", "Hex Encode/Decode"));
            Arguments.RegisterArgument("help", new ArgumentDefinition(ArgumentType.Flag, "help", "h", "Shows Help Dialogue"));
            Arguments.RegisterArgument("version", new ArgumentDefinition(ArgumentType.Flag, "version", "v", "Shows Version"));
        }
    }
}
