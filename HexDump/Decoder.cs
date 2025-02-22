namespace HexDump
{
    public static class Decoder
    {
        public static byte[] Decode(string data, ViewType view)
        {
            return view switch
            {
                ViewType.HexView => DecodeHex(data),
                ViewType.FullView => DecodeFull(data),
                ViewType.CompactView => DecodeCompact(data),
                ViewType.AsciiView => throw new Exception("Ascii mode not supported."),
                _ => throw new Exception("Mode must be selected, cannot assume full mode."),
            };
        }


        private static byte[] DecodeHex(string data)
        {
            data = data.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            if (!IsHexOnly(data))
            {
                throw new Exception("Not a valid Hex");
            }
            var bytes = new List<byte>();
            for (int i = 0; i < data.Length; i += 2)
            {
                bytes.Add(Convert.ToByte(data[i..(i + 2)], 16));
            }
            return [.. bytes];
        }

        private static byte[] DecodeFull(string data)
        {
            var splitData = data.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            foreach (var line in splitData)
            {
                var end = line.IndexOf('|');
                if (end == -1)
                {
                    throw new Exception("Incorrect format of Hex");
                }
                var splitLine = line[0..(end - 1)].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < splitLine.Length; i++)
                {
                    if (!IsHexOnly(splitLine[i]))
                    {
                        throw new Exception("Not a valid Hex");
                    }
                    bytes.Add(Convert.ToByte(splitLine[i], 16));
                }
            }
            return [.. bytes];
        }

        private static byte[] DecodeCompact(string data)
        {
            var splitData = data.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            foreach (var line in splitData)
            {
                var end = line.IndexOf('|');
                if (end == -1)
                {
                    throw new Exception("Incorrect format of Hex");
                }
                var newLine = line[0..(end - 1)];
                newLine = newLine.Replace(" ", "");
                if (!IsHexOnly(newLine))
                {
                    throw new Exception("Not a valid Hex");
                }
                for (int i = 0; i < newLine.Length; i += 2)
                {
                    bytes.Add(Convert.ToByte(newLine[i..(i + 2)], 16));
                }
            }
            return [.. bytes];
        }

        private static bool IsHexOnly(string str)
        {
            return !(str.ToUpper().Any(c => !(((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')))));
        }
    }
}
