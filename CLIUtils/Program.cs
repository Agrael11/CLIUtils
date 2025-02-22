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
                args = [.. Arguments.ExtraArguments];
                Arguments.Reset();
                Base64.Program.Main(args);
                return;
            }

            if (Arguments.IsArgumentSet("url"))
            {
                args = [.. Arguments.ExtraArguments];
                Arguments.Reset();
                URLUtil.Program.Main(args);
                return;
            }

            if (Arguments.IsArgumentSet("hex"))
            {
                args = [.. Arguments.ExtraArguments];
                Arguments.Reset();
                HexDump.Program.Main(args);
                return;
            }

            Console.WriteLine(Generator.GenerateHelp());
            Console.WriteLine(Generator.GenerateVersion());
        }

        static void RegisterArg()
        {
            Config.ErrorOnUnkownArguments = false;
            Config.FullName = "CLUtils Collection";
            Config.Version = "1.0.0";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "CLI Util Collection - Collection of various CLI Utilities";
            Arguments.RegisterArgument("b64", new ArgumentDefinition(ArgumentType.Flag, "base64", "b64", "Base64 Encode/Decode"));
            Arguments.RegisterArgument("url", new ArgumentDefinition(ArgumentType.Flag, "urlutil", "url", "URL Encode/Decode"));
            Arguments.RegisterArgument("hex", new ArgumentDefinition(ArgumentType.Flag, "hexdump", "hex", "Hex Encode/Decode"));
        }
    }
}
