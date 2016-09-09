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
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Windows.Input;

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
        
        // сохранение нового расхода
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                int categoryId = ((Category)CategoriesListPicker.SelectedItem).CategoryId;
                string userComment = commentTxt.Text.Equals("Комментарий") ? "" : commentTxt.Text;
                if (countTxt.Text.Equals("Сумма")) { MessageBox.Show("Не указана сумма!"); return; }
                float inputCount = Convert.ToSingle(countTxt.Text.Replace(',', '.'));

                string fileName = "cost-photo.jpg";
                byte[] readBuffer = null;
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(fileName))
                    {
                        using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            readBuffer = new byte[rawStream.Length];
                            rawStream.Read(readBuffer, 0, readBuffer.Length);
                        }
                    }
                }
                Consumption newConsumption = new Consumption { Count = inputCount
                                                            , CategoryId = categoryId
                                                            , UserName = "Test"
                                                            , CreateDate = currDateDP.Value.Value
                                                            , UpdateDate = currDateDP.Value.Value
                                                            , IsDeleted = false
                                                            , Photo = (readBuffer!=null) ? readBuffer : null
                                                            , Comment = userComment };
                Consumptions.Add(newConsumption);
                costsDB.Consumptions.InsertOnSubmit(newConsumption);

                costsDB.SubmitChanges();
                MessageBox.Show("Сохранено");
                removePhotoISF();
                currDateDP.Value = DateTime.Now;
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

            // TODO: хранить наименование временного файла в общедоступном хранилище
            string fileName = "cost-photo-th.jpg";
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        BitmapImage img = new BitmapImage();
                        img.SetSource(rawStream);
                        costImage.Source = img;
                    }
                }
                else
                    costImage.Source = new BitmapImage(new Uri("/Assets/feature.camera.png", UriKind.Relative));
            }

            // Call the base method.
            base.OnNavigatedTo(e);
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

        private void newPhoto_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewCamera.xaml", UriKind.RelativeOrAbsolute));
        }

        private void commentTxt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // focus the page in order to remove focus from the text box
                // and hide the soft keyboard
                this.Focus();
            }
        }

        private void removePhoto_Click(object sender, RoutedEventArgs e)
        {
            removePhotoISF();
        }

        protected void removePhotoISF()
        {
            string fullFile = "cost-photo.jpg";
            string thumbFile = "cost-photo-th.jpg";
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fullFile)) isf.DeleteFile(fullFile);
                if (isf.FileExists(thumbFile)) isf.DeleteFile(thumbFile);
            }
            costImage.Source = new BitmapImage(new Uri("/Assets/feature.camera.png", UriKind.Relative));
        }

    }
}