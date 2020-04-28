using Terminal.Gui;
using System;
using System.IO;

namespace nxfw_tool.Gui.Cli
{
    public class FileSelectionWindowManager
    {
        private Window Window;
        public string Path = "";

        private ListView DirectoryListView;
        private TextField PathTextField;
        private System.Collections.Generic.List<string> DirEntryNames;
        
        protected bool Selection = false;

        protected void HandleFileSelectionEvent()
        {
            string opt = DirEntryNames[DirectoryListView.SelectedItem];
            switch (opt)
            {
            case "Choose This Directory":
                Path = System.IO.Path.GetFullPath(PathTextField.Text.ToString());
                ShowEntryView();
                break;
            case "..":
                PathTextField.Text = System.IO.Path.GetDirectoryName(PathTextField.Text.ToString());        
                ShowSelectionView();        
                break;
            default:
                PathTextField.Text = System.IO.Path.GetFullPath(PathTextField.Text.ToString() + System.IO.Path.DirectorySeparatorChar + opt);
                ShowSelectionView();
                break;
            }
        }
        protected void HandleOkPress()
        {
            Path = System.IO.Path.GetFullPath(PathTextField.Text.ToString());
            FwTui.FwDir = Path;
            FwTui.ReloadActiveNcas();
            FwTui.InfoWin.RemoveAll();            
        }
        public void ShowSelectionView()
        {
            Window.RemoveAll();
            
            DirEntryNames = new System.Collections.Generic.List<string>();

            foreach(var name in Directory.EnumerateDirectories(PathTextField.Text.ToString(), "*"))
            {
                DirEntryNames.Add(System.IO.Path.GetFileName(name));
            }

            DirEntryNames.Sort();
            DirEntryNames.Insert(0, "Choose This Directory");
            DirEntryNames.Insert(1 ,"..");

            DirectoryListView = new SelectionListView(DirEntryNames, new Action(HandleFileSelectionEvent));
            Window.Add(DirectoryListView);
            Application.Top.SetFocus(DirectoryListView);
        }
        public void ShowEntryView()
        {
            Window.RemoveAll();

            var PathLabel = new Label ("Path: ") { X = 1, Y = 1 };
            PathTextField = new TextField (Path) {
                X = Pos.Right (PathLabel),
                Y = 1,
                Width = 40,
                Text = Path
            };

            var OkButton = new Button ("Ok") { X = 1, Y = 3 };
            OkButton.Clicked += HandleOkPress;

            var CancelButton = new Button ("Cancel") { X = Pos.Right(OkButton), Y = 3 };

            var BrowseButton = new Button ("Browse") { X = Pos.Right(CancelButton), Y = 3 };
            BrowseButton.Clicked += ShowSelectionView;

            Window.Add(PathLabel);
            Window.Add(PathTextField);
            Window.Add(OkButton);
            Window.Add(CancelButton);
            Window.Add(BrowseButton);
            Application.Top.SetFocus(OkButton);
        }

        public FileSelectionWindowManager(Window window, string path)
        {
            Path = path;
            Window = window;

            if (!System.IO.Directory.Exists(path))
                throw new ApplicationException($"Path {path} does not exist");
        }
        
    }
}