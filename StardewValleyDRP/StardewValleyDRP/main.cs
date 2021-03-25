#define EHTRUE
// ^ allows exception handling
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewValleyDRP
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
#if DEBUG
            this.Monitor.Log($"HEY!!! You shouldn't be seeing this!", LogLevel.Alert);
            this.Monitor.Log($"Unless you're Tortellini... then maybe this should be here", LogLevel.Alert);
            this.Monitor.Log($"IF YOURE NOT TORTELLINI THEN YELL AT HIM TO BUILD IN RELEASE MODE", LogLevel.Alert);
#endif
            var discord = new Discord.Discord(824160913908039729, (UInt64)Discord.CreateFlags.NoRequireDiscord);

            var Activity = new Discord.Activity
            {
                State = $"In a Menu",
                Details = $" ",
                Timestamps =
                {
                    Start = 0,
                },
                Assets =
                {
                    LargeImage = " ",
                    LargeText = " ",
                    SmallImage = " ",
                    SmallText = " ",
                },
                Party =
                {
                    Id = "foo partyID",
                    Size =
                    {
                        CurrentSize = 1,
                        MaxSize = 1,
                    }
                },

                Secrets =
                {
                    Match = " ",
                    Join = " ",
                    Spectate = " ",
                },
                Instance = true,
            };
            var userManager = discord.GetUserManager();
            /*
            var imageManager = discord.GetImageManager();
            var lobbyManager = discord.GetLobbyManager(); 
            var applicationManager = discord.GetApplicationManager();
            */
#if CALLBACK
            // Return via callback
            userManager.GetUser(290926444748734465, (Discord.Result result, ref Discord.User otherUser) =>
            {
                if (result == Discord.Result.Ok)
                {
                    this.Monitor.Log($"Username: {otherUser.Username}", LogLevel.Debug);
                    this.Monitor.Log($"ID: {otherUser.Id}", LogLevel.Debug);
                }
            });
#endif
            // Return normally
            userManager.OnCurrentUserUpdate += () =>
            {
                var currentUser = userManager.GetCurrentUser();
                this.Monitor.Log($"Discord User: {currentUser.Username}#{currentUser.Discriminator}", LogLevel.Debug);
                //this.Monitor.Log($"Discriminator: {currentUser.Discriminator}", LogLevel.Debug);
                //this.Monitor.Log($"ID: {currentUser.Id}", LogLevel.Debug);
            };

            helper.Events.GameLoop.DayStarted += this.NewDay;
            helper.Events.GameLoop.UpdateTicked += (sender, e) => this.Update(sender, e, discord, Activity);
            helper.Events.GameLoop.ReturnedToTitle += (sender, e) => this.TimesUp(sender, e, discord);
        }
        /// <summary>
        /// Runs at the start of every new day to update rich presence
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void NewDay(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
#if DEBUG
            this.Monitor.Log($"{Game1.player.Name} on {Game1.player.farmName} farm is on day {Game1.dayOfMonth} of {Game1.currentSeason}", LogLevel.Debug);
#endif
        }
        /// <summary>
        /// Update the discord activity.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Update event args.</param>
        /// <param name="discord">Discord object</param>
        /// <param name="activity">Activity object</param>
        private void Update(object sender, UpdateTickedEventArgs e, Discord.Discord discord, Discord.Activity activity)
        {
#if MULTI_WAIT
            if (!Context.IsWorldReady)
                return;
#endif
            var activityManager = discord.GetActivityManager();
            activityManager.RegisterSteam(413150);
            activityManager.RegisterCommand("steam://run-game-id/413150");
            try
            {
                if (e.IsMultipleOf(30))
                {
                    activityManager.UpdateActivity(CheckActivity(activity), (result) =>
                    {
                        if (result != Discord.Result.Ok)
                            this.Monitor.Log($"Update Activity: {result}", LogLevel.Warn);
                    });

                    discord.RunCallbacks();
                }
            }
            catch { }
            }
        /// <summary>
        /// Checks the activity and updates it when needed.
        /// </summary>
        /// <returns>The activity.</returns>
        /// <param name="a">The activity.</param>
        private Discord.Activity CheckActivity(Discord.Activity a)
        {
            if (!Context.IsWorldReady)
            {
                a.State = "In a Menu";
                a.Details = " ";
                a.Assets.LargeText = " ";
                a.Assets.LargeImage = "menu_small_icon";
                a.Assets.SmallText = " ";
                a.Assets.SmallImage = "menu_small_icon";
                a.Timestamps.Start = 0;
                a.Party.Size.CurrentSize = 1;
                a.Party.Size.MaxSize = 1;
            }
            else
            {
                if (Context.IsMultiplayer)
                {
                    a.Party.Id = Game1.MasterPlayer.UniqueMultiplayerID.ToString();
                    a.Secrets.Join = Game1.server.getInviteCode();
                    a.Party.Size.MaxSize = Game1.getFarm().getNumberBuildingsConstructed("Cabin") + 1;
                    string deets;
                    if (Context.IsMainPlayer)
                        deets = "Hosting Co-op";
                    else
                        deets = "Playing Co-op";
                    a.Details = $"{deets} ({Game1.numberOfPlayers()} of {a.Party.Size.MaxSize})";
                }
                else
                {
                    a.Details = $"Playing solo";
                }
                a.State = $"{Game1.player.farmName} farm ({Game1.player.Money}G) {(Game1.player.hasPet() ? " with " + Game1.player.getPetDisplayName() : ".")}";
                a.Assets.LargeImage = $"{Game1.currentSeason}_{Farmlayout_type()}";
                a.Assets.LargeText = $"At {SplitName(Game1.currentLocation.Name)}";
                a.Assets.SmallImage = $"weather_{Weather_type()}_icon";
                a.Assets.SmallText = $"Day {Game1.dayOfMonth} of {Season()}, Year {Game1.year}";
                a.Timestamps.Start = InGameTime();
#if DEBUG
                this.Monitor.Log($"Ingame Time UTC: {InGameTime()}", LogLevel.Debug);
#endif
            }
            return a;
        }
        /// <summary>
        /// Calls the Discord Dispose function 
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Returned to title event args</param>
        /// <param name="discord">Discord object.</param>
            private void TimesUp(object sender, ReturnedToTitleEventArgs e, Discord.Discord discord)
        {
            discord.Dispose();
        }

        private string Farmlayout_type()
        {
            switch (Game1.whichFarm)
            {
                case Farm.default_layout:
                    return "standard";
                case Farm.riverlands_layout:
                    return "riverland";
                case Farm.forest_layout:
                    return "forest";
                case Farm.mountains_layout:
                    return "hilltop";
                case Farm.combat_layout:
                    return "wilderness";
                default:
                    return "default";
            }
        }
        /// <summary>
        /// Returns the weather type (raining, lightning, et cetera)
        /// </summary>
        /// <returns>The weather type.</returns>
        private string Weather_type()
        {
            if (Game1.isRaining)
                return Game1.isLightning ? "stormy" : "rainy";
            if (Game1.isDebrisWeather)
                return "windy_" + Game1.currentSeason;
            if (Game1.isSnowing)
                return "snowy";
            if (Game1.weddingToday)
                return "wedding";
            if (Game1.isFestival())
                return "festival";
            return "sunny";
        }
        /// <summary>
        /// Splits the name between lowercase and uppercase characters.
        /// </summary>
        /// <returns>The split name.</returns>
        /// <param name="str">String.</param>
        private string SplitName(string str)
        {
            return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
        }
        /// <summary>
        /// Returns the correctly capitalized season
        /// </summary>
        /// <returns>The season, but capitalized correctly.</returns>
        private string Season()
        {
            return char.ToUpper(Game1.currentSeason[0]) + Game1.currentSeason.Substring(1);
        }
        /// <summary>
        /// Returns player rank string based on the Game1.player.Level
        /// </summary>
        /// <returns>The rank.</returns>
        private string PlayerRank()
        {
            //double rank = (Game1.player.FarmingLevel + Game1.player.FishingLevel + Game1.player.ForagingLevel + Game1.player.CombatLevel + Game1.player.MiningLevel + Game1.player.LuckLevel) / 2.0;
            switch (Game1.player.Level)
            {
                case  0: case  1: case  2:
                    return "Newcomer";
                case  3: case  4:
                    return "Greenhorn";
                case  5: case  6:
                    return "Bumpkin";
                case  7: case  8:
                    return "Cowpoke";
                case  9: case 10:
                    return "Farmhand";
                case 11: case 12:
                    return "Tiller";
                case 13: case 14:
                    return "Smallholder";
                case 15: case 16:
                    return "Sodbuster";
                case 17: case 18:
                    if (Game1.player.isMale)
                        return "Farmboy";
                    else return "Farmgirl";
                case 19: case 20:
                    return "Granger";
                case 21: case 22:
                    return "Planter";
                case 23: case 24:
                    return "Rancher";
                case 25: case 26:
                    return "Farmer";
                case 27: case 28:
                    return "Agriculturalist";
                case 29:
                    return "Cropmaster";
                default:
                    return "Farm King";
            }
        }
        /// <summary>
        /// Turns the ingame time into a unix timestamp for the discord API
        /// </summary>
        /// <returns>The game time.</returns>
        private int InGameTime()
        {
            int hours = Game1.timeOfDay % 1000 + Game1.timeOfDay % 10000;
            int mins  = Game1.timeOfDay % 10 + Game1.timeOfDay % 100;
#if UTCOFFSET
            return ((hours * 60 * 60) + (mins * 60) + (DateTimeOffset.Now.Hour * 60 * 60) + (DateTimeOffset.Now.Minute * 60));
#else
            return hours/2 + mins/3;
            //return ((hours * 60 * 60) + (mins * 60));
#endif
        }
    }
}
