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
using System.Windows.Data;
using System.Windows.Input;

namespace costs
{
    public partial class DetailCost : PhoneApplicationPage
    {
        private CostsDataContext costsDB;
        protected int categoryId;
        protected string categoryName;
        protected float count;
        protected DateTime startDate;
        protected DateTime endDate;

        public DetailCost()
        {
            InitializeComponent();
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            this.DataContext = this;
            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            bool parsed = true;
            categoryId = 0;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryId")) parsed = Int32.TryParse(PhoneApplicationService.Current.State["DetailCost-categoryId"].ToString(), out categoryId);
            else parsed = false;
            categoryName = String.Empty;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryName")) categoryName = Convert.ToString(PhoneApplicationService.Current.State["DetailCost-categoryName"].ToString());
            else parsed = false;
            count = 0;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-count")) parsed = Single.TryParse(PhoneApplicationService.Current.State["DetailCost-count"].ToString(), out count);
            else parsed = false;
            startDate = DateTime.Now;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-startDate")) parsed = DateTime.TryParse(PhoneApplicationService.Current.State["DetailCost-startDate"].ToString(), out startDate);
            else parsed = false;
            endDate = DateTime.Now;
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-endtDate")) parsed = DateTime.TryParse(PhoneApplicationService.Current.State["DetailCost-endtDate"].ToString(), out endDate);
            else parsed = false;

            if (!parsed) NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            headerTitle.Text = ((count > 0) ? "Доходы" : "Расходы");
            headerCategory.Text = "Категория : " + categoryName;
            headerPeriod.Text = "Период :" + startDate.ToShortDateString() + "-" + endDate.ToShortDateString();
            // TODO: написать нормальное заполнение по условию
            BindConsListBox();
        }

        protected void BindConsListBox()
        {
            if (count > 0)
            {
                var costsDetailed = from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Date
                                            && consumptions.CreateDate.Date <= endDate.Date
                                            && consumptions.Count > 0
                                            && categories.CategoryId == categoryId
                                    select new
                                    {id = consumptions.ConsumptionId
                                     ,date = consumptions.CreateDate.Date.ToShortDateString()
                                     ,comment = consumptions.Comment
                                     ,count = consumptions.Count
                                     ,imagePhoto = (consumptions.Photo != null) ? new BitmapImage(new Uri("Assets/feature.camera.png", UriKind.Relative)) : null
                                     ,IsDeleted = consumptions.IsDeleted
                                    };
                consumptionsDetailListBox.ItemsSource = costsDetailed;
            }
            else
            {
                var costsDetailed = from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Date
                                            && consumptions.CreateDate.Date <= endDate.Date
                                            && consumptions.Count < 0
                                            && categories.CategoryId == categoryId
                                    select new
                                    {id = consumptions.ConsumptionId
                                     ,date = consumptions.CreateDate.Date.ToShortDateString()
                                     ,comment = consumptions.Comment
                                     ,count = consumptions.Count
                                     ,imagePhoto = (consumptions.Photo != null) ? new BitmapImage(new Uri("Assets/feature.camera.png", UriKind.Relative)) : null
                                     ,IsDeleted = consumptions.IsDeleted
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
        
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (window.IsOpen) window.IsOpen = false;
            else
            {
                clearCurrentState();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
            e.Cancel = true;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        { 
            bool deleted = Convert.ToBoolean(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("IsDeleted").GetValue(consumptionsDetailListBox.SelectedItem, null));
            if (!deleted)
            {
                int consumptionId = Convert.ToInt32(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("id").GetValue(consumptionsDetailListBox.SelectedItem, null));

                var photoConsumtion = from Consumption consumptions in costsDB.Consumptions
                                      where consumptions.ConsumptionId == consumptionId
                                      select consumptions.Photo;
                
                using (Stream photoStream = new MemoryStream(photoConsumtion.Single()))
                {
                    WriteableBitmap writeableBmp = BitmapFactory.New(1, 1).FromStream(photoStream);
                    WriteableBitmap rotated = writeableBmp.Rotate(90);
                    MemoryStream rotatedStream = new MemoryStream();
                    rotated.SaveJpeg(rotatedStream, 480, 640, 0, 100);

                    panZoom.Source = rotated;
                    panZoom.Height = 640;
                    panZoom.Width = 480;
                    window.IsOpen = true;
                }

            }
        }

        private void popupImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            window.IsOpen = false;
        }

        public void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            bool deleted = Convert.ToBoolean(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("IsDeleted").GetValue(consumptionsDetailListBox.SelectedItem, null));
            if (!deleted)
            {
                int consumptionId = Convert.ToInt32(consumptionsDetailListBox.SelectedItem.GetType().GetProperty("id").GetValue(consumptionsDetailListBox.SelectedItem, null));

                PhoneApplicationService.Current.State["addLossType"] = "edit";
                PhoneApplicationService.Current.State["addLossEditId"] = consumptionId.ToString();
                NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            deleteConsumption(true, ((MenuItem)sender));
            BindConsListBox();
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            deleteConsumption(false, ((MenuItem)sender));
            BindConsListBox();
        }

        protected void clearCurrentState()
        {
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryId")) PhoneApplicationService.Current.State.Remove("DetailCost-categoryId");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-categoryName")) PhoneApplicationService.Current.State.Remove("DetailCost-categoryName");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-count")) PhoneApplicationService.Current.State.Remove("DetailCost-count");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-startDate")) PhoneApplicationService.Current.State.Remove("DetailCost-startDate");
            if (PhoneApplicationService.Current.State.ContainsKey("DetailCost-endtDate")) PhoneApplicationService.Current.State.Remove("DetailCost-endtDate");
        }

        protected void deleteConsumption(bool removeValue, MenuItem menuItem)
        {
            int consumptionID = 0;
            if (Int32.TryParse(menuItem.Tag.ToString(), out consumptionID))
            {
                var updatingConsumption = (from Consumption consimptions in costsDB.Consumptions
                                           where consimptions.ConsumptionId == consumptionID
                                           select consimptions).Single();

                updatingConsumption.IsDeleted = removeValue;
                try
                {
                    costsDB.SubmitChanges();
                    MessageBox.Show((removeValue)?"Удалено":"Восстановлено");
                    NavigationService.Navigate(new Uri("/DetailCost.xaml", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
        }
    }

    public class DeletedConsumptionBrushconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NonDeletedConsumptionVisibilityconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OnlyDeletedConsumptionVisibilityconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}