using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>Pelvic tilt (PT) angle logic.</summary>
public sealed class PGLogicPT : PGLogicBase
{
    public PGLogicPT()
    {
        Register(AwwsGroup.Healthy,                         p => Pt(p) is >= 10 and <= 29);
        Register(AwwsGroup.FlatBack,                        p => Pt(p) is >= 5  and <= 20);
        Register(AwwsGroup.KyphoticBack,                    p => Pt(p) is >= 10 and <= 30);
        Register(AwwsGroup.LordoticBack,                    p => Pt(p) is >= 20 and <= 40);
        Register(AwwsGroup.IS_LowRiskGroup,                 p => Pt(p) is >= 10 and <= 30);
        Register(AwwsGroup.IS_MediumRiskGroup,              p => Pt(p) is >= 10 and <= 30);
        Register(AwwsGroup.IS_HighRiskGroup,                _ => true);
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs, _ => true);
    }

    private static int Pt(IReadOnlyDictionary<string, object> p) => Get<int>(p, AwwsParams.PT);
}
