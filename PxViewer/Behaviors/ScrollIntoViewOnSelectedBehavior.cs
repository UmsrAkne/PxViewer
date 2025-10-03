using System;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace PxViewer.Behaviors;

public class ScrollIntoViewOnSelectedBehavior : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
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
        // 上下キーの入力を無効化
        if (e.Key is Key.Up or Key.Down)
        {
            e.Handled = true;

            var listBox = AssociatedObject;
            var index = listBox.SelectedIndex + (e.Key == Key.Up ? -1 : 1);
            listBox.SelectedIndex = Math.Min(Math.Max(0, index), listBox.Items.Count - 1);
        }
    }
}