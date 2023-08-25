using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace TerrorTown
{
    public partial class Idiot : BaseTeam
    {
        public override TeamAlignment TeamAlignment
        {
            get
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return TeamAlignment.Traitor;
                }
                return TeamAlignment.NoAllies;
            }
        }

        public override string TeamName
        {
            get
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return "Idiot";
                }
                // Name cannot be "Innocent" because of various TeamName checks.
                return " Innocent ";
            }
        }

        public override Color TeamColour
        {
            get 
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return Color.FromRgb(0xFF4F3F);
                }
                return Color.Green;
            }
        }

        public override TeamMemberVisibility TeamMemberVisibility
        {
            get
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return TeamMemberVisibility.Alignment | TeamMemberVisibility.PublicWhenConfirmedDead;
                }
                return TeamMemberVisibility.None | TeamMemberVisibility.PublicWhenConfirmedDead;
            }
        }

        public override bool CanSeePrivateTime 
        { 
            get 
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return true;
                }
                return false;
            }
        }

        public override bool CanSeeMIA
        {
            get
            {
                if (IdiotManager.IdiotRevealed)
                {
                    return true;
                }
                return false;
            }
        }

		public override bool HasTeamVoiceChat
		{
			get
			{
				if ( IdiotManager.IdiotRevealed )
				{
					return true;
				}
				return false;
			}
		}

        public override string VictimKillMessage => "You were killed by {0}. They were the Idiot";
        public override string RoleDescription => "You are an innocent Terrorist! But there are traitors around... Who can you trust, and who is out to fill you with bullets?\r\n\r\nWatch your back and work with your comrades to get out of this alive!";
        public override string IdentifyString => "{0} found the body of {1}. They were the Idiot";
        public override string OverheadIcon => "ui/world/idiot.png";
        public override bool DisplayComrades => false;
        public override float TeamPlayerPercentage => 0.0f;
        public override int TeamPlayerMinimum => 0;
        public override int TeamPlayerMaximum => 1;

        public override void DoGameruleTick()
        {
            if (!MyGame.PreventWin && ShouldWin())
            {
                Idiot idiotTeam = Teams.RegisteredTeams.OfType<Idiot>().FirstOrDefault();
                if (idiotTeam?.Players.Count == 1)
                {
                    IdiotManager.IdiotRevealed = true;
                    MyGame.Current.OnTeamWin(this);
                }
            }
        }

        public override bool ShouldWin()
        {
            var adversaryTeams = (from i in Teams.RegisteredTeams
                               where i.GetType() != typeof(Spectator)
                               where i.TeamAlignment != TeamAlignment.NoEnemies && i.TeamAlignment != TeamAlignment.Traitor && i != this
                               select i.TeamName).ToList();
            foreach (string adversaryTeam in adversaryTeams)
            {
                _ = TeamName;
                _ = from i in Teams.GetByName(adversaryTeam).Players
                    where i.LifeState == LifeState.Alive && i.IsValid
                    select i.Name;
                if (Teams.GetByName(adversaryTeam).Players.Any((Player i) => i.LifeState == LifeState.Alive && i.IsValid))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
