using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley
{
    public class FromAllianceGateToStormpikePath : StaticPath
    {
        public override WowMapId MapId { get; } = WowMapId.AlteracValley;

        public override List<Vector3> Path { get; } = new List<Vector3>
        {
            new Vector3(767.5657f, -491.3152f, 97.73199f),
            new Vector3(760.97015f, -489.98685f, 96.90681f),
            new Vector3(754.3333f, -487.8242f, 93.837036f),
            new Vector3(747.85693f, -485.37796f, 89.94934f),
            new Vector3(734.748f, -480.33716f, 83.01633f),
            new Vector3(728.11597f, -478.23462f, 80.13582f),
            new Vector3(721.34393f, -476.15698f, 76.90527f),
            new Vector3(714.87885f, -473.58084f, 73.2258f),
            new Vector3(708.7134f, -470.23953f, 69.98244f),
            new Vector3(698.94806f, -460.43375f, 65.035545f),
            new Vector3(696.9488f, -453.7466f, 63.504196f),
            new Vector3(696.2089f, -446.7753f, 62.697742f),
            new Vector3(697.0531f, -439.87137f, 62.743217f),
            new Vector3(697.73364f, -432.92636f, 62.679283f),
            new Vector3(685.44147f, -417.99954f, 64.73705f),
            new Vector3(678.674f, -416.2372f, 65.5502f),
            new Vector3(672.09534f, -413.84995f, 66.717354f),
            new Vector3(666.6383f, -409.857f, 67.263824f),
            new Vector3(662.9947f, -396.37808f, 66.26583f),
            new Vector3(661.37225f, -388.48373f, 46.772392f),
            new Vector3(660.3977f, -381.11612f, 41.22321f),
            new Vector3(660.9974f, -374.10327f, 39.019623f),
            new Vector3(662.58746f, -360.2058f, 29.842514f),
            new Vector3(664.2384f, -353.39017f, 29.515827f),
            new Vector3(665.2953f, -346.53775f, 29.65914f),
            new Vector3(666.1572f, -339.57043f, 29.665482f),
            new Vector3(667.0171f, -325.56674f, 30.218176f),
            new Vector3(667.52f, -318.68307f, 29.829739f),
            new Vector3(667.9025f, -311.60944f, 29.427355f),
            new Vector3(668.3361f, -304.6653f, 29.83096f),
            new Vector3(668.92816f, -297.71857f, 30.290682f)
        };
    }
}
