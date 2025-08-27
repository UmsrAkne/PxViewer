namespace PxViewer.Models
{
    // フォルダーの識別子のラッパークラス
    public class FolderId
    {
        public FolderId(string imagesDir)
        {
            Value = imagesDir;
        }

        public string Value { get; set; }
    }
}