using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;
using Sandbox;

namespace TerrorTown
{
    public static class IdiotManager
    {
        // Default chance is 100% while testing, change before sending it to live!
        [ConVar.Replicated("idiot_role_chance", Max = 1, Min = 0)]
        public static float IdiotChance { get; set; } = 1;
        private static bool IdiotExists { get; set; } = false;

        // Hope we can remove this in the future
        private static bool IdiotBought { get; set; } = false;

        [Event("Game.Team.PostSelection")]
        public static void ChangeToIdiot()
        {
            if (!Game.IsServer) { return; }

            if (Game.Random.Float() < IdiotChance)
            {
                var idiota = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();

                if (idiota == null)
                {
                    Log.Info("Where idiot *monkey-emoji*");
                    return;
                }

                var traitorteam = Teams.RegisteredTeams.OfType<Traitor>().FirstOrDefault();

                if (traitorteam == null)
                {
                    Log.Info("Where traitor *monkey-emoji*");
                    return;
                }

                // Don't have to randomize because this list because it is done during team selection.
                var ply = traitorteam.Players.FirstOrDefault();

                // Debug log
                Log.Info("Selected " + ply.Owner.Name + " from traitors");

                traitorteam.RemovePlayer(ply);
                idiota.AddPlayer(ply);
                IdiotExists = true;
                IdiotBought = false;
            }
        }
        
        [Event("Game.Round.Ending")]
        public static void RoundEnding(MyGame _)
        {
            IdiotExists = false;        
        }

        [GameEvent.Tick.Server]
        public static void GameTickServer()
        {
            if (IdiotExists)
            {
                if (MyGame.Current.TimeSinceRoundStateChanged > Idiot.TimeToReveal && MyGame.Current.RoundState == RoundState.Started)
                {
                    Idiot idiotTeam = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();
                    TerrorTown.Player ply = idiotTeam?.Players.FirstOrDefault();
                    ((IClient)ply?.Owner)?.SendCommandToClient("idiot_role_play_sound");
                    IdiotExists = false;  
                }
            }
        }

        [GameEvent.Tick.Client]
        public static void GameTickClient()
        {
            if (Input.Pressed("View"))
            {
                if (MyGame.Current.TimeSinceRoundStateChanged > Idiot.TimeToReveal && MyGame.Current.RoundState == RoundState.Started)
                {
                    ConsoleSystem.Run("idiot_simulate_buy_menu");
                }
            }
        }

        [Event("Game.Initialized")]
        public static void GameInitialized(MyGame _)
        {
            Teams.RegisteredTeams = Teams.RegisteredTeams.OrderBy(x => x.TeamName).ToList();
        }



        [ConCmd.Client("idiot_role_play_sound")]
        public static void IdiotRolePlaySound() 
        {
            Sound.FromScreen(To.Single(Game.LocalPawn), "idiot");
        }

        [ConCmd.Server("idiot_simulate_buy_menu")]
        public static void IdiotSimulateBuyMenu()
        {
            if (ConsoleSystem.Caller.Pawn is TerrorTown.Player ply && ply.Team.GetType() == typeof(Idiot) && !IdiotBought)
            {
                if (MyGame.Current.TimeSinceRoundStateChanged > Idiot.TimeToReveal && MyGame.Current.RoundState == RoundState.Started)
                {
                    new Radar().Touch(ply);
                    new BodyArmour().Touch(ply);
                    IdiotBought = true;
                }
            }
        }
    }
}
