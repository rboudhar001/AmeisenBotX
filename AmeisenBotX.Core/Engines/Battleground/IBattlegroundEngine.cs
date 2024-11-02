using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles;

namespace AmeisenBotX.Core.Engines.Battleground
{
    public interface IBattlegroundEngine
    {
        string Author { get; }

        string Description { get; }

        string Name { get; }

        IBattlegroundProfile Profile { get; set; }

        void Execute();

        void Reset();
    }
}