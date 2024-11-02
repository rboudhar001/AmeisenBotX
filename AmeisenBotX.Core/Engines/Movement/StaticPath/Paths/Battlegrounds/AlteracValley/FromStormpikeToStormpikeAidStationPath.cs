using AmeisenBotX.Common.Math;
using System.Collections.Generic;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley
{
    public class FromStormpikeToStormpikeAidStationPath : StaticPath
    {
        public override WowMapId MapId { get; } = WowMapId.AlteracValley;

        public override List<Vector3> Path { get; } =
        [
            new Vector3(666.7774f, -293.63446f, 30.290295f),
            new Vector3(660.4977f, -293.262f, 30.25285f),
            new Vector3(653.699f, -291.89404f, 30.165657f),
            new Vector3(638.67834f, -278.7764f, 30.239704f),
            new Vector3(637.06256f, -271.9542f, 30.139925f),
            new Vector3(636.06683f, -265.02734f, 31.36272f),
            new Vector3(635.2236f, -258.06448f, 33.015354f),
            new Vector3(632.1098f, -237.3055f, 36.772926f),
            new Vector3(630.9856f, -230.38948f, 37.625225f),
            new Vector3(629.9583f, -223.49988f, 38.289932f),
            new Vector3(626.8583f, -202.63947f, 39.0946f),
            new Vector3(625.56854f, -195.79541f, 38.96449f),
            new Vector3(624.35425f, -188.87325f, 38.62062f),
            new Vector3(621.9001f, -168.04068f, 36.371277f),
            new Vector3(621.04065f, -161.11511f, 35.1947f),
            new Vector3(620.30725f, -154.16078f, 33.79984f),
            new Vector3(621.1953f, -133.24696f, 33.51617f),
            new Vector3(622.81995f, -126.5121f, 34.105587f),
            new Vector3(624.721f, -119.70128f, 35.781174f),
            new Vector3(630.20135f, -99.55507f, 40.747654f),
            new Vector3(630.78156f, -92.675186f, 41.320354f),
            new Vector3(631.87836f, -85.70688f, 41.432262f),
            new Vector3(633.02875f, -78.78949f, 41.43106f),
            new Vector3(633.7236f, -57.879917f, 41.950775f),
            new Vector3(634.6282f, -50.931416f, 42.344135f),
            new Vector3(636.18414f, -44.072083f, 44.015717f),
            new Vector3(637.69635f, -37.18873f, 45.18385f),
            new Vector3(638.2428f, -34.585583f, 45.69306f)
        ];
    }
}
