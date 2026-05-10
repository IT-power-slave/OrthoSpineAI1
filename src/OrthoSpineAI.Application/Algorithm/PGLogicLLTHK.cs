using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>Lumbar lordosis (LL) and thoracic kyphosis (THK) sagittal balance logic.</summary>
public sealed class PGLogicLLTHK : PGLogicBase
{
    public PGLogicLLTHK()
    {
        Register(AwwsGroup.Healthy,     p => Age(p) is >= 6 and <= 12 ? Ll(p) is >= 20 and <= 45 : Thk(p) is >= 15 and <= 50);
        Register(AwwsGroup.FlatBack,    p => Age(p) is >= 6 and <= 12 ? Ll(p) <= 15 && Thk(p) <= 15 : Ll(p) < 19 && Thk(p) <= 19);
        Register(AwwsGroup.KyphoticBack,p => Age(p) is >= 6 and <= 12 ? Ll(p) <= 15 && Thk(p) >= 46 : Ll(p) < 19 && Thk(p) > 50);
        Register(AwwsGroup.LordoticBack,p => Age(p) is >= 6 and <= 12 ? Ll(p) >= 46 && Thk(p) < 15 : Ll(p) >= 50 && Thk(p) < 19);
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs, _ => true);
        Register(AwwsGroup.IS_LowRiskGroup,                 _ => true);
        Register(AwwsGroup.IS_MediumRiskGroup,              _ => true);
        Register(AwwsGroup.IS_HighRiskGroup,                _ => true);
    }

    private static int Ll(IReadOnlyDictionary<string, object> p)  => Get<int>(p, AwwsParams.LL);
    private static int Thk(IReadOnlyDictionary<string, object> p) => Get<int>(p, AwwsParams.THK);
    private static int Age(IReadOnlyDictionary<string, object> p) => Get<int>(p, AwwsParams.AGE);
}
