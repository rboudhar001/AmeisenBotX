using System;
using AmeisenBotX.BehaviorTree.Interfaces;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public abstract class Blackboard : IBlackboard
    {
        ///<inheritdoc cref="IBlackboard.MyTeamScore"/>
        public abstract int MyTeamScore { get; set; }

        ///<inheritdoc cref="IBlackboard.MyTeamMaxScore"/>
        public abstract int MyTeamMaxScore { get; set; }

        ///<inheritdoc cref="IBlackboard.EnemyTeamScore"/>
        public abstract int EnemyTeamScore { get; set; }

        ///<inheritdoc cref="IBlackboard.EnemyTeamMaxScore"/>
        public abstract int EnemyTeamMaxScore { get; set; }

        private Action UpdateAction { get; }

        protected Blackboard(Action updateAction)
        {
            UpdateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
        }

        public virtual void Update()
        {
            //  TODO: update generic values of the battleground
            // ...

            // update specific values of the battleground
            UpdateAction();
        }
    }
}