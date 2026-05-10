using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>ATR (Angle of Trunk Rotation) and HS (Hump Score) logic.</summary>
public sealed class PGLogicAtr : PGLogicBase
{
    public PGLogicAtr()
    {
        Register(AwwsGroup.Healthy,                           p => Atr(p) <= 2 && Hs(p) < 4);
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs,  p => Atr(p) <= 2 && Hs(p) < 4);
        Register(AwwsGroup.FlatBack,                         p => Atr(p) <= 2 && Hs(p) < 4);
        Register(AwwsGroup.KyphoticBack,                     p => Atr(p) <= 2 && Hs(p) < 4);
        Register(AwwsGroup.LordoticBack,                     p => Atr(p) <= 2 && Hs(p) < 4);
        Register(AwwsGroup.IS_LowRiskGroup,                  p => (Atr(p) >= 3 && Atr(p) <= 4) || (Hs(p) >= 4 && Hs(p) <= 5));
        Register(AwwsGroup.IS_MediumRiskGroup,               p => (Atr(p) >= 5 && Atr(p) <= 6) || (Hs(p) >= 6 && Hs(p) <= 7));
        Register(AwwsGroup.IS_HighRiskGroup,                 p => Atr(p) >= 7 || Hs(p) >= 8);
    }

    private static int Atr(IReadOnlyDictionary<string, object> p) => Get<int>(p, AwwsParams.ATR);
    private static int Hs(IReadOnlyDictionary<string, object> p)  => Get<int>(p, AwwsParams.HS);
}
