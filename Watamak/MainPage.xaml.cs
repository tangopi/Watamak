using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Watamak
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CanvasBitmap baseCanvas;

        public StorageFile baseFile { get; private set; }
        public StorageFile watermarkFile { get; private set; }
        public CanvasBitmap watermarkCanvas { get; private set; }
         double ACTWIDTH;
         double ACTHEIGHT;
        public MainPage()
        {
            this.InitializeComponent();
            canvasOfAvaga.Draw += CanvasOfAvaga_Draw;
            BrowseBaseButton.Click += BrowseBaseButton_Click;
            BrowseWatermark.Click += BrowseWatermark_Click;
            SaveWatermark.Click += SaveWatermark_Click1;

            
        }

        private void SaveWatermark_Click1(object sender, RoutedEventArgs e)
        {
            SaveTheForest();
        }

        async void SaveTheForest()
        {
            var displayInformation = DisplayInformation.GetForCurrentView();
            var imageSize = new Size(ACTWIDTH, ACTHEIGHT);
            canvasOfAvaga.Measure(imageSize);
            canvasOfAvaga.UpdateLayout();
            canvasOfAvaga.Arrange(new Rect(0, 0, imageSize.Width, imageSize.Height));

            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(canvasOfAvaga, Convert.ToInt32(imageSize.Width), Convert.ToInt32(imageSize.Height));
            //await renderTargetBitmap.RenderAsync(canvasOfAvaga, Convert.ToInt32(ACTWIDTH), Convert.ToInt32(ACTHEIGHT));

            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add(".jpg",new[] {".jpg"});
            var file = picker.PickSaveFileAsync();
            //var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("D:\\Screen.jpg", CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await (await picker.PickSaveFileAsync()).OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);

                encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                        (uint)renderTargetBitmap.PixelWidth,
                        (uint)renderTargetBitmap.PixelHeight,
                        displayInformation.LogicalDpi,
                        displayInformation.LogicalDpi,
                        pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }
        }

        private void SaveWatermark_Click(object sender, RoutedEventArgs e)
        {
            SaveTheForest();
        }

        private async void BrowseWatermark_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            watermarkFile = file;
            watermarkCanvas = await CanvasBitmap.LoadAsync(canvasOfAvaga.Device, await watermarkFile.OpenReadAsync(), 96, CanvasAlphaMode.Premultiplied);
            canvasOfAvaga.Invalidate();
            
        }

        private async void BrowseBaseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            baseFile = file;
            baseCanvas = await CanvasBitmap.LoadAsync(canvasOfAvaga.Device, await baseFile.OpenReadAsync(), 96, CanvasAlphaMode.Premultiplied);
            canvasOfAvaga.Invalidate();

            ACTHEIGHT = baseCanvas.SizeInPixels.Height;
            ACTWIDTH = baseCanvas.SizeInPixels.Width;
        }
        //like this? no?
        private void CanvasOfAvaga_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.Clear(Color.FromArgb(255, 190, 210, 39));
            if (baseCanvas != null) {
                args.DrawingSession.DrawImage(baseCanvas,new Rect(0,0,ACTWIDTH,ACTHEIGHT));
                    }
            if(watermarkCanvas != null)
            {
                args.DrawingSession.DrawImage(watermarkCanvas, new Rect(128, 128, 128, 128));
            }
        }  
    }
}

//using (Image image1 = System.Drawing.Image.FromFile("filename1"))
//using (Image image2 = System.Drawing.Image.FromFile("filename2"))
//using (Bitmap b = new Bitmap(image2.Width, image2.Height + image1.Height))
//using (Graphics g = Graphics.FromImage(b))
//{ 
//    g.CompositingMode = CompositingMode.SourceCopy; 
//    g.DrawImageUnscaled(image2, 0, 0);
//    g.DrawImageUnscaled(image1, 0, image2.Height);
       
//    b.Save(context.Response.OutputStream, ImageFormat.Png);
//}
