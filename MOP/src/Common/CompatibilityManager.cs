// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using MSCLoader;
using UnityEngine;
using MOP.Items;
using System.Diagnostics;
using System.Linq;
using System;
using System.IO;

namespace MOP.Common
{
    static class CompatibilityManager
    {
        // This script manages the compatibility with other mods
       
        // Use this method of ensuring compatibility ONLY in specific cases,
        // that are not supported by Rule Files.

        // CarryMore
        // https://www.racedepartment.com/downloads/carry-more-backpack-alternative.22396/
        static bool CarryMore { get; set; }
        
        // Advanced Backpack
        static bool AdvancedBackpack { get; set; }
        static readonly Vector3 AdvancedBackpackPosition = new Vector3(630f, 10f, 1140f);
        const int AdvancedBackpackDistance = 30;

        // WreckMP Multiplayer
        static bool WreckMPActive { get; set; }

        static readonly string[] incompatibleMods = { "KruFPS", "ImproveFPS", "OptimizeMSC", "ZZDisableAll", "DisableAll" };

        public static void Initialize()
        {
            // CarryMore = MSCLoader.ModLoader.Mods.Any(m => m.ID == "CarryMore");
            // AdvancedBackpack = MSCLoader.ModLoader.Mods.Any(m => m.ID == "AdvancedBackpack");
            CarryMore = false;
            AdvancedBackpack = false;
            
            // Detect WreckMP multiplayer mod
            WreckMPActive = DetectWreckMP();
        }

        public static bool IsInBackpack(ItemBehaviour behaviour)
        {
            if (CarryMore)
            {
                return behaviour.transform.position.y < -900;
            }
            else if (AdvancedBackpack)
            {
                return Vector3.Distance(behaviour.transform.position, AdvancedBackpackPosition) < AdvancedBackpackDistance;
            }

            return false;
        }

        /// <summary>
        /// Detects if WreckMP multiplayer mod is active
        /// </summary>
        /// <returns>True if WreckMP is detected, false otherwise</returns>
        private static bool DetectWreckMP()
        {
            try
            {
                // Method 1: Check for WreckMP GameObjects in scene
                if (GameObject.Find("WreckMP") != null || 
                    GameObject.Find("WreckMP_Manager") != null ||
                    GameObject.Find("WreckMP_Network") != null)
                {
#if DEBUG
                    ModConsole.Log("[MOP] WreckMP detected via GameObject check");
#endif
                    return true;
                }

                // Method 2: Check for WreckMP processes
                var processes = Process.GetProcessesByName("WreckMPLauncher");
                if (processes.Length > 0)
                {
#if DEBUG
                    ModConsole.Log("[MOP] WreckMP detected via process check");
#endif
                    return true;
                }

                // Method 3: Check for common WreckMP files
                string gamePath = Application.dataPath.Replace("My Summer Car_Data", "");
                string wreckmpDir = Path.Combine(gamePath, "WreckMP");
                string modsDir = Path.Combine(gamePath, "Mods");
                string[] wreckmpPaths = {
                    Path.Combine(gamePath, "WreckMP.dll"),
                    Path.Combine(wreckmpDir, "WreckMP.dll"),
                    Path.Combine(modsDir, "WreckMP")
                };

                foreach (string path in wreckmpPaths)
                {
                    if (File.Exists(path) || Directory.Exists(path))
                    {
#if DEBUG
                        ModConsole.Log("[MOP] WreckMP detected via file check");
#endif
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ModConsole.LogError($"[MOP] Error detecting WreckMP: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if WreckMP multiplayer mod is active
        /// </summary>
        public static bool IsWreckMPActive => WreckMPActive;

        public static bool IsConfilctingModPresent(out string conflictingModName)
        {
            conflictingModName = "";
            return false;
            /*
            foreach (string a in incompatibleMods)
            {
                var mod = MSCLoader.ModLoader.Mods.FirstOrDefault(m => m.ID == a);
                if (mod != null)
                {
                    conflictingModName = mod.Name;
                    return true;
                }
            }

            conflictingModName = "";
            return false;
            */
        }

        public static bool IsMySummerCar => Application.productName == "My Summer Car";

#if PRO
        // Compatibility layer between MSCLoader's and Mod Loader Pro settings.
        public static int GetValue(this SettingSlider slider)
        {
            return (int)slider.Value;
        }

        public static bool GetValue(this SettingToggle toggle)
        {
            return toggle.Value;
        }


        public static void AddTooltip(this ModSetting setting, string text)
        {
            setting.gameObject.AddComponent<UITooltip>().toolTipText = text;
        }

        public static void SetValue(this SettingText settingText, string text)
        {
            settingText.Enabled = !string.IsNullOrEmpty(text);
            settingText.Text = text;
        }
#endif

        public static bool IsMSCLoader()
        {
            return GameObject.Find("MSCLoader Canvas menu") != null;
        }

        public static bool IsModLoaderPro()
        {
            return GameObject.Find("MSCLoader Canvas menu") == null;
        }
    }
}
