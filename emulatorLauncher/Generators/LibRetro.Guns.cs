﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using emulatorLauncher.Tools;

namespace emulatorLauncher.libRetro
{
    partial class LibRetroGenerator : Generator
    {
        static List<string> coreGunConfig = new List<string>() 
        { 
            "mednafen_psx", 
            "mednafen_psx_hw" 
        };

        /// <summary>
        /// Injects guns settings
        /// </summary>
        /// <param name="retroarchConfig"></param>
        /// <param name="deviceType"></param>
        /// <param name="playerIndex"></param>
        private void SetupLightGuns(ConfigFile retroarchConfig, string deviceType, string core, int playerIndex = 1)
        {
            if (!SystemConfig.getOptBoolean("use_guns"))
                return;

            bool multigun = false;
            bool guninvert = SystemConfig.isOptSet("gun_invert") && SystemConfig.getOptBoolean("gun_invert");

            int gunCount = RawLightgun.GetUsableLightGunCount();
            var guns = RawLightgun.GetRawLightguns();
            if (gunCount > 1 && guns.Length > 1 && playerIndex == 1)
                multigun = true;

            if (!multigun)
            {
                // Set mouse buttons for one player (default mapping)
                retroarchConfig["input_libretro_device_p" + playerIndex] = deviceType;
                retroarchConfig["input_player" + playerIndex + "_mouse_index"] = "0";
                retroarchConfig["input_player" + playerIndex + "_gun_trigger_mbtn"] = "1";
                retroarchConfig["input_player" + playerIndex + "_gun_offscreen_shot_mbtn"] = "2";
                retroarchConfig["input_player" + playerIndex + "_gun_start_mbtn"] = "3";

                retroarchConfig["input_player" + playerIndex + "_analog_dpad_mode"] = "0";
                retroarchConfig["input_player" + playerIndex + "_joypad_index"] = "0";
            }
            else
            {
                // DirectInput does not differenciate mouse indexes. We have to use "Raw" with multiple guns
                retroarchConfig["input_driver"] = "raw";

                // Set mouse buttons for multigun
                for (int i = 1; i <= guns.Length; i++)
                {
                    int deviceIndex = guns[i - 1].Index; // i-1;

                    SimpleLogger.Instance.Debug("[LightGun] Assigned player " + i + " to -> " + guns[i - 1].ToString());

                    retroarchConfig["input_libretro_device_p" + i] = deviceType;
                    retroarchConfig["input_player" + i + "_mouse_index"] = deviceIndex.ToString();
                    retroarchConfig["input_player" + i + "_gun_trigger_mbtn"] = "1";
                    retroarchConfig["input_player" + i + "_gun_offscreen_shot_mbtn"] = "2";
                    retroarchConfig["input_player" + i + "_gun_start_mbtn"] = "3";

                    retroarchConfig["input_player" + i + "_analog_dpad_mode"] = "0";
                    retroarchConfig["input_player" + i + "_joypad_index"] = deviceIndex.ToString();
                }
            }

            if (guns.Length <= 16)
            {
                for (int i = guns.Length + 1; i == 16; i++)
                {
                    foreach (string cfg in gunButtons)
                        retroarchConfig["input_player" + i + cfg] = "nul";
                }
            }

            // Set additional buttons gun mapping default
            ConfigureLightgunKeyboardActions(retroarchConfig, deviceType, playerIndex, core, multigun, guninvert);
        }

        /// <summary>
        /// Injects keyboard actions for lightgun games
        /// </summary>
        /// <param name="retroarchConfig"></param>
        /// <param name="playerId"></param>
        private void ConfigureLightgunKeyboardActions(ConfigFile retroarchConfig, string deviceType, int playerIndex, string core, bool multigun, bool guninvert)
        {
            if (!SystemConfig.getOptBoolean("use_guns"))
                return;

            if (!coreGunConfig.Contains(core))
            {
                var keyb = Controllers.Where(c => c.Name == "Keyboard" && c.Config != null && c.Config.Input != null).Select(c => c.Config).FirstOrDefault();
                if (keyb != null)
                {
                    var start = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.start);
                    retroarchConfig["input_player" + playerIndex + "_gun_start"] = start == null ? "nul" : LibretroControllers.GetConfigValue(start);

                    var select = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.select);
                    retroarchConfig["input_player" + playerIndex + "_gun_select"] = select == null ? "nul" : LibretroControllers.GetConfigValue(select);

                    var aux_a = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.b);
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_a"] = aux_a == null ? "nul" : LibretroControllers.GetConfigValue(aux_a);

                    var aux_b = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.a);
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_b"] = aux_b == null ? "nul" : LibretroControllers.GetConfigValue(aux_b);

                    var aux_c = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.y);
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_c"] = aux_c == null ? "nul" : LibretroControllers.GetConfigValue(aux_c);

                    var dpad_up = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.up);
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_up"] = dpad_up == null ? "nul" : LibretroControllers.GetConfigValue(dpad_up);

                    var dpad_down = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.down);
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_down"] = dpad_down == null ? "nul" : LibretroControllers.GetConfigValue(dpad_down);

                    var dpad_left = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.left);
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_left"] = dpad_left == null ? "nul" : LibretroControllers.GetConfigValue(dpad_left);

                    var dpad_right = keyb.Input.FirstOrDefault(i => i.Name == Tools.InputKey.right);
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_right"] = dpad_right == null ? "nul" : LibretroControllers.GetConfigValue(dpad_right);
                }
                else
                {
                    retroarchConfig["input_player" + playerIndex + "_gun_start"] = "enter";
                    retroarchConfig["input_player" + playerIndex + "_gun_select"] = "backspace";
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_a"] = "w";
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_b"] = "x";
                    retroarchConfig["input_player" + playerIndex + "_gun_aux_c"] = "s";
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_up"] = "up";
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_down"] = "down";
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_left"] = "left";
                    retroarchConfig["input_player" + playerIndex + "_gun_dpad_right"] = "right";
                }
            }
            
            // Configure core specific mappings            
            else
            {
                ConfigureGunsMednafenPSX(retroarchConfig, playerIndex, core, deviceType, multigun, guninvert);
            }
        }

        // Mednafen psx mapping
        // GunCon buttons : trigger, A, B (offscreen reload)

        private void ConfigureGunsMednafenPSX(ConfigFile retroarchConfig, int playerIndex, string core, string deviceType, bool multigun = false, bool guninvert = false)
        {
            if (core != "mednafen_psx_hw" && core != "mednafen_psx")
                return;

            if (SystemConfig.isOptSet("gun_type") && !string.IsNullOrEmpty(SystemConfig["gun_type"]))
                deviceType = SystemConfig["gun_type"];
            else
                deviceType = "260";

            var guns = RawLightgun.GetRawLightguns();
            if (guns.Length == 0)
                return;

            for (int i = 1; i <= guns.Length; i++)
            {
                int deviceIndex = guns[i - 1].Index; // i-1;

                SimpleLogger.Instance.Debug("[LightGun] Assigned player " + i + " to -> " + guns[i - 1].ToString());

                retroarchConfig["input_libretro_device_p" + i] = deviceType;
                retroarchConfig["input_player" + i + "_mouse_index"] = deviceIndex.ToString();

                if (SystemConfig.isOptSet("mednafen_reload") && SystemConfig.getOptBoolean("mednafen_reload"))
                {
                    retroarchConfig["input_player" + i + "_gun_trigger_mbtn"] = guninvert ? "2" : "1";
                    retroarchConfig["input_player" + i + "_gun_offscreen_shot_mbtn"] = guninvert ? "1" : "2";
                    retroarchConfig["input_player" + i + "_gun_aux_a_mbtn"] = "3";
                    retroarchConfig["input_player" + i + "_gun_aux_b_mbtn"] = "nul";
                }
                else
                {
                    retroarchConfig["input_player" + i + "_gun_trigger_mbtn"] = guninvert ? "2" : "1";
                    retroarchConfig["input_player" + i + "_gun_offscreen_shot_mbtn"] = "nul";
                    retroarchConfig["input_player" + i + "_gun_aux_a_mbtn"] = guninvert ? "1" : "2";
                    retroarchConfig["input_player" + i + "_gun_aux_b_mbtn"] = "3";
                }

                retroarchConfig["input_player" + i + "_gun_start_mbtn"] = "nul";
                retroarchConfig["input_player" + i + "_analog_dpad_mode"] = "0";
                retroarchConfig["input_player" + i + "_joypad_index"] = deviceIndex.ToString();

                if (i == 1)
                {
                    retroarchConfig["input_player" + i + "_gun_start"] = "enter";
                    retroarchConfig["input_player" + i + "_gun_select"] = "backspace";

                    if (SystemConfig.isOptSet("mednafen_gun_ab") && SystemConfig["mednafen_gun_ab"] == "directions")
                    {
                        retroarchConfig["input_player1_gun_aux_a"] = "left";
                        retroarchConfig["input_player1_gun_aux_b"] = "right";
                        retroarchConfig["input_player1_gun_aux_c"] = "up";
                    }
                    else
                    {
                        var keyb = Controllers.Where(c => c.Name == "Keyboard" && c.Config != null && c.Config.Input != null).Select(c => c.Config).FirstOrDefault();
                        if (keyb != null)
                        {
                            var aux_a = keyb.Input.FirstOrDefault(k => k.Name == Tools.InputKey.b);
                            retroarchConfig["input_player1_gun_aux_a"] = aux_a == null ? "nul" : LibretroControllers.GetConfigValue(aux_a);

                            var aux_b = keyb.Input.FirstOrDefault(k => k.Name == Tools.InputKey.a);
                            retroarchConfig["input_player1_gun_aux_b"] = aux_b == null ? "nul" : LibretroControllers.GetConfigValue(aux_b);

                            var aux_c = keyb.Input.FirstOrDefault(k => k.Name == Tools.InputKey.y);
                            retroarchConfig["input_player" + playerIndex + "_gun_aux_c"] = aux_c == null ? "nul" : LibretroControllers.GetConfigValue(aux_c);
                        }
                        else
                        {
                            retroarchConfig["input_player1_gun_aux_a"] = "w";
                            retroarchConfig["input_player1_gun_aux_b"] = "x";
                            retroarchConfig["input_player1_gun_aux_b"] = "s";
                        }
                    }
                }
            }
        }
        
        static List<string> gunButtons = new List<string>()
        {
            "_mouse_index",
            "_gun_aux_a",
            "_gun_aux_a_axis",
            "_gun_aux_a_btn",
            "_gun_aux_a_mbtn",
            "_gun_aux_b",
            "_gun_aux_b_axis",
            "_gun_aux_b_btn",
            "_gun_aux_b_mbtn",
            "_gun_aux_c",
            "_gun_aux_c_axis",
            "_gun_aux_c_btn",
            "_gun_aux_c_mbtn",
            "_gun_dpad_down",
            "_gun_dpad_down_axis",
            "_gun_dpad_down_btn",
            "_gun_dpad_down_mbtn",
            "_gun_dpad_left",
            "_gun_dpad_left_axis",
            "_gun_dpad_left_btn",
            "_gun_dpad_left_mbtn",
            "_gun_dpad_right",
            "_gun_dpad_right_axis",
            "_gun_dpad_right_btn",
            "_gun_dpad_right_mbtn",
            "_gun_dpad_up",
            "_gun_dpad_up_axis",
            "_gun_dpad_up_btn",
            "_gun_dpad_up_mbtn",
            "_gun_offscreen_shot",
            "_gun_offscreen_shot_axis",
            "_gun_offscreen_shot_btn",
            "_gun_offscreen_shot_mbtn",
            "_gun_select",
            "_gun_select_axis",
            "_gun_select_btn",
            "_gun_select_mbtn",
            "_gun_start",
            "_gun_start_axis",
            "_gun_start_btn",
            "_gun_start_mbtn",
            "_gun_trigger",
            "_gun_trigger_axis",
            "_gun_trigger_btn",
            "_gun_trigger_mbtn"
        };

    }
}