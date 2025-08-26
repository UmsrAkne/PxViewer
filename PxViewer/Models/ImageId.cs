namespace PxViewer.Models
{
    // 画像ファイルの識別子のラッパークラス
    public class ImageId
    {
        public ImageId(string toLowerInvariant)
        {
            Value = toLowerInvariant;
        }

        public string Value { get; set; }
    }
}