#define EHTRUE
// ^ allows exception handling
using System;
using System.Collections.Generic;
using System.Linq;
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
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            var Activity = new Discord.Activity
            {
                State = $"Stardew Valley!",
                Details = $"Main Menu",
                Timestamps =
                {
                    Start = 0,
                },
                Assets =
                {
                    LargeImage = "",
                    LargeText = "",
                    SmallImage = "",
                    SmallText = "",
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
                    Match = "",
                    Join = "foo joinSecret",
                    Spectate = "foo spectateSecret",
                },
                            Instance = true,
            };

            if (Game1.IsMultiplayer)
            {
                this.Monitor.Log($"Multiplayer Mode", LogLevel.Info);
            }
            var imageManager = discord.GetImageManager();
            var userManager = discord.GetUserManager();
            var lobbyManager = discord.GetLobbyManager(); 
            var applicationManager = discord.GetApplicationManager();
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
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
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
            this.Monitor.Log($"{Game1.player.Name} on {Game1.player.farmName} farm is on day {Game1.dayOfMonth} of {Game1.currentSeason}", LogLevel.Debug);
        }
        private void Update(object sender, UpdateTickedEventArgs e, Discord.Discord discord, Discord.Activity activity)
        {
            if (!Context.IsWorldReady)
                return;
            var activityManager = discord.GetActivityManager();
            try
            {
                if (e.IsMultipleOf(30))
                {
                    activityManager.UpdateActivity(CheckActivity(activity), (result) =>
                    {
                        if (result != Discord.Result.Ok)
                            Monitor.Log("Update Activity: " + result);
                    });

                    discord.RunCallbacks();
                }
            }
            catch { }
#if false
            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Discord.Result.Ok)
                {
#if DEBUG
                    this.Monitor.Log($"Activity Update Success!", LogLevel.Debug);
#endif
                }
                else
                {
                    this.Monitor.Log($"Activity Update Failed", LogLevel.Error);
                }
            });
#if EHTRUE
            try
            {
                discord.RunCallbacks();
            } catch (Discord.ResultException res)
            {
                this.Monitor.Log($"Error! {res}", LogLevel.Error);
            } catch (System.NullReferenceException)
            {
                this.Monitor.Log($"Discord DLL not found, null reference exception", LogLevel.Error);
            } catch (System.AccessViolationException acc)
            { 
                this.Monitor.Log($"Error! {acc}", LogLevel.Error);
            }
#else
            discord.RunCallbacks();
#endif
#endif
            }
        private Discord.Activity CheckActivity(Discord.Activity a)
        {
            if (Context.IsMultiplayer)
            {
                a.Party.Id = Game1.MasterPlayer.UniqueMultiplayerID.ToString();
                a.Party.Size.CurrentSize = Game1.numberOfPlayers();
                a.Party.Size.MaxSize = Game1.getFarm().getNumberBuildingsConstructed("Cabin") + 1;
                a.Secrets.Join = Game1.server.getInviteCode();
            }
            a.State = $"On {Game1.player.farmName} farm{(Game1.player.hasPet() ? " with " + Game1.player.getPetDisplayName() : ".")}";
            a.Details = $"{Game1.currentLocation.Name} - Day {Game1.dayOfMonth} of {char.ToUpper(Game1.currentSeason[0]) + Game1.currentSeason.Substring(1)}, Rank {Game1.player.Level}";
            a.Assets.LargeImage = $"{Game1.currentSeason}_{Farmlayout_type()}";
            a.Assets.SmallImage = "weather_" + Weather_type();
            a.Timestamps.Start = Game1.timeOfDay;
            return a;
        }
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
    }
}
