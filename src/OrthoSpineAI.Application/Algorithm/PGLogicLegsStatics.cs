using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>Lower limb statics assessment logic.</summary>
public sealed class PGLogicLegsStatics : PGLogicBase
{
    public PGLogicLegsStatics()
    {
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs,  p => Get<bool>(p, AwwsParams.LEGSSTAT_DISTURBED));
        Register(AwwsGroup.Healthy,                          p => Get<bool>(p, AwwsParams.LEGSSTAT_CORRECT));
        Register(AwwsGroup.FlatBack,                         _ => true);
        Register(AwwsGroup.KyphoticBack,                     _ => true);
        Register(AwwsGroup.LordoticBack,                     _ => true);
        Register(AwwsGroup.IS_LowRiskGroup,                  _ => true);
        Register(AwwsGroup.IS_MediumRiskGroup,               _ => true);
        Register(AwwsGroup.IS_HighRiskGroup,                 _ => true);
    }
}
