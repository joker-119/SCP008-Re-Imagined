using System.Net.NetworkInformation;
using Smod2;
using Smod2.API;
using System;
using System.Linq;
using Smod2.Commands;
using System.Collections.Generic;

namespace SCP008
{
	public class SCP008Command : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "";
		}
		public string GetUsage()
		{
			return "SCP008 Command List \n"+
			"[scp008 / scp08 / scp8 / infect] \n"+
			"scp008 - enabled/disables plugin functionalist. \n"+
			"infect PlayerName/ID - infects the specified player with SCP-008";
		}
		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (Functions.singleton.IsAllowed(sender))
			{
				if (args.Length == 0 && sender is Player p)
				{
					if (SCP008.playersToDamage.Contains(p.SteamId))
					{
						SCP008.playersToDamage.Remove(p.SteamId);
						return new string[] { "Cured infection " + p.Name };
					}
					else
					{
						SCP008.playersToDamage.Add(p.SteamId);
						return new string[] { " Infected " + p.Name };
					}
				}
				else if (args.Length > 0)
				{
					switch (args[0].ToLower())
					{
						case "all":
						case "*":
						{
							int x = 0;
							foreach (Player player in SCP008.singleton.Server.GetPlayers().Where(ply => ply.TeamRole.Role != Role.SPECTATOR && ply.TeamRole.Role != Role.UNASSIGNED && ply.TeamRole.Role != Role.SCP_049_2))
							{
								string arg = (args.Length > 1 && !string.IsNullOrEmpty(args[1])) ? args[1].ToLower() : "";
								if (SCP008.playersToDamage.Contains(player.SteamId) && arg != "infect")
									SCP008.playersToDamage.Remove(player.SteamId);
								else if (!SCP008.playersToDamage.Contains(player.SteamId) && arg != "infect")
									SCP008.playersToDamage.Add(player.SteamId);
								x++;
							}
							return new string[] { "Toggled infection on " + x + " players!" };
						}
						case "debug":
						{
							Functions.singleton.InfectRandom();
							return new string[] {"Debugging random infection."};
						}
						default:
						{
							List<Player> players = SCP008.singleton.Server.GetPlayers(args[0]);
							Player player;
							if (players == null || players.Count == 0) return new string[] { "No players on the server called " + args[0] };
							player = players.OrderBy(ply => ply.Name.Length).First();

							if (!SCP008.playersToDamage.Contains(player.SteamId))
							{
								SCP008.playersToDamage.Add(player.SteamId);
								return new string[] { "Infected " + player.Name };
							}
							else if (SCP008.playersToDamage.Contains(player.SteamId))
							{
								SCP008.playersToDamage.Remove(player.SteamId);
								return new string[] { "Cured " + player.Name };
							}
							else
								return new string[] { "Infection error." };
						}
					}
				}
				else
					return new string[] { GetUsage() };
			}
			else
				return new string[] { "You are not permitted to use this command." };
		}
	}
}