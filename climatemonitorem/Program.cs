using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;

namespace climatemonitorem
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            //Do one time tasks here
            Debug.Print("Program Started");
        }
        private Text txtMessage;

        private void SetupWindow(string write)
        {
            Font baseFont = Resources.GetFont(Resources.FontResources.NinaB);
            Window window = display.WPFWindow;
            Canvas canvas = new Canvas();
            window.Child = canvas;
            txtMessage = new Text(baseFont, write);
            canvas.Children.Add(txtMessage);
            Canvas.SetTop(txtMessage, 200);
            Canvas.SetLeft(txtMessage, 90);
        }
    }
}
