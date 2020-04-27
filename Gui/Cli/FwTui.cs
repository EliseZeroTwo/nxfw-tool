using nxfw_tool.Firmware;
using nxfw_tool.Gui.Cli;
using nxfw_tool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Terminal.Gui;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;

namespace nxfw_tool.Gui.Cli
{
    public static class FwTui
    {
        static ColorScheme DarkScheme = new ColorScheme(){
            Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            HotFocus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            HotNormal  = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
        };
        
        static Window FirmwareWin;
        static ListView FirmwareListView;
        static Window InfoWin;
        public static SelectionListView InfoListView;
        static MenuBar Menu;
        static public string FwDir;
        static List<string> NcaNames;
        static List<string> DirEntryNames;

        static TextField PathTextField;
        public static void SelectFileCallback()
        {
            FwDir = PathTextField.Text.ToString();
            FirmwareListView.RemoveAll();
            InfoWin.RemoveAll();
            ReloadActiveNcas();
        }
        public static void FilePickerCallback()
        {
            string opt = DirEntryNames[InfoListView.SelectedItem];
            
            if (opt == "Choose This Directory")
            {
                FwDir = PathTextField.Text.ToString();
                ReloadActiveNcas();
                return;
            }
            else if (DirEntryNames[InfoListView.SelectedItem] == "..")
            {
                if (PathTextField.Text.Last() == '/')
                    PathTextField.Text = PathTextField.Text.ToString().Remove(PathTextField.Text.Length - 1);
                
                PathTextField.Text = PathTextField.Text.Replace(PathTextField.Text.Split("/").Last(), "");
            }
            else
            {
                PathTextField.Text = opt + "/";
            }
            FilePicker();
        }
        public static void FilePicker()
        {
            InfoWin.RemoveAll();
            
            DirEntryNames = new List<string>();

            foreach(var name in Directory.EnumerateDirectories(PathTextField.Text.ToString(), "*"))
            {
                DirEntryNames.Add(name);
            }

            DirEntryNames.Sort();
            DirEntryNames.Insert(0, "Choose This Directory");
            DirEntryNames.Insert(1 ,"..");

            
            InfoListView = new SelectionListView(DirEntryNames);
            InfoWin.Add(InfoListView);
            Application.Top.SetFocus(InfoListView);

           
        }

        public static void SelectFile()
        {
            InfoWin.RemoveAll();
            
            var Path = new Label ("New Path: ") { X = 1, Y = 1 };
            PathTextField = new TextField (FwDir) {
                X = Pos.Right (Path),
                Y = 1,
                Width = 40,
                Text = FwDir,
            };

            InfoWin.Add(Path);
            InfoWin.Add(PathTextField);

            var OkButton = new Button ("Ok") { X = 1, Y = 3 };
            OkButton.Clicked += SelectFileCallback;
            var CancelButton = new Button ("Cancel") { X = Pos.Right(OkButton), Y = 3 };
            CancelButton.Clicked += UpdateNcaInfo;
            var BrowseButton = new Button ("Browse") { X = Pos.Right(CancelButton), Y = 3 };
            BrowseButton.Clicked += FilePicker;
            InfoWin.Add(OkButton);
            InfoWin.Add(CancelButton);
            InfoWin.Add(BrowseButton);
            Application.Top.SetFocus(OkButton);

        }

        public static void CreateNand()
        {
            FirmwareInfo nandBuilder = new FirmwareInfo(FwDir);
        }

        public static void UpdateNcaInfo()
        {
            InfoWin.RemoveAll();
            string NcaPath = (string)Utils.FirmwareUtils.OpenNcaStorageByTitleName(FwDir, NcaNames[FirmwareListView.SelectedItem], true);
            NcaInfo ncaInfo = new NcaInfo(new LocalStorage(NcaPath, FileAccess.Read));
            
            List<string> NcaInfoLines = new List<string>();

            string FormattedTid = $"{ncaInfo.Nca.Header.TitleId:X16}";
            string FormattedName = $"{ncaInfo.TitleName}";

            FirmwareInfo fwInfo = new FirmwareInfo(FwDir);
            NcaInfoLines.Add($"Title ID: {FormattedTid}");

            if(FormattedName != FormattedTid)
            {
                NcaInfoLines.Add($"Title Name: {FormattedName}");
            }

            NcaInfoLines.Add($"Firmware Version: {fwInfo.VersionInfo.Major:d}.{fwInfo.VersionInfo.Minor:d}.{fwInfo.VersionInfo.Micro:d}");

            NcaInfoLines.Add($"Content Type: {ncaInfo.Nca.Header.ContentType}");
            
            string ncaID = NcaPath.Split('/').Last();
            if (ncaID == "00.nca")
                ncaID = NcaPath.Split('/')[NcaPath.Split('/').Length - 2];
            else
                ncaID = ncaID.Replace(".nca", "");
            
            NcaInfoLines.Add($"Nca ID: {ncaID}");

            for (NcaSectionType section = NcaSectionType.Code; section <= NcaSectionType.Logo; section++)
            {
                if (ncaInfo.Nca.SectionExists(section))
                {
                    NcaInfoLines.Add($"\n{section}");
                    try
                    {
                        using (PartitionFileSystem pfs = new PartitionFileSystem(ncaInfo.TryOpenStorageSection(section)))
                        {
                            
                            foreach(DirectoryEntryEx dirEnt in pfs.EnumerateEntries())
                            {
                                NcaInfoLines.Add($"{dirEnt.Name} - {dirEnt.Size} bytes");
                            }
                        }
                    }
                    catch (LibHac.HorizonResultException) { }
                }
            }


            //Actually draw them
            int Y = 1;
            foreach (string content in NcaInfoLines)
            {
                int lines = System.Text.RegularExpressions.Regex.Matches(content, "\n").Count + 1;
                InfoWin.Add(new Label(1, Y, content));
                Y += lines;
            }
            //InfoWin.Add(ContentTypeLbl);

        }
        public static void Init()
        {
            Application.Init ();
            DarkScheme = 

            var Top = Application.Top;

            // Creates the top-level window to show
            FirmwareWin = new Window ("Firmware") {
                ColorScheme = DarkScheme,
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Percent(50),
                Height = Dim.Fill ()
            };
            Top.Add (FirmwareWin);

            InfoWin = new Window ("Info") {
                ColorScheme = DarkScheme,
                X = Pos.Right(FirmwareWin),
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill ()
            };
            Top.Add (InfoWin);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("Tools/Settings", new MenuItem [] {
                    new MenuItem ("Open New FW", "Opens a new fw dir", SelectFile),
                    new MenuItem ("Create Nand", "", CreateNand),
                    new MenuItem ("Extract All", "", null),
                    new MenuItem ("Quit", "", Application.RequestStop),
                }),
            })
            {
                ColorScheme = DarkScheme,
            };
            Top.Add (menu);

            FirmwareListView = new ListView();
            Application.Run();
        }
        public static void ReloadActiveNcas()
        {
            
            var Top = Application.Top;

            NcaNames = Utils.FirmwareUtils.GetAllNcasAndName(FwDir).Values.ToList();
            NcaNames.Sort();

            InfoWin.RemoveAll();
            
            FirmwareListView = new ListView(NcaNames);
            FirmwareListView.SelectedChanged += UpdateNcaInfo;

             
            FirmwareWin.Add (FirmwareListView);
            Application.Top.SetFocus(FirmwareListView);
        }

    }
}