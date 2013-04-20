using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;
using System.Text;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;

namespace climatemonitorem
{
    public partial class Program
    {
        GT.Networking.WebEvent webEventtemperature;
        GT.Networking.WebEvent webEventhumidity;
        GT.Networking.WebEvent webEventlight;
        double temperature;
        double relativeHumidity;
        // This method is run when the mainboard is powered up or reset.   
        ///NetComm.Host Server;
        void ProgramStarted()
        {
            //SetupWindow("Cimate Control Monitor : "+ DateTime.Now.ToString("HH:mm"));
            temperatureHumidity.MeasurementComplete += new TemperatureHumidity.MeasurementCompleteEventHandler(temperatureHumidity_MeasurementComplete);
            Debug.Print("Program Started");
            temperatureHumidity.StartContinuousMeasurements();
            ethernet.UseDHCP();
            ethernet.NetworkUp +=
               new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkUp);
            ethernet.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkDown);
            display.SimpleGraphics.AutoRedraw = true;
        }
                Font baseFont = Resources.GetFont(Resources.FontResources.NinaB);
        private void SetupWindow(string write, uint line)
        {
            display.SimpleGraphics.DisplayText(write, baseFont,GT.Color.White, 10, line);

        }
        private void RedSetupWindow(string write, uint line)
        {
            display.SimpleGraphics.DisplayText(write, baseFont, GT.Color.Red, 10, line);

        }
        private void rdw()
        {
            //display.SimpleGraphics.Clear();
            //display.SimpleGraphics.Redraw();
        }
        void temperatureHumidity_MeasurementComplete(TemperatureHumidity sender, double temp, double humid)
        {
            temperature = temp;
            relativeHumidity = humid;
            double fahrenheit = (temperature * 1.8) + 32;
            SetupWindow("Temperature read successfully ", 140);
            //SetupWindow("Temperature: " + temp.ToString(), 155);
            rdw();                     
        }
        void ethernet_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("connection failure");
        }

        void ethernet_NetworkUp(GTM.Module.NetworkModule sender,
                              GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Connected to network.");
            Debug.Print("IP address: " + ethernet.NetworkSettings.IPAddress);
            string ipAddress = ethernet.NetworkSettings.IPAddress;
            WebServer.StartLocalServer(ipAddress, 80);
            display.SimpleGraphics.Clear();
            SetupWindow("Climate Control Monitor",10);
            SetupWindow("Enter this into PC application:", 50);
            RedSetupWindow(ipAddress, 70);
            SetupWindow("Please use the user guide to help you", 90);


            webEventtemperature = WebServer.SetupWebEvent("gettemp");
            webEventtemperature.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventtemperature_WebEventReceived);

            webEventhumidity = WebServer.SetupWebEvent("gethumid");
            webEventhumidity.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventhumidity_WebEventReceived);

            //webEventlight = WebServer.SetupWebEvent("led");
            //webEventlight.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventlight_WebEventReceived);

            Debug.Print("web event setup success");
        }
        void webEventtemperature_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>" + temperature + "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void webEventhumidity_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>" + relativeHumidity+ "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
    }
}
