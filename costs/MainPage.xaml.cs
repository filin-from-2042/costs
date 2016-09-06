using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using costs.Resources;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Collections.ObjectModel;
using System.Globalization;

namespace costs
{
    public partial class MainPage : PhoneApplicationPage
    {
        private CostsDataContext costsDB;

        // Конструктор
        public MainPage()
        {
            InitializeComponent();
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DateTextBlock.Text = DateTime.Now.ToString("D", new CultureInfo("ru-RU"));

            fillConsumptionsList(startRangeDP.Value, endRangeDP.Value);            
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
        }

        protected void fillConsumptionsList(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null) return;
            var consumptionsInDB = (from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Value.Date && consumptions.CreateDate.Date <= endDate.Value.Date && consumptions.Count > 0
                                    group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                    //select consumptionGroupped;
                                    select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) })
                                   .Union(
                                    from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Value.Date && consumptions.CreateDate.Date <= endDate.Value.Date && consumptions.Count < 0
                                    group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                    //select consumptionGroupped;
                                    select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) });
            //Consumptions = new ObservableCollection<Consumption>(consumptionsInDB);
            

            consumptionsListBox.ItemsSource = consumptionsInDB;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void consumptionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            ListBox lb = sender as ListBox;
            int categoryId = Convert.ToInt32(lb.SelectedItem.GetType().GetProperty("CategoryID").GetValue(lb.SelectedItem, null));
            string categoryName = Convert.ToString(lb.SelectedItem.GetType().GetProperty("ConsumptionCategory").GetValue(lb.SelectedItem, null));
            float count = Convert.ToSingle(lb.SelectedItem.GetType().GetProperty("SummCount").GetValue(lb.SelectedItem, null));
            string startDate = startRangeDP.Value.Value.Date.ToShortDateString();
            string endDate = endRangeDP.Value.Value.Date.ToShortDateString();

            NavigationService.Navigate(new Uri("/DetailCost.xaml?categoryId=" + categoryId.ToString() + "&categoryName=" + categoryName + "&count=" + count.ToString() + "&startDate=" + startDate + "&endtDate=" + endDate, UriKind.RelativeOrAbsolute));
        }

    }
}