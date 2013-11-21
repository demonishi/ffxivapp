﻿// FFXIVAPP.Client
// PlayerInfoWorker.cs
// 
// © 2013 Ryan Wilson

#region Usings

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using FFXIVAPP.Client.Helpers;
using FFXIVAPP.Common.Core.Memory;
using FFXIVAPP.Common.Utilities;
using NLog;
using SmartAssembly.Attributes;

#endregion

namespace FFXIVAPP.Client.Memory
{
    [DoNotObfuscate]
    internal class PlayerInfoWorker : INotifyPropertyChanged, IDisposable
    {
        #region Property Bindings

        private uint PlayerInfoMap { get; set; }

        #endregion

        #region Declarations

        private static readonly Logger Tracer = LogManager.GetCurrentClassLogger();
        private readonly Timer _scanTimer;
        private bool _isScanning;

        #endregion

        public PlayerInfoWorker()
        {
            _scanTimer = new Timer(1000);
            _scanTimer.Elapsed += ScanTimerElapsed;
        }

        #region Timer Controls

        /// <summary>
        /// </summary>
        public void StartScanning()
        {
            _scanTimer.Enabled = true;
        }

        /// <summary>
        /// </summary>
        public void StopScanning()
        {
            _scanTimer.Enabled = false;
        }

        #endregion

        #region Threads

        /// <summary>
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void ScanTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isScanning)
            {
                return;
            }
            _isScanning = true;
            Func<bool> scannerWorker = delegate
            {
                if (MemoryHandler.Instance.SigScanner.Locations.ContainsKey("CHARMAP"))
                {
                    if (MemoryHandler.Instance.SigScanner.Locations.ContainsKey("PLAYERINFO"))
                    {
                        PlayerInfoMap = MemoryHandler.Instance.SigScanner.Locations["PLAYERINFO"];
                        if (PlayerInfoMap <= 6496)
                        {
                            return false;
                        }
                        try
                        {
                            var playerInfo = MemoryHandler.Instance.GetStructure<Structures.PlayerInfo>(PlayerInfoMap);
                            var playerEntity = new PlayerEntity
                            {
                                Name = MemoryHandler.Instance.GetString(PlayerInfoMap, 1),
                                Accuracy = playerInfo.Accuracy,
                                ACN_CurrentEXP = playerInfo.ACN_CurrentEXP,
                                ALC = playerInfo.ALC,
                                ARC = playerInfo.ARC,
                                ARC_CurrentEXP = playerInfo.ARC_CurrentEXP,
                                ARM = playerInfo.ARM,
                                ARM_CurrentEXP = playerInfo.ARM_CurrentEXP,
                                AttackMagicPotency = playerInfo.AttackMagicPotency,
                                AttackPower = playerInfo.AttackPower,
                                BOT = playerInfo.BOT,
                                BOT_CurrentEXP = playerInfo.BOT_CurrentEXP,
                                BSM = playerInfo.BSM,
                                BSM_CurrentEXP = playerInfo.BSM_CurrentEXP,
                                BaseDexterity = playerInfo.BaseDexterity,
                                BaseIntelligence = playerInfo.BaseIntelligence,
                                BaseMind = playerInfo.BaseMind,
                                BasePiety = playerInfo.BasePiety,
                                BaseStrength = playerInfo.BaseStrength,
                                BaseVitality = playerInfo.BaseVitality,
                                CNJ = playerInfo.CNJ,
                                CNJ_CurrentEXP = playerInfo.CNJ_CurrentEXP,
                                CPMax = playerInfo.CPMax,
                                CPT = playerInfo.CPT,
                                CPT_CurrentEXP = playerInfo.CPT_CurrentEXP,
                                CUL = playerInfo.CUL,
                                CUL_CurrentEXP = playerInfo.CUL_CurrentEXP,
                                Control = playerInfo.Control,
                                Craftmanship = playerInfo.Craftmanship,
                                CriticalHitRate = playerInfo.CriticalHitRate,
                                Defense = playerInfo.Defense,
                                Determination = playerInfo.Determination,
                                Dexterity = playerInfo.Dexterity,
                                FSH = playerInfo.FSH,
                                FSH_CurrentEXP = playerInfo.FSH_CurrentEXP,
                                FireResistance = playerInfo.FireResistance,
                                GLD = playerInfo.GLD,
                                GLD_CurrentEXP = playerInfo.GLD_CurrentEXP,
                                GPMax = playerInfo.GPMax,
                                GSM = playerInfo.GSM,
                                GSM_CurrentEXP = playerInfo.GSM_CurrentEXP,
                                Gathering = playerInfo.Gathering,
                                HPMax = playerInfo.HPMax,
                                HealingMagicPotency = playerInfo.HealingMagicPotency,
                                IceResistance = playerInfo.IceResistance,
                                Intelligence = playerInfo.Intelligence,
                                JobID = playerInfo.JobID,
                                LNC = playerInfo.LNC,
                                LNC_CurrentEXP = playerInfo.LNC_CurrentEXP,
                                LTW = playerInfo.LTW,
                                LTW_CurrentEXP = playerInfo.LTW_CurrentEXP,
                                LightningResistance = playerInfo.LightningResistance,
                                MIN = playerInfo.MIN,
                                MIN_CurrentEXP = playerInfo.MIN_CurrentEXP,
                                MPMax = playerInfo.MPMax,
                                MRD = playerInfo.MRD,
                                MRD_CurrentEXP = playerInfo.MRD_CurrentEXP,
                                MagicDefense = playerInfo.MagicDefense,
                                Mind = playerInfo.Mind,
                                PGL = playerInfo.PGL,
                                PGL_CurrentEXP = playerInfo.PGL_CurrentEXP,
                                Parry = playerInfo.Parry,
                                Perception = playerInfo.Perception,
                                PiercingResistance = playerInfo.PiercingResistance,
                                Piety = playerInfo.Piety,
                                SkillSpeed = playerInfo.SkillSpeed,
                                SlashingResistance = playerInfo.SlashingResistance,
                                SpellSpeed = playerInfo.SpellSpeed,
                                Strength = playerInfo.Strength,
                                THM = playerInfo.THM,
                                THM_CurrentEXP = playerInfo.THM_CurrentEXP,
                                TPMax = playerInfo.TPMax,
                                Vitality = playerInfo.Vitality,
                                WVR = playerInfo.WVR,
                                WVR_CurrentEXP = playerInfo.WVR_CurrentEXP,
                                WaterResistance = playerInfo.WaterResistance,
                                WindResistance = playerInfo.WindResistance
                            };
                            ApplicationContextHelper.GetContext()
                                                    .PlayerInfoWorker.RaiseEntityEvent(playerEntity);
                        }
                        catch (Exception ex)
                        {
                            Logging.Log(LogManager.GetCurrentClassLogger(), "", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            PlayerInfoMap = MemoryHandler.Instance.SigScanner.Locations["CHARMAP"] + 5716;
                            MemoryHandler.Instance.SigScanner.Locations.Add("PLAYERINFO", PlayerInfoMap);
                        }
                        catch (Exception ex)
                        {
                            Logging.Log(LogManager.GetCurrentClassLogger(), "", ex);
                        }
                    }
                }
                _isScanning = false;
                return true;
            };
            scannerWorker.BeginInvoke(delegate { }, scannerWorker);
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            _scanTimer.Elapsed -= ScanTimerElapsed;
        }

        #endregion
    }
}