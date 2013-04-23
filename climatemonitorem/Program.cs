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
using Microsoft.SPOT.Net.NetworkInformation;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;

namespace climatemonitorem
{
    public partial class Program
    {
        //NetworkInterface[] connet = NetworkInterface.GetAllNetworkInterfaces();
        GT.Networking.WebEvent webEventtemperature;
        GT.Networking.WebEvent webEventhumidity;
        GT.Networking.WebEvent webEventlight;
        GT.Networking.WebEvent authentication;
        GT.Networking.WebEvent view;
        public static TimeSpan GetMachineTime;
        double temperature;
        double relativeHumidity;
        double lightpercentage;
        string systemip;
        

        // This method is run when the mainboard is powered up or reset.   
        
        void ProgramStarted()
        {
            
            GT.Timer light = new GT.Timer(1000);
            //GT.Timer check = new GT.Timer(30000);
            //check.Tick += new GT.Timer.TickEventHandler(checkformessages);
            light.Tick += new GT.Timer.TickEventHandler(measurelight);
            
            light.Start();
            //check.Start();
                        temperatureHumidity.MeasurementComplete += new TemperatureHumidity.MeasurementCompleteEventHandler(temperatureHumidity_MeasurementComplete);
            Debug.Print("Program Started");
            temperatureHumidity.StartContinuousMeasurements();
            ///ethernet.UseStaticIP("192.168.14.254", "255.255.240.0", "192.168.0.66");
            ethernet.UseDHCP();
            led.TurnOff();
            ethernet.NetworkUp +=
               new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkUp);
            ethernet.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkDown);
            display.SimpleGraphics.AutoRedraw = true;
            //cellularRadio = new GTM.Seeed.CellularRadio(4);
            //cellularRadio.DebugPrintEnabled = true;
            //cellularRadio.PowerOn(5);
        }
        void runled(GT.Timer timer)
        {
            led.TurnRed();
            Thread.Sleep(500);
            led.TurnGreen();


        }
        Font baseFont = Resources.GetFont(Resources.FontResources.NinaB);
        private void SetupWindow(string write, uint line)
        {
            display.SimpleGraphics.DisplayText(write, baseFont, GT.Color.White, 10, line);

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
            //SetupWindow("Temperature read successfully ", 140);
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
            systemip = ipAddress;
            WebServer.StartLocalServer(ipAddress, 80);
            display.SimpleGraphics.Clear();
            SetupWindow("Climate Control Monitor", 10);
            SetupWindow("Enter this into PC application:", 50);
            RedSetupWindow(systemip, 70);
            SetupWindow("Please use the user guide to help you", 90);
            SetupWindow("Access this device online at: ",130);
            RedSetupWindow(systemip + "/view", 150);
            


            webEventtemperature = WebServer.SetupWebEvent("gettemp");
            webEventtemperature.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventtemperature_WebEventReceived);

            webEventhumidity = WebServer.SetupWebEvent("gethumid");
            webEventhumidity.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventhumidity_WebEventReceived);

            webEventlight = WebServer.SetupWebEvent("getlight");
            webEventlight.WebEventReceived += new WebEvent.ReceivedWebEventHandler(webEventlight_WebEventReceived);
            authentication = WebServer.SetupWebEvent("authenticate");
            authentication.WebEventReceived += new WebEvent.ReceivedWebEventHandler(authenticatesystem_WebEventReceived);
            view = WebServer.SetupWebEvent("view");
            view.WebEventReceived += new WebEvent.ReceivedWebEventHandler(view_WebEventReceived);


            Debug.Print("web event setup success");
        }
        
        void view_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            led.TurnWhite();
            Thread.Sleep(2000);
            led.TurnOff();

            Debug.Print("View page requested");
            TimeSpan time = GetMachineTime;
            new DateTime(time.Ticks).ToString("HH:mm:ss");
           string content = "<html><title>Web Access</title><center><h1>Room Climate Monitor Web Access</center></h1><body></br></br></br>This is the web page for the Room Climate Control Monitor on IP" + systemip  + ".</br></br>Currently the following values are detected: </br></br>Temperature: " + temperature.ToString() + "'c</br></br>Humidity: " + relativeHumidity + "</br></br>Percentage Light(Higher is Lighter): "+ lightpercentage + "%</br></br>Values change every few seconds but require the page to be refreshed for them to be updated.</br></br>Time: " + DateTime.Now + " </br></br></br>IP: " + systemip + "</br>Subnet Mask: " + ethernet.Interface.NetworkInterface.SubnetMask + "</br>Gateway: " + ethernet.Interface.NetworkInterface.GatewayAddress + "</br></br></br></br><center>Hosted on the .Net Gadgeteer Platform</center></body></html>" ;
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void authenticatesystem_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>connectionsuccessfulrunprogram</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void webEventtemperature_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>" + temperature + "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void webEventhumidity_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>" + relativeHumidity + "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void webEventlight_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>" + lightpercentage + "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");
        }
        void measurelight(GT.Timer light)
        {
            lightpercentage =  lightsense.ReadLightSensorPercentage();
            ///Debug.Print("Success");
        }
        //void checkformessages(GT.Timer check)
        //{
            

                    

        //}
    }
}
