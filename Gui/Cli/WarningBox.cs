using System;
using Terminal.Gui;
namespace nxfw_tool.Gui.Cli
{
    public class WarningBox : Dialog
    {
        public Button OkButton;
        public WarningBox(string text) : base("Warning", 0, 0)
        {
            X = Pos.Center();
            Y = Pos.Center();
            Width = Dim.Percent(60);
            Height = Dim.Percent(80);
            
            Add(new Label(text){
                X = Pos.Center(),
                Y = Pos.Center(),
            });

            OkButton = new Button("Ok", true);

            AddButton(OkButton);

            SetFocus(OkButton);
            
        }
    }
}