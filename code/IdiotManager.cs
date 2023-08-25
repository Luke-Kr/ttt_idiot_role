using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;
using Sandbox;

namespace TerrorTown
{
    public static partial class IdiotManager
    {
        // Chance that one of the traitors will be an idiot. Default is 25%.
        [ConVar.Replicated("idiot_role_chance", Max = 1, Min = 0, Help = "Chance that one of the traitors will be an idiot. Default is 25%.", Saved = true)]
        public static float IdiotChance { get; set; } = 0.25f;
        public static bool IdiotRevealed { get; set; } = true;

        // How long it takes for the idiot to be known he is a traitor. Default is 90.
        [ConVar.Replicated("idiot_time_to_reveal", Min = 0, Help = "How long it takes for the idiot to be known he is a traitor. Default is 90.", Saved = true)]
        public static int TimeToReveal { get; set; } = 90;

        // Upper limit on how much the time to reveal will be randomly offset. Default is 10.
        [ConVar.Replicated("idiot_reveal_offset", Min = 0, Help = "Upper limit on how much the time to reveal will be randomly offset. Default is 10.", Saved = true)]
        public static int RevealOffset { get; set; } = 10;
        private static int RealTimeToReveal { get; set; }

        // Hope we can remove this in the future.
        public static bool IdiotBought { get; set; } = false;

        [Event("Game.Team.PostSelection")]
        public static void ChangeToIdiot()
        {
            if (!Game.IsServer) { return; }

            if (Game.Random.Float() < IdiotChance)
            {
                var teamIdiot = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();

                if (teamIdiot == null)
                {
                    Log.Error("Team Idiot not found! This shouldn't be possible.");
                    return;
                }

                var teamTraitor = Teams.RegisteredTeams.OfType<Traitor>().FirstOrDefault();

                if (teamTraitor == null)
                {
                    Log.Error("Team Traitor not found! This shouldn't be possible.");
                    return;
                }

                // Don't have to randomize because this list because it is done during team selection.
                var ply = teamTraitor.Players.FirstOrDefault();

                // Debug log
                Log.Info("Selected " + ply.Owner.Name + " from traitors");

                teamTraitor.RemovePlayer(ply);
                teamIdiot.AddPlayer(ply);
                RealTimeToReveal = TimeToReveal + (int) (RevealOffset * 2 * Game.Random.Float()) - RevealOffset;
                IdiotRevealed = false;
                IdiotBought = false;
				ResetVars();
            }
        }

		[ClientRpc]
		public static void ResetVars()
		{
			IdiotBought = false;
		}
        
        // This function might not be necessary but it can't hurt.
        [Event("Game.Round.Ending")]
        public static void RoundEnding(MyGame _)
        {
            IdiotRevealed = true;        
        }

        [Event("Player.PostOnKilled")]
        public static void SetRagdollValues (DamageInfo _, Player ply)
        {
            Idiot idiotTeam = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();
            TerrorTown.Player idiotPly = idiotTeam?.Players.FirstOrDefault();
            if (idiotPly == ply)
            {
                Corpse corpse = ply.Corpse as Corpse;
                corpse.TeamName = "Idiot";
                corpse.TeamColour = Color.FromRgb(0xFF4F3F);
                IdiotRevealed = true;
            }
        }

        [GameEvent.Tick.Server]
        public static void GameTickServer()
        {
            if (!IdiotRevealed)
            {
                if (MyGame.Current.TimeSinceRoundStateChanged > RealTimeToReveal && MyGame.Current.RoundState == RoundState.Started)
                {
                    Idiot idiotTeam = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();
                    TerrorTown.Player ply = idiotTeam?.Players.FirstOrDefault();
                    ((IClient)ply?.Owner)?.SendCommandToClient("idiot_role_play_sound");
                    IdiotRevealed = true;  
                }
            }
        }



        [GameEvent.Tick.Client]
        public static void GameTickClient()
        {
            if (Input.Pressed("View"))
            {
                if (IdiotRevealed && Game.LocalPawn is TerrorTown.Player ply && ply.TeamName == "Idiot" )
                {
                    ConsoleSystem.Run( "idiot_toggle_ui" );
                }
            }
        }

		[ConCmd.Client("idiot_toggle_ui")]
		public static void ToggleUI()
		{
			// Repeat the check for when funny people try to open it via console.
			if ( IdiotRevealed && Game.LocalPawn is TerrorTown.Player ply && ply.TeamName == "Idiot" )
			{
				IdiotBuyMenu panel = Game.RootPanel.ChildrenOfType<IdiotBuyMenu>().FirstOrDefault();
				if ( panel != null ) { panel.Delete(); return; }
				if ( IdiotBought ) return;
				Game.RootPanel.AddChild<IdiotBuyMenu>();
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
                if (IdiotRevealed)
                {
                    new Radar().Touch(ply);
                    new BodyArmour().Touch(ply);
                    IdiotBought = true;
                }
            }
        }
    }
}
