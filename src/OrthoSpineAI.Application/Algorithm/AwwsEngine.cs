using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>
/// AWWS/PiLS diagnostic engine.
/// Aggregates all PGLogic* modules and applies the PiLS decision tree.
/// </summary>
public sealed class AwwsEngine
{
    private static readonly IReadOnlyList<string> PilsConclusions = new[]
    {
        "Brak wnioskowania — dane niewystarczające.",
        "Dziecko nie wymaga leczenia.",
        "Kwalifikacja do profilaktyki czynnej — gimnastyka korekcyjna lub seria 10 zabiegów rehabilitacyjnych oraz zwiększenie ukierunkowanej aktywności fizycznej.",
        "Kwalifikacja do rehabilitacji wg indywidualnego programu oraz zwiększenie ukierunkowanej aktywności fizycznej.",
        "Kwalifikacja do RTG kręgosłupa P-A i bocznego oraz leczenia w poradni ortopedycznej i/lub rehabilitacyjnej wg zaleceń SOSORT/SRS."
    };

    private static readonly IReadOnlyDictionary<int, string> PilsControlRecommendations = new Dictionary<int, string>
    {
        [1] = "Wiek 3–9 lat: kontrola za 12 miesięcy.",
        [2] = "Wiek 10–12 lat: kontrola za 6 miesięcy.",
        [3] = "Wiek 3–9 lat: kontrola za 6 miesięcy.",
        [4] = "Wiek 10–12 lat: kontrola za 3 miesiące.",
        [5] = "Kontrola za 3 miesiące.",
        [6] = "Kontrola co 2 miesiące lub wg zaleceń lekarza."
    };

    private readonly IReadOnlyList<IPGLogic> _logics = new IPGLogic[]
    {
        new PGLogicAtr(),
        new PGLogicBeightonScaleNumeric(),
        new PGLogicFLLD(),
        new PGLogicLegsStatics(),
        new PGLogicLLTHK(),
        new PGLogicPT(),
        new PGLogicPatientAge(),
        new PGLogicPatientHeight(),
        new PGLogicPatientWeight()
    };

    /// <summary>
    /// Runs all PGLogic modules and applies the PiLS decision tree.
    /// </summary>
    public AwwsResultDto Evaluate(IReadOnlyDictionary<string, object> parameters)
    {
        // Aggregate: a group is satisfied when ALL logics return true for it
        var aggregated = new Dictionary<AwwsGroup, bool>();
        foreach (AwwsGroup group in Enum.GetValues<AwwsGroup>())
        {
            aggregated[group] = _logics.All(l => l.Perform(parameters)[group]);
        }

        int atrMax = parameters.TryGetValue(AwwsParams.ATR, out var atrObj) && atrObj is int atr ? atr : 0;
        int beighton = parameters.TryGetValue(AwwsParams.BEIGHTON, out var bObj) && bObj is int b ? b : 0;
        bool flldPositive = parameters.TryGetValue(AwwsParams.FLLD_POSITIVE, out var fp) && fp is bool fpv && fpv;
        bool flldNegative = parameters.TryGetValue(AwwsParams.FLLD_NEGATIVE, out var fn) && fn is bool fnv && fnv;
        int kp = parameters.TryGetValue(AwwsParams.THK, out var kpObj) && kpObj is int k ? k : 0;
        int ll = parameters.TryGetValue(AwwsParams.LL, out var llObj) && llObj is int lv ? lv : 0;
        int age = parameters.TryGetValue(AwwsParams.AGE, out var ageObj) && ageObj is int av ? av : 0;

        var (variant, controlKey) = DeterminePilsVariant(atrMax, beighton, flldPositive, flldNegative, kp, ll, age);

        var groupResults = aggregated.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

        return new AwwsResultDto(
            MedTestId: 0,
            ExaminationDate: DateTime.UtcNow,
            SurveyName: string.Empty,
            PilsVariant: variant,
            PilsControlKey: controlKey,
            Conclusion: PilsConclusions[Math.Clamp(variant, 0, PilsConclusions.Count - 1)],
            ControlRecommendation: PilsControlRecommendations.TryGetValue(controlKey, out var rec) ? rec : string.Empty,
            GroupResults: groupResults);
    }

    private static (int variant, int controlKey) DeterminePilsVariant(
        int atrMax, int beighton, bool flldPos, bool flldNeg, int kp, int ll, int age)
    {
        // Priority 1: ATR > 7°
        if (atrMax > 7)
            return (4, 6);

        // Priority 2: 5° < ATR ≤ 7° AND Beighton ≥ 6
        if (atrMax is > 5 and <= 7 && beighton >= 6)
            return (3, 5);

        // Priority 3: 3° < ATR ≤ 5° AND Beighton ≤ 5 AND FLLD+ AND KP < 19 AND LL < 19
        if (atrMax is > 3 and <= 5 && beighton <= 5 && flldPos && kp < 19 && ll < 19)
            return age < 10 ? (2, 3) : (2, 4);

        // Priority 4: 3° < ATR ≤ 5° AND Beighton ≤ 5 AND FLLD− AND KP > 20 AND LL > 20
        if (atrMax is > 3 and <= 5 && beighton <= 5 && flldNeg && kp > 20 && ll > 20)
            return age < 10 ? (1, 1) : (1, 2);

        return (0, 0);
    }
}
