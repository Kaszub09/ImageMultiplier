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

namespace ImageMultiplier
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String filepath=null;
        int width;
        int height;
        int percent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void multiplyButton_Click(object sender, RoutedEventArgs e)
        {

            if (filepath == null)
            {
                infoLabel.Content = "Choose the image.";
                return;
            }
            if(!Int32.TryParse(widthTextBox.Text, out width)|| !Int32.TryParse(heightTextBox.Text, out height)||width<1||height<1)
            {
                infoLabel.Content = "Incorrect width or height value.";
                return;
            }
            dialogBoxButton.IsEnabled = false;
            multiplyButton.IsEnabled = false;
            Bitmap output = await CreateBitmap();

            if (tintCheckBox.IsChecked == true)
            {
                if (!Int32.TryParse(percentTextBox.Text, out percent))
                {
                    infoLabel.Content = "Incorrect tint percentage.";
                    dialogBoxButton.IsEnabled = true;
                    multiplyButton.IsEnabled = true;
                    return;
                }
                if (percent < 0 || percent > 100)
                {
                    infoLabel.Content = "Incorrect tint percentage.";
                    dialogBoxButton.IsEnabled = true;
                    multiplyButton.IsEnabled = true;
                    return;
                }
                output = await Tint(new Bitmap(filepath), output, percent);
            }


            output.Save(filepath.Remove(filepath.Length- System.IO.Path.GetExtension(filepath).Length-1) + " "+width+"x"+height+".png",ImageFormat.Png);
            output.Dispose();
            infoLabel.Content = "Finished";
            dialogBoxButton.IsEnabled = true;
            multiplyButton.IsEnabled = true;
        }

        private async Task<Bitmap> CreateBitmap()
        {
            var progressStart = new Progress<int>(value => infoLabel.Content = "Processing image...");
            var progress = new Progress<int>(value => infoLabel.Content = "Multiplying " + value.ToString() + " %");
            Bitmap source=null;
            Bitmap output=null;
            await Task.Run(() =>
            {
                ((IProgress<int>)progressStart).Report(1);
                source = new Bitmap(filepath);
                while (source.Width * width > 16385 || source.Height * height > 16385)
                {
                    source = new Bitmap(source, new System.Drawing.Size(source.Width / 2, source.Height / 2));
                }

                output = new Bitmap(source.Width * width, source.Height * height);

                unsafe
                {
                    BitmapData sourceBmpData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    int* sourcePtr = (int*)sourceBmpData.Scan0;

                    BitmapData outputBmpData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    int* outputPtr = (int*)outputBmpData.Scan0;

                    for (int j = 0; j < height; j++)
                    {
                        for (int y = 0; y < source.Height; y++)
                        {
                            for (int i = 0; i < width; i++)
                            {
                                for (int x = 0; x < source.Width; x++)
                                {
                                    outputPtr[0] = sourcePtr[0];
                                    outputPtr++;
                                    sourcePtr++;
                                }
                                sourcePtr -= source.Width;
                            }
                            sourcePtr += source.Width;
                            ((IProgress<int>)progress).Report((y+1 + j * source.Height) * 100 / output.Height);
                            
                        }
                        sourcePtr = (int*)sourceBmpData.Scan0;
                    }
                    source.UnlockBits(sourceBmpData);
                    output.UnlockBits(outputBmpData);
                }
            });
            
            source.Dispose();
            return output;
        }

        private async Task<Bitmap> Tint(Bitmap input,Bitmap output, int percent)
        {
            var progress = new Progress<int>(value => infoLabel.Content = "Tinting " + value.ToString() + " %");
            input = new Bitmap(input, new System.Drawing.Size(output.Width, output.Height));
            await Task.Run(() =>
            {
                unsafe
                {
                BitmapData sourceBmpData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                byte* sourcePtr = (byte*)sourceBmpData.Scan0;

                BitmapData outputBmpData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                byte* outputPtr = (byte*)outputBmpData.Scan0;

                    for (int y = 0; y < input.Height; y++)
                    {
                        for (int x = 0; x < input.Width * 4; x++)
                        {
                            outputPtr[0] = (byte)(((100 - percent) * outputPtr[0] + percent * sourcePtr[0]) / 100);
                            outputPtr++;
                            sourcePtr++;

                        }
                            ((IProgress<int>)progress).Report(y * 100 / input.Height);
                    }
                input.UnlockBits(sourceBmpData);
                output.UnlockBits(outputBmpData);
                }
            });
            input.Dispose();
            return output;
        }

        

        private void dialogBoxButton_Click(object sender, RoutedEventArgs e)
        {          
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                filepath = openFileDialog.FileName;
                filenameLabel.Content = filepath;
            }             
        }
    }
}
