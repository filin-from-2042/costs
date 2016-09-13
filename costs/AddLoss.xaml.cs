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
        PhotoChooserTask photoChooserTask;
        //--------------------------------------------------- STANDART PAGE EVENTS -------------------------------------------------------

        public AddLoss()
        {
            InitializeComponent();

            // Connect to the database and instantiate data context.
            costsDB = new CostsDataContext(CostsDataContext.DBConnectionString);
            // Data context and observable collection are children of the main page.
            this.DataContext = this;
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            saveCurrentState();
            base.OnNavigatedFrom(e);
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var consumptionsInDB = from Consumption consumptions in costsDB.Consumptions
                                   select consumptions;
            Consumptions = new ObservableCollection<Consumption>(consumptionsInDB);

            if (PhoneApplicationService.Current.State.ContainsKey("currDateDP")) currDateDP.Value = Convert.ToDateTime(PhoneApplicationService.Current.State["currDateDP"].ToString());

            // либо восставнавливаем их хранилища, либо это первый раз и выставляем по параметру
            if (PhoneApplicationService.Current.State.ContainsKey("countTxt"))
            {
                countTxt.Text = PhoneApplicationService.Current.State["countTxt"].ToString();
                countTxt.Foreground = new SolidColorBrush(Colors.Black);
                float inputNumber = 0F;
                if (Single.TryParse(countTxt.Text, out inputNumber))
                {
                    if (inputNumber > 0) fillOutCategories("EARNINGS");
                    else fillOutCategories("CONSUMPTION");
                }
                if (PhoneApplicationService.Current.State.ContainsKey("categoryListPickerSI"))
                {
                    CategoriesListPicker.SelectedIndex = Convert.ToInt32(PhoneApplicationService.Current.State["categoryListPickerSI"]);
                }
            }
            else
            {
                string type = "";
                if (NavigationContext.QueryString.TryGetValue("type", out type))
                {
                    type = type.ToUpper();
                    if(type.Equals("CONSUMPTION"))
                    {
                        countTxt.Text = "-" ;
                        countTxt.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    fillOutCategories(type);
                }
                else fillOutCategories("EARNINGS");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("commentTxt"))
            {
                commentTxt.Text = PhoneApplicationService.Current.State["commentTxt"].ToString();
                commentTxt.Foreground = new SolidColorBrush(Colors.Black);
            }

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


        //--------------------------------------------------- CONTROLS EVENTS -----------------------------------------------------

        // сохранение нового расхода
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: убарать из categories пустую категория и сделать как полагается
                int categoryId = ((Category)CategoriesListPicker.SelectedItem).CategoryId;
                if (categoryId == 1) throw new Exception("Не указана категория!");

                string userComment = commentTxt.Text.Equals("Комментарий") ? "" : commentTxt.Text;
                if (countTxt.Text.Equals("Сумма")) throw new Exception("Не указана сумма!"); 

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
                                                            , CreateDate = currDateDP.Value.Value.Date
                                                            , UpdateDate = currDateDP.Value.Value.Date
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
                clearCurrentState();
                CategoriesListPicker.SelectedIndex = 0;
                commentTxt.Text = "Комментарий";
                commentTxt.Foreground = new SolidColorBrush(Colors.Gray);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            clearCurrentState();
            removePhotoISF();
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
            else 
            {
                float inputNumber = 0F;
                if (Single.TryParse(countTxt.Text, out inputNumber))
                {
                    if (inputNumber > 0) fillOutCategories("EARNINGS");
                    else fillOutCategories("CONSUMPTION");
                }
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
            saveCurrentState();
            NavigationService.Navigate(new Uri("/NewCamera.xaml", UriKind.RelativeOrAbsolute));
        }

        private void commentTxt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void removePhoto_Click(object sender, RoutedEventArgs e)
        {
            removePhotoISF();
        }

        private void libraryPhoto_Click_1(object sender, RoutedEventArgs e)
        {
            saveCurrentState();
            photoChooserTask.Show();
        }


        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                removePhotoISF();
                savePhotoStreamToFile(e.ChosenPhoto, "cost-photo.jpg");
                WriteableBitmap thWBI = new WriteableBitmap(getBImageFromFile("cost-photo.jpg"));
                MemoryStream ms = new MemoryStream();
                thWBI.SaveJpeg(ms, 640, 480, 0, 100);
                savePhotoStreamToFile(ms, "cost-photo-th.jpg");
            }
        }

        //--------------------------------------------------------------- GENERAL FUNCTIONS --------------------------------------------------
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

        protected void fillOutCategories(string type)
        {
            var categoriesInDB = from Category cat in costsDB.Categories where cat.CategoryType == type || cat.CategoryType == "GENERAL" select cat;
            Categories = new ObservableCollection<Category>(categoriesInDB);
            CategoriesListPicker.ItemsSource = Categories;
        }

        protected byte[] getBytePhotoFromFile(string fileName)
        {
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

            return readBuffer;
        }

        protected BitmapImage getBImageFromFile(string fileName)
        {
            BitmapImage img = new BitmapImage();
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        img.SetSource(rawStream);
                        costImage.Source = img;
                    }
                }
            }
            return img;
        }

        protected void savePhotoStreamToFile(Stream photoStream, string fileName)
        {
            photoStream.Seek(0, SeekOrigin.Begin);
            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isStore.FileExists(fileName))
                {
                    isStore.DeleteFile(fileName);
                }

                using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] readBuffer = new byte[4096];
                    int bytesRead = -1;
                    while ((bytesRead = photoStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        targetStream.Write(readBuffer, 0, bytesRead);
                    }
                }
            }
        
        }

        protected void saveCurrentState()
        {
            if (currDateDP.Value != null) PhoneApplicationService.Current.State["currDateDP"] = currDateDP.Value.Value.Date.ToShortDateString();
            if (!String.IsNullOrEmpty(countTxt.Text) && !countTxt.Text.ToString().Equals("Сумма")) PhoneApplicationService.Current.State["countTxt"] = countTxt.Text;
            if (CategoriesListPicker.SelectedIndex > 0) PhoneApplicationService.Current.State["categoryListPickerSI"] = CategoriesListPicker.SelectedIndex;
            if (!String.IsNullOrEmpty(commentTxt.Text) && !commentTxt.Text.ToString().Equals("Комментарий")) PhoneApplicationService.Current.State["commentTxt"] = commentTxt.Text;            
        }

        protected void clearCurrentState()
        {
            if (PhoneApplicationService.Current.State.ContainsKey("currDateDP")) PhoneApplicationService.Current.State.Remove("currDateDP");
            if (PhoneApplicationService.Current.State.ContainsKey("countTxt")) PhoneApplicationService.Current.State.Remove("countTxt");
            if (PhoneApplicationService.Current.State.ContainsKey("commentTxt")) PhoneApplicationService.Current.State.Remove("commentTxt");
            if (PhoneApplicationService.Current.State.ContainsKey("categoryListPickerSI")) PhoneApplicationService.Current.State.Remove("categoryListPickerSI");   
        }

        private void currDateDP_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            PhoneApplicationService.Current.State["currDateDP"] = e.NewDateTime.ToString();
        }

    }
}