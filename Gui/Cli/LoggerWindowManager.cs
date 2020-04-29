using Terminal.Gui;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace nxfw_tool.Gui.Cli
{
    public class LoggerWindowManager
    {
        public Window ChildWindow;

        public ListView LoggerListView;

        public List<string> LogList = new List<string>();

        public void Log(string message, [CallerMemberName]string callerName = "")
        {
            LogList.Add($"[{callerName} - {DateTime.Now.ToString("HH:mm:ss")}] {message}");
        }

        public void Clear()
        {
            LogList.Clear();
            ChildWindow.Redraw(ChildWindow.Bounds);
        }

        public LoggerWindowManager(Window window)
        {
            ChildWindow = window;

            LoggerListView = new ListView(LogList);

            ChildWindow.RemoveAll();
            ChildWindow.Add(LoggerListView);

        }
        
    }
}