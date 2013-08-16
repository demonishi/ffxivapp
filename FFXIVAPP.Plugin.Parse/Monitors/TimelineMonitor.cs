﻿// FFXIVAPP.Plugin.Parse
// TimelineMonitor.cs
// 
// © 2013 Ryan Wilson

#region Usings

using System;
using System.Text.RegularExpressions;
using FFXIVAPP.Common.Helpers;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.Parse.Enums;
using FFXIVAPP.Plugin.Parse.Helpers;
using FFXIVAPP.Plugin.Parse.Models;
using FFXIVAPP.Plugin.Parse.Models.Events;
using FFXIVAPP.Plugin.Parse.Models.Fights;
using FFXIVAPP.Plugin.Parse.RegularExpressions;
using NLog;

#endregion

namespace FFXIVAPP.Plugin.Parse.Monitors
{
    public class TimelineMonitor : EventMonitor
    {
        /// <summary>
        /// </summary>
        /// <param name="parseControl"> </param>
        public TimelineMonitor(ParseControl parseControl) : base("Timeline", parseControl)
        {
            Filter = (EventParser.SubjectMask | EventParser.DirectionMask | (UInt32) EventType.Loot | (UInt32) EventType.Defeats);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"> </param>
        protected override void HandleEvent(Event e)
        {
            if (String.IsNullOrWhiteSpace(e.RawLine))
            {
                return;
            }
            switch (e.Type)
            {
                case EventType.Defeats:
                    ProcessDefeated(e);
                    break;
                case EventType.Loot:
                    ProcessLoot(e);
                    break;
                default:
                    //ProcessParty(e);
                    break;
            }
        }

        /// <summary>
        /// </summary>
        private void ProcessDefeated(Event e)
        {
            Match matches;
            var you = Constants.CharacterName;
            switch (Constants.GameLanguage)
            {
                case "French":
                    matches = PlayerRegEx.DefeatsFr.Match(e.RawLine);
                    break;
                case "Japanese":
                    matches = PlayerRegEx.DefeatsJa.Match(e.RawLine);
                    break;
                case "German":
                    matches = PlayerRegEx.DefeatsDe.Match(e.RawLine);
                    break;
                default:
                    matches = PlayerRegEx.DefeatsEn.Match(e.RawLine);
                    break;
            }
            if (!matches.Success)
            {
                ParsingLogHelper.Log(LogManager.GetCurrentClassLogger(), "Defeat", e);
                return;
            }
            var target = matches.Groups["target"];
            var source = matches.Groups["source"];
            if (!target.Success)
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("KillEvent : Got RegEx Match For Monster Defeat; No <target> Capture Group. Line: {0}", e.RawLine));
                return;
            }
            if (ParseControl.Timeline.Party.HasGroup(target.Value) || Regex.IsMatch(target.Value, @"^[Yy]our?$") || target.Value == you)
            {
                return;
            }
            var targetName = StringHelper.TitleCase(target.Value);
            var sourceName = StringHelper.TitleCase(source.Value);
            Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("KillEvent : {0} By : {1}", targetName, sourceName));
            ParseControl.Timeline.PublishTimelineEvent(TimelineEventType.MobKilled, targetName);
        }

        /// <summary>
        /// </summary>
        private void ProcessLoot(Event e)
        {
            Match matches;
            switch (Constants.GameLanguage)
            {
                case "French":
                    matches = PlayerRegEx.ObtainsFr.Match(e.RawLine);
                    break;
                case "Japanese":
                    matches = PlayerRegEx.ObtainsJa.Match(e.RawLine);
                    break;
                case "German":
                    matches = PlayerRegEx.ObtainsDe.Match(e.RawLine);
                    break;
                default:
                    matches = PlayerRegEx.ObtainsEn.Match(e.RawLine);
                    break;
            }
            if (!matches.Success)
            {
                ParsingLogHelper.Log(LogManager.GetCurrentClassLogger(), "Loot", e);
                return;
            }
            if (!ParseControl.Instance.Timeline.FightingRightNow)
            {
                ParsingLogHelper.Log(LogManager.GetCurrentClassLogger(), "Loot.NoKillInLastFiveSeconds", e);
                return;
            }
            var thing = StringHelper.TitleCase(matches.Groups["item"].Value);
            AttachDropToMonster(thing);
        }

        /// <summary>
        /// </summary>
        /// <param name="thing"> </param>
        private void AttachDropToMonster(string thing)
        {
            Fight fight;
            if (ParseControl.Timeline.Fights.TryGet(ParseControl.LastKilled, out fight))
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("DropEvent : {0} Dropped {1}", fight.MobName, thing));
                if (fight.MobName.Replace(" ", "") == "")
                {
                    return;
                }
                var mobGroup = ParseControl.Timeline.GetSetMob(fight.MobName);
                mobGroup.SetDrop(thing);
            }
            else
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("DropEvent : Loot Drop (\"{0}\"), No Current Fight Info. Adding To Last killed.", thing));
                if (ParseControl.LastKilled.Replace(" ", "") == "")
                {
                    return;
                }
                var mobGroup = ParseControl.Timeline.GetSetMob(ParseControl.LastKilled);
                mobGroup.SetDrop(thing);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="line"> </param>
        private void Processparty(string line)
        {
            //var join = Regex.Match("ph", @"^\.$");
            //var disband = Regex.Match("ph", @"^\.$");
            //var left = Regex.Match("ph", @"^\.$");
            //switch (Common.Constants.GameLanguage)
            //{
            //    case "French":
            //        join = PlayerRegEx.JoinFr.Match(line);
            //        disband = PlayerRegEx.DisbandFr.Match(line);
            //        left = PlayerRegEx.LeftFr.Match(line);
            //        break;
            //    case "Japanese":
            //        join = PlayerRegEx.JoinJa.Match(line);
            //        disband = PlayerRegEx.DisbandJa.Match(line);
            //        left = PlayerRegEx.LeftJa.Match(line);
            //        break;
            //    case "German":
            //        join = PlayerRegEx.JoinDe.Match(line);
            //        disband = PlayerRegEx.DisbandDe.Match(line);
            //        left = PlayerRegEx.LeftDe.Match(line);
            //        break;
            //    default:
            //        join = PlayerRegEx.JoinEn.Match(line);
            //        disband = PlayerRegEx.DisbandEn.Match(line);
            //        left = PlayerRegEx.LeftEn.Match(line);
            //        break;
            //}
            //string who;
            //if (join.Success)
            //{
            //    who = @join.Groups["who"].Value;
            //    Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("PartyEvent : Joined {0}", who));
            //    ParseControl.Timeline.PublishTimelineEvent(TimelineEventType.PartyJoin, who);
            //    return;
            //}
            //if (disband.Success)
            //{
            //    Logging.Log(LogManager.GetCurrentClassLogger(), "PartyEvent : Disbanned");
            //    ParseControl.Timeline.PublishTimelineEvent(TimelineEventType.PartyDisband, String.Empty);
            //    return;
            //}
            //if (!left.Success)
            //{
            //    return;
            //}
            //who = left.Groups["who"].Value;
            //Logging.Log(LogManager.GetCurrentClassLogger(), String.Format("PartyEvent : Left {0}", who));
            //ParseControl.Timeline.PublishTimelineEvent(TimelineEventType.PartyLeave, who);
        }
    }
}
