﻿#pragma checksum "d:\DocKonov\Visual Studio 2012\Projects\costs.git\costs\AddLoss.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "497AF6CB7510B394583A706543F345DF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace costs {
    
    
    public partial class AddLoss : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Grid countRow;
        
        internal System.Windows.Controls.TextBox countTxt;
        
        internal System.Windows.Controls.Grid categoriesRow;
        
        internal Microsoft.Phone.Controls.ListPicker CategoriesListPicker;
        
        internal System.Windows.Controls.TextBox commentTxt;
        
        internal System.Windows.Controls.Image costImage;
        
        internal System.Windows.Controls.Button newPhoto;
        
        internal System.Windows.Controls.Button removePhoto;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/costs;component/AddLoss.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.countRow = ((System.Windows.Controls.Grid)(this.FindName("countRow")));
            this.countTxt = ((System.Windows.Controls.TextBox)(this.FindName("countTxt")));
            this.categoriesRow = ((System.Windows.Controls.Grid)(this.FindName("categoriesRow")));
            this.CategoriesListPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("CategoriesListPicker")));
            this.commentTxt = ((System.Windows.Controls.TextBox)(this.FindName("commentTxt")));
            this.costImage = ((System.Windows.Controls.Image)(this.FindName("costImage")));
            this.newPhoto = ((System.Windows.Controls.Button)(this.FindName("newPhoto")));
            this.removePhoto = ((System.Windows.Controls.Button)(this.FindName("removePhoto")));
        }
    }
}

