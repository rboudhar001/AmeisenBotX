using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley
{
    public class FromAllianceBaseToAllianceGatePath : StaticPath
    {
        public override WowMapId MapId { get; } = WowMapId.AlteracValley;

        public override List<Vector3> Path { get; } = new List<Vector3>
        {
            new Vector3(873.0401f, -491.1974f, 96.54045f),
            new Vector3(872.00714f, -492.8713f, 96.56789f),
            new Vector3(857.6211f, -506.1832f, 96.261536f),
            new Vector3(851.0563f, -503.74808f, 96.30707f),
            new Vector3(844.47394f, -501.35f, 97.82756f),
            new Vector3(837.95905f, -498.791f, 98.77303f),
            new Vector3(817.1932f, -496.2735f, 100.2319f),
            new Vector3(810.2378f, -495.5721f, 100.1237f),
            new Vector3(803.2877f, -494.86823f, 99.842384f),
            new Vector3(797.4663f, -494.3541f, 99.697266f)
        };
    }
}