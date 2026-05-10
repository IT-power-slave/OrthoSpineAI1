using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>Pass-through anthropometric logics — always true for all groups.</summary>
public sealed class PGLogicPatientAge : PGLogicBase
{
    public PGLogicPatientAge()
    {
        foreach (AwwsGroup g in Enum.GetValues<AwwsGroup>())
            Register(g, _ => true);
    }
}

public sealed class PGLogicPatientHeight : PGLogicBase
{
    public PGLogicPatientHeight()
    {
        foreach (AwwsGroup g in Enum.GetValues<AwwsGroup>())
            Register(g, _ => true);
    }
}

public sealed class PGLogicPatientWeight : PGLogicBase
{
    public PGLogicPatientWeight()
    {
        foreach (AwwsGroup g in Enum.GetValues<AwwsGroup>())
            Register(g, _ => true);
    }
}
