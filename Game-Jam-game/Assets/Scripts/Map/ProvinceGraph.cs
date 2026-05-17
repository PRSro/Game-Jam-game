using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ProvinceGraph
{
    static Dictionary<string, List<string>> cachedAttackNeighbours;

    public static List<string> GetAttackNeighbours(string cityName, int maxNeighbours = 2)
    {
        EnsureCache(maxNeighbours);
        return cachedAttackNeighbours.TryGetValue(cityName, out List<string> neighbours)
            ? new List<string>(neighbours)
            : new List<string>();
    }

    public static bool CanAttack(string from, string to)
    {
        return GetAttackNeighbours(from).Contains(to);
    }

    public static void ClearCache()
    {
        cachedAttackNeighbours = null;
    }

    static void EnsureCache(int maxNeighbours)
    {
        if (cachedAttackNeighbours != null) return;

        cachedAttackNeighbours = new Dictionary<string, List<string>>();
        List<TerritoryData> territories = TerritoryDatabase.GetAllTerritories();
        Dictionary<string, TerritoryData> byName = territories.ToDictionary(t => t.name, t => t);

        foreach (TerritoryData city in territories)
        {
            List<string> neighbours = TerritoryGraph.GetAdjacentTerritories(city.name)
                .Where(byName.ContainsKey)
                .OrderBy(name => Vector2.SqrMagnitude(byName[name].mapPosition - city.mapPosition))
                .Take(Mathf.Max(0, maxNeighbours))
                .ToList();

            cachedAttackNeighbours[city.name] = neighbours;
        }
    }
}
