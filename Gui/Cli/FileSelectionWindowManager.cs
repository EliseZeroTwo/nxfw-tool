using Terminal.Gui;
using System;
using System.IO;

namespace nxfw_tool.Gui.Cli
{
    public class FileSelectionWindowManager
    {
        private Window ChildWindow;
        public string Path = "";

        private ListView DirectoryListView;
        private TextField PathTextField;
        private System.Collections.Generic.List<string> DirEntryNames;
        public Button OkButton;
        public Button InvalidPathOkButton;
        
        protected bool Selection = false;

        public void ShowInvalidPathDialog()
        {
            WarningBox warningBox = new WarningBox("Invalid Path!");
            warningBox.OkButton.Clicked += () => { ChildWindow.Remove(warningBox); Application.Top.SetFocus(ChildWindow); };
            ChildWindow.Add(warningBox);
            ChildWindow.SetFocus(warningBox);

            FwTui.LoggerWM.Log($"Tried to open invalid dir: {PathTextField.Text.ToString()}");

            /*
            if(InvalidPathOkButton != null)
                return;
            
            InvalidPathOkButton = new Button("Ok", true);
            Dialog dialog = new Dialog("Warning", 50, 15);
            
            InvalidPathOkButton.Clicked += () => 
            {
                FwTui.InfoWin.Remove(dialog);
                Application.Top.SetFocus(OkButton);
                InvalidPathOkButton = null;
            };

            dialog.AddButton(InvalidPathOkButton);

            Window.Add(dialog);*/
        }

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
            if (!Directory.Exists(PathTextField.Text.ToString()))
            {
                ShowInvalidPathDialog();
                return;
            }

            Path = System.IO.Path.GetFullPath(PathTextField.Text.ToString());
            FwTui.FwDir = Path;
            FwTui.ReloadActiveNcas();
            FwTui.InfoWin.RemoveAll();            
        }
        public void ShowSelectionView()
        {
            if (!Directory.Exists(PathTextField.Text.ToString()))
            {
                ShowInvalidPathDialog();
                return;
            }

            ChildWindow.RemoveAll();
            
            DirEntryNames = new System.Collections.Generic.List<string>();
            foreach(var name in Directory.EnumerateDirectories(PathTextField.Text.ToString(), "*"))
            {
                DirEntryNames.Add(System.IO.Path.GetFileName(name));
            }

            DirEntryNames.Sort();
            DirEntryNames.Insert(0, "Choose This Directory");
            DirEntryNames.Insert(1 ,"..");

            DirectoryListView = new SelectionListView(DirEntryNames, new Action(HandleFileSelectionEvent));
            ChildWindow.Add(DirectoryListView);
            Application.Top.SetFocus(DirectoryListView);
        }
        public void ShowEntryView()
        {
            ChildWindow.RemoveAll();

            var PathLabel = new Label ("Path: ") { X = 1, Y = 1 };
            PathTextField = new TextField (Path) {
                X = Pos.Right (PathLabel),
                Y = 1,
                Width = 40,
                Text = Path
            };

            OkButton = new Button ("Ok") { X = 1, Y = 3 };
            OkButton.Clicked += HandleOkPress;

            var CancelButton = new Button ("Cancel") { X = Pos.Right(OkButton), Y = 3 };

            var BrowseButton = new Button ("Browse") { X = Pos.Right(CancelButton), Y = 3 };
            BrowseButton.Clicked += ShowSelectionView;

            ChildWindow.Add(PathLabel);
            ChildWindow.Add(PathTextField);
            ChildWindow.Add(OkButton);
            ChildWindow.Add(CancelButton);
            ChildWindow.Add(BrowseButton);
            Application.Top.SetFocus(OkButton);
        }

        public FileSelectionWindowManager(Window window, string path)
        {
            Path = path;
            ChildWindow = window;

            if (!System.IO.Directory.Exists(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        
    }
}