using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using PxViewer.Models;
using PxViewer.ViewModels;

namespace PxViewer.Behaviors
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class KeyInputBehavior : Behavior<Window>, IDisposable
    {
        private bool waitingForSecondKey;
        private CancellationTokenSource cts;
        private IMainWindowVm vm;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            vm = AssociatedObject.DataContext as IMainWindowVm;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }

        protected virtual void Dispose(bool disposing)
        {
            cts.Dispose();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (!waitingForSecondKey)
                {
                    if (e.Key == Key.C)
                    {
                        // Step 1: Ctrl + C 押されたので待機開始
                        waitingForSecondKey = true;
                        StartSecondKeyWaitAsync();
                        e.Handled = true;
                    }
                }
                else
                {
                    // Step 2: 待機中に Ctrl + S 押されたか判定
                    if (e.Key == Key.S)
                    {
                        CopyToClipboard(nameof(PngGenerationMetadata.Seed));
                        CancelWait();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.P)
                    {
                        CopyToClipboard(nameof(PngGenerationMetadata.RawPositive));
                        CancelWait();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.N)
                    {
                        CopyToClipboard(nameof(PngGenerationMetadata.RawNegative));
                        CancelWait();
                        e.Handled = true;
                    }
                    else
                    {
                        // 想定外のキー → キャンセル
                        CancelWait();
                    }
                }
            }
            else if (waitingForSecondKey)
            {
                // Ctrl 離された → キャンセル
                CancelWait();
            }
        }

        private void CopyToClipboard(string targetPropertyName)
        {
            if (vm.CurrentTab != null)
            {
                switch (targetPropertyName)
                {
                    case nameof(PngGenerationMetadata.Seed):
                        CopyToClipboardSafe(vm.CurrentTab.SelectedItemMeta.Seed.ToString());
                        break;
                    case nameof(PngGenerationMetadata.RawPositive):
                        CopyToClipboardSafe(vm.CurrentTab.SelectedItemMeta.RawPositive);
                        break;
                    case nameof(PngGenerationMetadata.RawNegative):
                        CopyToClipboardSafe(vm.CurrentTab.SelectedItemMeta.RawNegative);
                        break;
                }
            }

            return;

            // 制御文字等の不正な文字が混じっているため、クリップボードへの転送に失敗するケースがある。
            // 通常の文字以外を取り除くためのメソッド
            void CopyToClipboardSafe(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                // コントロール文字を除去（改行・タブ以外）
                var clean = new string(text.Where(c =>
                    !char.IsControl(c) || c == '\r' || c == '\n' || c == '\t').ToArray());

                // 末尾改行を削る
                clean = clean.TrimEnd('\r', '\n');

                try
                {
                    Clipboard.SetText(clean);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Clipboard error: {ex.Message}");
                }
            }
        }

        // ReSharper disable once AsyncVoidMethod
        // 待機する必要も、例外を補足する必要もないため void とする。
        private async void StartSecondKeyWaitAsync()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                // 指定秒数待機する。待機時間内に次の入力がなければキャンセル。
                await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
                CancelWait();
            }
            catch (TaskCanceledException)
            {
                // キャンセルされた場合は何もしない
            }
        }

        private void CancelWait()
        {
            cts?.Cancel();
            waitingForSecondKey = false;
        }
    }
}