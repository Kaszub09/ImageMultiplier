using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls.Primitives;
using ImageMultiplierLibrary;


namespace ImageMultiplier
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<String> filepaths = null;
        int width;
        int height;
        int percent;
        String tintSource = null;


        public MainWindow()
        {
            InitializeComponent();
            this.MinHeight = 300;
            this.MinWidth = 500;
            tintCheckBox_Click(null, null);
        }

        private async void multiplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (filepaths == null)
            {
                infoLabel.Content = "Choose the image.";
                return;
            }
            if(!Int32.TryParse(widthTextBox.Text, out width)|| !Int32.TryParse(heightTextBox.Text, out height)||width<1||height<1)
            {
                infoLabel.Content = "Incorrect width or height value.";
                return;
            }
            if (tintCheckBox.IsChecked == true) {
                if (!Int32.TryParse(percentTextBox.Text, out percent) || percent < 0 || percent > 100) {
                    infoLabel.Content = "Incorrect tint percentage.";
                    return;
                }
                if (!(bool)tintWithSourceRadioButton.IsChecked) {
                      if (tintSource == null) {
                        infoLabel.Content = "Choose tint image.";
                        return;
                      }
                }
            }
            RelevantButtonVisible(false);

            foreach (string file in filepaths) {
                infoLabel.Content = "Processing file " + System.IO.Path.GetFileName(file);
                Bitmap output = await Task.Run(() => ImageMultiplierClass.Multiply(new Bitmap(file), width, height));
                RelevantButtonVisible(true);
                if (tintCheckBox.IsChecked == true) {
                    if ((bool)tintWithSourceRadioButton.IsChecked) {
                        output = await Task.Run(() => ImageMultiplierClass.Tint(output, new Bitmap(file), ((float)percent) / 100));
                    }
                    else {
                        output = await Task.Run(() => ImageMultiplierClass.Tint(output, new Bitmap(tintSource), ((float)percent) / 100));
                    }
                }
                output.Save(file.Remove(file.Length - System.IO.Path.GetExtension(file).Length - 1) + " " + width + "x" + height + ".png", ImageFormat.Png);
            }
            infoLabel.Content = "Finished.";
            RelevantButtonVisible(true);
        }

        private void RelevantButtonVisible(bool visible)
        {
            dialogBoxButton.IsEnabled = visible;
            multiplyButton.IsEnabled = visible;
        }


        private void dialogBoxButton_Click(object sender, RoutedEventArgs e)
        {          
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                filenameLabel.Content = "";
                filepaths = new List<string>(openFileDialog.FileNames);
                foreach(string file in filepaths) {
                    filenameLabel.Content = filenameLabel.Content + System.IO.Path.GetFileName(file) + ", ";
                }            
            }             
        }

        private void dialogTintBoxButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                tintSource = openFileDialog.FileName;
                tintSourceLabel.Content = tintSource;
            }
        }

        private void tintCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool flag = tintCheckBox.IsChecked.HasValue ? tintCheckBox.IsChecked.Value : false;
            if (!flag)
            {

                tintGrid.Height = 0;
                Thickness t = multiplyButton.Margin;
                t.Top = 110;
                multiplyButton.Margin = t;
                this.MinHeight =250;
                Application.Current.MainWindow.Height -=50;
            }
            else
            {
                tintGrid.Height = 70;
                Thickness t = multiplyButton.Margin;
                t.Top = 150;
                multiplyButton.Margin = t;
                Application.Current.MainWindow.Height += 50;
                this.MinHeight = 300;

            }
            
           
        }
    }
}
