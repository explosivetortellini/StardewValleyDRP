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
            var discord = new Discord.Discord(824160913908039729, (UInt64)Discord.CreateFlags.NoRequireDiscord);
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.DayStarted += this.NewDay;
            helper.Events.GameLoop.OneSecondUpdateTicked += (sender, e) => this.Update(sender, e, discord);
            if (Game1.IsMultiplayer)
                this.Monitor.Log($"Multiplayer Mode", LogLevel.Info);
            var userManager = discord.GetUserManager();

            // Return via callback
            userManager.GetUser(290926444748734465, (Discord.Result result, ref Discord.User otherUser) =>
            {
                if (result == Discord.Result.Ok)
                {
                    this.Monitor.Log($"Username: {otherUser.Username}", LogLevel.Debug);
                    this.Monitor.Log($"ID: {otherUser.Id}", LogLevel.Debug);
                }
            });
            // Return normally
            userManager.OnCurrentUserUpdate += () =>
            {
                var currentUser = userManager.GetCurrentUser();
                this.Monitor.Log($"Username: {currentUser.Username}", LogLevel.Debug);
                this.Monitor.Log($"Discriminator: {currentUser.Discriminator}", LogLevel.Debug);
                this.Monitor.Log($"ID: {currentUser.Id}", LogLevel.Debug);
            };
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
            this.Monitor.Log($"{Game1.player.Name} on {Game1.player.farmName} farm is on day {Game1.dayOfMonth} of {Game1.currentSeason}", LogLevel.Info);
        }
        private void Update(object sender, OneSecondUpdateTickedEventArgs e, Discord.Discord discord)
        {
            if (!Context.IsWorldReady)
                return;
#if EHTRUE
            try
            {
                discord.RunCallbacks();
            } catch (System.NullReferenceException)
            {
                this.Monitor.Log($"Discord DLL not found, null reference exception", LogLevel.Error);
            }
#else
            discord.RunCallbacks();
#endif
        }
    }
}
