using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Algorithm;

/// <summary>
/// Base class for all AWWS logic modules.
/// Iterates all AwwsGroup values and calls the registered condition per group.
/// </summary>
public abstract class PGLogicBase : IPGLogic
{
    private readonly Dictionary<AwwsGroup, Func<IReadOnlyDictionary<string, object>, bool>> _conditions = new();

    protected void Register(AwwsGroup group, Func<IReadOnlyDictionary<string, object>, bool> condition)
    {
        _conditions[group] = condition;
    }

    public IReadOnlyDictionary<AwwsGroup, bool> Perform(IReadOnlyDictionary<string, object> parameters)
    {
        var result = new Dictionary<AwwsGroup, bool>();
        foreach (AwwsGroup group in Enum.GetValues<AwwsGroup>())
        {
            result[group] = _conditions.TryGetValue(group, out var cond) && cond(parameters);
        }
        return result;
    }

    protected static T Get<T>(IReadOnlyDictionary<string, object> p, string key, T fallback = default!) =>
        p.TryGetValue(key, out var val) && val is T typed ? typed : fallback;
}
