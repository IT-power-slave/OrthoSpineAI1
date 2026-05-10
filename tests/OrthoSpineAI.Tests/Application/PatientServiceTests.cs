using NSubstitute;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PatientServiceTests
{
    private readonly IPatientRepository _repo = Substitute.For<IPatientRepository>();
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _service = new PatientService(_repo);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllPatientsMappedToDtos()
    {
        _repo.GetAllAsync().Returns(new List<Patient>
        {
            MakePatient(1, "Jan", "Kowalski"),
            MakePatient(2, "Anna", "Nowak"),
        });

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Jan", result[0].FirstName);
        Assert.Equal("Nowak", result[1].LastName);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repo.GetAllAsync().Returns(Array.Empty<Patient>());
        var result = await _service.GetAllAsync();
        Assert.Empty(result);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedDto()
    {
        _repo.GetByIdAsync(42).Returns(MakePatient(42, "Piotr", "Wiśniewski"));
        var result = await _service.GetByIdAsync(42);
        Assert.NotNull(result);
        Assert.Equal(42, result.PatientId);
        Assert.Equal("Piotr", result.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _repo.GetByIdAsync(99).Returns((Patient?)null);
        var result = await _service.GetByIdAsync(99);
        Assert.Null(result);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CallsRepositoryAddAndReturnsDto()
    {
        var dto = new PatientDto(0, "Marek", "Zając", "90051512357",
            PatientSex.Male, new DateTime(1990, 5, 15),
            "ul. Testowa 1", "Warszawa", "00-001", 1);

        await _service.CreateAsync(dto);

        await _repo.Received(1).AddAsync(Arg.Is<Patient>(p =>
            p.FirstName == "Marek" &&
            p.LastName == "Zając" &&
            p.PESEL == "90051512357"));
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_CallsRepositoryUpdate()
    {
        var dto = new PatientDto(5, "Ewa", "Maj", "00231012348",
            PatientSex.Female, new DateTime(2000, 3, 10),
            string.Empty, "Kraków", "30-001", 1);

        await _service.UpdateAsync(dto);

        await _repo.Received(1).UpdateAsync(Arg.Is<Patient>(p => p.PatientId == 5));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        await _service.DeleteAsync(7);
        await _repo.Received(1).DeleteAsync(7);
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ByLastName_ReturnsMatchingPatients()
    {
        _repo.GetAllAsync().Returns(new List<Patient>
        {
            MakePatient(1, "Jan", "Kowalski"),
            MakePatient(2, "Anna", "Nowak"),
            MakePatient(3, "Piotr", "Kowalczyk"),
        });

        var result = await _service.SearchAsync("kowal");

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("kowal", p.LastName.ToLowerInvariant()));
    }

    [Fact]
    public async Task SearchAsync_EmptyText_ReturnsAll()
    {
        _repo.GetAllAsync().Returns(new List<Patient>
        {
            MakePatient(1, "Jan", "Kowalski"),
            MakePatient(2, "Anna", "Nowak"),
        });

        var result = await _service.SearchAsync(string.Empty);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        _repo.GetAllAsync().Returns(new List<Patient>
        {
            MakePatient(1, "Jan", "Kowalski"),
        });

        var result = await _service.SearchAsync("xyz");

        Assert.Empty(result);
    }

    // ── FullName computed property ────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_MappedDto_FullNameCombinesFirstAndLastName()
    {
        _repo.GetAllAsync().Returns(new List<Patient> { MakePatient(1, "Jan", "Kowalski") });
        var result = await _service.GetAllAsync();
        Assert.Equal("Jan Kowalski", result[0].FullName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Patient MakePatient(int id, string first, string last) => new()
    {
        PatientId = id,
        FirstName = first,
        LastName = last,
        PESEL = string.Empty,
        Sex = PatientSex.Male,
        BirthDate = new DateTime(1990, 1, 1),
        AddressSt = string.Empty,
        AddressCity = string.Empty,
        ZipCode = string.Empty,
        ClinicId = 1,
    };
}
