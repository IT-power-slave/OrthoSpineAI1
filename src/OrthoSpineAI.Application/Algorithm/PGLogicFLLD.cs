using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>FLLD — Derbolowski functional leg length discrepancy test.</summary>
public sealed class PGLogicFLLD : PGLogicBase
{
    public PGLogicFLLD()
    {
        Register(AwwsGroup.StaticsDisordersOfTheLowerLimbs,  p => Get<bool>(p, AwwsParams.FLLD_POSITIVE));
        Register(AwwsGroup.IS_LowRiskGroup,                  p => Get<bool>(p, AwwsParams.FLLD_POSITIVE));
        Register(AwwsGroup.IS_MediumRiskGroup,               p => Get<bool>(p, AwwsParams.FLLD_POSITIVE));
        Register(AwwsGroup.Healthy,                          p => Get<bool>(p, AwwsParams.FLLD_NEGATIVE));
        Register(AwwsGroup.FlatBack,                         _ => true);
        Register(AwwsGroup.KyphoticBack,                     _ => true);
        Register(AwwsGroup.LordoticBack,                     _ => true);
        Register(AwwsGroup.IS_HighRiskGroup,                 _ => true);
    }
}
