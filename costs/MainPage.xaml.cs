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
            else startRangeDP.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (PhoneApplicationService.Current.State.ContainsKey("endRangeDP"))
                endRangeDP.Value = Convert.ToDateTime(PhoneApplicationService.Current.State["endRangeDP"].ToString());
            
            var earnings = from Consumption consumptions in costsDB.Consumptions
                            join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                           where consumptions.CreateDate.Date >= startRangeDP.Value.Value.Date 
                           && consumptions.CreateDate.Date <= endRangeDP.Value.Value.Date 
                           && consumptions.Count > 0
                           && consumptions.IsDeleted == false
                            group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                            select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) };
            
            var consumptionsDB = from Consumption consumptions in costsDB.Consumptions
                                join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                where consumptions.CreateDate.Date >= startRangeDP.Value.Value.Date 
                                && consumptions.CreateDate.Date <= endRangeDP.Value.Value.Date
                                && consumptions.Count < 0
                                && consumptions.IsDeleted == false
                                group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                //select consumptionGroupped;
                                select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) };
           
            if (earnings.Count() > 0)
            {
                float allEarnSumm = earnings.AsEnumerable().Select(i => i.SummCount).Sum();
                earinignsText.Text = "Всего доходов на сумму: " + Math.Abs(allEarnSumm).ToString();
                earinignsText.Visibility = System.Windows.Visibility.Visible;
                earinignsText.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
            else earinignsText.Visibility = System.Windows.Visibility.Collapsed;

            if (consumptionsDB.Count() > 0)
            {
                float allCounsSumm = consumptionsDB.AsEnumerable().Select(i => i.SummCount).Sum();
                consumptionsText.Text = "Всего расходов на сумму: " + Math.Abs(allCounsSumm).ToString();
                consumptionsText.Visibility = System.Windows.Visibility.Visible;
                consumptionsText.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
            else consumptionsText.Visibility = System.Windows.Visibility.Collapsed;

            fillPieChart();
            fillConsumptionsList(startRangeDP.Value, endRangeDP.Value);            
            base.OnNavigatedTo(e);
        }

        //protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        //{
        //    e.Cancel = true;
        //    //base.OnBackKeyPress(e);
        //}

        //--------------------------------------------------- CONTROLS EVENTS ------------------------------------------------------------

        private void startRangeDP_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if (e.NewDateTime.Value.Date > endRangeDP.Value.Value.Date) { startRangeDP.Value = e.OldDateTime; return; }
            PhoneApplicationService.Current.State["startRangeDP"] = startRangeDP.Value.Value.Date.ToShortDateString();
        }

        private void endRangeDP_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if (e.NewDateTime.Value.Date < startRangeDP.Value.Value.Date) { endRangeDP.Value = e.OldDateTime; return; }
            PhoneApplicationService.Current.State["endRangeDP"] = endRangeDP.Value.Value.Date.ToShortDateString();
        }

        private void newRecord_Click(object sender, EventArgs e)
        {
            // предварительно чистим сохраненное состоянии контролов на странице добавления            
            if (PhoneApplicationService.Current.State.ContainsKey("currDateDP")) PhoneApplicationService.Current.State.Remove("currDateDP");
            if (PhoneApplicationService.Current.State.ContainsKey("countTxt")) PhoneApplicationService.Current.State.Remove("countTxt");
            if (PhoneApplicationService.Current.State.ContainsKey("commentTxt")) PhoneApplicationService.Current.State.Remove("commentTxt");
            if (PhoneApplicationService.Current.State.ContainsKey("categoryListPickerSI")) PhoneApplicationService.Current.State.Remove("categoryListPickerSI");   
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            ListBox lb = sender as ListBox;
            int categoryId = Convert.ToInt32(lb.SelectedItem.GetType().GetProperty("CategoryID").GetValue(lb.SelectedItem, null));
            string categoryName = Convert.ToString(lb.SelectedItem.GetType().GetProperty("ConsumptionCategory").GetValue(lb.SelectedItem, null));
            float count = Convert.ToSingle(lb.SelectedItem.GetType().GetProperty("SummCount").GetValue(lb.SelectedItem, null));
            string startDate = startRangeDP.Value.Value.Date.ToShortDateString();
            string endDate = endRangeDP.Value.Value.Date.ToShortDateString();

            PhoneApplicationService.Current.State["DetailCost-categoryId"] = categoryId.ToString();
            PhoneApplicationService.Current.State["DetailCost-categoryName"] = categoryName;
            PhoneApplicationService.Current.State["DetailCost-count"] = count.ToString();
            PhoneApplicationService.Current.State["DetailCost-startDate"] = startDate;
            PhoneApplicationService.Current.State["DetailCost-endtDate"] = endDate;
            NavigationService.Navigate(new Uri("/DetailCost.xaml?categoryId=" + categoryId.ToString() + "&categoryName=" + categoryName + "&count=" + count.ToString() + "&startDate=" + startDate + "&endtDate=" + endDate, UriKind.RelativeOrAbsolute));
        }

        //--------------------------------------------------------------- GENERAL FUNCTIONS --------------------------------------------------
        protected void fillConsumptionsList(DateTime? startDate, DateTime? endDate)
        {
            // TODO: сделать нормальный select new, при получении из ListBox получается неопнятный объект
            if (startDate == null || endDate == null) return;

            var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Value.Date 
                                    && consumptions.CreateDate.Date <= endDate.Value.Date
                                    && consumptions.Count < 0
                                    && consumptions.IsDeleted == false
                                    group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                    //select consumptionGroupped;
                                    select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) };

            consumptionsListBox.ItemsSource = consumptionsInDB;

            var earningsInDB = from Consumption consumptions in costsDB.Consumptions
                                join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                where consumptions.CreateDate.Date >= startDate.Value.Date
                                && consumptions.CreateDate.Date <= endDate.Value.Date
                                && consumptions.Count > 0
                                && consumptions.IsDeleted == false
                                group consumptions by new { categories.CategoryName, categories.CategoryId } into consumptionGroupped
                                //select consumptionGroupped;
                                select new { CategoryID = consumptionGroupped.Key.CategoryId, ConsumptionCategory = consumptionGroupped.Key.CategoryName, SummCount = consumptionGroupped.Sum(i => i.Count) };

            earningsListBox.ItemsSource = earningsInDB;
        }

        private void fillPieChart()
        {
            var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                   join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                   where consumptions.CreateDate.Date >= startRangeDP.Value.Value.Date 
                                   && consumptions.CreateDate.Date <= endRangeDP.Value.Value.Date
                                   && consumptions.Count < 0
                                    && consumptions.IsDeleted == false
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
            double part;

            List<PData> Data = new List<PData>();
            if (consumptionsInDB.Count() > 0)
            {
                foreach (var item in consumptionsInDB)
                {
                    part = (Math.Abs(item.SummCount) / (Math.Abs(allSumm)/100));
                    Data.Add(new PData { title = item.ConsumptionCategory.ToString(), value = Convert.ToInt32(Math.Round(part, 0)) });
                }
            }
            PieChart.DataContext = Data;
        }
    }
    public class PData
    {
        public string title {get;set;}
        public int value { get; set; }
    }
}