using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath
{
    public abstract class StaticPath : IStaticPath
    {
        public abstract WowMapId MapId { get; }

        public abstract List<Vector3> Path { get; }

        public BoundingBox BoundingBox { get; }

        // Constructor

        protected StaticPath()
        {
            BoundingBox = BoundingBox.Generate(Path);
        }

        // Public

        /// <inheritdoc cref="IStaticPath.GetPathFromWhereToStart(Vector3)"/>
        public List<Vector3> GetPathFromWhereToStart(Vector3 playerPosition)
        { 
            int index = Path.IndexOf(Path.MinBy(e => e.GetDistance(playerPosition)));
            return Path.GetRange(index, Path.Count - index);
        }

        /// <inheritdoc cref="IStaticPath.IsUsable(WowMapId)"/>
        public bool IsUsable(WowMapId mapId)
        {
            return MapId == mapId;
        }
    }
}