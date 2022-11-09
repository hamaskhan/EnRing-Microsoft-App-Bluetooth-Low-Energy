
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input.Preview.Injection;
using System.Runtime.InteropServices;
using Windows.Storage;
namespace SDKTemplate
{
    // This scenario connects to the device selected in the "Discover
    // GATT Servers" scenario and communicates with it.
    // Note that this scenario is rather artificial because it communicates
    // with an unknown service with unknown characteristics.
    // In practice, your app will be interested in a specific service with
    // a specific characteristic.
    public sealed partial class Scenario2_Client : Page
    {
        private MainPage rootPage = MainPage.Current;
        private ObservableCollection<BluetoothLEAttributeDisplay> ServiceCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();
        private ObservableCollection<BluetoothLEAttributeDisplay> CharacteristicCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();

        private BluetoothLEDevice bluetoothLeDevice = null;
        private GattCharacteristic selectedCharacteristic;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;

        private bool readtoggle = false;   //readtoggle is for readvalue button to toggle between read and not read
        private bool connect = false;      //connect is to call Connect function once
        private int[] values;
        public string[] s_keys;
        private string app1;
        private string app1_keys;
        private string[] l;
        private string[] r;
        private string[] u;
        private string[] d;
        private string[] c;
        string token;
        IList<string> text;
        IList<string> text1;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);

        #region Error Codes
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion
        
        public async void Read_file()
        {
            token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
            StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
            Windows.Storage.StorageFile sampleFile = await downloadsFolder.GetFileAsync("macros.txt");
            text = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            Windows.Storage.StorageFile sampleFile1 = await downloadsFolder.GetFileAsync("apps.txt");
            text1 = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile1);
        }

        #region UI Code
        public Scenario2_Client()
        {
            InitializeComponent();
            Read_file();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(rootPage.SelectedBleDeviceId))
            {
                //ConnectButton.IsEnabled = false;
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            var success = await ClearBluetoothLEDeviceAsync();
            if (!success)
            {
                rootPage.NotifyUser("Error: Unable to reset app state", NotifyType.ErrorMessage);
            }
        }
        #endregion
        
        #region Enumerating Services
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            return true;
        }

        private async void ConnectButton_Click()
        {
           // ConnectButton.IsEnabled = false;

            if (!await ClearBluetoothLEDeviceAsync())
            {
                rootPage.NotifyUser("Error: Unable to reset state, try again.", NotifyType.ErrorMessage);
                //ConnectButton.IsEnabled = false;
                return;
            }

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(rootPage.SelectedBleDeviceId);

                if (bluetoothLeDevice == null)
                {
                    rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                rootPage.NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    foreach (var service in services)
                    {
                        ServiceCollection.Add(new BluetoothLEAttributeDisplay(service));
                    }
                    //ConnectButton.Visibility = Visibility.Collapsed;          //This line commented
                    //ServiceList.Visibility = Visibility.Visible;              //This line commented

                    ServiceList_SelectionChanged();                             //This line added
                }
                else
                {
                    rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                }
            }
            //ConnectButton.IsEnabled = true;
        }
        #endregion

        #region Enumerating Characteristics
        private async void ServiceList_SelectionChanged()
        {
            ServiceList.SelectedIndex = 3;                            //This line added

            var attributeInfoDisp = (BluetoothLEAttributeDisplay)ServiceList.SelectedItem;

            CharacteristicCollection.Clear();

            RemoveValueChangedHandler();

            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await attributeInfoDisp.service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await attributeInfoDisp.service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        rootPage.NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    rootPage.NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                rootPage.NotifyUser("Restricted service. Can't read characteristics: " + ex.Message,
                    NotifyType.ErrorMessage);
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }

            foreach (GattCharacteristic c in characteristics)
            {
                CharacteristicCollection.Add(new BluetoothLEAttributeDisplay(c));
            }
            //CharacteristicList.Visibility = Visibility.Visible;          //This line commented

            CharacteristicList_SelectionChanged();          //This line added
        }
        #endregion

        //Removed AddValueChangedHandler() function

        private void RemoveValueChangedHandler()
        {
            //Removed ValueChangedSubscribeToggle.Content = "Subscribe to value changes";
            if (subscribedForNotifications)
            {
                registeredCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                registeredCharacteristic = null;
                subscribedForNotifications = false;
            }
        }

        private async void CharacteristicList_SelectionChanged()
        {
            selectedCharacteristic = null;
            CharacteristicList.SelectedIndex = 0;          //This line added
            var attributeInfoDisp = (BluetoothLEAttributeDisplay)CharacteristicList.SelectedItem;
            if (attributeInfoDisp == null)
            {
                // EnableCharacteristicPanels(GattCharacteristicProperties.None);          //This line commented
                return;
            }

            selectedCharacteristic = attributeInfoDisp.characteristic;
            if (selectedCharacteristic == null)
            {
                rootPage.NotifyUser("No characteristic selected", NotifyType.ErrorMessage);
                return;
            }

            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                rootPage.NotifyUser("Descriptor read failure: " + result.Status.ToString(), NotifyType.ErrorMessage);
            }

            // BT_Code: There's no need to access presentation format unless there's at least one. 
            presentationFormat = null;
            if (selectedCharacteristic.PresentationFormats.Count > 0)
            {

                if (selectedCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = selectedCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }

            // Enable/disable operations based on the GattCharacteristicProperties.
            // EnableCharacteristicPanels(selectedCharacteristic.CharacteristicProperties);          //This line commented
            connect = true;
            CharacteristicReadButton_Click();
        }

        private void SetVisibility(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /* private void EnableCharacteristicPanels(GattCharacteristicProperties properties)
         {
             // BT_Code: Hide the controls which do not apply to this characteristic.
             SetVisibility(CharacteristicReadButton, properties.HasFlag(GattCharacteristicProperties.Read));

             SetVisibility(CharacteristicWritePanel,
                 properties.HasFlag(GattCharacteristicProperties.Write) ||
                 properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse));
             CharacteristicWriteValue.Text = "";

             //Removed SetVisibilty for ValueChangedSubscribeToggle
         }*/

        private void Read_button()
        {
            ReadButton.Content = "Disconnect";
            readtoggle = !readtoggle;
            if (connect == false)
            {
                ConnectButton_Click();
            }
            if (readtoggle == false)
            {
                ReadButton.Content = "Connect";
                rootPage.NotifyUser("enRING Disconnected", NotifyType.StatusMessage);
            }
            else
                CharacteristicReadButton_Click();
        }
        private async void CharacteristicReadButton_Click()
        {
            try
            {
                rootPage.NotifyUser("enRING Connected", NotifyType.StatusMessage);
                double sensitive;
                while (readtoggle)
                {
                    // BT_Code: Read the actual value from the device by using Uncached.
                    GattReadResult result = await selectedCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        //string formattedResult = FormatValueByPresentation(result.Value, presentationFormat);
                        string formattedResult = Returndata(result.Value);
                        rootPage.NotifyUser($"Read: {formattedResult}", NotifyType.StatusMessage);
                        values = formattedResult.Split(',').Select(s => int.Parse(s)).ToArray();
                        string a = GetActiveWindowTitle();
                        string active = Getcurrent(a);
                        Debug.WriteLine(a);
                        Debug.WriteLine(active);

                        if (active != "Switching")
                        {
                            /*if (values[1] == 1 && active != null)
                            {
                            sensitive=Return_Keys(active);
                            SendKey(l);
                            await Task.Delay(TimeSpan.FromSeconds(sensitive));
                            //rootPage.NotifyUser($"Read: {formattedResult} Sensitive: {sensitive}", NotifyType.StatusMessage);
                            }*/
                            if (values[2] == 1 && active != null)
                            {
                                sensitive = Return_Keys(active);
                                SendKey(r);
                                await Task.Delay(TimeSpan.FromSeconds(sensitive));
                                //rootPage.NotifyUser($"Read: {formattedResult} Sensitive: {sensitive}", NotifyType.StatusMessage);
                            }
                            else if (values[4] == 1 && active != null)
                            {
                                sensitive = Return_Keys(active);
                                SendKey(u);
                                await Task.Delay(TimeSpan.FromSeconds(sensitive));
                                //rootPage.NotifyUser($"Read: {formattedResult} Sensitive: {sensitive}", NotifyType.StatusMessage);
                            }
                            else if (values[0] == 1 && active != null)
                            {
                                sensitive = Return_Keys(active);
                                SendKey(c);
                                await Task.Delay(TimeSpan.FromSeconds(sensitive));
                                //rootPage.NotifyUser($"Read: {formattedResult} Sensitive: {sensitive}", NotifyType.StatusMessage);                
                            }
                        }
                        else if(active=="Switching")
                            TSwitch();
                    }
                    else
                    {
                        rootPage.NotifyUser($"Read failed: {result.Status}", NotifyType.ErrorMessage);
                    }
                }
            }
            catch
            {
                rootPage.NotifyUser("Please connect enRING", NotifyType.ErrorMessage);
            }
        }

        private string GetActiveWindowTitle()
        {
            IntPtr handle = GetForegroundWindow();
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        private string Getcurrent(string current)
        {
            try {
                if (current.Contains("YouTube"))
                    return "YouTube";
                else if (current.Contains("Chrome"))
                    return "Chrome";
                else if (current.Contains("Groove"))
                    return "Groove";
                else if (current.Contains("Paint 3D"))
                    return "Paint3D";
                else if (current.Contains("Maps"))
                    return "Maps";
                else if (current.Contains("Premiere"))
                    return "Premiere";
                else if (current.Contains("Photoshop"))
                    return "Photoshop";
                else if (current.Contains("PowerPoint"))
                    return "PowerPoint";
                else if (current.Contains("Word"))
                    return "Word";
                else if (current.Contains("Switching"))
                    return "Switching";
                else
                   {
                    for (int k = 0; k < text1.Count; k++)
                    {
                        var cstring = CommonString(current, text1[k]);
                        foreach (var c in cstring)
                        {
                            return c;
                        }
                    }
                    return null;
                   }
            }
            catch {
                return null;
            }       
        }

        public static string[] CommonString(string left, string right)
        {
            List<string> result = new List<string>();
            string[] rightArray = right.ToLower().Split();
            string[] leftArray = left.ToLower().Split();

            result.AddRange(rightArray.Where(r => leftArray.Any(l => l.StartsWith(r))));

            // must check other way in case left array contains smaller words than right array
            result.AddRange(leftArray.Where(l => rightArray.Any(r => r.StartsWith(l))));

            return result.Distinct().ToArray();
        }

        private double Return_Keys(string App)
        {
            double sensitive;
            Read_file();
            bool app_present = false;
            for (int k = 0; k < text.Count; k++)
            {
                if (text[k].ToLower().Contains(App.ToLower()))
                { 
                    s_keys =  text[k].Split(':');
                    app1 = s_keys[0];
                    app1_keys = s_keys[1];
                    sensitive = double.Parse(s_keys[2]);
                    s_keys = app1_keys.Split(';');
                    l = s_keys[0].Split('^');
                    r = s_keys[1].Split('^');
                    u = s_keys[2].Split('^');
                    d = s_keys[3].Split('^');
                    c = s_keys[4].Split('^');
                    app_present = true;
                    return sensitive;
                }
            }
            if (app_present == false)
            {
                rootPage.NotifyUser("Behavior for this App not saved", NotifyType.ErrorMessage);
            }
            return 0.1;
        }



        private void SendKey(string[] k)
        {
            InjectedInputKeyboardInfo myinput1 = new InjectedInputKeyboardInfo();
            InjectedInputKeyboardInfo myinput2 = new InjectedInputKeyboardInfo();
            InjectedInputKeyboardInfo myinput3 = new InjectedInputKeyboardInfo();
            var injector = InputInjector.TryCreate();
            if (k[2] == "")
            {
                if (k[1] == "")
                {
                    if (k[0] == "")
                        rootPage.NotifyUser("No keys saved for this action", NotifyType.ErrorMessage);
                    else
                    {
                        myinput1.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[0]);
                        injector.InjectKeyboardInput(new[] { myinput1 });
                        myinput1.KeyOptions = InjectedInputKeyOptions.KeyUp;
                        injector.InjectKeyboardInput(new[] { myinput1 });
                    }

                }
                else
                {
                    myinput1.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[0]);
                    myinput2.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[1]);
                    injector.InjectKeyboardInput(new[] { myinput1, myinput2 });
                    if (k[0] == "Alt" && k[1] == "Tab")
                    {
                        myinput2.KeyOptions = InjectedInputKeyOptions.KeyUp;
                        injector.InjectKeyboardInput(new[] { myinput2 });
                    }
                    else
                    {
                        myinput1.KeyOptions = InjectedInputKeyOptions.KeyUp;
                        myinput2.KeyOptions = InjectedInputKeyOptions.KeyUp;
                        injector.InjectKeyboardInput(new[] { myinput1, myinput2 });
                    }
                }
            }
            else {
                myinput1.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[0]);
                myinput2.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[1]);
                myinput3.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), k[2]);
                injector.InjectKeyboardInput(new[] { myinput1, myinput2, myinput3 });
                myinput1.KeyOptions = InjectedInputKeyOptions.KeyUp;
                myinput2.KeyOptions = InjectedInputKeyOptions.KeyUp;
                myinput3.KeyOptions = InjectedInputKeyOptions.KeyUp;
                injector.InjectKeyboardInput(new[] { myinput1, myinput2, myinput3 });
            }
        }

        private async void TSwitch()
        {
            if (values[2] == 1)
            {
                Return_Keys("Switching");
                SendKey(r);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
           /* else if (values[1] == 1)
            {
                Return_Keys("Switching");
                SendKey(l);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }*/
            else if (values[0] == 1)
            {
                InjectedInputKeyboardInfo myinput1 = new InjectedInputKeyboardInfo();
                var injector = InputInjector.TryCreate();
                myinput1.VirtualKey = (ushort)(Key_codes.Keys)System.Enum.Parse(typeof(Key_codes.Keys), "Alt");
                myinput1.KeyOptions = InjectedInputKeyOptions.KeyUp;
                injector.InjectKeyboardInput(new[] { myinput1 });
                await Task.Delay(TimeSpan.FromSeconds(4));
            }
        }


        private bool subscribedForNotifications = false;
        //Removed function ValueChangedSubscribeToggle_Click() along with xaml code for it


        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue, presentationFormat);
            var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = message);
        }

        private string Returndata(IBuffer buffer)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            try
            {
                return Encoding.UTF8.GetString(data);
            }
            catch (ArgumentException)
            {
                return "Unknown format xxxx";
            }
        }
        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format:" + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            else if (data != null)
            {
                // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
                // This is our custom calc service Result UUID. Format it like an Int
                if (selectedCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                // No guarantees on if a characteristic is registered for notifications.
                else if (registeredCharacteristic != null)
                {
                    // This is our custom calc service Result UUID. Format it like an Int
                    if (registeredCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
                    {
                        return BitConverter.ToInt32(data, 0).ToString();
                    }
                }
                else
                {
                    try
                    {
                        return "Unknown format: " + Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "Unknown format ";
                    }
                }
            }
            else
            {
                return "Empty data received";
            }
            return "Unknown format ";
        }
    }
}
