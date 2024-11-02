using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles
{
    public interface IBattlegroundProfile
    {
        Vector3 EnemyBasePosition { get; }

        Vector3 EnemyGraveyardPosition { get; }

        Vector3 GatePosition { get; }

        Vector3 OwnBasePosition { get; }

        Vector3 OwnGraveyardPosition { get; }

        Blackboard Blackboard { get; }

        void Execute();
    }
}