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
        private string errorMessage;

        public event Action<IDialogResult> RequestClose;

        public string Title => string.Empty;

        public DelegateCommand CloseCommand => new (() =>
        {
            ErrorMessage = string.Empty;
            RequestClose?.Invoke(new DialogResult());
        });

        public DelegateCommand ConfirmCommand => new (() =>
        {
            // 入力チェック（空・不正文字）
            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                ErrorMessage = $"({CurrentPath}) is an invalid folder path. Please enter a valid folder path.";
                return;
            }

            var folderName = (InputText ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(folderName) || folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ErrorMessage = $"({folderName}) is an invalid folder name. Please enter a valid folder name.";
                return;
            }

            try
            {
                var newDir = Path.Combine(CurrentPath, folderName);
                Directory.CreateDirectory(newDir);
            }
            catch
            {
                ErrorMessage = "The folder could not be created due to an unexpected error.";
                return;
            }

            ErrorMessage = string.Empty;
            RequestClose?.Invoke(new DialogResult());
        });

        public string InputText { get => inputText; set => SetProperty(ref inputText, value); }

        public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

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