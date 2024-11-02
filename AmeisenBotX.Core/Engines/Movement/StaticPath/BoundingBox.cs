using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath
{
    /**
     * It represents a 3D square that encapsulates all the nodes of a path from A to B.
     *
     * It is used to find the closest static path to a position,
     * if the position is inside the square you can execute that static path,
     * otherwise it is too far away.
     */
    public class BoundingBox(Vector3 min, Vector3 max)
    {
        public Vector3 Min { get; } = min;

        public Vector3 Max { get; } = max;

        public bool Contains(Vector3 position)
        {
            return (position.X >= Min.X && position.X <= Max.X) &&
                   (position.Y >= Min.Y && position.Y <= Max.Y) &&
                   (position.Z >= Min.Z && position.Z <= Max.Z);
        }

        public float DistanceTo(Vector3 position)
        {
            // distance from the position to the nearest edge of the bounding box
            float dx = Math.Max(Math.Max(Min.X - position.X, 0), position.X - Max.X);
            float dy = Math.Max(Math.Max(Min.Y - position.Y, 0), position.Y - Max.Y);
            float dz = Math.Max(Math.Max(Min.Z - position.Z, 0), position.Z - Max.Z);
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        // Static

        public static BoundingBox Generate(List<Vector3> path)
        {
            float minX = path.Min(p => p.X);
            float minY = path.Min(p => p.Y);
            float minZ = path.Min(p => p.Z);
            float maxX = path.Max(p => p.X);
            float maxY = path.Max(p => p.Y);
            float maxZ = path.Max(p => p.Z);

            return new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
    }
}