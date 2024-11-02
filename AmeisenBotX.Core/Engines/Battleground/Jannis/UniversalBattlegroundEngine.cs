using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles;
using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.AlteracValley;
using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles.WarsongGulch;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class UniversalBattlegroundEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config) : IBattlegroundEngine
    {
        public string Author => "Jannis";

        public string Description => "Working battlegrounds:\n - Warsong Gulch\n - Alterac Valley";

        public string Name => "Universal Battleground Engine";

        public IBattlegroundProfile Profile { get; set; }

        private AmeisenBotInterfaces Bot { get; } = bot;

        private AmeisenBotConfig Config { get; } = config;

        public void Execute()
        {
            if (Profile == null)
            {
                TryLoadProfile();
            }

            Bot.CombatClass?.OutOfCombatExecute();
            Profile?.Execute();
        }

        public void Reset()
        {
            Profile = null;
        }

        public override string ToString()
        {
            return $"{Name} ({Author})";
        }

        private bool TryLoadProfile()
        {
            switch (Bot.Objects.MapId)
            {
                case WowMapId.WarsongGulch:
                    Profile = new WarsongGulchProfile(Bot);
                    return true;

                case WowMapId.AlteracValley:
                    Profile = new AlteracValleyProfile(Bot, Config);
                    return true;

                default:
                    Profile = null;
                    return false;
            }
        }
    }
}