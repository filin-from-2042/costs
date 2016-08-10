using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace costs
{
    public partial class AddLoss : PhoneApplicationPage
    {
        // Data context for the local database
        private CostsDataContext costsDB;
        // Define an observable collection property that controls can bind to.
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
                    //NotifyPropertyChanged("Consumptions");
                }
            }
        }
        public AddLoss()
        {
            InitializeComponent();

            // Connect to the database and instantiate data context.
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            // Data context and observable collection are children of the main page.
            this.DataContext = this;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            // Create a new to-do item based on the text box.
            Consumption newConsumption = new Consumption { Count = Convert.ToSingle(countTxt.Text) };

            // Add a to-do item to the observable collection.
            Consumptions.Add(newConsumption);

            // Add a to-do item to the local database.
            costsDB.Consumptions.InsertOnSubmit(newConsumption); 
        }
    }
}