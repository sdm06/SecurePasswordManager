using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SecureVault.Linux.ViewModels;
using System;

namespace SecureVault.Linux.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void CopyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Get the ViewModel
        if (DataContext is MainWindowViewModel vm)
        {
            // Check if we are ready to copy
            if (vm.SelectedEntry == null || vm.CurrentKey == null) return;

            try 
            {
                // 1. Decrypt the password
                string password = vm.GetDecryptedPasswordForSelection();
                
                // 2. Copy to System Clipboard
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(password);
                    vm.StatusMessage = "COPIED TO CLIPBOARD";
                }
            }
            catch (Exception)
            {
                vm.StatusMessage = "FAILED TO DECRYPT";
            }
        }
    }
}