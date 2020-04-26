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
    public class FwTui
    {
        ColorScheme DarkScheme;
        ListView listView;
        Window InfoWin;

        public string FwDir;
        List<string> NcaNames;
        public void OpenNewDir()
        {

        }

        public void UpdateNcaInfo()
        {
            InfoWin.RemoveAll();
            string NcaPath = (string)Utils.FirmwareUtils.OpenNcaStorageByTitleName(FwDir, NcaNames[listView.SelectedItem], true);
            NcaInfo ncaInfo = new NcaInfo(new LocalStorage(NcaPath, FileAccess.Read));
            
            List<string> NcaInfoLines = new List<string>();

            string FormattedTid = $"{ncaInfo.Nca.Header.TitleId:X16}";
            string FormattedName = $"{ncaInfo.TitleName}";

            NcaInfoLines.Add($"Title ID: {FormattedTid}");

            if(FormattedName != FormattedTid)
            {
                NcaInfoLines.Add($"Title Name: {FormattedName}");
            }

            NcaInfoLines.Add($"Content Type: {ncaInfo.Nca.Header.ContentType}");
            
            string ncaID = NcaPath.Split("/").Last();
            if (ncaID == "00.nca")
                ncaID = NcaPath.Split("/")[NcaPath.Split("/").Length - 2];
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
                                //pfs.OpenFile(out IFile iFile, LibHac.Common.U8StringHelpers.ToU8Span(dirEnt.FullPath), OpenMode.Read);
                                //NcaInfoLines.Add($"{(string)FirmwareUtils.ParseFileFromStream(iFile.AsStream())}");
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
        public void Init()
        {
            Application.Init ();
            DarkScheme = new ColorScheme(){
                Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                HotFocus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
                HotNormal  = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            };
        }
        public void DrawNcaSelection()
        {
            
            var Top = Application.Top;

            // Creates the top-level window to show
            var Win = new Window ("Firmware") {
                ColorScheme = DarkScheme,
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Percent(50),
                Height = Dim.Fill ()
            };
            Top.Add (Win);

            InfoWin = new Window ("Info") {
                ColorScheme = DarkScheme,
                X = Pos.Right(Win),
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill ()
            };
            Top.Add (InfoWin);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar (new MenuBarItem [] {
                new MenuBarItem ("Program", new MenuItem [] {
                    new MenuItem ("Open New FW", "Opens a new fw dir", OpenNewDir),
                    new MenuItem ("Quit", "", Application.RequestStop),
                }),
            });
            menu.ColorScheme = DarkScheme;
            Top.Add (menu);

            NcaNames = Utils.FirmwareUtils.GetAllNcasAndName(FwDir).Values.ToList();
            NcaNames.Sort();


            listView = new ListView(NcaNames);
            listView.SelectedChanged += UpdateNcaInfo;

            
            // Add some controls, 
            Win.Add (
                // The ones with my favorite layout system
                listView
            );
            Application.Top.SetFocus(listView);

            Application.Run ();
        }

    }
}