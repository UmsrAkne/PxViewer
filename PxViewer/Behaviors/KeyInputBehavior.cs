using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
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
                        // ComboCommand?.Execute(null);
                        CopySeed();
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

        private void CopySeed()
        {
            if (vm.CurrentTab != null)
            {
                Clipboard.SetText(vm.CurrentTab.SelectedItemMeta.Seed.ToString());
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