﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;
using Sandbox;

namespace TerrorTown
{
    public partial class Idiot : BaseTeam
    {
        public static readonly int TimeToReveal = 10;
        public override TeamAlignment TeamAlignment
        {
            get
            {
                TimeSince roundtime = MyGame.Current.TimeSinceRoundStateChanged;
                if (roundtime > TimeToReveal || MyGame.Current.RoundState == RoundState.Ending)
                {
                    return TeamAlignment.Traitor;
                }
                else
                {
                    return TeamAlignment.NoAllies;
                }
            }
        }

        public override string TeamName
        {
            get
            {
                TimeSince roundtime = MyGame.Current.TimeSinceRoundStateChanged;
                if (roundtime > TimeToReveal)
                {
                    return "Idiot";
                }
                else if (MyGame.Current.RoundState != RoundState.Ending)
                {
                    return "Innocent (idiot)";
                }
                return "Idiot";
            }
        }

        public override Color TeamColour
        {
            get 
            {
                TimeSince roundtime = MyGame.Current.TimeSinceRoundStateChanged;
                if (roundtime > TimeToReveal)
                {
                    return Color.FromRgb(0xFF4F3F);
                }
                else if (MyGame.Current.RoundState != RoundState.Ending)
                {
                    return Color.Green;
                }
                return Color.FromRgb(0xFF4F3F);
            }
        }

        public override TeamMemberVisibility TeamMemberVisibility
        {
            get
            {
                TimeSince roundtime = MyGame.Current.TimeSinceRoundStateChanged;
                if (roundtime > TimeToReveal)
                {
                    return TeamMemberVisibility.Alignment;
                }
                else if (MyGame.Current.RoundState != RoundState.Ending)
                {
                    return TeamMemberVisibility.None;
                }
                return TeamMemberVisibility.Alignment;
            }
        }

        public override IList<string> AdversaryTeams => (from i in Teams.RegisteredTeams
                                                         where i.GetType() != typeof(Spectator)
                                                         where i.TeamAlignment != TeamAlignment.NoEnemies && i.TeamAlignment != TeamAlignment.Traitor && i != this
                                                         select i.TeamName).ToList();
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
                
                TimeSince roundtime = MyGame.Current.TimeSinceRoundStateChanged;
                if (roundtime < TimeToReveal && MyGame.Current.RoundState != RoundState.Ending)
                {
                    // var teams = (from i in Teams.RegisteredTeams
                    //                   where i.GetType() != typeof(Spectator)
                    //                   where i.TeamAlignment == TeamAlignment.Traitor && i != this
                    //                   select i).ToList();
                    //foreach (var team in teams)
                    //{
                    //    MyGame.Current.OnTeamWin(team);
                    //}
                    //MyGame.PreventWin = true;
                }
                MyGame.Current.RoundState = RoundState.Ending;
                MyGame.Current.OnTeamWin(this);
            }
        }

        //[Event("Game.Round.End")]
        //public static void ResetGamePreventWin()
        //{
        //    MyGame.PreventWin = false;
        //}
    }
}