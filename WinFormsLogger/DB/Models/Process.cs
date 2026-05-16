using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinFormsLogger.DB.Models;

public class Process : INotifyPropertyChanged
{
    private int _id;
    private string? _processName;
    private string? _windowsName;
    private DateTime _processStart;
    private int _duration;
    private bool _isSynced;

    public int Id 
    { 
        get => _id; 
        set => SetField(ref _id, value); 
    }

    public string? ProcessName 
    { 
        get => _processName; 
        set => SetField(ref _processName, value); 
    }

    public string? WindowsName 
    { 
        get => _windowsName; 
        set => SetField(ref _windowsName, value); 
    }

    public DateTime ProcessStart 
    { 
        get => _processStart; 
        set => SetField(ref _processStart, value); 
    }

    public int Duration 
    { 
        get => _duration; 
        set => SetField(ref _duration, value); 
    }

    public bool IsSynced 
    { 
        get => _isSynced; 
        set => SetField(ref _isSynced, value); 
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public override string ToString()
    {
        return $"Id: {Id}, ProcessName: {ProcessName}, WindowsName: {WindowsName}, ProcessStart: {ProcessStart}, Duration: {Duration}, IsSynced: {IsSynced}";
    }
}
