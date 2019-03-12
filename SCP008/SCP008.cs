using Smod2;
using Smod2.API;
using Smod2.Config;
using Smod2.Attributes;
using System;
using System.Collections.Generic;
using Smod2.Events;
using scp4aiur;

namespace SCP008
{
	[PluginDetails(
		author = "Evan/Joker119",
		name = "SCP008",
		description = "Plugin that replicated SCP008 Behaviour",
		id = "rnen.scp.008",
		version = "1.0.0",
		SmodMajor = 3,
		SmodMinor = 3,
		SmodRevision = 0
	)]

	public class SCP008 : Plugin
	{
		internal static SCP008 singleton;
		public static string[] validRanks;
		public static readonly Random gen = new System.Random();
		public static bool
			plague,
			kill_infects,
			cure_enabled,
			hit_tut,
			announce,
			announce_req049,
			infect,
			can_announce = false,
			enabled;
		public static int
			damage_amount,
			damage_interval,
			swing_dmg,
			infect_chance,
			kill_infect_chance,
			cure_chance,
			infect_num;
		public static List<int> roles = new List<int>();
		public static List<string> playersToDamage = new List<string>();

		public override void OnDisable() => this.Info(this.Details.name + " v." + this.Details.version + " has been disabled.");
		public override void OnEnable()
		{
			singleton = this;
			this.Info(this.Details.name + " v." + this.Details.version + " has been enabled.");
		}
		public override void Register()
		{
			this.AddEventHandlers(new EventHandlers(this), Priority.Normal);
			this.AddCommands(new string[] {"scp008", "scp08", "scp8", "infect"}, new SCP008Command());
			this.AddConfig(new ConfigSetting("scp008_enabled", true, SettingType.BOOL, true, "If the plugin should be enabled."));
			this.AddConfig(new ConfigSetting("scp008_damage_amount", 1, SettingType.NUMERIC, true, "The amount of damage per tick 00 does."));
			this.AddConfig(new ConfigSetting("scp008_damage_interval", 2, SettingType.NUMERIC, true, "The amount of time between damage ticks."));
			this.AddConfig(new ConfigSetting("scp008_swing_damage", 0, SettingType.NUMERIC, true, "The amount of extra damage zombie swings does."));
			this.AddConfig(new ConfigSetting("scp008_zombiekill_infects", true, SettingType.BOOL, true, "If zombie kills cause infection."));
			this.AddConfig(new ConfigSetting("scp008_infect_chance", 100, SettingType.NUMERIC, true, "The percent chance a zombie attack will infect."));
			this.AddConfig(new ConfigSetting("scp008_killinfect_chance", 100, SettingType.NUMERIC, true, "This percent chance a zombie kill will infect."));
			this.AddConfig(new ConfigSetting("scp008_cure_enabled", true, SettingType.BOOL, true, "If the infection can be cured."));
			this.AddConfig(new ConfigSetting("scp008_cure_chance", 100, SettingType.NUMERIC, true, "The percent chance a cure will be effective."));
			this.AddConfig(new ConfigSetting("scp008_ranklist_commands", new string[] {"owner", "admin"}, SettingType.LIST, true, "The ranks allowed to use scp008 commands."));
			this.AddConfig(new ConfigSetting("scp008_roles_caninfect", new int[] { -1 }, SettingType.NUMERIC_LIST, true, "The roles able to be infected."));
			this.AddConfig(new ConfigSetting("scp008_canhit_tutorial", true, SettingType.BOOL, true, "If the infection can affect tutorials."));
			this.AddConfig(new ConfigSetting("scp008_announcement_enabled", true, SettingType.BOOL, true, "If cassie announcements should be made."));
			this.AddConfig(new ConfigSetting("scp008_announcement_count049", false, SettingType.BOOL, true, "If the announcements require and 049 present."));
			this.AddConfig(new ConfigSetting("scp008_infect", true, SettingType.BOOL, true, "If SCP-008 shoudl only infect if there's no 049."));
			this.AddConfig(new ConfigSetting("scp008_infect_num", 3, SettingType.NUMERIC, true, "How many players shoudl be infected automatically."));
			new Functions(this);
			Timing.Init(this);
		}
	}
}