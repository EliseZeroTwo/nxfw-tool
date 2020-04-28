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
        public static SelectionListView InfoListView;
        public static MenuBar Menu;
        public static string FwDir;
        public static NcaInfoWindowManager NcaInfoWM;
        public static List<string> NcaNames;
        public static void OpenNewDir()
        {
            FileSelectionWindowManager fileSelection = new FileSelectionWindowManager(InfoWin, FwDir);
            fileSelection.ShowEntryView();
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
                Height = Dim.Fill (),
                ColorScheme = DarkScheme,
            };
            
            Menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("Tools/Settings", new MenuItem [] {
                    new MenuItem ("Open New FW", "", OpenNewDir),
                    new MenuItem ("Extract All", "", null),
                    new MenuItem ("Quit", "", Application.RequestStop),
                }),
            })
            {
                ColorScheme = DarkScheme,
            };


            Top.Add (FirmwareWin);
            Top.Add (InfoWin);
            Top.Add (Menu);
            Application.Run();
        }
        public static void ReloadActiveNcas()
        {
            
            var Top = Application.Top;

            NcaNames = Utils.FirmwareUtils.GetAllNcasAndName(FwDir).Values.ToList();
            NcaNames.Sort();

            
            FirmwareListView = new ListView(NcaNames);
            NcaInfoWM = new NcaInfoWindowManager(FwDir, InfoWin);
            FirmwareListView.SelectedChanged += NcaInfoWM.ShowNcaInfo;

            FirmwareListView.RemoveAll(); 
            FirmwareWin.Add (FirmwareListView);
            Application.Top.SetFocus(FirmwareListView);
        }

    }
}