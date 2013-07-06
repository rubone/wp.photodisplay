using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Windows.Phone.System.UserProfile;
using System;
using System.Threading;
using System.IO.IsolatedStorage;

namespace LockerScreenAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        static ScheduledAgent()
        {            
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }
        
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {                
                Debugger.Break();
            }
        }
        
        protected override void OnInvoke(ScheduledTask task)
        {
            if (LockScreenManager.IsProvidedByCurrentApplication)
            {
                try
                {
                    var currentImage = LockScreen.GetImageUri();
                    string Imagename = string.Empty;
                    IsolatedStorageFile isoStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
                    string[] totalImags = isoStorageFile.GetFileNames("/Shared/ShellContent/");
                    

                    string imgCount = currentImage.ToString().Substring(currentImage.ToString().IndexOf('_') + 1, currentImage.ToString().Length - (currentImage.ToString().IndexOf('_') + 1)).Replace(".jpg", "");
                    Random miIdRand = new Random();
                    Imagename = string.Format("Img{0}.jpg", miIdRand.Next(1, totalImags.Length));
                    LockScreenChange(Imagename);

                    //ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(5));
                    Thread.Sleep(6000);
                    OnInvoke(task);
                }
                catch
                {
                    NotifyComplete();
                }
            }
            else
            {
                
                ScheduledActionService.Remove(task.Name);
            }            
        }

        private async void LockScreenChange(string filePathOfTheImage)
        {
            if (!LockScreenManager.IsProvidedByCurrentApplication)
            {
                
                await LockScreenManager.RequestAccessAsync();
            }            
            if (LockScreenManager.IsProvidedByCurrentApplication)
            {
                
               
                var uri = new Uri("ms-appdata:///Local/Shared/ShellContent/"+ filePathOfTheImage, UriKind.Absolute);
                
                LockScreen.SetImageUri(uri);
                  
            }
        }
    }
}