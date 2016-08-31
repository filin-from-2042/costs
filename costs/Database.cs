using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Collections.ObjectModel;

namespace costs
{

    public class CostsDataContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/Costs.sdf";

        // Pass the connection string to the base class.
        public CostsDataContext(string connectionString)
            : base(connectionString)
        { }

        public Table<Consumption> Consumptions;
        public Table<Category> Categories;
    }


    [Table]
    public class Consumption : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define ID: private field, public property and database column.
        private int _consumptionId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ConsumptionId
        {
            get
            {
                return _consumptionId;
            }
            set
            {
                if (_consumptionId != value)
                {
                    NotifyPropertyChanging("ConsumptionId");
                    _consumptionId = value;
                    NotifyPropertyChanged("ConsumptionId");
                }
            }
        }

        private float _count;

        [Column]
        public float Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (_count != value)
                {
                    NotifyPropertyChanging("Count");
                    _count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }
        
        private int _categoryId;

        [Column]
        public int CategoryId
        {
            get
            {
                return _categoryId;
            }
            set
            {
                if (_categoryId != value)
                {
                    NotifyPropertyChanging("CategoryId");
                    _categoryId = value;
                    NotifyPropertyChanged("CategoryId");
                }
            }
        }

        private string _userName;

        [Column]
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    NotifyPropertyChanging("UserName");
                    _userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        private DateTime _createDate;

        [Column]
        public DateTime CreateDate
        {
            get
            {
                return _createDate;
            }
            set
            {
                if (_createDate != value)
                {
                    NotifyPropertyChanging("CreateDate");
                    _createDate = value;
                    NotifyPropertyChanged("CreateDate");
                }
            }
        }

        private DateTime _updateDate;

        [Column]
        public DateTime UpdateDate
        {
            get
            {
                return _updateDate;
            }
            set
            {
                if (_updateDate != value)
                {
                    NotifyPropertyChanging("UpdateDate");
                    _updateDate = value;
                    NotifyPropertyChanged("UpdateDate");
                }
            }
        }
        
        //// Define completion value: private field, public property and database column.
        private byte[] _photo;

        [Column(DbType = "image")]
        public byte[] Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                if (_photo != value)
                {
                    NotifyPropertyChanging("Photo");
                    _photo = value;
                    NotifyPropertyChanged("Photo");
                }
            }
        }

        // Define completion value: private field, public property and database column.
        private string _comment;

        [Column]
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_comment != value)
                {
                    NotifyPropertyChanging("Comment");
                    _comment = value;
                    NotifyPropertyChanged("Comment");
                }
            }
        }

        // Define completion value: private field, public property and database column.
        private bool _isDeleted;

        [Column]
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (_isDeleted != value)
                {
                    NotifyPropertyChanging("IsDeleted");
                    _isDeleted = value;
                    NotifyPropertyChanged("IsDeleted");
                }
            }
        }

        // Version column aids update performance.
        //[Column(IsVersion = true)]
        //private Binary _version;
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
    

    [Table]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define ID: private field, public property and database column.
        private int _categoryId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int CategoryId
        {
            get
            {
                return _categoryId;
            }
            set
            {
                if (_categoryId != value)
                {
                    NotifyPropertyChanging("CategoryId");
                    _categoryId = value;
                    NotifyPropertyChanged("CategoryId");
                }
            }
        }

        // Define item name: private field, public property and database column.
        private string _categoryName;

        [Column]
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }
            set
            {
                if (_categoryName != value)
                {
                    NotifyPropertyChanging("CategoryName");
                    _categoryName = value;
                    NotifyPropertyChanged("CategoryName");
                }
            }
        }

        // Define item name: private field, public property and database column.
        private string _categoryDescription;

        [Column]
        public string CategoryDescription
        {
            get
            {
                return _categoryDescription;
            }
            set
            {
                if (_categoryDescription != value)
                {
                    NotifyPropertyChanging("CategoryDescription");
                    _categoryDescription = value;
                    NotifyPropertyChanged("CategoryDescription");
                }
            }
        }
        // Define completion value: private field, public property and database column.
        private bool _isDeleted;

        [Column]
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (_isDeleted != value)
                {
                    NotifyPropertyChanging("IsDeleted");
                    _isDeleted = value;
                    NotifyPropertyChanged("IsDeleted");
                }
            }
        }

        // Version column aids update performance.
        //[Column(IsVersion = true)]
        //private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    
}
