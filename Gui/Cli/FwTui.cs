using nxfw_tool.Firmware;
using nxfw_tool.Gui.Cli;
using nxfw_tool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace nxfw_tool.Gui.Cli
{
    public static class FwTui
    {
        public static ColorScheme DarkScheme;
        public static Window FirmwareWin;
        public static ListView FirmwareListView;
        public static Window InfoWin;
        public static Window LoggerWin;
        public static LoggerWindowManager LoggerWM;
        public static SelectionListView InfoListView;
        public static MenuBar Menu;
        public static string FwDir;
        public static FileSelectionWindowManager FileSelectionWM;
        public static NcaInfoWindowManager NcaInfoWM;
        public static List<string> NcaNames;
        public static void OpenNewDir()
        {
            FileSelectionWM = new FileSelectionWindowManager(InfoWin, FwDir);
            FileSelectionWM.ShowEntryView();
        }

        public static void Init()
        {
            Application.Init ();
            DarkScheme = new ColorScheme() {
                Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                HotFocus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                HotNormal  = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            };
            var Top = Application.Top;
            
            FirmwareWin = new Window ("Firmware") {
                X = 0,
                Y = 1, 
                Width = Dim.Percent(50),
                Height = Dim.Fill (),
                ColorScheme = DarkScheme,
            };
            
            InfoWin = new Window ("Info") {
                X = Pos.Right(FirmwareWin),
                Y = 1,  
                Width = Dim.Fill(),
                Height = Dim.Percent (50),
                ColorScheme = DarkScheme,
            };

            LoggerWin = new Window ("Log") {
                X = Pos.Right(FirmwareWin),
                Y = Pos.Bottom(InfoWin),  
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = DarkScheme,
            };

            LoggerWM = new LoggerWindowManager(LoggerWin);
 
            Menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("Tools", new MenuItem [] {
                    new MenuItem ("Open New FW", "", OpenNewDir),
                    new MenuItem ("Extract All", "", null),
                }),
                new MenuBarItem ("Settings", new MenuItem[] {
                    new MenuItem ("Clear Log", "", LoggerWM.Clear),
                    new MenuItem ("Quit", "", Application.RequestStop),
                })
            })
            {
                ColorScheme = DarkScheme,
            };
            
            Top.Add (FirmwareWin);
            Top.Add (InfoWin);
            Top.Add (LoggerWin);
            Top.Add (Menu);

            ReloadActiveNcas();

            Application.Run();
        }
        public static void ReloadActiveNcas()
        {
            FirmwareWin.RemoveAll();
            var Top = Application.Top;

            NcaNames = Utils.FirmwareUtils.GetAllNcaPathsAndNames(FwDir).Values.ToList();
            NcaNames.Sort();
            
            FirmwareListView = new ListView(NcaNames);
            NcaInfoWM = new NcaInfoWindowManager(FwDir, InfoWin, LoggerWM);
            FirmwareListView.SelectedChanged += NcaInfoWM.ShowNcaInfo;

            
            FirmwareWin.Add (FirmwareListView);
            Application.Top.SetFocus(FirmwareListView);
        }

    }
}