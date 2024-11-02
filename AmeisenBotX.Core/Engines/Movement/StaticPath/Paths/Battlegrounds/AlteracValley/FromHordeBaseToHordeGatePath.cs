using AmeisenBotX.Common.Math;
using System.Collections.Generic;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley
{
    public class FromHordeBaseToHordeGatePath : StaticPath
    {
        public override WowMapId MapId { get; } = WowMapId.AlteracValley;

        public override List<Vector3> Path { get; } =
        [
            new Vector3(-1437.67f, -610.089f, 51.16038f),
            new Vector3(-1437.67f, -610.089f, 51.16038f),
            new Vector3(-1437.9172f, -607.36743f, 51.208977f),
            new Vector3(-1437.5074f, -600.46857f, 51.225266f),
            new Vector3(-1436.2422f, -593.59863f, 50.93214f),
            new Vector3(-1432.9438f, -587.49414f, 51.00303f),
            new Vector3(-1427.4045f, -583.2445f, 51.7688f),
            new Vector3(-1421.6677f, -579.38275f, 53.06444f),
            new Vector3(-1415.9248f, -575.2392f, 54.070396f),
            new Vector3(-1410.5547f, -570.8608f, 55.223526f),
            new Vector3(-1405.1605f, -566.37885f, 55.621784f),
            new Vector3(-1399.9225f, -561.6619f, 55.247536f),
            new Vector3(-1394.8026f, -556.971f, 55.298496f),
            new Vector3(-1389.6022f, -552.2541f, 54.92299f),
            new Vector3(-1384.3885f, -547.5308f, 55.01532f),
            new Vector3(-1383.6616f, -546.84454f, 54.950428f)
        ];
    }
}