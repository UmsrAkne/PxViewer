using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PxViewer.Models;

namespace PxViewer.Utils
{
    public static class PngMetadataReader
    {
        public static string ReadPngMetadata(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            // PNGシグネチャ（8バイト）
            var signature = reader.ReadBytes(8);
            var expectedSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, };

            if (!signature.AsSpan().SequenceEqual(expectedSignature))
            {
                return string.Empty;
            }

            var text = string.Empty;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                try
                {
                    var length = ReadBigEndianInt(reader);
                    var chunkType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    var data = reader.ReadBytes(length);
                    reader.ReadBytes(4); // CRC

                    // ReSharper disable once StringLiteralTypo
                    if (chunkType == "IDAT")
                    {
                        // 画像データに入ったらメタデータ終わりとみなして中断
                        break;
                    }

                    if (chunkType != "tEXt" && chunkType != "iTXt" && chunkType != "zTXt")
                    {
                        continue;
                    }

                    text = Encoding.UTF8.GetString(data);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            return text;
        }

        public static PngGenerationMetadata Parse(string metadataText)
        {
            var meta = new PngGenerationMetadata();

            if (!metadataText.StartsWith("parameters"))
            {
                meta.IsEmpty = true;
                return meta;
            }

            var lines = metadataText.Replace("\r", string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var positiveBuffer = new List<string>();
            var negativeBuffer = new List<string>();
            var positiveBuilder = new StringBuilder();
            var negativeBuilder = new StringBuilder();
            var isNegative = false;

            // Positive, Negative プロンプトを抽出する。
            // それ以外の情報(Steps:)が来たら、終了する。
            foreach (var line in lines)
            {
                if (line.StartsWith("Steps:"))
                {
                    break;
                }

                if (line.StartsWith("Negative prompt:", StringComparison.OrdinalIgnoreCase))
                {
                    isNegative = true;
                    var negLine = line.Substring("Negative prompt:".Length).Trim();
                    if (!string.IsNullOrEmpty(negLine))
                    {
                        negativeBuffer.AddRange(SplitPrompts(negLine));
                    }

                    continue;
                }

                if (!isNegative)
                {
                    var posLine = line;
                    if (line.StartsWith("parameters", StringComparison.OrdinalIgnoreCase))
                    {
                        posLine = line.Substring("parameters".Length).Trim();
                    }

                    positiveBuffer.AddRange(SplitPrompts(posLine));
                    positiveBuilder.AppendLine(posLine);
                }
                else
                {
                    negativeBuffer.AddRange(SplitPrompts(line));
                    negativeBuilder.AppendLine(line);
                }
            }

            meta.PositivePrompts = positiveBuffer;
            meta.NegativePrompts = negativeBuffer;
            meta.RawPositive = positiveBuilder.ToString().TrimEnd().TrimEnd('r', 'n');
            meta.RawNegative = negativeBuilder.ToString().TrimEnd().TrimEnd('r', 'n');

            // プロンプト以外のメタデータを読み取っていく。行頭には必ず Steps: が来る前提で組んである。現状ではそれ以外では動作しない。
            var otherInfoLine = lines.FirstOrDefault(l => l.StartsWith("Steps:", StringComparison.OrdinalIgnoreCase));
            if (otherInfoLine == null)
            {
                return meta;
            }

            var infoTexts = otherInfoLine.Split(',');
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var infoText in infoTexts)
            {
                var parts = infoText.Split(':', 2);
                if (parts.Length == 2)
                {
                    dict.Add(parts[0].Trim(), parts[1].Trim());
                }
            }

            if (dict.TryGetValue("Steps", out var steps))
            {
                meta.Steps = int.Parse(steps);
            }

            if (dict.TryGetValue("Seed", out var seed))
            {
                meta.Seed = int.Parse(seed);
            }

            meta.ModelName = TryGetString("Model");
            meta.VaeName = TryGetString("VAE");
            meta.Sampler = TryGetString("Sampler");
            meta.Version = TryGetString("Version");

            return meta;

            string TryGetString(string key)
            {
                if (dict.TryGetValue(key, out var val))
                {
                    return meta.ModelName = val;
                }

                return string.Empty;
            }
        }

        private static int ReadBigEndianInt(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (bytes.Length < 4)
            {
                throw new EndOfStreamException();
            }

            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

        private static List<string> SplitPrompts(string line)
        {
            return line
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }
    }
}