using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Application.Utilities;
using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.UI.ViewModels;

public partial class AddPatientViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly int _clinicId;

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _pesel = string.Empty;
    [ObservableProperty] private DateTime _birthDate = DateTime.Today.AddYears(-20);
    [ObservableProperty] private PatientSex _sex = PatientSex.Male;
    [ObservableProperty] private string _addressSt = string.Empty;
    [ObservableProperty] private string _addressCity = string.Empty;
    [ObservableProperty] private string _zipCode = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public PatientSex[] SexOptions { get; } = [PatientSex.Male, PatientSex.Female];

    public event Action<PatientDto>? PatientSaved;
    public event Action? Cancelled;

    public AddPatientViewModel(IPatientService patientService, int clinicId)
    {
        _patientService = patientService;
        _clinicId = clinicId;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var dto = new PatientDto(0, FirstName, LastName, Pesel, Sex,
                BirthDate, AddressSt, AddressCity, ZipCode, _clinicId);
            var saved = await _patientService.CreateAsync(dto);
            PatientSaved?.Invoke(saved);
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
        !IsBusy;

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();

    partial void OnFirstNameChanged(string v) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnLastNameChanged(string v) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool v) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnPeselChanged(string value)
    {
        var info = PeselDecoder.Decode(value);
        if (info is not null)
        {
            BirthDate = info.BirthDate;
            Sex = info.Sex;
        }
    }
}
