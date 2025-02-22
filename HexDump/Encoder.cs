using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HexDump
{
    public static class Encoder
    {
        public static string Encode(byte[] data, ViewType view, int width)
        {
            width = GetWidthOrDefault(width, view);
            var builder = new StringBuilder();
            for (var startIndex = 0; startIndex < data.Length; startIndex += width)
            {
                switch (view)
                {
                    case ViewType.InvalidView:
                    case ViewType.FullView:
                        if (width == -1) width = 16;
                        EncodeLineNormal(data, startIndex, width, builder);
                        break;
                    case ViewType.HexView:
                        EncodeLineHex(data, startIndex, width, builder);
                        break;
                    case ViewType.AsciiView:
                        EncodeLineAscii(data, startIndex, width, builder);
                        break;
                    case ViewType.CompactView:
                        if (width == -1) width = 8;
                        EncodeLineCompact(data, startIndex, width, builder);
                        break;
                }
            }

            return builder.ToString();
        }
        
        private static int GetWidthOrDefault(int width, ViewType view)
        {
            if (width > 0) return width;

            return view switch
            {
                ViewType.InvalidView or ViewType.FullView => 16,
                ViewType.CompactView => 8,
                _ => 1,
            };
        }

        private static void EncodeLineNormal(byte[] data, int startIndex, int width, StringBuilder builder)
        {
            var line = startIndex.ToString("X").PadLeft(8, '0') + ' ';
            int i;
            for (i = 0; i < width; i++)
            {
                var index = startIndex + i;
                if (index >= data.Length) break;
                if (i % 8 == 0) line += ' ';
                line += data[index].ToString("X").PadLeft(2, '0') + ' ';
            }
            var lwidth = width * 3 + 9 + 2;
            lwidth += ((width - 1) / 8);

            builder.Append(line.PadRight(lwidth, ' '));
            builder.Append('|');
            builder.Append(GetString(data, startIndex, i).PadRight(width, ' '));
            builder.Append('|');
            builder.AppendLine();
        }

        private static void EncodeLineHex(byte[] data, int startIndex, int width, StringBuilder builder)
        {
            for (var i = 0; i < width; i++)
            {
                var index = startIndex + i;
                if (index >= data.Length) break;
                builder.Append(data[index].ToString("X").PadLeft(2, '0'));
            }
            if (width > 1) builder.AppendLine();
        }

        private static void EncodeLineAscii(byte[] data, int startIndex, int width, StringBuilder builder)
        {
            for (var i = 0; i < width; i++)
            {
                var index = startIndex + i;
                if (index >= data.Length) break;
                var c = (char)data[index];
                if (c < 32 || c > 126) c = '.';
                builder.Append(c);
            }
            if (width > 1) builder.AppendLine();
        }

        private static void EncodeLineCompact(byte[] data, int startIndex, int width, StringBuilder builder)
        {
            var line = "";
            int i;
            for (i = 0; i < width; i++)
            {
                var index = startIndex + i;
                if (index >= data.Length) break;
                if (index % 4 == 0 && i != 0) line += ' ';
                line += data[index].ToString("X").PadLeft(2, '0');
            }
            var lwidth = width * 2 + 2;
            lwidth += ((width - 1) / 4);

            builder.Append(line.PadRight(lwidth, ' '));
            builder.Append('|');
            builder.Append(GetString(data, startIndex, i).PadRight(width, ' '));
            builder.Append('|');
            builder.AppendLine();
        }

        private static string GetString(byte[] data, int index, int length)
        {
            var result = "";
            for (var i = index; i < index + length; i++)
            {
                var c = (char)data[i];
                if (c < 32 || c > 126) c = '.';
                result += c;
            }
            return result;
        }
    }
}
