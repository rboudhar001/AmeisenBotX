using System.Collections.Generic;
using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath
{
    public class StaticPathFinder
    {
        public static StaticPath FindNearestPath(Vector3 playerPosition, List<StaticPath> staticPaths)
        {
            StaticPath nearestPath = default;
            float shortestDistance = float.MaxValue;

            foreach (StaticPath staticPath in staticPaths)
            {
                if (staticPath.BoundingBox.Contains(playerPosition))
                {
                    return staticPath;
                }

                float distance = staticPath.BoundingBox.DistanceTo(playerPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPath = staticPath;
                }
            }

            return nearestPath;
        }
    }
}