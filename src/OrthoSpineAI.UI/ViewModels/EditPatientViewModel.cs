using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Application.Utilities;
using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.UI.ViewModels;

public partial class EditPatientViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly PatientDto _original;

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName  = string.Empty;
    [ObservableProperty] private string _pesel     = string.Empty;
    [ObservableProperty] private DateTime _birthDate;
    [ObservableProperty] private PatientSex _sex;
    [ObservableProperty] private string _addressSt   = string.Empty;
    [ObservableProperty] private string _addressCity = string.Empty;
    [ObservableProperty] private string _zipCode     = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _peselError = string.Empty;

    public PatientSex[] SexOptions { get; } = [PatientSex.Male, PatientSex.Female];

    public event Action<PatientDto>? Saved;
    public event Action? Cancelled;

    public EditPatientViewModel(IPatientService patientService, PatientDto patient)
    {
        _patientService = patientService;
        _original = patient;

        FirstName   = patient.FirstName;
        LastName    = patient.LastName;
        Pesel       = patient.PESEL;
        BirthDate   = patient.BirthDate;
        Sex         = patient.Sex;
        AddressSt   = patient.AddressSt;
        AddressCity = patient.AddressCity;
        ZipCode     = patient.ZipCode;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var dto = new PatientDto(_original.PatientId, FirstName, LastName, Pesel,
                Sex, BirthDate, AddressSt, AddressCity, ZipCode, _original.ClinicId);
            await _patientService.UpdateAsync(dto);
            Saved?.Invoke(dto);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        string.IsNullOrEmpty(PeselError) &&
        !IsBusy;

    partial void OnFirstNameChanged(string v)  => SaveCommand.NotifyCanExecuteChanged();
    partial void OnLastNameChanged(string v)   => SaveCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool v)       => SaveCommand.NotifyCanExecuteChanged();

    partial void OnPeselChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            PeselError = string.Empty;
        }
        else
        {
            var info = PeselDecoder.Decode(value);
            if (info is not null)
            {
                PeselError = string.Empty;
                BirthDate = info.BirthDate;
                Sex = info.Sex;
            }
            else
            {
                PeselError = value.Length < 11 ? string.Empty : "Nieprawidłowy numer PESEL.";
            }
        }
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
