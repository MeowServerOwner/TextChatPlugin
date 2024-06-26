﻿using Exiled.API.Features;
using HintServiceMeow;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TextChatMeow
{
    internal class PlayerMessageHandler
    {
        private static List<PlayerMessageHandler> messagesManagers = new List<PlayerMessageHandler>();

        private static CoroutineHandle AutoUpdateCoroutine = Timing.RunCoroutine(AutoUpdateMethod());

        private Player player;

        private DateTime timeCreated;

        private HintServiceMeow.Hint TextChatTip = new HintServiceMeow.Hint(580, HintAlignment.Left, Plugin.instance.Config.ChatTip);
        private List<HintServiceMeow.Hint> MessageSlots = new List<HintServiceMeow.Hint>()
        {
            new HintServiceMeow.Hint(600, HintAlignment.Left, ""),
            new HintServiceMeow.Hint(620, HintAlignment.Left, ""),
            new HintServiceMeow.Hint(640, HintAlignment.Left, ""),
        };

        public PlayerMessageHandler(PlayerDisplay playerDisplay)
        {
            this.player = playerDisplay.player;

            //Add hint onto player display
            playerDisplay.AddHint(TextChatTip);
            playerDisplay.AddHints(MessageSlots);

            timeCreated = DateTime.Now;

            messagesManagers.Add(this);
        }

        public static void RemoveMessageManager(Player player)
        {
            messagesManagers.RemoveAll(x => x.player == player);
        }

        public void UpdateMessage()
        {
            //Get all the message that should be display
            List<ChatMessage> displayableMessages = new List<ChatMessage>();

            try
            {
                displayableMessages = MessagesList
                    .messageList
                    .Where(x => x.CanSee(this.player))
                    .ToList();
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }

            //Update the message onto player's screen
            try
            {
                foreach (HintServiceMeow.Hint hint in MessageSlots)
                {
                    hint.hide = true;
                }

                for (var i = 0; i < MessageSlots.Count() && i < displayableMessages.Count(); i++)
                {
                    ChatMessage message = displayableMessages[i];

                    string text = string.Empty;

                    if (Plugin.instance.Config.AddCountDown && Plugin.instance.Config.MessagesDisappears)
                    {
                        int countdown = Plugin.instance.Config.MessagesHideTime - (int)(DateTime.Now - message.TimeSent).TotalSeconds;
                        text += $"[{countdown}]";//Add countdown in front of the message (if enabled
                    }
                        

                    text += message.text;

                    text += new string(' ', message.TimeSent.Second % 10);

                    MessageSlots[i].message = text;
                    MessageSlots[i].hide = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            //Set tip's visibility based on the message's visibility
            try
            {
                TimeSpan tipTimeToDisplay = TimeSpan.FromSeconds(Plugin.instance.Config.TipDisplayTime);

                if (Plugin.instance.Config.TipDisappears == false||MessageSlots.Any(x => !x.hide) || timeCreated + tipTimeToDisplay >= DateTime.Now)
                {
                    TextChatTip.hide = false;
                }
                else
                {
                    TextChatTip.hide = true;
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static IEnumerator<float> AutoUpdateMethod()
        {
            while (true)
            {
                try
                {
                    foreach (var manager in messagesManagers)
                    {
                        manager.UpdateMessage();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                yield return Timing.WaitForSeconds(0.1f);
            }
        }
    }
}
