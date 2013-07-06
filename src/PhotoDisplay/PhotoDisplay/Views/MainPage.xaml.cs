using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhotoDisplay.Resources ;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using Windows.Phone.System.UserProfile;
namespace FotoPantallaDev
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        List<BitmapImage> photoList;
        public MainPage()
        {
            InitializeComponent();
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
            Loaded += MainPage_Loaded;
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            photoList = new List<BitmapImage>();
            ValidarPermiso();
            ReadPhotos();            
        }

        private async void ValidarPermiso()
        {
            bool isProvider = LockScreenManager.IsProvidedByCurrentApplication;
            if (!isProvider)
            {
                LockScreenRequestResult op = await LockScreenManager.RequestAccessAsync();
                isProvider = op == LockScreenRequestResult.Granted;
            }
        }

        void appBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Imagenes.xaml", UriKind.Relative));
        }
        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/feature.search.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarButtonText;
            appBarButton.Click += appBarButton_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }


        private void ReadPhotos()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] totalFiles = isoStore.GetFileNames("/shared/ShellContent/");
                for (int i = 1; i < totalFiles.Length + 1; i++)
                {
                    string photoName = string.Format("/shared/ShellContent/Img{0}.jpg", i);
                    //string photoName = string.Format("/Shared/Transfers/Img{0}.jpg", i);
                    if (isoStore.FileExists(photoName))
                    {
                        BitmapImage Bit_Img = new BitmapImage();
                        using (IsolatedStorageFileStream FS = isoStore.OpenFile(photoName, FileMode.Open, FileAccess.Read))
                        {
                            Bit_Img.SetSource(FS);
                            photoList.Add(Bit_Img);
                        }
                    }
                }
            }
            ImagenesLoaded.ItemsSource = photoList;
        }
    }
}