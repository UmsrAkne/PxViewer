using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace PxViewer.Behaviors
{
    // ListBox からファイルパスを FileDrop でドラッグ開始する Behavior
    public sealed class ListBoxFileDragBehavior : Behavior<ListBox>
    {
        private Point dragStartPoint;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            base.OnDetaching();
        }

        // PxViewer.ViewModels.ImageItemViewModel を優先し、無ければリフレクションで Entry.FullPath / FullPath を探す
        private static string GetFilePathFromItem(object item)
        {
            // 型が参照できる場合は強く型付け（推奨）
            if (item is ViewModels.ImageItemViewModel vm)
            {
                return vm.Entry?.FullPath;
            }

            return null;
        }

        private static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(pos.Y - dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var listBox = AssociatedObject;
            if (listBox == null)
            {
                return;
            }

            // 複数選択があればそれを、なければカーソル直下のアイテムを対象にする
            var items = new List<object>();
            if (listBox.SelectedItems is { Count: > 0, })
            {
                items.AddRange(listBox.SelectedItems.Cast<object>());
            }
            else
            {
                var itemContainer = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
                if (itemContainer?.DataContext != null)
                {
                    items.Add(itemContainer.DataContext);
                }
            }

            if (items.Count == 0)
            {
                return;
            }

            // アイテムからファイルパスを取り出す
            var filePaths = items
                .Select(GetFilePathFromItem)
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p!))
                .Distinct()
                .ToArray();

            if (filePaths.Length == 0)
            {
                return;
            }

            // FileDrop でドラッグ開始（コピー扱い）
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, filePaths);

            // 一部アプリ向けにテキストも付けておく（任意）
            data.SetText(string.Join(Environment.NewLine, filePaths));

            DragDrop.DoDragDrop(listBox, data, DragDropEffects.Copy);
        }
    }
}