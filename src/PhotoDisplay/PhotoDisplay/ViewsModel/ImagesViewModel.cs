using Microsoft.Phone.Shell;
using MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotoPantallaDev.Views;
using Microsoft.Phone.BackgroundTransfer;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;

namespace MVVM.ViewModels
{
    public class ImagesViewModel : NotificationEnabledObject
    {
        bool isBusy;
        ServiceModel serviceModel = new ServiceModel();
        System.Windows.Threading.Dispatcher dispatcher;
        
        ObservableCollection<ImagesBrand> newImages = new ObservableCollection<ImagesBrand>();
        ActionCommand getImagesCommand;
        ObservableCollection<ImagesBrand> imageList;


        String progreso;
        public String Progreso
        {
            get { return progreso; }
            set
            {
                progreso = value;
                //OnPropertyChanged();
            }
        }

        int countimages;
        public int CountImages
        {
            get { return countimages; }
            set
            {
                countimages = value;
                //OnPropertyChanged();
            }
        }

        int totaltimages;
        public int TotalImages
        {
            get { return totaltimages; }
            set
            {
                totaltimages = value;
                //OnPropertyChanged();
            }
        }

        String search;
        public String Search
        {
            get { return Search; }
            set
            {
                search = value;
                //OnPropertyChanged();
            }
        }


        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value;
            OnPropertyChanged();
            }
        }


                
        public ActionCommand GetImagesCommand
        {
            get
            {
                if (getImagesCommand == null)
                {
                    getImagesCommand = new ActionCommand(() =>
                    {
                        IsBusy = true;

                        dispatcher = FotoPantallaDev.App.Current.RootVisual.Dispatcher;

                        serviceModel.GetImagesBrands(search);
                    });
                }

                return getImagesCommand;
            }
        }

        


        public ObservableCollection<ImagesBrand> ImageList
        {
            get
            {
                if (imageList == null)
                {
                    imageList = new ObservableCollection<ImagesBrand>();
                }

                if (DesignerProperties.IsInDesignTool)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        imageList.Add(new ImagesBrand() { Name = Guid.NewGuid().ToString() });
                    }
                }

                return imageList;
            }

            set
            {
                imageList = value;
                OnPropertyChanged();
            }
        }


      

        public ImagesViewModel()
        {

            serviceModel.GetImagesCompleted += (s, a) =>
            {
                var ImagenesEncontradas = new ObservableCollection<ImagesBrand>(a.Results);
                // ImageList = new ObservableCollection<ImagesBrand>(a.Results);
               // TotalImages = a.Results.Count();

                 dispatcher.BeginInvoke(() =>
                 {

                     ImageList = ImagenesEncontradas;

                     //ActualizaEstatus();
                     // LLamadaCallBTS();

                     IsBusy = false;

                 });
            };

            
        }

        #region eventos del BTS

        private void LLamadaCallBTS()
        {
            CountImages = 1;
            foreach (var uri in imageList)
            {
                var localUri = new Uri(string.Format("/shared/transfers/Imagen_{0}.jpg", CountImages), UriKind.Relative);
                BackgroundTransferRequest request = new BackgroundTransferRequest(uri.uri , localUri);
                BackgroundTransferService.Add(request);
                CountImages++;
                Progreso = string.Format("Bytes Recibidos: {0}/{1}", request.BytesReceived, request.TotalBytesToReceive);
            }
            //BindAndAttachEH();
        }


        
        #endregion

        private void ActualizaEstatus()
        {
            var tile = Microsoft.Phone.Shell.ShellTile.ActiveTiles.First();

            //var Txt = new Imagenes();

            //var iconicTile = new Iconic()
            //{
            //    Count = imageList.Count,
            //    WideContent1 = imageList.Count.ToString() + " " + "marcas de cámaras",
            //    WideContent2 = "Última actualización: " + DateTime.Now.AddDays(-7).ToString()
            //};


            //Txt.progressStatus.Visibility = System.Windows.Visibility.Collapsed;
            //Txt.txtProgressPhoto.Text = "Descargando Imagenes";
            //tile.Update(iconicTile);
        }






    }
}
