using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SDKTemplate
{

    public class Apps_class
    {
        public string App { get; set; }
        public string Appname { get; set; }
        public string Appicon { get; set; }
    }


    public sealed partial class Scenario3_Custom : Page
    {
        public string[] s_keys;
        private string app1;
        private string app1_keys;
        private string[] l;
        private string[] r;
        private string[] u;
        private string[] d;
        private string[] c;
        Apps_class app_obj;
        private bool app_present;
        IList<string> text3;

        public MainPage rootPage = MainPage.Current;
        Scenario2_Client scenario2 = new Scenario2_Client();

        List<Key_codes.Keys> keys = Enum.GetValues(typeof(Key_codes.Keys)).Cast<Key_codes.Keys>().ToList();
        public ObservableCollection<Apps_class> Appsdata { get; set; } = new ObservableCollection<Apps_class>
            { new Apps_class { App = "Paint3D", Appname = "Paint 3D", Appicon = "ms-appx:///Assets/Paint3D.png" },
            new Apps_class { App = "YouTube", Appname = "YouTube", Appicon = "ms-appx:///Assets/Youtube.png" },
            new Apps_class { App = "Groove", Appname = "Groove Music", Appicon = "ms-appx:///Assets/Groove.png" },
            new Apps_class { App = "Maps", Appname = "Maps", Appicon = "ms-appx:///Assets/Maps.png" },
            new Apps_class { App = "Premiere", Appname = "Adobe Premiere", Appicon = "ms-appx:///Assets/Premiere.jpg" },
            new Apps_class { App = "Photoshop", Appname = "Adobe Photoshop", Appicon = "ms-appx:///Assets/Photoshop.png" },
            new Apps_class { App = "Chrome", Appname = "Chrome", Appicon = "ms-appx:///Assets/Chrome.png" },
            new Apps_class { App = "PowerPoint", Appname = "PowerPoint", Appicon = "ms-appx:///Assets/PowerPoint.png" },
            new Apps_class { App = "Word", Appname = "Word", Appicon = "ms-appx:///Assets/Word.png" },
            };

        public Scenario3_Custom()
        {
            this.InitializeComponent();
            New_apps();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            //Uncomment these lines if macro file is deleted from Downloads folder
            /*           try
                       {
                           string token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
                           StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                           StorageFile destinationFile = await downloadsFolder.CreateFileAsync("macros.txt", CreationCollisionOption.OpenIfExists);
                           StorageFile destinationFile1 = await downloadsFolder.CreateFileAsync("macros_index.txt", CreationCollisionOption.OpenIfExists);
                           try
                           {
                               // Append a list of strings, one per line, to the file
                               rootPage.NotifyUser("Behaviour Saved Successfully", NotifyType.StatusMessage);
                               await Windows.Storage.FileIO.WriteTextAsync(destinationFile, "Chrome:" + comboBoxL1.SelectedItem + ";" + comboBoxR1.SelectedItem + ";" + comboBoxU1.SelectedItem + ";" + comboBoxD1.SelectedItem + ";" + comboBoxC1.SelectedItem + ";");
                               await Windows.Storage.FileIO.WriteTextAsync(destinationFile1, "Chrome:" + comboBoxL1.SelectedIndex + ";" + comboBoxR1.SelectedIndex + ";" + comboBoxU1.SelectedIndex + ";" + comboBoxD1.SelectedIndex + ";" + comboBoxC1.SelectedIndex + ";");
                           }
                           catch (Exception)
                           {
                               // Display an error message
                               rootPage.NotifyUser("Error in Saving", NotifyType.ErrorMessage);
                           }
                       }

                       catch (Exception ex)
                       {

                           if (ApplicationData.Current.LocalSettings.Values["folderToken"] != null)
                           {
                           */
            bool app_present1 = false;
            if (app_obj==null)
                rootPage.NotifyUser("No App selected", NotifyType.ErrorMessage);
            else
            {
                string token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
                StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                Windows.Storage.StorageFile sampleFile1 = await downloadsFolder.GetFileAsync("macros.txt");
                Windows.Storage.StorageFile sampleFile2 = await downloadsFolder.GetFileAsync("macros_index.txt");
                StorageFile destinationFile = await downloadsFolder.CreateFileAsync("macros.txt", CreationCollisionOption.OpenIfExists);
                StorageFile destinationFile1 = await downloadsFolder.CreateFileAsync("macros_index.txt", CreationCollisionOption.OpenIfExists);
                var text1 = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile1);
                var text2 = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile2);

                for (int k = 0; k < text1.Count; k++)
                {
                    if (text1[k].Contains(app_obj.App))
                    {
                        try
                        {
                            rootPage.NotifyUser("Behavior Saved Successfully", NotifyType.StatusMessage);
                            text1[k] = app_obj.App + ":" + comboBoxL1.SelectedItem+"^"+comboBoxL2.SelectedItem+"^"+ comboBoxL3.SelectedItem + ";" + 
                                comboBoxR1.SelectedItem+"^"+ comboBoxR2.SelectedItem+ "^" + comboBoxR3.SelectedItem + ";" + 
                                comboBoxU1.SelectedItem + "^" + comboBoxU2.SelectedItem + "^" + comboBoxU3.SelectedItem + ";" + 
                                comboBoxD1.SelectedItem + "^" + comboBoxD2.SelectedItem + "^" + comboBoxD3.SelectedItem + ";" + 
                                comboBoxC1.SelectedItem + "^" + comboBoxC2.SelectedItem + "^" + comboBoxC3.SelectedItem+":"+Sensitivity.Value;
                            text2[k] = app_obj.App + ":" + comboBoxL1.SelectedIndex + "^" + comboBoxL2.SelectedIndex + "^" + comboBoxL3.SelectedIndex + ";" +
                                comboBoxR1.SelectedIndex + "^" + comboBoxR2.SelectedIndex + "^" + comboBoxR3.SelectedIndex + ";" +
                                comboBoxU1.SelectedIndex + "^" + comboBoxU2.SelectedIndex + "^" + comboBoxU3.SelectedIndex + ";" +
                                comboBoxD1.SelectedIndex + "^" + comboBoxD2.SelectedIndex + "^" + comboBoxD3.SelectedIndex + ";" +
                                comboBoxC1.SelectedIndex + "^" + comboBoxC2.SelectedIndex + "^" + comboBoxC3.SelectedIndex + ":" + Sensitivity.Value;
                            await Windows.Storage.FileIO.WriteLinesAsync(destinationFile,text1);
                            await Windows.Storage.FileIO.WriteLinesAsync(destinationFile1,text2 );
                        }
                        catch (Exception)
                        {
                            // Display an error message
                            rootPage.NotifyUser("Error in Saving", NotifyType.ErrorMessage);
                        }
                        scenario2.Read_file();
                        app_present1 = true;
                    }
                }
                if (app_present1 == false)
                {
                    try
                    {
                        rootPage.NotifyUser("Behavior Saved Successfully", NotifyType.StatusMessage);
                        await Windows.Storage.FileIO.AppendTextAsync(destinationFile, app_obj.App + ":" + comboBoxL1.SelectedItem + "^" + comboBoxL2.SelectedItem + "^" + comboBoxL3.SelectedItem + ";" +
                                comboBoxR1.SelectedItem + "^" + comboBoxR2.SelectedItem + "^" + comboBoxR3.SelectedItem + ";" +
                                comboBoxU1.SelectedItem + "^" + comboBoxU2.SelectedItem + "^" + comboBoxU3.SelectedItem + ";" +
                                comboBoxD1.SelectedItem + "^" + comboBoxD2.SelectedItem + "^" + comboBoxD3.SelectedItem + ";" +
                                comboBoxC1.SelectedItem + "^" + comboBoxC2.SelectedItem + "^" + comboBoxC3.SelectedItem + ":" + Sensitivity.Value + "\n");
                        await Windows.Storage.FileIO.AppendTextAsync(destinationFile1, app_obj.App + ":" + comboBoxL1.SelectedIndex + "^" + comboBoxL2.SelectedIndex + "^" + comboBoxL3.SelectedIndex + ";" +
                                comboBoxR1.SelectedIndex + "^" + comboBoxR2.SelectedIndex + "^" + comboBoxR3.SelectedIndex + ";" +
                                comboBoxU1.SelectedIndex + "^" + comboBoxU2.SelectedIndex + "^" + comboBoxU3.SelectedIndex + ";" +
                                comboBoxD1.SelectedIndex + "^" + comboBoxD2.SelectedIndex + "^" + comboBoxD3.SelectedIndex + ";" +
                                comboBoxC1.SelectedIndex + "^" + comboBoxC2.SelectedIndex + "^" + comboBoxC3.SelectedIndex + ":" + Sensitivity.Value + "\n");
                    }
                    catch (Exception)
                    {
                        // Display an error message
                        rootPage.NotifyUser("Error while Saving", NotifyType.ErrorMessage);
                    }
                    scenario2.Read_file();
                }
            }

        }

        private async void app_selected(object sender, SelectionChangedEventArgs e)
        {
            app_obj = gridView.SelectedItem as Apps_class;
            //Or Apps_class a = (sender as GridView).SelectedItem as Apps_class;
            App_select.Content = app_obj.App;


            string token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
            StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
            Windows.Storage.StorageFile sampleFile = await downloadsFolder.GetFileAsync("macros_index.txt");
            var text = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            app_present = false;
            for (int k = 0; k < text.Count; k++)
            {
                if (text[k].Contains(app_obj.App))
                {
                    s_keys = text[k].Split(':');
                    app1 = s_keys[0];
                    app1_keys = s_keys[1];
                    Sensitivity.Value = float.Parse(s_keys[2]);
                    s_keys = app1_keys.Split(';');
                    l = s_keys[0].Split('^');
                    r = s_keys[1].Split('^');
                    u = s_keys[2].Split('^');
                    d = s_keys[3].Split('^');
                    c = s_keys[4].Split('^');
                    comboBoxL1.SelectedIndex = Int16.Parse(l[0]);
                    comboBoxR1.SelectedIndex = Int16.Parse(r[0]);
                    comboBoxU1.SelectedIndex = Int16.Parse(u[0]);
                    comboBoxD1.SelectedIndex = Int16.Parse(d[0]);
                    comboBoxC1.SelectedIndex = Int16.Parse(c[0]);
                    comboBoxL2.SelectedIndex = Int16.Parse(l[1]);
                    comboBoxR2.SelectedIndex = Int16.Parse(r[1]);
                    comboBoxU2.SelectedIndex = Int16.Parse(u[1]);
                    comboBoxD2.SelectedIndex = Int16.Parse(d[1]);
                    comboBoxC2.SelectedIndex = Int16.Parse(c[1]);
                    comboBoxL3.SelectedIndex = Int16.Parse(l[2]);
                    comboBoxR3.SelectedIndex = Int16.Parse(r[2]);
                    comboBoxU3.SelectedIndex = Int16.Parse(u[2]);
                    comboBoxD3.SelectedIndex = Int16.Parse(d[2]);
                    comboBoxC3.SelectedIndex = Int16.Parse(c[2]);
                    App_select.Content = app1;
                    app_present = true;
                }
            }
            if(app_present==false)
            {
                rootPage.NotifyUser("Behavior for this App not saved", NotifyType.ErrorMessage);
                Clear_content();
            }
        }
        private async void New_apps()
        {
            string token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
            StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
            Windows.Storage.StorageFile sampleFile = await downloadsFolder.GetFileAsync("apps.txt");
            text3 = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            for (int k = 0; k < text3.Count; k++)
            {
                Appsdata.Add(new Apps_class { App = text3[k], Appname = text3[k], Appicon = "ms-appx:///Assets/new_app.png" });
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear_content();
        }
        private void Clear_content()
        {
            comboBoxL1.SelectedItem = null;
            comboBoxR1.SelectedItem = null;
            comboBoxU1.SelectedItem = null;
            comboBoxD1.SelectedItem = null;
            comboBoxC1.SelectedItem = null;
            comboBoxL2.SelectedItem = null;
            comboBoxR2.SelectedItem = null;
            comboBoxU2.SelectedItem = null;
            comboBoxD2.SelectedItem = null;
            comboBoxC2.SelectedItem = null;
            comboBoxL3.SelectedItem = null;
            comboBoxR3.SelectedItem = null;
            comboBoxU3.SelectedItem = null;
            comboBoxD3.SelectedItem = null;
            comboBoxC3.SelectedItem = null;
            Sensitivity.Value = 0.1;
        }

        private async void Add_app(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".exe");
            picker.FileTypeFilter.Add(".lnk");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                rootPage.NotifyUser(file.DisplayName, NotifyType.StatusMessage);
                bool app_present = false;
                string token = ApplicationData.Current.LocalSettings.Values["folderToken"].ToString();
                StorageFolder downloadsFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                Windows.Storage.StorageFile sampleFile = await downloadsFolder.GetFileAsync("apps.txt");
                StorageFile destinationFile = await downloadsFolder.CreateFileAsync("apps.txt", CreationCollisionOption.OpenIfExists);
                var text = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);

                for (int k = 0; k < text.Count; k++)
                {
                    if (text[k].Contains(file.DisplayName))
                    {
                        try
                        {
                            rootPage.NotifyUser("App already added", NotifyType.StatusMessage);
                        }
                        catch (Exception)
                        {
                            rootPage.NotifyUser("Error while Adding new App", NotifyType.ErrorMessage);
                        }
                        app_present = true;
                    }
                }
                if (app_present == false)
                {
                    try
                    {
                        rootPage.NotifyUser("New App added", NotifyType.StatusMessage);
                        await Windows.Storage.FileIO.AppendTextAsync(destinationFile, file.DisplayName + "\n");
                        Appsdata.Add(new Apps_class { App = file.DisplayName, Appname = file.DisplayName, Appicon = "ms-appx:///Assets/new_app.png" });
                    }
                    catch (Exception)
                    {
                        rootPage.NotifyUser("Error while Adding new App", NotifyType.ErrorMessage);
                    }
                }
            }
        }
    }
}
