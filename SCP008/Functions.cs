using Smod2;
using Smod2.API;
using System;
using System.Linq;
using System.Collections.Generic;
using Smod2.Commands;
using scp4aiur;

namespace SCP008
{
	public class Functions
	{
		public static Functions singleton;
		public SCP008 SCP008;
		public Functions(SCP008 plugin)
		{
			this.SCP008 = plugin;
			Functions.singleton = this;
		}

		public bool SCP008Dead()
		{
			bool scp049alive = (SCP008.announce_req049) ?
				SCP008.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049).Count() > 0 : false;
			bool scp008alive = SCP008.playersToDamage.Count() < 1 &&
				SCP008.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2).Count() == 0 && !scp049alive;
			return !scp008alive;
		}
		public IEnumerable<float> Check049(float delay)
		{
			yield return delay;
			if (Is049Spawned())
			{
				int ran = SCP008.gen.Next(1,100);
				if (ran > 50)
				{
					List<Player> players = SCP008.Server.GetPlayers();
					foreach (Player player in players)
					{
						if (player.TeamRole.Role == Role.SCP_049)
						{
							player.ChangeRole(Role.CLASSD, true, true, true, true);
							Timing.Run(Infect(player));
							player.PersonalClearBroadcasts();
							player.PersonalBroadcast(10, "You have been infected with SCP-008!", false);
						}
					}
				}
				else
					SCP008.enabled = false;
			}
			else
				SCP008.enabled = false;
		}
		public bool Is049Spawned()
		{
			List<Player> players = SCP008.Server.GetPlayers();
			foreach (Player player in players)
			{
				if (player.TeamRole.Role == Role.SCP_049)
					return true;
			}
			return false;
		}
		public void InfectRandom()
		{
			SCP008.Info("Randomly selecting infected personnel.");
			List<Player> players = SCP008.Server.GetPlayers();
			SCP008.Debug("List obtained.");
			for (int i = 0; i < SCP008.infect_num; i++)
			{
				SCP008.Debug("For loop started.");
				if (players.Count == 0) break;
				int ran = SCP008.gen.Next(players.Count);
				Player ply = players[ran];
				if (ply.TeamRole.Role == Role.CLASSD && players.Count > 0)
				{

					SCP008.playersToDamage.Add(ply.SteamId);
					players.Remove(ply);
					SCP008.Info(ply.Name + " randomly selected for infection!");
				}
				else if (ply.TeamRole.Role != Role.CLASSD && players.Count > 0)
				{
					i--;
					players.Remove(ply);
					SCP008.Debug("Selected player not Class-D, re-running loop!");
				}
				else
				{
					SCP008.Debug("Not enough players in the list, breaking!");
					break;
				}
			}
		}
		public void ChangeToSCP008(Player player)
		{
			if (SCP008.playersToDamage.Contains(player.SteamId))
				SCP008.playersToDamage.Remove(player.SteamId);
			Vector pos = player.GetPosition();
			player.ChangeRole(Role.SCP_049_2, true, false, false, false);
			player.Teleport(pos);
			SCP008.can_announce = false;
			SCP008.Debug("Changed " + player.Name + " to SCP-008");
		}
		public IEnumerable<float> Infect(Player player)
		{
			yield return 25;
			SCP008.Info("Waiting 25 seconds before infecting first player.");
			SCP008.playersToDamage.Add(player.SteamId);
		}
		public bool IsAllowed(ICommandSender sender)
		{
			Player player = (sender is Player) ? sender as Player : null;
			if (player != null)
			{
				string [] configList = SCP008.validRanks;
				List<string> roleList = (configList != null && configList.Length > 0) ? 
					configList.Select(role => role.ToUpper()).ToList() : new List<string>();
				
				if (roleList != null && roleList.Count > 0 && (roleList.Contains(player.GetUserGroup().Name.ToUpper()) || roleList.Contains(player.GetRankName().ToUpper())))
				{
					return true;
				}
				else if (roleList == null || roleList.Count == 0)
					return true;
				else
					return false;
			}
			else
				return true;
		}
	}
}