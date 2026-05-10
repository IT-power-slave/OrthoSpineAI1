using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>Beighton joint hypermobility scale logic (age-dependent thresholds).</summary>
public sealed class PGLogicBeightonScaleNumeric : PGLogicBase
{
    public PGLogicBeightonScaleNumeric()
    {
        Register(AwwsGroup.Healthy,                          p => (Age(p) is >= 5 and <= 15 && Beighton(p) >= 4) || (Age(p) is >= 16 and <= 18 && Beighton(p) >= 3));
        Register(AwwsGroup.IS_LowRiskGroup,                  p => (Age(p) is >= 5 and <= 15 && Beighton(p) >= 5) || (Age(p) is >= 16 and <= 18 && Beighton(p) >= 4));
        Register(AwwsGroup.IS_MediumRiskGroup,               p => (Age(p) is >= 5 and <= 15 && Beighton(p) >= 5) || (Age(p) is >= 16 and <= 18 && Beighton(p) >= 4));
        Register(AwwsGroup.IS_HighRiskGroup,                 _ => true);
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs,  _ => true);
        Register(AwwsGroup.FlatBack,                         _ => true);
        Register(AwwsGroup.KyphoticBack,                     _ => true);
        Register(AwwsGroup.LordoticBack,                     _ => true);
    }

    private static int Beighton(IReadOnlyDictionary<string, object> p) => Get<int>(p, AwwsParams.BEIGHTON);
    private static int Age(IReadOnlyDictionary<string, object> p)      => Get<int>(p, AwwsParams.AGE);
}
