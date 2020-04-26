using Terminal.Gui;
using System.Collections.Generic;
namespace nxfw_tool.Gui.Cli
{
    public class SelectionListView : ListView
    {
        public override bool ProcessKey (KeyEvent keyEvent)
        {
            switch (keyEvent.Key)
            {
                case Key.Enter:
                {
                    FwTui.FilePickerCallback();
                    return true;
                }
            }
            return base.ProcessKey(keyEvent);
        }

        public SelectionListView(List<string> Strings) : base(Strings)
        {
            
        }
    }
}