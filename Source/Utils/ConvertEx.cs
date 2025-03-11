using System;
using System.Text.RegularExpressions;

public static class ConvertEx
{
    private static string SanitizeHex(string hex)
    {
        return Regex.Replace(hex, "[^0-9A-Fa-f]", "");
    }

    public static byte[] ToBytes(string hex)
    {
        hex = SanitizeHex(hex);

        // 文字列の文字数(半角)が奇数の場合、頭に「0」を付ける
        int length = hex.Length;
        if (length % 2 == 1)
        {
            hex = "0" + hex;
            length++;
        }

        List<byte> data = new List<byte>();
        for (int i = 0; i < length - 1; i += 2)
        {
            // 16進数表記の文字列かどうかをチェック
            string buffer = hex.Substring(i, 2);

            data.Add(Convert.ToByte(buffer, 16));
        }
        return data.ToArray();
    }

    public static uint ToUInt32(string hex)
    {
        hex = SanitizeHex(hex);
        if(hex.Length > 8)
        {
            hex = hex.Substring(hex.Length - 8);
        }
        else if(hex.Length < 1)
        {
            return 0;
        }
        return Convert.ToUInt32(hex, 16);
    }
}
