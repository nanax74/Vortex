using System;
using System.Text.RegularExpressions;

public static class ConvertEx
{
    public static byte[] ToBytes(string hex)
    {
        hex = new string(hex.Where(c => "0123456789ABCDEFabcdef".Contains(c)).ToArray());

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
}
