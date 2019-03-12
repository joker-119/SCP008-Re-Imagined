using System.Data;
using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventSystem;
using Smod2.EventHandlers;
using System;
using System.Linq;
using System.Collections.Generic;
using scp4aiur;

namespace SCP008
{
	class EventHandlers : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerWaitingForPlayers, IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerMedkitUse, IEventHandlerUpdate, IEventHandlerCheckEscape
	{
		private readonly SCP008 plugin;
		public EventHandlers(SCP008 plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			SCP008.kill_infects = this.plugin.GetConfigBool("scp008_zombiekill_infects");
			SCP008.enabled = this.plugin.GetConfigBool("scp008_enabled");
			SCP008.damage_amount = this.plugin.GetConfigInt("scp008_damage_amount");
			SCP008.damage_interval = this.plugin.GetConfigInt("scp008_damage_interval");
			SCP008.swing_dmg = this.plugin.GetConfigInt("scp008_swing_damage");
			SCP008.infect_chance = this.plugin.GetConfigInt("scp008_infect_chance");
			SCP008.kill_infect_chance = this.plugin.GetConfigInt("scp008_killinfect_chance");
			SCP008.cure_enabled = this.plugin.GetConfigBool("scp008_cure_enabled");
			SCP008.cure_chance = this.plugin.GetConfigInt("scp008_cure_chance");
			SCP008.validRanks = this.plugin.GetConfigList("scp008_ranklist_commands");
			SCP008.roles = this.plugin.GetConfigIntList("scp008_roles_caninfect").ToList();
			SCP008.hit_tut = this.plugin.GetConfigBool("scp008_canhit_tutorial");
			SCP008.announce = this.plugin.GetConfigBool("scp008_announcement_enabled");
			SCP008.announce_req049 = this.plugin.GetConfigBool("scp008_announcement_count049");
			SCP008.infect = this.plugin.GetConfigBool("scp008_infect");
			SCP008.infect_num = this.plugin.GetConfigInt("scp008_infect_num");
		}
		public void OnRoundStart(RoundStartEvent ev)
		{
			if (!SCP008.enabled) return;
			SCP008.playersToDamage.Clear();

			List<Player> players = ev.Server.GetPlayers();

			Timing.Run(Functions.singleton.Check049(1.5f));
		}
		public void OnRoundEnd(RoundEndEvent ev)
		{
			SCP008.playersToDamage.Clear();
		}
		DateTime updateTimer = DateTime.Now;
		DateTime announcementTimer = DateTime.Now;
		public void OnUpdate(UpdateEvent ev)
		{
			if (!SCP008.enabled) return;
			if (announcementTimer < DateTime.Now)
			{
				foreach (var id in SCP008.playersToDamage)
				{
					plugin.Debug(id.ToString());
				}
				announcementTimer = SCP008.can_announce ? DateTime.Now.AddSeconds(5) : DateTime.Now.AddSeconds(30);
				if (SCP008.can_announce && Functions.singleton.SCP008Dead() && SCP008.announce)
				{
					plugin.Debug("Before Announce");
					plugin.Server.Map.AnnounceScpKill("008");
					SCP008.can_announce = false;
					plugin.Debug("After announce.");
				}
			}

			if (updateTimer < DateTime.Now)
			{
				updateTimer = DateTime.Now.AddSeconds(SCP008.damage_interval);

				if (plugin.Server.GetPlayers().Count > 0)
					foreach (Player player in plugin.Server.GetPlayers())
					{
						if ((player.TeamRole.Team != Smod2.API.Team.SCP && player.TeamRole.Team != Smod2.API.Team.SPECTATOR) && SCP008.playersToDamage.Contains(player.SteamId))
						{
							plugin.Debug("Damaging " + player.Name + " for " + SCP008.damage_amount + ".");
							if (SCP008.damage_amount < player.GetHealth())
								player.Damage(SCP008.damage_amount);
							else if (SCP008.damage_amount >= player.GetHealth())
								Functions.singleton.ChangeToSCP008(player);
						}
					}
			}
		}
		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}
		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}
		public void OnMedkitUse(PlayerMedkitUseEvent ev)
		{
			if (!SCP008.enabled) return;
			if (SCP008.cure_enabled && SCP008.playersToDamage.Contains(ev.Player.SteamId) && SCP008.cure_chance > 0 && SCP008.cure_chance >= new Random().Next(1,100))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}
		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.PlayerId == ev.Player.PlayerId || !SCP008.enabled) return;

			int damageAmount = (ev.Attacker.TeamRole.Role == Role.SCP_049_2) ? SCP008.swing_dmg : 0;
			int infectChance = (ev.Attacker.TeamRole.Role == Role.SCP_049_2) ? SCP008.infect_chance : 0;
			int infectOnKillChance = (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && ev.Damage >= ev.Player.GetHealth()) ? SCP008.kill_infect_chance : 0;

			if (ev.Player.TeamRole.Team == Smod2.API.Team.TUTORIAL && ev.Attacker.TeamRole.Role == Smod2.API.Role.SCP_049_2 && !SCP008.hit_tut)
			{
				ev.Damage = 0f; return;
			}

			if (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;
			
			if (SCP008.enabled && ev.Attacker.TeamRole.Role == Role.SCP_049_2 && !SCP008.playersToDamage.Contains(ev.Player.SteamId) && infectChance > 0 && new Random().Next(1,100) <= infectChance && !SCP008.plague)
			{
				if ( (SCP008.roles == null || SCP008.roles.Count == 0 || SCP008.roles.FirstOrDefault() == -1) || (SCP008.roles.Count > 0 && SCP008.roles.Contains((int)ev.Player.TeamRole.Role)))
					SCP008.playersToDamage.Add(ev.Player.SteamId);
			}

			if (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && SCP008.kill_infects && ev.Damage >= ev.Player.GetHealth() && new Random().Next(1,100) <= infectOnKillChance && !SCP008.plague)
			{
				ev.Damage = 0f;
				Functions.singleton.ChangeToSCP008(ev.Player);
			}
		}
	} 
}