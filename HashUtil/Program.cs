using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;
using CLIHelper;

namespace HashUtil
{
    public class Program
    {
        private enum HashTypes { MD5, CRC32, SHA1, SHA256, SHA384, SHA512 }
        private static readonly Dictionary<HashTypes, string[]> hashAliases = new()
        {
            { HashTypes.MD5, ["MD5"]},
            { HashTypes.CRC32, ["CRC32"]},
            { HashTypes.SHA1, ["SHA1", "SHA-1"]},
            { HashTypes.SHA256, ["SHA256", "SHA-256"]},
            { HashTypes.SHA384, ["SHA384", "SHA-384"]},
            { HashTypes.SHA512, ["SHA512", "SHA-512"]},
        };

        private static readonly Dictionary<HashTypes, string> hashExtensions = new()
        {
            { HashTypes.MD5, "md5"},
            { HashTypes.CRC32, "crc32"},
            { HashTypes.SHA1, "sha1"},
            { HashTypes.SHA256, "sha256"},
            { HashTypes.SHA384, "sha384"},
            { HashTypes.SHA512, "sha512"},
        };

        static public void Main(string[] args)
        {
            args = "/check /if test.txt".Split(' ');
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

            string inputfile;
            if (Arguments.IsArgumentSet("inputfile"))
            {
                inputfile = (string)Arguments.GetArgumentData("inputfile");
            }
            else
            {
                Console.Error.WriteLine("Input file needs to be selected");
                Console.WriteLine(Generator.GenerateHelp());

                return;
            }

            if (!File.Exists(inputfile))
            {
                Console.Error.WriteLine($"Input file {inputfile} not found");
                return;
            }

            var input = File.ReadAllBytes(inputfile);

            if (Arguments.IsArgumentSet("hash"))
            {
                var hashtype = (string)Arguments.GetArgumentData("hash");
                if (hashtype.Equals("ALL", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var hashType in Enum.GetValues(typeof(HashTypes)))
                    {
                        var resulthash = GetHash(input, (HashTypes)hashType);
                        Console.WriteLine(Enum.GetName(typeof(HashTypes), hashType) + " : " + resulthash);
                    }
                }
                else
                {
                    var hashtypeSelected = hashAliases.Where(alias => alias.Value.Contains(hashtype.ToUpper()));
                    if (hashtypeSelected.Any())
                    {
                        var resulthash = GetHash(input, hashtypeSelected.First().Key);
                        Console.WriteLine(resulthash);
                    }
                    else
                    {
                        Console.Error.WriteLine($"[ERROR] Unknown Hash Type {hashtype}");
                        return;
                    }
                }

                return;
            }

            if (Arguments.IsArgumentSet("check"))
            {
                if (Arguments.IsArgumentSet("inputhash"))
                {
                    var hashinput = (string)Arguments.GetArgumentData("inputhash");
                    if (File.Exists(hashinput))
                    {
                        hashinput = File.ReadAllText(hashinput);
                    }
                    if (!TryGetHashType(hashinput, out var hashtype) || hashtype is null)
                    {
                        Console.Error.WriteLine($"[ERROR] Unknown Hash Type or Hash File not found {hashinput}");
                        return;
                    }
                    hashinput = hashinput.Replace("\r", "").Replace("\n", "");
                    var myhash = GetHash(input, (HashTypes)hashtype);
                    if (myhash.Equals(hashinput, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine($"Hashes Match for {Enum.GetName(typeof(HashTypes), hashtype)}");
                    }
                    else
                    {
                        Console.WriteLine($"Hashes Mismatch for {Enum.GetName(typeof(HashTypes), hashtype)}");
                    }
                }
                else
                {
                    var basefile = inputfile[..inputfile.LastIndexOf('.')];
                    var foundFile = false;
                    foreach (var hashtype in Enum.GetValues(typeof(HashTypes)))
                    {
                        var hashextension = hashExtensions[(HashTypes)hashtype];
                        var hashfile = basefile + "." + hashextension;
                        if (File.Exists(hashfile))
                        {
                            foundFile = true;
                            var otherhash = File.ReadAllText(hashfile);
                            otherhash = otherhash.Replace("\r", "").Replace("\n", "");
                            var myhash = GetHash(input, (HashTypes)hashtype);
                            if (myhash.Equals(otherhash, StringComparison.CurrentCultureIgnoreCase))
                            {
                                Console.WriteLine($"Hashes Match for {Enum.GetName(typeof(HashTypes), hashtype)}");
                            }
                            else
                            {
                                Console.WriteLine($"Hashes Mismatch for {Enum.GetName(typeof(HashTypes), hashtype)}");
                            }
                        }
                    }
                    if (!foundFile)
                    {
                        Console.Error.WriteLine("[ERROR] No input hash or hashfile selected AND no hashfile automatically detected.");
                    }
                }
            }
        }

        private static string GetHash(byte[] data, HashTypes hashType)
        {
            byte[] result = hashType switch
            {
                HashTypes.CRC32 => System.IO.Hashing.Crc32.Hash(data),
                HashTypes.MD5 => System.Security.Cryptography.MD5.HashData(data),
                HashTypes.SHA1 => System.Security.Cryptography.SHA1.HashData(data),
                HashTypes.SHA256 => System.Security.Cryptography.SHA256.HashData(data),
                HashTypes.SHA384 => System.Security.Cryptography.SHA384.HashData(data),
                HashTypes.SHA512 => System.Security.Cryptography.SHA512.HashData(data),
                _ => throw new NotImplementedException(),
            };
            var builder = new StringBuilder();
            foreach (var num in result)
            {
                builder.Append(num.ToString("X2"));
            }
            return builder.ToString();
        }

        private static bool IsHexOnly(string str)
        {
            return !(str.ToUpper().Any(c => !(((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')))));
        }

        private static string ParseInput(string input)
        {
            if (File.Exists(input))
            {
                return File.ReadAllText(input);
            }
            return input;
        }

        private static bool TryGetHashType(string hash, out HashTypes? hashType)
        {
            hashType = null;
            if (!IsHexOnly(hash)) return false;
            switch (hash.Length)
            {
                case 8: hashType = HashTypes.CRC32; break;
                case 32: hashType = HashTypes.MD5; break;
                case 40: hashType = HashTypes.SHA1; break;
                case 64: hashType = HashTypes.SHA256; break;
                case 96: hashType = HashTypes.SHA384; break;
                case 128: hashType = HashTypes.SHA512; break;
                default:
                {
                        hashType = null;
                        return false;
                }
            }

            return true;
        }


        static void RegisterArg()
        {
            Config.FullName = "HashUtil";
            Config.Version = "0.1.0";
            Config.License = "Copyright (C) 2025 Oliver Neuschl\r\nThis software uses GPL 3.0 License";
            Config.HelpHeader = "File Hashing & Hash Checker";
            Config.ErrorOnUnkownArguments = false;
            Arguments.RegisterArgument("hash", new ArgumentDefinition(ArgumentType.String, "hash", "h", "Get the hash of file", "Hash Algoritms"));
            Arguments.RegisterArgument("check", new ArgumentDefinition(ArgumentType.Flag, "check", "c", "Compare the hash"));
            Arguments.RegisterArgument("inputfile", new ArgumentDefinition(ArgumentType.String, "inputfile", "if", "Selects input file", "File Name"));
            Arguments.RegisterArgument("inputhash", new ArgumentDefinition(ArgumentType.String, "inputhash", "ih", "Selects input hash or file contaning it", "HASH"));
            Arguments.RegisterArgument("help", new ArgumentDefinition(ArgumentType.Flag, "help", "h", "Shows Help"));
            Arguments.RegisterArgument("version", new ArgumentDefinition(ArgumentType.Flag, "version", "v", "Shows Version"));
            Config.HelpExample = "-if file.exe -c -ih 414fa339";
            var builder = new StringBuilder();
            builder.AppendLine("HashUtil is a simple utility to get hash of file and compare it with given hash.");
            builder.Append("Supported algoritms are ");
            var enumnames = (((HashTypes[])Enum.GetValues(typeof(HashTypes))).Select(type => Enum.GetName(type)));
            for (int i = 0; i < enumnames.Count() - 1; i++)
            {
                builder.Append(enumnames.ElementAt(i));
                if (i < enumnames.Count() - 2)
                {
                    builder.Append(", ");
                }
                else
                {
                    builder.Append(" and ");
                }
            }
            builder.Append(enumnames.Last());
            builder.AppendLine(".");
            builder.AppendLine("You can also use \"ALL\" to list all hashes.");
            Config.HelpFooter = builder.ToString();
        }
    }
}
