using System.Text.Json;
using System.Text.Json.Serialization;
using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles
{
    public abstract class BattlegroundProfile : IBattlegroundProfile
    {
        // Static

        protected static readonly JsonSerializerOptions Options = new()
        {
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // Abstract

        ///<inheritdoc cref="IBattlegroundProfile.EnemyBasePosition"/>
        public abstract Vector3 EnemyBasePosition { get; }

        ///<inheritdoc cref="IBattlegroundProfile.EnemyGraveyardPosition"/>
        public abstract Vector3 EnemyGraveyardPosition { get; }

        ///<inheritdoc cref="IBattlegroundProfile.GatePosition"/>
        public abstract Vector3 GatePosition { get; }

        ///<inheritdoc cref="IBattlegroundProfile.OwnBasePosition"/>
        public abstract Vector3 OwnBasePosition { get; }

        ///<inheritdoc cref="IBattlegroundProfile.OwnGraveyardPosition"/>
        public abstract Vector3 OwnGraveyardPosition { get; }

        ///<inheritdoc cref="IBattlegroundProfile.Blackboard"/>
        public abstract Blackboard Blackboard { get; }

        // Override

        public virtual void Execute()
        {
            //TODO: implement generic logic

            // 1- IsGateOpen => Move to GatePosition
            // ...

            // 2- Move to EnemyBasePosition for fights
            // ...

            // 3- Move to OwnBasePosition (to leave combat for eat)
            // ...
        }
    }
}