using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace PxViewer.Behaviors;

public class ScrollIntoViewOnSelectedBehavior : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
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
}