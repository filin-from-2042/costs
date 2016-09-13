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
        public ObservableCollection<PData> Data = new ObservableCollection<PData>();
        // Конструктор
        public MainPage()
        {
            InitializeComponent();
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            this.DataContext = this;
        }

        //--------------------------------------------------- STANDART PAGE EVENTS -------------------------------------------------------
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DateTextBlock.Text = DateTime.Now.ToString("D", new CultureInfo("ru-RU"));
            if (PhoneApplicationService.Current.State.ContainsKey("startRangeDP"))
                startRangeDP.Value = Convert.ToDateTime(PhoneApplicationService.Current.State["startRangeDP"].ToString());
            if (PhoneApplicationService.Current.State.ContainsKey("endRangeDP"))
                endRangeDP.Value = Convert.ToDateTime(PhoneApplicationService.Current.State["endRangeDP"].ToString());

            else startRangeDP.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fillPieChart();
            fillConsumptionsList(startRangeDP.Value, endRangeDP.Value);            
            base.OnNavigatedTo(e);
        }

        //--------------------------------------------------- CONTROLS EVENTS ------------------------------------------------------------

        private void startRangeDP_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            PhoneApplicationService.Current.State["startRangeDP"] = startRangeDP.Value.Value.ToShortDateString();
        }

        private void endRangeDP_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            PhoneApplicationService.Current.State["endRangeDP"] = endRangeDP.Value.Value.ToShortDateString();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
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

        //--------------------------------------------------------------- GENERAL FUNCTIONS --------------------------------------------------


        protected void fillConsumptionsList(DateTime? startDate, DateTime? endDate)
        {
            // TODO: сделать нормальный select new, при получении из ListBox получается неопнятный объект
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
                                    select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) }
                                    );
            consumptionsListBox.ItemsSource = consumptionsInDB;
        }

        private void fillPieChart()
        {
            var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                   join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                   where consumptions.CreateDate.Date >= startRangeDP.Value.Value.Date && consumptions.CreateDate.Date <= endRangeDP.Value.Value.Date && consumptions.Count < 0
                                   group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                   select new
                                   {
                                       CategoryID = consumptionGroupped.Key.CategoryId
                                       ,
                                       ConsumptionCategory = consumptionGroupped.Key.CategoryName
                                       ,
                                       SummCount = consumptionGroupped.Sum(i => i.Count)
                                   };

            float allSumm = consumptionsInDB.AsEnumerable().Select(i => i.SummCount).Sum();
            float part;
            Data = new ObservableCollection<PData>();
            if (consumptionsInDB.Count() > 0)
            {
                foreach (var item in consumptionsInDB)
                {
                    part = (item.SummCount / (allSumm / 100));
                    Data.Add(new PData { title = item.ConsumptionCategory, value = Math.Round(part, 2) });
                }
                PieChart.Visibility = System.Windows.Visibility.Visible;
                PieChart.DataSource = Data;
            }
            else PieChart.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
    public class PData
    {
        public string title { get; set; }
        public double value { get; set; }
    }
}