using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using SecureVault.Linux.Models;
using SecureVault.Linux.Services;
using SecureVault.Linux.Helpers; // <--- ENSURE THIS IS HERE

namespace SecureVault.Linux.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly PasswordService _passwordService;

    // -- STATE --
    private string _masterPassword = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoggedIn = false;
    private bool _isFirstRun = false;
    
    private string _revealedPassword = "****************";

    // -- INPUTS --
    private string _newServiceName = "";
    private string _newUsername = "";
    private string _newPassword = "";
    private PasswordEntry? _selectedEntry;

    public byte[]? CurrentKey { get; private set; }
    public ObservableCollection<PasswordEntry> PasswordList { get; } = new();

    public MainWindowViewModel()
    {
        _authService = new AuthService();
        _passwordService = new PasswordService();

        IsFirstRun = !_authService.IsVaultInitialized();
        UpdateStatus();

        // Initialize Commands
        ExecuteAuthCommand = ReactiveCommand.Create(OnAuthExecute);
        AddPasswordCommand = ReactiveCommand.Create(OnAddPassword);
        TogglePasswordCommand = ReactiveCommand.Create(OnTogglePassword);
        
        // NEW COMMAND FOR GENERATOR
        GeneratePasswordCommand = ReactiveCommand.Create(OnGeneratePassword);
        DeletePasswordCommand = ReactiveCommand.Create(OnDeletePassword);
    }
    
    // -- PROPERTIES --
    public ReactiveCommand<Unit, Unit> DeletePasswordCommand { get; }
    public string MasterPassword
    {
        get => _masterPassword;
        set => this.RaiseAndSetIfChanged(ref _masterPassword, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    public bool IsFirstRun
    {
        get => _isFirstRun;
        set => this.RaiseAndSetIfChanged(ref _isFirstRun, value);
    }

    public string ButtonText => IsFirstRun ? "CREATE VAULT" : "UNLOCK VAULT";

    public string RevealedPassword
    {
        get => _revealedPassword;
        set => this.RaiseAndSetIfChanged(ref _revealedPassword, value);
    }

    public PasswordEntry? SelectedEntry
    {
        get => _selectedEntry;
        set 
        {
            this.RaiseAndSetIfChanged(ref _selectedEntry, value);
            RevealedPassword = "****************";
        }
    }

    public string NewServiceName { get => _newServiceName; set => this.RaiseAndSetIfChanged(ref _newServiceName, value); }
    public string NewUsername { get => _newUsername; set => this.RaiseAndSetIfChanged(ref _newUsername, value); }
    public string NewPassword { get => _newPassword; set => this.RaiseAndSetIfChanged(ref _newPassword, value); }

    // -- COMMANDS --
    public ReactiveCommand<Unit, Unit> ExecuteAuthCommand { get; }
    public ReactiveCommand<Unit, Unit> AddPasswordCommand { get; }
    public ReactiveCommand<Unit, Unit> TogglePasswordCommand { get; }
    public ReactiveCommand<Unit, Unit> GeneratePasswordCommand { get; } // <--- NEW

    // -- HELPER FOR VIEW --
    public string GetDecryptedPasswordForSelection()
    {
        if (SelectedEntry == null || CurrentKey == null) return string.Empty;
        try { return _passwordService.DecryptPassword(SelectedEntry.Id, CurrentKey); }
        catch { return string.Empty; }
    }

    // -- METHODS --
    private void OnDeletePassword()
    {
        if (SelectedEntry == null)
        {
            StatusMessage = "SELECT AN ENTRY TO DELETE";
            return;
        }

        try
        {
            // 1. Delete from Database
            _passwordService.DeletePassword(SelectedEntry.Id);

            // 2. Remove from UI List immediately
            PasswordList.Remove(SelectedEntry);
        
            // 3. Reset Selection
            SelectedEntry = null;
            StatusMessage = "ENTRY DELETED PERMANENTLY";
        }
        catch (Exception ex)
        {
            StatusMessage = $"DELETE FAILED: {ex.Message}";
        }
    }
    private void OnGeneratePassword()
    {
        // Generates a 20-character strong password
        NewPassword = PasswordGenerator.Generate(20, true, true, true);
    }

    private void OnTogglePassword()
    {
        if (SelectedEntry == null || CurrentKey == null) return;

        if (RevealedPassword == "****************")
        {
            try
            {
                RevealedPassword = _passwordService.DecryptPassword(SelectedEntry.Id, CurrentKey);
                StatusMessage = "PASSWORD REVEALED";
            }
            catch
            {
                StatusMessage = "DECRYPTION ERROR";
            }
        }
        else
        {
            RevealedPassword = "****************";
            StatusMessage = "PASSWORD HIDDEN";
        }
    }

    private void OnAuthExecute()
    {
        if (string.IsNullOrWhiteSpace(MasterPassword)) { StatusMessage = "PASSWORD REQUIRED"; return; }
        try
        {
            if (IsFirstRun)
            {
                _authService.Register(MasterPassword);
                StatusMessage = "VAULT CREATED. LOGIN.";
                IsFirstRun = false;
                this.RaisePropertyChanged(nameof(ButtonText));
                MasterPassword = "";
            }
            else
            {
                var key = _authService.Login(MasterPassword);
                if (key != null)
                {
                    CurrentKey = key;
                    IsLoggedIn = true;
                    StatusMessage = "ACCESS GRANTED";
                    LoadPasswords();
                }
                else StatusMessage = "ACCESS DENIED";
            }
        }
        catch (Exception ex) { StatusMessage = $"ERROR: {ex.Message}"; }
    }

    private void LoadPasswords()
    {
        try
        {
            PasswordList.Clear();
            var list = _passwordService.GetAll();
            foreach (var item in list) PasswordList.Add(item);
        }
        catch (Exception ex) { StatusMessage = $"DB ERROR: {ex.Message}"; }
    }

    private void OnAddPassword()
    {
        if (CurrentKey == null) return;
        if (string.IsNullOrWhiteSpace(NewServiceName) || string.IsNullOrWhiteSpace(NewPassword))
        {
            StatusMessage = "MISSING INFO"; return;
        }
        try
        {
            _passwordService.AddPassword(NewServiceName, NewUsername, NewPassword, CurrentKey);
            NewServiceName = ""; NewUsername = ""; NewPassword = "";
            StatusMessage = "SAVED";
            LoadPasswords();
        }
        catch (Exception ex) { StatusMessage = $"SAVE FAILED: {ex.Message}"; }
    }

    private void UpdateStatus() => StatusMessage = IsFirstRun ? "NO VAULT FOUND" : "LOCKED";
}