using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SDKTemplate
{
    public partial class MainPage : Page
    {

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Discover enRING", ClassType=typeof(Scenario1_Discovery) },
            new Scenario() { Title="Connect enRING", ClassType=typeof(Scenario2_Client) },
            new Scenario() { Title="Customize enRING", ClassType=typeof(Scenario3_Custom) },       //Added for swipe functionalities and macros
        };
        public string SelectedBleDeviceId;
        public string SelectedBleDeviceName = "No device selected";
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
