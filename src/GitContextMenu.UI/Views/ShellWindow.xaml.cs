using System.Windows;
using GitContextMenu.UI.ViewModels;

namespace GitContextMenu.UI.Views;

public partial class ShellWindow : Window
{
    public ShellWindow(BaseViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Title = $"Git — {viewModel.Title}";
    }
}
