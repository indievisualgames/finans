using System.Collections.Generic;
using UnityEngine;

namespace MiniGameServices
{
public static class MinigameScoreService
{
    private static readonly HashSet<MinigameScoreManager> _instances = new HashSet<MinigameScoreManager>();
    private static readonly Dictionary<string, HashSet<MinigameScoreManager>> _byId = new Dictionary<string, HashSet<MinigameScoreManager>>();

    public static void Register(MinigameScoreManager manager)
    {
        if (manager == null) return;
        _instances.Add(manager);
        var id = manager.serviceId;
        if (!string.IsNullOrEmpty(id))
        {
            if (!_byId.TryGetValue(id, out var set))
            {
                set = new HashSet<MinigameScoreManager>();
                _byId[id] = set;
            }
            set.Add(manager);
        }
    }

    public static void Unregister(MinigameScoreManager manager)
    {
        if (manager == null) return;
        _instances.Remove(manager);
        var id = manager.serviceId;
        if (!string.IsNullOrEmpty(id) && _byId.TryGetValue(id, out var set))
        {
            set.Remove(manager);
            if (set.Count == 0) _byId.Remove(id);
        }
    }

    public static MinigameScoreManager GetClosest(Transform origin)
    {
        if (origin == null)
        {
            foreach (var m in _instances) return m;
            return null;
        }

        MinigameScoreManager best = null;
        float bestSqr = float.MaxValue;
        foreach (var m in _instances)
        {
            if (m == null) continue;
            var t = m.transform;
            float sqr = (t.position - origin.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = m;
            }
        }
        return best;
    }

    public static MinigameScoreManager GetById(string serviceId, Transform origin = null)
    {
        if (string.IsNullOrEmpty(serviceId)) return GetClosest(origin);
        if (!_byId.TryGetValue(serviceId, out var set) || set.Count == 0) return null;
        if (origin == null)
        {
            foreach (var m in set) return m;
            return null;
        }
        MinigameScoreManager best = null;
        float bestSqr = float.MaxValue;
        foreach (var m in set)
        {
            if (m == null) continue;
            float sqr = (m.transform.position - origin.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = m;
            }
        }
        return best;
    }

    public static IReadOnlyCollection<MinigameScoreManager> GetAll() => _instances;
}
}


