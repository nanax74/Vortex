using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class Assembler
{
    // 同時アセンブルは1つまで
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    public static async Task<(string, string)> AssemblePowerPC(string asmCode)
    {
        // 枠が空くまで待機
        await semaphore.WaitAsync();

        try
        {
            // ディレクトリのセットアップ
            string workingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PPC");
            Directory.CreateDirectory(workingDir);

            // リソースファイルを展開
            var files = PreparePaths(workingDir);
            await ExtractToolchainAsync(files);

            // ソースコードをファイルに書き込む
            await File.WriteAllTextAsync(files.SourcePath, asmCode + Environment.NewLine);

            // アセンブラを実行
            var result = await RunToolAsync(files.AsPath, $"-mregnames -mgekko \"{files.SourcePath}\" -o \"{files.ObjectPath}\"");
            if (!result.Success) return ("", $"アセンブリエラー: {result.Error}");

            // シンボルをチェック
            var nmResult = await RunToolAsync(files.NmPath, $"\"{files.ObjectPath}\"");
            var undefined = ParseUndefinedSymbols(nmResult.Output);
            if (undefined.Any()) return ("", $"未定義シンボル: {string.Join(", ", undefined)}");

            // バイナリに変換
            var copyResult = await RunToolAsync(files.ObjcopyPath, $"-O binary \"{files.ObjectPath}\" \"{files.BinaryPath}\"");
            if (!copyResult.Success) return ("", $"objcopyエラー: {copyResult.Error}");

            byte[] bytes = await File.ReadAllBytesAsync(files.BinaryPath);
            return (FormatBinary(bytes), "");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private record AsmPaths
    (
        string SourcePath,
        string ObjectPath,
        string BinaryPath,
        string AsPath,
        string NmPath,
        string ObjcopyPath
    );

    private static AsmPaths PreparePaths(string dir) => new
    (
        Path.Combine(dir, "code.s"),
        Path.Combine(dir, "code.o"),
        Path.Combine(dir, "code.bin"),
        Path.Combine(dir, "powerpc-eabi-as.exe"),
        Path.Combine(dir, "powerpc-eabi-nm.exe"),
        Path.Combine(dir, "powerpc-eabi-objcopy.exe")
    );

    // リソースファイルを一時ファイルに展開する
    private static async Task ResourceInit(string resourceName, string outputPath)
    {
        // リソースストリームを取得
        using (Stream? resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (resourceStream == null)
                throw new Exception($"リソース '{resourceName}' が見つかりません。");

            // リソースをファイルに非同期で書き込む
            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
            {
                await resourceStream.CopyToAsync(fileStream);
            }
        }
    }

    private static async Task ExtractToolchainAsync(AsmPaths paths)
    {
        await ResourceInit("Vortex.powerpc-eabi-as.exe", paths.AsPath);
        await ResourceInit("Vortex.powerpc-eabi-nm.exe", paths.NmPath);
        await ResourceInit("Vortex.powerpc-eabi-objcopy.exe", paths.ObjcopyPath);
    }

    private record ToolResult(bool Success, string Output, string Error);

    private static async Task<ToolResult> RunToolAsync(string exe, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = Process.Start(psi)!;
        string output = await p.StandardOutput.ReadToEndAsync();
        string error = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        return new ToolResult(string.IsNullOrEmpty(error), output, error);
    }

    private static List<string> ParseUndefinedSymbols(string output)
    {
        return output.Split('\n')
            .Where(line => line.Contains(" U "))
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last())
            .ToList();
    }

    private static string FormatBinary(byte[] binData)
    {
        StringBuilder sb = new();
        for (int i = 0; i < binData.Length; i += 8)
        {
            var chunk = binData.Skip(i).Take(8).ToArray();
            string part1 = string.Concat(chunk.Take(4).Select(b => $"{b:X2}"));
            string part2 = string.Concat(chunk.Skip(4).Take(4).Select(b => $"{b:X2}"));
            sb.AppendLine($"{part1} {part2}");
        }
        return sb.ToString().TrimEnd();
    }

    public static string RewriteBranchToRelative(string instruction, uint currentAddress)
    {
        var match = Regex.Match(instruction, @"^\s*(b[l]?)\s+0x([0-9a-fA-F]+)");
        if (match.Success)
        {
            string opcode = match.Groups[1].Value;
            Debug.WriteLine($"opcode: {match.Groups[1].Value}");
            uint absoluteTarget = Convert.ToUInt32(match.Groups[2].Value, 16);
            Debug.WriteLine($"absoluteTarget: {match.Groups[2].Value}");

            // PowerPCは現在の命令の「次の命令」が基準なので +4
            int offset = (int)absoluteTarget - (int)currentAddress;

            // 26bit分の範囲チェック（符号付き）
            if (offset < -(1 << 25) || offset >= (1 << 25))
            {
                Debug.WriteLine("Branch instruction out of range");
                return "";
            }

            Debug.WriteLine($"Instruction:{opcode} 0x{offset}");
            return $"{opcode} 0x{offset}";
        }
        Debug.WriteLine("Branch instruction not found");
        return instruction;
    }
}
