﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace costs
{
    public partial class NewCamera : PhoneApplicationPage
    {
        PhotoCamera cam;
        public NewCamera()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the phone.
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true)
            {
                // Otherwise, use standard camera on back of phone.
                cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                
                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                // Event is fired when the capture sequence is complete and a thumbnail image is available.
                cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable); 
                
                cam.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);

                viewfinderCanvas.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(focus_Tapped);

                //Set the VideoBrush source to the camera.
                viewfinderBrush.SetSource(cam);
                viewfinderTransform.Rotation = cam.Orientation;


                //resolutionPicker.ItemsSource = cam.AvailableResolutions;
                List<Size> resList = cam.AvailableResolutions.ToList<Size>();
                foreach(Size resolution in resList)
                {
                    RadioButton radioBtn = new RadioButton();
                    radioBtn.Content = resolution.Width.ToString() + 'x' + resolution.Height.ToString();
                    resolutionRadioContainer.Children.Add(radioBtn);
                }
                if (resolutionRadioContainer.Children.Count>0) ((RadioButton)resolutionRadioContainer.Children[0]).IsChecked = true;
            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
                cam.AutoFocusCompleted -= cam_AutoFocusCompleted;
            }
        }
        public void cam_CaptureCompleted(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(delegate()
            {
                string fileName = "cost-photo.jpg";
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(fileName))
                    {
                        using (IsolatedStorageFileStream rawStream = isf.OpenFile(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            WriteableBitmap writeableBmp = BitmapFactory.New(1, 1).FromStream(rawStream);
                            WriteableBitmap rotated = writeableBmp.Rotate(90);
                            MemoryStream rotatedStream = new MemoryStream();
                            rotated.SaveJpeg(rotatedStream, 480, 640, 1, 100);

                            panZoom.Source = rotated;
                            panZoom.Height = 640;
                            panZoom.Width = 480;
                            window.IsOpen = true;
                            fileSize.Text = "Размер файла: " + rawStream.Length.ToString() + " bytes";
                        }
                    }
                }
            });

        }

        void cam_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {

            this.Dispatcher.BeginInvoke(delegate()
            {
                focusBrackets.Visibility = Visibility.Collapsed;
                capturePhoto();
            });
        }

        // Informs when full resolution photo has been taken, saves to local media library and the local folder.
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            // TODO: сохранять в отдельную папку cache
            string fileName = "cost-photo.jpg";

            try
            {
                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);

                // Save photo as JPEG to the local folder.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isStore.FileExists(fileName))
                    {
                        isStore.DeleteFile(fileName);
                    }

                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to the local folder. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }

        // Informs when thumbnail photo has been taken, saves to the local folder
        // User will select this image in the Photos Hub to bring up the full-resolution. 
        public void cam_CaptureThumbnailAvailable(object sender, ContentReadyEventArgs e)
        {
            // TODO: сохранять в отдельную папку cache
            string fileName = "cost-photo-th.jpg";

            try
            {
                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);

                // Save thumbnail as JPEG to the local folder.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isStore.FileExists(fileName))
                    {
                        isStore.DeleteFile(fileName);
                    }

                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the thumbnail to the local folder. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }
        }

        private void capturePhoto()
        {

            if (cam != null)
            {
                try
                {
                    foreach (var element in resolutionRadioContainer.Children)
                    {
                        if (element.GetType().Name.Equals("RadioButton"))
                        {
                            RadioButton radio = (RadioButton)element;
                            if (radio.IsChecked==true)
                            {
                                foreach (Size resolition in cam.AvailableResolutions)
                                {
                                    if (radio.Content.Equals(resolition.Width.ToString() + "x" + resolition.Height.ToString()))
                                    {
                                        cam.Resolution = resolition;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        // Cannot capture an image until the previous capture has completed.
                        MessageBox.Show(ex.Message);
                    });
                }
            }
        }

        // Provide touch focus in the viewfinder.
        void focus_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                if (cam.IsFocusAtPointSupported == true)
                {
                    try
                    {
                        // Determine the location of the tap.
                        Point tapLocation = e.GetPosition(viewfinderCanvas);

                        // Position the focus brackets with the estimated offsets.
                        focusBrackets.SetValue(Canvas.LeftProperty, tapLocation.X - 30);
                        focusBrackets.SetValue(Canvas.TopProperty, tapLocation.Y - 28);

                        // Determine the focus point.
                        double focusXPercentage = tapLocation.X / viewfinderCanvas.Width;
                        double focusYPercentage = tapLocation.Y / viewfinderCanvas.Height;

                        // Show the focus brackets and focus at point.
                        focusBrackets.Visibility = Visibility.Visible;
                        cam.FocusAtPoint(focusXPercentage, focusYPercentage);

                    }
                    catch (Exception focusError)
                    {
                    }
                }
                else
                {
                }
            }
        }

        private void popupImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            reCapture();
        }

        private void savePhoto_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(delegate()
            {
                NavigationService.Navigate(new Uri("/AddLoss.xaml", UriKind.RelativeOrAbsolute));
            });        
        }

        private void removePhoto_Click(object sender, RoutedEventArgs e)
        {
            reCapture();
        }

        protected void reCapture()
        {
            string fullFile = "cost-photo.jpg";
            string thumbFile = "cost-photo-th.jpg";
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fullFile)) isf.DeleteFile(fullFile);
                if (isf.FileExists(thumbFile)) isf.DeleteFile(thumbFile);
            }
            window.IsOpen = false;
        }
    }
}