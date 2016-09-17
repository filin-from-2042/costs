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
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryId")) parsed = Int32.TryParse(PhoneApplicationService.Current.State["DetailCost-categoryId"].ToString(), out categoryId);
            else parsed = false;
            string categoryName = String.Empty;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryName")) categoryName = Convert.ToString(PhoneApplicationService.Current.State["DetailCost-categoryName"].ToString());
            else parsed = false;
            float count = 0;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-count")) parsed = Single.TryParse(PhoneApplicationService.Current.State["DetailCost-count"].ToString(), out count);
            else parsed = false;
            DateTime startDate = DateTime.Now;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-startDate")) parsed = DateTime.TryParse(PhoneApplicationService.Current.State["DetailCost-startDate"].ToString(), out startDate);
            else parsed = false;
            DateTime endDate = DateTime.Now;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-endtDate")) parsed = DateTime.TryParse(PhoneApplicationService.Current.State["DetailCost-endtDate"].ToString(), out endDate);
            else parsed = false;

            if (!parsed) NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
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
                                            && consumptions.IsDeleted == false
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
                                            && consumptions.IsDeleted == false
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
            clearCurrentState();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            clearCurrentState();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            e.Cancel = true;
            //base.OnBackKeyPress(e);
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

            PhoneApplicationService.Current.State["addLossType"] = "edit";
            PhoneApplicationService.Current.State["addLossEditId"] = consumptionId.ToString();
            NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            int consumptionID = 0;
            if (Int32.TryParse(((MenuItem)sender).Tag.ToString(), out consumptionID))
            {
                var updatingConsumption = (from Consumption consimptions in costsDB.Consumptions
                                           where consimptions.ConsumptionId == consumptionID
                                           select consimptions).Single();

                updatingConsumption.IsDeleted = true;
                try
                {
                    costsDB.SubmitChanges();
                    MessageBox.Show("Удалено");
                    NavigationService.Navigate(new Uri("/DetailCost.xaml", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        protected void clearCurrentState()
        {
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryId")) PhoneApplicationService.Current.State.Remove("DetailCost-categoryId");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryName")) PhoneApplicationService.Current.State.Remove("DetailCost-categoryName");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-count")) PhoneApplicationService.Current.State.Remove("DetailCost-count");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-startDate")) PhoneApplicationService.Current.State.Remove("DetailCost-startDate");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-endtDate")) PhoneApplicationService.Current.State.Remove("DetailCost-endtDate");
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            int consumptionID = 0;
            if (Int32.TryParse(((MenuItem)sender).Tag.ToString(), out consumptionID))
            {
                PhoneApplicationService.Current.State["addLossType"] = "edit";
                PhoneApplicationService.Current.State["addLossEditId"] = consumptionID.ToString();
                NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
            }

        }

    }
}