﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace emulatorLauncher
{
    class RuffleGenerator : Generator
    {
        public RuffleGenerator()
        {
            DependsOnDesktopResolution = false;
        }

        public override System.Diagnostics.ProcessStartInfo Generate(string system, string emulator, string core, string rom, string playersControllers, ScreenResolution resolution)
        {
            string path = AppConfig.GetFullPath("ruffle");

            string exe = Path.Combine(path, "ruffle.exe");
            if (!File.Exists(exe))
                return null;

            bool fullscreen = true;

            var commandArray = new List<string>();

            commandArray.Add("--graphics");
            if (SystemConfig.isOptSet("ruffle_renderer") && !string.IsNullOrEmpty(SystemConfig["ruffle_renderer"]))
                commandArray.Add(SystemConfig["ruffle_renderer"]);
            else
                commandArray.Add("default");

            commandArray.Add("--force-align");

            if (SystemConfig.isOptSet("ruffle_gui") && SystemConfig.getOptBoolean("ruffle_gui"))
            {
                commandArray.Add("--width");
                commandArray.Add((resolution == null ? Screen.PrimaryScreen.Bounds.Width : resolution.Width).ToString());
                commandArray.Add("--height");
                commandArray.Add((resolution == null ? Screen.PrimaryScreen.Bounds.Height : resolution.Height).ToString());
                fullscreen = false;
            }
            else
                commandArray.Add("--fullscreen");

            commandArray.Add("-q");

            if (SystemConfig.isOptSet("ruffle_quality") && !string.IsNullOrEmpty(SystemConfig["ruffle_quality"]))
                commandArray.Add(SystemConfig["ruffle_quality"]);
            else
                commandArray.Add("high");

                commandArray.Add("\"" + rom + "\"");

            string args = string.Join(" ", commandArray);

            if (fullscreen)
            {
                return new ProcessStartInfo()
                {
                    FileName = exe,
                    WorkingDirectory = path,
                    Arguments = args,
                };
            }
            else
            {
                return new ProcessStartInfo()
                {
                    FileName = exe,
                    WorkingDirectory = path,
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Maximized,
                };
            }
        }
    }
}