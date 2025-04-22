using System;
using System.Buffers.Binary;
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

    public static uint[] ToUInt32Array(byte[] data)
    {
        // 4の倍数になるように調整された長さを計算
        int remainder = data.Length % 4; // 4で割った余り
        int newLength = (remainder == 0) ? data.Length : data.Length + (4 - remainder);

        // 0埋めされた新しいバイト配列を作成
        byte[] newData = new byte[newLength];

        // 元のデータをコピー（不足分はデフォルトの0のまま）
        for (int i = 0; i < data.Length; i++)
        {
            newData[i] = data[i];
        }

        // uintの配列を作成し、4バイトごとに変換
        uint[] result = new uint[newLength / 4];
        for (int i = 0; i < result.Length; i++)
        {
            uint res = BitConverter.ToUInt32(newData, i * 4);
            result[i] = BinaryPrimitives.ReverseEndianness(res);
        }

        return result;
    }

    public static byte[] ReverseEndian(byte[] data, int blockSize)
    {
        if (blockSize <= 0)
            throw new ArgumentException("blockSize は正の整数である必要があります。");

        if (data.Length % blockSize != 0)
            throw new ArgumentException("配列の長さは blockSize の倍数である必要があります。");

        byte[] result = new byte[data.Length];

        for (int i = 0; i < data.Length; i += blockSize)
        {
            for (int j = 0; j < blockSize; j++)
            {
                result[i + j] = data[i + blockSize - 1 - j];
            }
        }

        return result;
    }

}
