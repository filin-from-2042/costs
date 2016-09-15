using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;

namespace costs
{
    public partial class EditConsumption : PhoneApplicationPage
    {
        private CostsDataContext costsDB;
        PhotoChooserTask photoChooserTask;
        public EditConsumption()
        {
            InitializeComponent();
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            this.DataContext = this;
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            int consumptionId = 0;
            bool parsed = true;
            if (NavigationContext.QueryString.Keys.Contains("consumptionId")) parsed = Int32.TryParse(NavigationContext.QueryString["consumptionId"].ToString(), out consumptionId);

            if (!parsed) return;

            var costsDetailed = (from Consumption consumptions in costsDB.Consumptions
                                join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                where consumptions.ConsumptionId == consumptionId
                                select new
                                {
                                  id = consumptions.ConsumptionId
                                 ,category = categories.CategoryName
                                 ,date = consumptions.CreateDate
                                 ,comment = consumptions.Comment
                                 ,count = consumptions.Count
                                }).Single();
            date.Text = costsDetailed.date.Date.ToShortDateString();
            category.Text = costsDetailed.category;
            commentTxt.Text = costsDetailed.comment;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removePhoto_Click(object sender, RoutedEventArgs e)
        {
            //removePhotoISF();
        }

        private void libraryPhoto_Click_1(object sender, RoutedEventArgs e)
        {
            //saveCurrentState();
            photoChooserTask.Show();
        }
        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                removePhotoISF();
                savePhotoStreamToFile(e.ChosenPhoto, "cost-photo.jpg");
                WriteableBitmap thWBI = new WriteableBitmap(getBImageFromFile("cost-photo.jpg"));
                MemoryStream ms = new MemoryStream();
                thWBI.SaveJpeg(ms, 640, 480, 0, 100);
                savePhotoStreamToFile(ms, "cost-photo-th.jpg");
            }
        }

        //--------------------------------------------------------------- GENERAL FUNCTIONS --------------------------------------------------
        protected void removePhotoISF()
        {
            string fullFile = "cost-photo.jpg";
            string thumbFile = "cost-photo-th.jpg";
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fullFile)) isf.DeleteFile(fullFile);
                if (isf.FileExists(thumbFile)) isf.DeleteFile(thumbFile);
            }
            costImage.Source = new BitmapImage(new Uri("/Assets/feature.camera.png", UriKind.Relative));
        }
        protected byte[] getBytePhotoFromFile(string fileName)
        {
            byte[] readBuffer = null;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        readBuffer = new byte[rawStream.Length];
                        rawStream.Read(readBuffer, 0, readBuffer.Length);
                    }
                }
            }

            return readBuffer;
        }

        protected BitmapImage getBImageFromFile(string fileName)
        {
            BitmapImage img = new BitmapImage();
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        img.SetSource(rawStream);
                        costImage.Source = img;
                    }
                }
            }
            return img;
        }

        protected void savePhotoStreamToFile(Stream photoStream, string fileName)
        {
            photoStream.Seek(0, SeekOrigin.Begin);
            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isStore.FileExists(fileName))
                {
                    isStore.DeleteFile(fileName);
                }

                using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] readBuffer = new byte[4096];
                    int bytesRead = -1;
                    while ((bytesRead = photoStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        targetStream.Write(readBuffer, 0, bytesRead);
                    }
                }
            }

        }
    }
}