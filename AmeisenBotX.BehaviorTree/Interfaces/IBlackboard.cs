namespace AmeisenBotX.BehaviorTree.Interfaces
{
    public interface IBlackboard
    {
        int MyTeamScore { get; }

        int MyTeamMaxScore { get; }

        int EnemyTeamScore { get; }

        int EnemyTeamMaxScore { get; }

        void Update();
    }
}