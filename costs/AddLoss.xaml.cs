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
using System.Windows.Media;

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
                    NotifyPropertyChanged("Consumptions");
                }
            }
        }        
        private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    NotifyPropertyChanged("Categories");
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
            try
            {
                int categoryId = ((Category)CategoriesListPicker.SelectedItem).CategoryId;
                string userComment = commentTxt.Text.Equals("Комментарий") ? "" : commentTxt.Text;
                if (countTxt.Text.Equals("Сумма")) { MessageBox.Show("Не указана сумма!"); return; }
                float inputCount = Convert.ToSingle(countTxt.Text.Replace(',', '.'));
                Consumption newConsumption = new Consumption { Count = inputCount
                                                            ,CategoryId = categoryId
                                                            , UserName = "Test"
                                                            , CreateDate = DateTime.Now
                                                            , UpdateDate = DateTime.Now
                                                            , IsDeleted = false
                                                            , Comment = userComment };
                Consumptions.Add(newConsumption);
                costsDB.Consumptions.InsertOnSubmit(newConsumption);

                costsDB.SubmitChanges();
                MessageBox.Show("Сохранено");
                countTxt.Text = "Сумма";
                countTxt.Foreground = new SolidColorBrush(Colors.Gray);
                CategoriesListPicker.SelectedIndex = 0;
                commentTxt.Text = "Комментарий";
                commentTxt.Foreground = new SolidColorBrush(Colors.Gray);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                   select consumptions;
            Consumptions = new ObservableCollection<Consumption>(consumptionsInDB);

            var categoriesInDB = from Category cat in costsDB.Categories select cat;
            Categories = new ObservableCollection<Category>(categoriesInDB);
            CategoriesListPicker.ItemsSource = Categories;

            // Call the base method.
            base.OnNavigatedTo(e);
        }
        //protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    // Call the base method.
        //    base.OnNavigatedFrom(e);

        //    try
        //    {
        //        costsDB.SubmitChanges();
        //        MessageBox.Show("Сохранено");
        //        countTxt.Text = "Сумма";
        //        countTxt.Foreground = new SolidColorBrush(Colors.Gray);
        //        commentTxt.Text = "Комментарий";
        //        commentTxt.Foreground = new SolidColorBrush(Colors.Gray);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
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

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));

        }

        private void countTxt_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (countTxt.Text.Equals("Сумма"))
            {
                countTxt.Text = String.Empty;
                countTxt.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void countTxt_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(countTxt.Text))
            {
                countTxt.Text = "Сумма";
                countTxt.Foreground = new SolidColorBrush(Colors.Gray);
            }

        }

        private void commentTxt_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (commentTxt.Text.Equals("Комментарий"))
            {
                commentTxt.Text = String.Empty;
                commentTxt.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void commentTxt_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(commentTxt.Text))
            {
                commentTxt.Text = "Комментарий";
                commentTxt.Foreground = new SolidColorBrush(Colors.Gray);            
            }
        }
    }
}