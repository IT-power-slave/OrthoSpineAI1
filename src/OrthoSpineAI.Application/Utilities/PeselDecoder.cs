using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.Utilities;

/// <summary>
/// Decodes a Polish PESEL number: validates the checksum and extracts
/// date of birth and biological sex.
/// </summary>
public static class PeselDecoder
{
    private static readonly int[] Weights = [1, 3, 7, 9, 1, 3, 7, 9, 1, 3];

    /// <summary>
    /// Attempts to decode a PESEL string.
    /// Returns <c>null</c> when the PESEL is invalid or incomplete.
    /// </summary>
    public static PeselInfo? Decode(string? pesel)
    {
        if (string.IsNullOrWhiteSpace(pesel) || pesel.Length != 11)
            return null;

        if (!pesel.All(char.IsDigit))
            return null;

        // Checksum
        int sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (pesel[i] - '0') * Weights[i];

        int checkDigit = (10 - sum % 10) % 10;
        if (checkDigit != pesel[10] - '0')
            return null;

        // Year encoding (months 21–32 → born 2000–2099; 1–12 → born 1900–1999)
        int yy = (pesel[0] - '0') * 10 + (pesel[1] - '0');
        int mm = (pesel[2] - '0') * 10 + (pesel[3] - '0');
        int dd = (pesel[4] - '0') * 10 + (pesel[5] - '0');

        int year;
        if (mm >= 21 && mm <= 32)
        {
            mm -= 20;
            year = 2000 + yy;
        }
        else if (mm >= 1 && mm <= 12)
        {
            year = 1900 + yy;
        }
        else
        {
            return null;
        }

        DateTime birthDate;
        try
        {
            birthDate = new DateTime(year, mm, dd);
        }
        catch
        {
            return null;
        }

        // Sex: last digit before checksum is odd → male, even → female
        var sex = (pesel[9] - '0') % 2 == 1 ? PatientSex.Male : PatientSex.Female;

        return new PeselInfo(birthDate, sex);
    }
}

public record PeselInfo(DateTime BirthDate, PatientSex Sex);
