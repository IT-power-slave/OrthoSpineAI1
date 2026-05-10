using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Interfaces;

public interface IPGLogic
{
    /// <summary>Evaluates all group conditions. Returns true per group if condition is satisfied.</summary>
    IReadOnlyDictionary<AwwsGroup, bool> Perform(IReadOnlyDictionary<string, object> parameters);
}
