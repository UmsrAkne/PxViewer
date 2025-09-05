using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PxViewer.Services
{
    public static class ThumbnailHelper
    {
        public static string GenerateThumbnailHash(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            // 絶対パス + 最終更新時刻のTicksを連結
            var key = $"{fileInfo.FullName}|{fileInfo.LastWriteTimeUtc.Ticks}";

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));

            // Base32っぽく（URL/ファイル名に優しい）
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // hex: 2桁
            }

            return sb.ToString()[..32];
        }
    }
}