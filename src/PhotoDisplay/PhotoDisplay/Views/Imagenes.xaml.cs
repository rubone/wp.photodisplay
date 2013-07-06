using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml.Linq;
//using System.Windows.Navigation;
//using Microsoft.Phone.Controls;
//using Microsoft.Phone.Shell;
using PhotoDisplay.Resources;
//using MVVM.ViewModels;
using Windows.Phone.System.UserProfile;
using Microsoft.Phone.BackgroundTransfer;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
//using MVVM.Models;
using System.Collections.ObjectModel;
using System.Threading;
using LockerScreenAgent;
using Microsoft.Phone.Scheduler;

namespace FotoPantallaDev.Views
{
    public partial class Imagenes : PhoneApplicationPage
    {
        //int countDownload = 1;
        //private HttpWebRequest request1;
        //private bool isNotFoundPhoto = false;
        
        //int CountImages;
        //private List<BitmapImage> imageList = new List<BitmapImage>();
        //List<Uri> Lista;
        //private bool isFinishedWork = false;


        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        private HttpWebRequest request1;
        private static List<Uri> imagesUri;
        private static List<String> ImagesDetails;
        private bool isFinishedWork = false;
        private bool isNotFoundPhoto = false;
        private List<BitmapImage> imageList;
        private static int countDownload = 1;


        public Imagenes()
        {
            InitializeComponent();
            imageList = new List<BitmapImage>(); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isNotFoundPhoto = false;
            isFinishedWork = false;
            progressStatus.IsIndeterminate = true;
            TxtTitulos.Text = "Starting Search, Please Wait";
            SaveUrisPhotos(TxtBuscar.Text.Trim());
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                while (!isFinishedWork && !isNotFoundPhoto) ;
                if (!isNotFoundPhoto)
                {
                    Dispatcher.BeginInvoke(delegate
                    {
                        this.BtnBuscar.IsEnabled = false;
                        this.TxtBuscar.IsEnabled = false;

                        progressStatus.Visibility = Visibility.Collapsed;
                        TxtTitulos.Text = "Images Found: " + ImagesDetails.Count().ToString() + " Downloading Images ";

                        LoadBTS();
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(delegate
                    {
                        this.BtnBuscar.IsEnabled = true ;
                        this.TxtBuscar.IsEnabled = true;
                        progressStatus.Visibility = Visibility.Collapsed;
                        TxtTitulos.Text = "No Imagess";
                        
                        
                    });
                }
            }));  
        }

       
        #region Fotos
        private void SaveUrisPhotos(string keyWord)
        {
            progressStatus.IsIndeterminate = true;
            TxtTitulos.Text = "Searching Images..";
            string uri = string.Format("http://www.degraeve.com/flickr-rss/rss.php?tags={0}&tagmode=all&sort=interestingness-desc&num=9", keyWord);
            request1 = (HttpWebRequest)HttpWebRequest.Create(new Uri(uri, UriKind.Absolute));
            request1.BeginGetResponse(HttpResponsePhoto, request1);
        }

        private void HttpResponsePhoto(IAsyncResult ar)
        {
           
                request1 = ar.AsyncState as HttpWebRequest;
                if (request1 != null)
                {
                    WebResponse response = null;
                    try
                    {
                        #region  do it
                        response = request1.EndGetResponse(ar);
                        request1 = null;
                        string dataResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        var doc = XDocument.Parse(dataResponse);
                        XNamespace ns = "http://search.yahoo.com/mrss/";

                        var query = (from c in doc.Descendants(ns + "content")
                                     select new Uri(c.Attribute("url").Value, UriKind.RelativeOrAbsolute)).Take(9);

                        var queryDetails = (from c in doc.Descendants(ns + "title")
                                                                                  
                                            select  c.Value.ToString()).ToList();

                        ImagesDetails = queryDetails;

                        imagesUri = query.ToList();
                        if (imagesUri.Count == 0) // hay fotos
                        {
                            Dispatcher.BeginInvoke(delegate
                            {
                                isNotFoundPhoto = true;
                            });
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(delegate
                            {
                                isFinishedWork = true;
                            });
                        }
                        #endregion
                    }
                    catch (Exception)
                    {

                    }
                }
           
        }

        #endregion

        #region Evento BTS

        private void LoadBTS()
        {
            ImagenesLoaded.ItemsSource = ImagesDetails ;

            int count = 1;
            foreach (var item in BackgroundTransferService.Requests)
            {
                BackgroundTransferService.Remove(item);  
            }
            foreach (var uri in imagesUri)
            {
                var localUri = new Uri(string.Format("/shared/transfers/Img{0}.jpg", count), UriKind.Relative);
                BackgroundTransferRequest request = new BackgroundTransferRequest(uri, localUri);
                BackgroundTransferService.Add(request);
                count++;
                TxtCantidad.Text = string.Format("Data Downloaded: {0}/{1}", request.BytesReceived, request.TotalBytesToReceive);
            }
            BindAndAttachEH();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BindAndAttachEH();
        }

        private void BindAndAttachEH()
        {
            if (BackgroundTransferService.Requests.Count() != 0)
            {
                foreach (var item in BackgroundTransferService.Requests)
                {
                    item.TransferProgressChanged += item_TransferProgressChanged;
                    item.TransferStatusChanged += item_TransferStatusChanged;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!(BackgroundTransferService.Requests.Select(x => x.TransferStatus == TransferStatus.Transferring)).Any(x => x == true))
            {
                DettachEvntHandlr();
                TxtImagen.Text = string.Empty;
                TxtCantidad.Text = string.Empty;
            }
        }

        private void DettachEvntHandlr()
        {
            foreach (var item in BackgroundTransferService.Requests)
            {
                BackgroundTransferService.Remove(item);
                item.TransferProgressChanged -= item_TransferProgressChanged;
                item.TransferStatusChanged -= item_TransferStatusChanged;
            }
        }

        void item_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            try
            {
                if (countDownload == imagesUri.Count)
                {
                    TxtTitulos.Text = "Process Completed";
                    CambiarFotosDescargadas();
                    MessageBox.Show("Process Completed", "State Download...", MessageBoxButton.OK);
                    NavigationService.Navigate(new Uri("/Views/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    if (e.Request.TransferStatus.ToString() == "Completed")
                    {
                        countDownload++;
                        TxtImagen.Text = string.Format("Imagen : {0} --- {1} - Imagen : {2}", countDownload, imagesUri.Count, ImagesDetails[countDownload - 1]);
                    }
                    else
                    {
                        TxtImagen.Text = string.Format("Imagen : {0} --- {1} - Imagen : {2}", countDownload, imagesUri.Count, ImagesDetails[countDownload - 1]);
                    }
                }
            }
            catch
            {
            }
        }

        void item_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            try
            {               
                if (countDownload == imagesUri.Count)
                {
                    TxtCantidad.Text = string.Format("Bytes Recibidos: {0}/0", 0);
                }
                else
                {
                    TxtCantidad.Text = string.Format("Bytes Recibidos: {0}/{1}", e.Request.BytesReceived.ToString(), e.Request.TotalBytesToReceive);
                }
            }
            catch
            {
            }
        }

        #endregion


        private void CycleTyleCreate()
        {
            try
            {
                CycleTileData cycleTileData = new CycleTileData();
                cycleTileData.Title = "Photo Display ";
                cycleTileData.Count = ImagesDetails.Count;
                cycleTileData.SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.RelativeOrAbsolute);
                    List<Uri> images = new List<Uri>();
                    IsolatedStorageFile isoStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
                    for (int i = 1; i <= ImagesDetails.Count ; i++)
                    {

                        var destPath = string.Format("isostore:/Shared/ShellContent/Img{0}.jpg", i);
                        images.Add(new Uri(destPath, UriKind.RelativeOrAbsolute));
                    }
                    cycleTileData.CycleImages = images;
                    ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("Imagenes"));

                    if (TileToFind != null)
                    {
                        TileToFind.Update(cycleTileData);
                    }

                    else
                    {
                        ShellTile.Create(new Uri("/Views/MainPage.xaml", UriKind.Relative), cycleTileData, true);

                        Uri tileUri = new Uri(string.Concat("/MainPage.xaml?", "tile=cycle"), UriKind.Relative);
                        ShellTileData tileData = this.CreateCycleTileData();
                        ShellTile.Create(tileUri, tileData, false);
                    }
            }

            catch { 
            }
        }

        private ShellTileData CreateCycleTileData()
        {
            string[] imageNames = { 
        "bonfillet.jpg", "bucket.jpg", "burger.jpg", "caesar.jpg", 
        "chicken.jpg", "corn.jpg", "fries.jpg", "wings.jpg"
    };
            CycleTileData cycleTileData = new CycleTileData();
            cycleTileData.Title = "Cycle tile";
            cycleTileData.Count = 7;
            cycleTileData.SmallBackgroundImage = new Uri("/Assets/pizza.lockicon.png", UriKind.Relative);
            cycleTileData.CycleImages = imageNames.Select(
            imageName => new Uri(string.Concat("/Assets/Images/", imageName), UriKind.Relative));

            return cycleTileData;
        }

        private void LockScreenChange()
        {
            try
            {
                bool isProvider = LockScreenManager.IsProvidedByCurrentApplication;
                if (isProvider)
                {
                    var uri = new Uri("ms-appdata:///Local/Shared/ShellContent/Img1.jpg", UriKind.Absolute);
                    LockScreen.SetImageUri(uri);
                }
            }
            catch
            {

            }
        }

        private void StartPeriodicAgent()
        {

            if (ScheduledActionService.Find(periodicTaskName) != null)
            {
                try
                {
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }
            periodicTask = new PeriodicTask(periodicTaskName);
            periodicTask.Description = "Aplicacion Protectora de Pantalla Dinamico";
            periodicTask.ExpirationTime = DateTime.Now.AddDays(14);
            try
            {
                ScheduledActionService.Add(periodicTask);
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(5));
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Agente de segundo plano ha sido desactivado por el usuario");
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                }
            }
            catch (SchedulerServiceException)
            {
            }
        }             

    

        public void CambiarFotosDescargadas()
        {
            imageList.Clear();

            IsolatedStorageFile isoStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
            //int count = 1;
            for (int i = 1; i <=ImagesDetails.Count; i++)
            {
                #region  Hechura
                string path = string.Format("/Shared/Transfers/Img{0}.jpg", i);
                string path2 = string.Format("/Shared/ShellContent/Img{0}.jpg", i);

                if (isoStorageFile.FileExists(path))
                {
                    BitmapImage imgTemp = new BitmapImage();

                    using (IsolatedStorageFileStream issfs = isoStorageFile.OpenFile(path, FileMode.Open, FileAccess.Read))
                    {
                        imgTemp.SetSource(issfs);
                        imageList.Add(imgTemp);
                    }

                    if (isoStorageFile.FileExists(path2))
                    {
                        isoStorageFile.DeleteFile(path2);
                    }
                    isoStorageFile.MoveFile(path, path2);
                }
                else if (isoStorageFile.FileExists(path2))
                {
                    BitmapImage tempImg = new BitmapImage();

                    using (IsolatedStorageFileStream issfs = isoStorageFile.OpenFile(path2, FileMode.Open, FileAccess.Read))
                    {
                        tempImg.SetSource(issfs);
                        imageList.Add(tempImg);
                    }
                }
                #endregion
            }

            LockScreenChange();
            StartPeriodicAgent();
            CycleTyleCreate();

        }
    }
}