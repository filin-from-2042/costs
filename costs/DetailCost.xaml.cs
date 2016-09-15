using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace costs
{
    public partial class DetailCost : PhoneApplicationPage
    {
        private CostsDataContext costsDB;
        public DetailCost()
        {
            InitializeComponent();
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            bool parsed = true;
            int categoryId = 0;
            if (NavigationContext.QueryString.Keys.Contains("categoryId")) parsed = Int32.TryParse(NavigationContext.QueryString["categoryId"].ToString(), out categoryId);
            string categoryName = String.Empty;
            if (NavigationContext.QueryString.Keys.Contains("categoryName")) categoryName = Convert.ToString(NavigationContext.QueryString["categoryName"].ToString());
            float count = 0;
            if (NavigationContext.QueryString.Keys.Contains("count")) parsed = Single.TryParse(NavigationContext.QueryString["count"].ToString(), out count);
            DateTime startDate = DateTime.Now;
            if (NavigationContext.QueryString.Keys.Contains("startDate")) parsed = DateTime.TryParse(NavigationContext.QueryString["startDate"].ToString(), out startDate);
            DateTime endDate = DateTime.Now;
            if (NavigationContext.QueryString.Keys.Contains("endDate")) parsed = DateTime.TryParse(NavigationContext.QueryString["endDate"].ToString(), out endDate);

            if (!parsed) return;
            headerTitle.Text = ((count > 0) ? "Доходы" : "Расходы");
            headerCategory.Text = "Категория : " + categoryName ;
            headerPeriod.Text = "Период :" + startDate.ToShortDateString() + "-" + endDate.ToShortDateString();

            // TODO: написать нормальное заполнение по условию
            if (count > 0)
            {
                var costsDetailed = from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Date
                                            && consumptions.CreateDate.Date <= endDate.Date
                                            && consumptions.Count > 0
                                            && categories.CategoryId == categoryId
                                    select new {id=consumptions.ConsumptionId
                                                ,date = consumptions.CreateDate
                                                ,comment = consumptions.Comment
                                                , count = consumptions.Count
                                                ,imagePhoto = (consumptions.Photo != null) ? new BitmapImage(new Uri("Assets/feature.camera.png", UriKind.Relative)) : null
                                    };
                consumptionsDetailListBox.ItemsSource = costsDetailed;
            }
            else
            {
               var  costsDetailed = from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Date
                                            && consumptions.CreateDate.Date <= endDate.Date
                                            && consumptions.Count < 0
                                            && categories.CategoryId == categoryId
                                    select new
                                    {id = consumptions.ConsumptionId
                                     ,date = consumptions.CreateDate
                                     ,comment = consumptions.Comment
                                     , count = consumptions.Count
                                     , imagePhoto = (consumptions.Photo != null) ? new BitmapImage(new Uri("Assets/feature.camera.png", UriKind.Relative)) : null
                                    };
                consumptionsDetailListBox.ItemsSource = costsDetailed;
            }
        }

        protected BitmapImage getBmImage(byte[] photo)
        {
            using (Stream stream = new MemoryStream(photo))
            {
                BitmapImage img = new BitmapImage();
                img.SetSource(stream);
                return img;
            }
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int consumptionId = Convert.ToInt32(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("id").GetValue(consumptionsDetailListBox.SelectedItem, null));

            var photoConsumtion = from Consumption consumptions in costsDB.Consumptions
                                  where consumptions.ConsumptionId == consumptionId
                                  select getBmImage(consumptions.Photo);

            //popupImage.Source = photoConsumtion.Single();
            
            WriteableBitmap thWBI = new WriteableBitmap(photoConsumtion.Single());
            MemoryStream ms = new MemoryStream();
            thWBI.SaveJpeg(ms, 640, 480, 0,100);
            BitmapImage popNewImage = new BitmapImage();
            
            //popupImage.Stretch = Stretch.UniformToFill;
            popupImage.Source = thWBI;
            popupImage.Height = 640;
            popupImage.Width = 480;
            ApplicationBar.IsVisible = false;
            pImage.IsOpen = true;          
        }

        private void popupImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image image = sender as Image;
            Popup popup = image.Parent as Popup;
            popup.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int consumptionId = Convert.ToInt32(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("id").GetValue(consumptionsDetailListBox.SelectedItem, null));

            NavigationService.Navigate(new Uri("/EditConsumption.xaml?consumptionId="+consumptionId.ToString(), UriKind.RelativeOrAbsolute));
        }
    }
}