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
        private ObservableCollection<Consumption> _consumptions;
        public ObservableCollection<Consumption> Consumptions
        {
            get
            {
                return _consumptions;
            }
            set
            {
                if (_consumptions != value)
                {
                    _consumptions = value;
                    NotifyPropertyChanged("Consumptions");
                }
            }
        }
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
            DayRbtn.IsChecked = true;
            fillConsmtiosList(DayRbtn.Name);            
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
        }

        protected void fillConsmtiosList(string groupType)
        {
            switch (groupType)
            {
                case "DayRbtn":
                    {
                    var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                           where consumptions.CreateDate.Date.Year == DateTime.Now.Year 
                                                    && consumptions.CreateDate.Date.Month == DateTime.Now.Month 
                                                    && consumptions.CreateDate.Date.Day == DateTime.Now.Day
                                           select consumptions;
                    Consumptions = new ObservableCollection<Consumption>(consumptionsInDB);
                }; break;
                case "month": ; break;
                case "year": ; break;
            }
            

            consumptionsListBox.ItemsSource = Consumptions;
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