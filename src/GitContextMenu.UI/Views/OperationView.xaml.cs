using System.Collections.Specialized;
using System.Windows.Controls;
using GitContextMenu.UI.ViewModels;

namespace GitContextMenu.UI.Views;

public partial class OperationView : UserControl
{
    public OperationView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is OperationViewModel vm)
                vm.Lines.CollectionChanged += ScrollToBottom;
        };
    }

    private void ScrollToBottom(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Scroll.ScrollToBottom();
    }
}
