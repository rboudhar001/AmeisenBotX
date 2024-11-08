﻿using System.Collections.Generic;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.StaticPath.Paths.Battlegrounds.AlteracValley
{
    public class FromIcebloodToSnowfallPath : StaticPath
    {
        public override WowMapId MapId { get; } = WowMapId.AlteracValley;

        public override List<Vector3> Path { get; } = new List<Vector3>
        {
            new Vector3(-614.5266f, -394.2111f, 60.85856f),
            new Vector3(-616.1315f, -391.04593f, 58.736458f),
            new Vector3(-619.1389f, -384.71298f, 58.29005f),
            new Vector3(-623.5151f, -371.44153f, 57.11699f),
            new Vector3(-624.6268f, -364.6144f, 56.66302f),
            new Vector3(-624.83325f, -357.56757f, 56.16484f),
            new Vector3(-623.1486f, -350.8948f, 55.600292f),
            new Vector3(-613.8691f, -340.56085f, 54.60712f),
            new Vector3(-608.55133f, -336.06393f, 53.272827f),
            new Vector3(-604.003f, -330.79318f, 51.568935f),
            new Vector3(-600.34595f, -324.74335f, 51.047703f),
            new Vector3(-596.0543f, -319.26605f, 50.34378f),
            new Vector3(-583.4209f, -313.9925f, 47.375668f),
            new Vector3(-576.5858f, -314.66827f, 45.4588f),
            new Vector3(-570.06433f, -317.29416f, 43.171833f),
            new Vector3(-564.16077f, -321.053f, 41.02221f),
            new Vector3(-552.8998f, -329.43237f, 38.61037f),
            new Vector3(-547.59937f, -333.92926f, 37.97838f),
            new Vector3(-542.12695f, -338.3369f, 37.45034f),
            new Vector3(-536.261f, -342.05994f, 36.429276f),
            new Vector3(-529.6554f, -344.33722f, 34.93437f),
            new Vector3(-516.7724f, -339.9699f, 34.105305f),
            new Vector3(-511.81076f, -335.04062f, 33.68471f),
            new Vector3(-507.72238f, -329.27786f, 33.46443f),
            new Vector3(-504.15094f, -323.30225f, 32.95253f),
            new Vector3(-501.16412f, -317.0212f, 32.208225f),
            new Vector3(-496.47498f, -303.8368f, 31.499636f),
            new Vector3(-494.53323f, -297.04605f, 30.892109f),
            new Vector3(-492.07715f, -290.55423f, 29.866354f),
            new Vector3(-489.17444f, -284.09018f, 28.30839f),
            new Vector3(-479.80078f, -274.13174f, 26.291891f),
            new Vector3(-473.11725f, -272.04337f, 24.80291f),
            new Vector3(-466.19144f, -272.51178f, 23.363804f),
            new Vector3(-459.38904f, -274.27542f, 22.456404f),
            new Vector3(-452.6076f, -275.88586f, 21.715601f),
            new Vector3(-438.75845f, -278.10098f, 20.708424f),
            new Vector3(-431.8536f, -278.90073f, 20.573904f),
            new Vector3(-424.91153f, -279.61493f, 19.728636f),
            new Vector3(-417.92276f, -280.3478f, 18.073837f),
            new Vector3(-410.9304f, -280.89236f, 16.053164f),
            new Vector3(-396.9455f, -281.13617f, 13.229681f),
            new Vector3(-390.6111f, -278.44016f, 12.630121f),
            new Vector3(-386.0124f, -273.1471f, 12.279286f),
            new Vector3(-382.55472f, -267.05286f, 12.289478f),
            new Vector3(-379.07715f, -261.0343f, 12.518372f),
            new Vector3(-371.98334f, -248.9728f, 12.862664f),
            new Vector3(-368.422f, -242.90584f, 12.72595f),
            new Vector3(-364.88898f, -236.88716f, 12.694399f),
            new Vector3(-361.28372f, -230.88733f, 12.401703f),
            new Vector3(-357.42566f, -224.94658f, 12.30656f),
            new Vector3(-349.9184f, -213.14621f, 12.005056f),
            new Vector3(-346.25687f, -207.24637f, 12.04588f),
            new Vector3(-342.6183f, -201.21182f, 12.947855f),
            new Vector3(-339.00378f, -195.21721f, 14.408154f),
            new Vector3(-335.43268f, -189.29454f, 12.68884f),
            new Vector3(-328.17838f, -177.26334f, 9.263382f),
            new Vector3(-324.7032f, -171.23598f, 9.260473f),
            new Vector3(-321.37372f, -165.07056f, 9.260473f),
            new Vector3(-318.05472f, -158.81218f, 9.384075f),
            new Vector3(-314.80298f, -152.61342f, 10.303284f),
            new Vector3(-308.3563f, -140.22334f, 13.004119f),
            new Vector3(-305.08795f, -134.00156f, 14.222678f),
            new Vector3(-301.82034f, -127.81896f, 15.281787f),
            new Vector3(-298.37418f, -121.774956f, 16.860703f),
            new Vector3(-290.40436f, -110.28462f, 25.383377f),
            new Vector3(-286.2167f, -104.60641f, 30.583195f),
            new Vector3(-281.83438f, -99.20732f, 35.95275f),
            new Vector3(-277.23016f, -93.88054f, 41.857903f),
            new Vector3(-272.42358f, -88.88019f, 46.191288f),
            new Vector3(-261.18384f, -80.52383f, 54.328293f),
            new Vector3(-254.9684f, -77.3435f, 58.026833f),
            new Vector3(-248.12677f, -76.46211f, 59.954834f),
            new Vector3(-241.59468f, -78.94419f, 61.274258f),
            new Vector3(-236.28798f, -83.4038f, 64.17618f),
            new Vector3(-224.65033f, -91.20191f, 75.41398f),
            new Vector3(-219.8624f, -96.228004f, 78.79392f),
            new Vector3(-214.80719f, -101.087265f, 79.513f),
            new Vector3(-209.71819f, -106.001144f, 79.04069f),
            new Vector3(-205.26834f, -110.29043f, 78.645134f)
        };
    }
}
