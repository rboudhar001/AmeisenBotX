﻿namespace AmeisenBotX.Core.Engines.Movement.Settings
{
    public class MovementSettings
    {
        public bool EnableDistanceMovedJumpCheck { get; set; } = true;

        public float MaxSteering { get; set; } = 3.0f;

        public float MaxSteeringCombat { get; set; } = 10.0f;

        public float MaxVelocity { get; set; } = 5.0f;

        public float SeperationDistance { get; set; } = 2.0f;

        public float WaypointCheckThreshold { get; set; } = 1.7f;

        public float WaypointCheckThresholdMounted { get; set; } = 3.5f;
    }
}