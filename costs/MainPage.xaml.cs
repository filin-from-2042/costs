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
        //private ObservableCollection<Consumption> _consumptions;
        //public ObservableCollection<Consumption> Consumptions
        //{
        //    get
        //    {
        //        return _consumptions;
        //    }
        //    set
        //    {
        //        if (_consumptions != value)
        //        {
        //            _consumptions = value;
        //            NotifyPropertyChanged("Consumptions");
        //        }
        //    }
        //}

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
                                    where consumptions.CreateDate.Date >= startDate.Value.Date && consumptions.CreateDate.Date <= endDate.Value.Date && consumptions.Count>0
                                    group consumptions by categories.CategoryName into consumptionGroupped
                                    //select consumptionGroupped;
                                    select new { ConsumptionCategory = consumptionGroupped.Key, SummCount = consumptionGroupped.Sum(i => i.Count) })
                                   .Union(
                                    from Consumption consumptions in costsDB.Consumptions
                                    join Category categories in costsDB.Categories on consumptions.CategoryId equals categories.CategoryId
                                    where consumptions.CreateDate.Date >= startDate.Value.Date && consumptions.CreateDate.Date <= endDate.Value.Date && consumptions.Count<0
                                    group consumptions by categories.CategoryName into consumptionGroupped
                                    //select consumptionGroupped;
                                    select new { ConsumptionCategory = consumptionGroupped.Key, SummCount = consumptionGroupped.Sum(i => i.Count) });
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

    }
}