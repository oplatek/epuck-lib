using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Elib;
using System.IO;

namespace TestElib {
  partial class  Program{
    ///////////////////////////////// tips and tricks ////////////////////////////////////
    #region tips and tricks

    /* * //useful function for WPF application in .Net 3.5 and higher, because there is used BitmapSource instead of Bitmap
    public static BitmapSource convertBitmap(System.Drawing.Bitmap source) {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
    }
    * */

    /// <summary>
    ///  Useful function for logging or debugging, prints the value from a array using ToString() function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static string ArrayToString<T>(T[] arr) {
      StringBuilder b = new StringBuilder();
      b.AppendFormat("{0}[{1}]=(", arr.ToString(), arr.Length);
      for (int i = 0; i < arr.Length; ++i)
        b.AppendFormat("{0},", arr[i].ToString());
      b.Remove(b.Length - 1, 1);
      b.Append(")\n");
      return b.ToString();
    }

    public static void KofOkfWaiting() { 
      //todo
      throw new NotImplementedException();
    }

    #region Example of Image Processing

    public static void ShowProcessedImage(Epuck ada) {
      IAsyncResult ar = ada.BeginSetCam(40, 40, Zoom.Small, CamMode.Color, 0.4, null, null);
      ada.EndFtion(ar);
      for (int i = 0; i < 3; ++i) {
        ar = ada.BeginGetImage(1, null, null);
        Bitmap bm = ada.EndGetImage(ar);
        processImage(bm);
        ShowBitmap(bm);
      }
    }
    private static void processImage(Bitmap bm) {
      double rdown = 1, rup = 1, gdown = 0, gup = 0.7, bdown = 0.0, bup = 0.7;
      double red, green, blue;
      for (int i = 0; i < bm.Width; ++i) {
        for (int j = 0; j < bm.Height; ++j) {
          Color c = bm.GetPixel(i, j);
          int darkness = Math.Max(Math.Max(c.R, c.G), c.B);
          red = c.R / (double)darkness; green = c.G / (double)darkness; blue = c.B / (double)darkness;
          if (rdown <= red && red <= rup && gdown <= green && green <= gup && bdown <= blue && blue <= bup)
            bm.SetPixel(i, j, Color.White);
          else
            bm.SetPixel(i, j, Color.Black);
        }
      }
    }
    #endregion Example of Image Processing

    public static void LoggingExample(Epuck ada,string name) {
      //to the name is added current date in order to use 
      string n=name;
      int i = 1;
      while (File.Exists(n)) {
        n = name + i.ToString();
        i++;
      }      
      ada.LogStream = new StreamWriter(new FileStream(n, FileMode.OpenOrCreate, FileAccess.Write));
      //starts logging
      ada.StartLogging();
      for(i = 0; i < 2; ++i)
       ConsoleTestSensorsTimeout(ada, 1.0 / (i+1));
      ada.StopLogging();
    }

    #endregion tips and tricks

  }
}
