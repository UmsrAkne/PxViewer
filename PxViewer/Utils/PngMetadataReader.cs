using System;
using System.IO;
using System.Text;

namespace PxViewer.Utils
{
    public class PngMetadataReader
    {
        public static string ReadPngMetadata(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            // PNGシグネチャ（8バイト）
            var signature = reader.ReadBytes(8);
            var expectedSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, };

            if (!signature.AsSpan().SequenceEqual(expectedSignature))
            {
                Console.WriteLine("PNG signature mismatch.");
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
                    Console.WriteLine($"[{chunkType}] {text}");
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            return text;
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
    }
}