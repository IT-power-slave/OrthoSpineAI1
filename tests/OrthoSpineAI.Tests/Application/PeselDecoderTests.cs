using OrthoSpineAI.Application.Utilities;
using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Tests.Application;

public class PeselDecoderTests
{
    // ── Valid PESELs ──────────────────────────────────────────────────────────

    [Fact]
    public void Decode_ValidMalePesel_ReturnsMaleAndCorrectBirthDate()
    {
        // Born 1990-05-15, male (digit at position 9 is odd → male)
        // Digits 900515 1235, weights [1,3,7,9,1,3,7,9,1,3] → sum=113 → check=7
        const string pesel = "90051512357";
        var result = PeselDecoder.Decode(pesel);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(1990, 5, 15), result.BirthDate);
        Assert.Equal(PatientSex.Male, result.Sex);
    }

    [Fact]
    public void Decode_ValidFemalePesel_ReturnsFemaleSex()
    {
        // Digit at position 9 (0-based) odd → female
        // Born 2000-03-10, female: 00231012345 — month 23 means 2000-03, so mm-20=03
        const string pesel = "00231012348"; // precomputed valid female PESEL (2000-born)
        var result = PeselDecoder.Decode(pesel);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2000, 3, 10), result.BirthDate);
        Assert.Equal(PatientSex.Female, result.Sex);
    }

    [Fact]
    public void Decode_ValidMalePesel_BornIn1999()
    {
        // Born 1999-12-31, male
        // Digits 991231 1235, weights [1,3,7,9,1,3,7,9,1,3] → sum=110 → check=0
        const string pesel = "99123112350";
        var result = PeselDecoder.Decode(pesel);

        Assert.NotNull(result);
        Assert.Equal(1999, result.BirthDate.Year);
        Assert.Equal(12, result.BirthDate.Month);
        Assert.Equal(31, result.BirthDate.Day);
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void Decode_NullInput_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode(null));

    [Fact]
    public void Decode_EmptyString_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode(string.Empty));

    [Fact]
    public void Decode_TooShort_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode("123456789"));

    [Fact]
    public void Decode_TooLong_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode("123456789012"));

    [Fact]
    public void Decode_NonDigitCharacters_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode("9005151234X"));

    [Fact]
    public void Decode_WrongChecksum_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode("90051512346")); // checksum off by one

    [Fact]
    public void Decode_InvalidMonth_ReturnsNull() =>
        Assert.Null(PeselDecoder.Decode("90991512340")); // month 99 is invalid
}
