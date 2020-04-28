using Terminal.Gui;
using System;
using System.Collections.Generic;
namespace nxfw_tool.Gui.Cli
{
    public class SelectionListView : ListView
    {
        private Action Action;
        public override bool ProcessKey (KeyEvent keyEvent)
        {
            switch (keyEvent.Key)
            {
                case Key.Enter:
                {
                    Action.Invoke();
                    return true;
                }
            }
            return base.ProcessKey(keyEvent);
        }

        public SelectionListView(List<string> Strings, Action action) : base(Strings)
        {
            Action = action;
        }
    }
}