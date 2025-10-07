using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

namespace PxViewer.Behaviors;

public class ListBoxSelectionNavigationBehavior : Behavior<ListBox>
{
    private ScrollViewer scrollViewer;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.SelectionChanged += OnSelectionChanged;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    }

    private static T FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root == null)
        {
            return null;
        }

        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current is T t)
            {
                return t;
            }

            var count = VisualTreeHelper.GetChildrenCount(current);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                queue.Enqueue(child);
            }
        }

        return null;
    }

    private static ListBoxItem GetFirstVisibleItem(ListBox listBox)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(listBox);
        if (scrollViewer == null)
        {
            return null;
        }

        for (var i = 0; i < listBox.Items.Count; i++)
        {
            var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            if (item == null)
            {
                continue;
            }

            var transform = item.TransformToAncestor(scrollViewer);
            var position = transform.Transform(new Point(0, 0));

            // item の上端が ScrollViewer の表示範囲に入っていれば表示中と判断
            if (position.Y + item.ActualHeight >= 0)
            {
                return item;
            }
        }

        return null;
    }

    private static ListBoxItem GetLastVisibleItem(ListBox listBox)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(listBox);
        if (scrollViewer == null)
        {
            return null;
        }

        ListBoxItem lastVisible = null;

        for (var i = 0; i < listBox.Items.Count; i++)
        {
            var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            if (item == null)
            {
                continue;
            }

            var transform = item.TransformToAncestor(scrollViewer);
            var position = transform.Transform(new Point(0, 0));

            // 下端が表示領域に入ってれば「見えてる」と判定
            if (position.Y < scrollViewer.ViewportHeight)
            {
                lastVisible = item;
            }
            else
            {
                break; // これ以上下はもう見えてない
            }
        }

        return lastVisible;
    }

    private static T FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed)
            {
                return typed;
            }

            var result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        scrollViewer ??= FindDescendant<ScrollViewer>(AssociatedObject);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = AssociatedObject;
        var item = listBox?.SelectedItem;
        if (item != null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var listBox = AssociatedObject;

        // D: PageDown、U: PageUp
        if (e.Key is Key.D or Key.U)
        {
            e.Handled = true;

            var sv = scrollViewer ??= FindDescendant<ScrollViewer>(listBox);
            if (sv != null)
            {
                if (e.Key == Key.D)
                {
                    sv.PageDown();
                }
                else
                {
                    sv.PageUp();
                }

                listBox.Dispatcher.BeginInvoke(
                () =>
                {
                    var targetItem = e.Key == Key.D
                        ? GetLastVisibleItem(listBox)
                        : GetFirstVisibleItem(listBox);

                    if (targetItem != null)
                    {
                        listBox.SelectedItem = listBox.ItemContainerGenerator.ItemFromContainer(targetItem);
                    }
                }, DispatcherPriority.Background); // レイアウト反映後に実行
            }

            return;
        }

        // 上下キーの入力を無効化して独自に選択移動
        if (e.Key is Key.Up or Key.Down)
        {
            e.Handled = true;

            var listBox = AssociatedObject;
            var index = listBox.SelectedIndex + (e.Key == Key.Up ? -1 : 1);
            listBox.SelectedIndex = Math.Min(Math.Max(0, index), listBox.Items.Count - 1);
        }

        if (e.Key is Key.G)
        {
            var listBox = AssociatedObject;
            if (listBox.Items.Count == 0)
            {
                return;
            }

            listBox.SelectedIndex = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
                ? listBox.Items.Count - 1
                : 0;
        }
    }
}