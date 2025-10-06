using System;
using System.IO;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace PxViewer.ViewModels
{
    public class InputDialogViewModel : BindableBase, IDialogAware
    {
        private string inputText;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public DelegateCommand CloseCommand => new (() =>
        {
            RequestClose?.Invoke(new DialogResult());
        });

        public DelegateCommand ConfirmCommand => new (() =>
        {
            // 入力チェック（空・不正文字）
            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                RequestClose?.Invoke(new DialogResult());
                return;
            }

            var folderName = (InputText ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(folderName) || folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                RequestClose?.Invoke(new DialogResult());
                return;
            }

            try
            {
                var newDir = Path.Combine(CurrentPath, folderName);
                Directory.CreateDirectory(newDir);
            }
            catch
            {
                // 必要に応じてログ出力やユーザー通知を行う
            }

            RequestClose?.Invoke(new DialogResult());
        });

        public string InputText { get => inputText; set => SetProperty(ref inputText, value); }

        public string CurrentPath { get; set; }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.ContainsKey("CurrentPath"))
            {
                CurrentPath = parameters.GetValue<string>("CurrentPath");
            }
        }

    }
}