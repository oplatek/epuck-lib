using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Elib;
using System.Drawing;
using System.Diagnostics;
using System.IO;


using System.Windows.Forms;

namespace TestElib {
  partial class Program {

    ///<summary>Tests if you are able to control e-Puck via Bluetooth using a specific port!
    /// The ports are specific according settings of your computer. The two lines below shows the typical format.
    /// string port="/dev/rfcomm0";//typical port name in Linux
    /// string port = "COM4";//typical port name in Windows
    /// if your e-Puck moves, than everything is OK.</summary>      
    public static void TestPortTurnAround(string port) {
      SerialPort p = new SerialPort(port);//port e.g =="COM4"
      p.Open();
      for (int i = 0; i < 10; ++i) {
        p.Write("d,-100,100\r");//turn around
        Thread.Sleep(100);
      }
      for (int i = 0; i < 30; ++i) {
        p.Write("d,0,0\r");//turn around
        Thread.Sleep(100);
      }
      p.Close();
    }

    static volatile bool end;
    ///<summary> Simple but inefficient way of waiting. <see cref="M:Program.KofOkfWaiting(Epuck)"/> for usage of  <see cref="EventWaitHandle"/>.</summary>
    static void wait(int gap) {
      while (!end) { 
        Thread.Sleep(5); 
      } 
      end = false; 
      Console.WriteLine("Ended: {0}",Stamp.Get());
      Thread.Sleep(gap);
      Console.WriteLine("Start: {0}", Stamp.Get());
    }
    public static void ConsoleTestSensorsTimeout(Epuck ada, double myto) {
      Console.WriteLine("---------------- Test of sensors begins --------------");      
      int[] sensors = null;
      string info=null;
      end = true;
      wait(0);
      ada.GetHelpInfo(
        (values, nth) => { 
          Console.WriteLine("GetHelpInfo(..) OK "); info = values; end = true; },
        (nth) => { 
          Console.WriteLine("GetHelpInfo(..) KO"); end = true; },
        null, myto);
        wait(0);
      ada.GetAccelerometer(
        (values, data) => { Console.WriteLine("GetAccelerometer(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (data) => { Console.WriteLine("GetAccelerometer(..) KO"); end = true; }
        , null, myto);
      wait(0);      
      ada.GetVersionInfo(
        (values, data) => { Console.WriteLine("GetVersionInfo(..) OK {0}", values); info = values; end = true; },
        (data) => { Console.WriteLine("GetVersionInfo(..) KO"); end = true; }
        , null,myto);
      wait(0);       
      ada.GetCamParams(
        (values, nth) => { Console.WriteLine("GetCamParams(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetCamParams (..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.GetEncoders(
        (values, nth) => { Console.WriteLine("GetEncoders(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetEncoders (..) KO"); end = true; },
        null, myto);
      wait(0);      
      ada.GetImage(
        (img, nth) => { 
          Console.WriteLine("GetImage(..) OK "); end = true; },
        (img, nth) => { 
          Console.WriteLine("GetImage(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.GetIR(
        (values, data) => { Console.WriteLine("GetIR(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (data) => { Console.WriteLine("GetIR(..) KO"); end = true; }
        , null, myto);
      wait(0);      
      ada.GetIRData(
        (values, nth) => { Console.WriteLine("GetIRInfo(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetIRInfo (..) KO"); end = true; },
        null, myto);
      wait(0);      
      ada.GetLight(
        (values, data) => { Console.WriteLine("GetLight(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (data) => { Console.WriteLine("GetLight (..) KO"); end = true; }
        , null, myto);
      wait(0);
      ada.GetMikes(
        (values, nth) => { Console.WriteLine("Microphones(..) OK {0}",ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("Microphones (..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.GetSelector(
        (values, nth) => { Console.WriteLine("Selector(..) OK {0}",ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetSelector (..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.GetSpeed(
        (values, nth) => { Console.WriteLine("GetSpeed(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetSpeed (..) KO"); end = true; },
        null, myto);
      wait(0);
      Console.WriteLine("---------------- Test of sensors ends --------------");
    }
    public static void ConsoleTestActuatorsTimeout(Epuck ada, double myto) {
      Console.WriteLine("---------------- Test of actuators begins --------------");
      end = true;
      wait(0);
      // Takes a long time. Good to comment it.
      ada.Reset(
        (nth) => { Console.WriteLine("Reset(..) OK "); end = true; },
        (nth) => { Console.WriteLine("Reset(..) KO"); end = true; },
        null, myto);
      wait(1500);
      ada.BodyLight(Turn.Inv,
        (nth) => { Console.WriteLine("BodyLight(..) OK "); end = true; },
        (nth) => { Console.WriteLine("BodyLight(..) KO"); end = true; },
        null, myto);
      wait(0);      
      ada.CalibrateIRSensors(
        (nth) => { Console.WriteLine("CalibrateIRSensors(..) OK "); end = true; },
        (nth) => { Console.WriteLine("CalibrateIRSensors(..) KO"); end = true; },
        null, myto);
      wait(1000);
      
      ada.FrontLight(Turn.Inv,
        (nth) => { Console.WriteLine("FrontLight(..) OK "); end = true; },
        (nth) => { Console.WriteLine("FrontLight(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.LightX(8,Turn.Inv,
        (nth) => { Console.WriteLine("LightX(..) OK "); end = true; },
        (nth) => { Console.WriteLine("LightX(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.Motors(1,-1,
        (nth) => { Console.WriteLine("Motors(..) OK "); end = true; },
        (nth) => { Console.WriteLine("Motors(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.PlaySound(3,
        (nth) => { Console.WriteLine("PlaySound(..) OK "); end = true; },
        (nth) => { Console.WriteLine("PlaySound(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.SetCam(40,40,Zoom.Small,CamMode.BaW,
        (nth) => { Console.WriteLine("SetCam(..) OK "); end = true; },
        (nth) => { Console.WriteLine("SetCam(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.SetEncoders(3001,-3000,
        (nth) => { Console.WriteLine("SetEncoders(..) OK "); end = true; },
        (nth) => { Console.WriteLine("SetEncoders(..) KO"); end = true; },
        null, myto);
      wait(0);
      ada.Stop(
        (nth) => { Console.WriteLine("Stop(..) OK "); end = true; },
        (nth) => { Console.WriteLine("Stop(..) KO"); end = true; },
        null, myto);
      wait(0);
      Console.WriteLine("----------------End of Test actuators------- ");
    }

    public static void ConsoleAsynchronousSensors(Epuck ada, double myto){
      Console.WriteLine("---------------- Test of ConsoleAsynchronoutSensors begins at {0} --------------",Stamp.Get());
      //placeholders for string answers 
      string info = null;
      int[] sensors = null;
      ada.GetHelpInfo(
        (values, nth) => { 
          Console.WriteLine("GetHelpInfo(..) OK "); info = values; },
        (nth) => { 
          Console.WriteLine("GetHelpInfo(..) KO"); },
        null, 1*myto);
        
      ada.GetAccelerometer(
        (values, data) => { Console.WriteLine("GetAccelerometer(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (data) => { Console.WriteLine("GetAccelerometer(..) KO"); }
        , null, 2*myto);
            
      ada.GetVersionInfo(
        (values, data) => { Console.WriteLine("GetVersionInfo(..) OK {0}", values); info = values; },
        (data) => { Console.WriteLine("GetVersionInfo(..) KO"); }
        , null,3*myto);
             
      ada.GetCamParams(
        (values, nth) => { Console.WriteLine("GetCamParams(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (nth) => { Console.WriteLine("GetCamParams (..) KO"); },
        null, 4*myto);
      
      ada.GetEncoders(
        (values, nth) => { Console.WriteLine("GetEncoders(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (nth) => { Console.WriteLine("GetEncoders (..) KO"); },
        null, 4* myto);
            
      ada.GetIR(
        (values, data) => { Console.WriteLine("GetIR(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (data) => { Console.WriteLine("GetIR(..) KO"); }
        , null, 5*myto);
            
      ada.GetIRData(
        (values, nth) => { Console.WriteLine("GetIRInfo(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (nth) => { Console.WriteLine("GetIRInfo (..) KO"); },
        null, 6*myto);
            
      ada.GetLight(
        (values, data) => { Console.WriteLine("GetLight(..) OK {0}", ArrayToString<int>(values)); sensors = values; },
        (data) => { Console.WriteLine("GetLight (..) KO"); }
        , null,7* myto);
      
      ada.GetMikes(
        (values, nth) => { Console.WriteLine("Microphones(..) OK {0}",ArrayToString<int>(values)); sensors = values; },
        (nth) => { Console.WriteLine("Microphones (..) KO"); },
        null, 8*myto);
      
      ada.GetSelector(
        (values, nth) => { Console.WriteLine("Microphones(..) OK {0}",ArrayToString<int>(values)); sensors = values; },
        (nth) => { Console.WriteLine("Microphones (..) KO"); },
        null, 9*myto);
      //We have to synchronize at the end in order to avoid collisions of behaviour in next example functions.
      // GetSpeed elapses at last, because its has started the last from commands in ConsoleAsynchronousSensors and has biggest timeout to.
      end = false;
      ada.GetSpeed(
        (values, nth) => { Console.WriteLine("GetSpeed(..) OK {0}", ArrayToString<int>(values)); sensors = values; end = true; },
        (nth) => { Console.WriteLine("GetSpeed (..) KO"); end = true; },
        null, 10 * myto);
      //Prints time after asynchronous calls.
      Console.WriteLine("See difference between asynchronous call and the time when the callback is called!!!!!! Compare this time {0} with next End of ConsoleAsynchronoutSensors time !!!!!!!!!", Stamp.Get());
      wait(0);
      //Prints time after synchronization.
      Console.WriteLine("---------------- Test of ConsoleAsynchronoutSensors ends at {0} --------------",Stamp.Get());
    }

    #region ShowImage
    /// <summary>
    /// DEBUGGING FUNCTION!:
    /// It shows the image grabbed from e-Puck in a Windows Forms window. It has to be run in Single Thread Apartment.
    /// The opened window blocks the current thread until is closed. 
    /// </summary>
    /// <param name="e">The e.</param>
    [STAThread]
    public static void ShowImage(Epuck e) {
      //timeouts are big enough to set cam and get picture if the connection is working
      try {
        IAsyncResult ar = e.BeginSetCam(40, 40, Zoom.Small, CamMode.Color, 1, null, null);
        e.EndFtion(ar);
        ar = e.BeginGetImage(toImg, null, null);
        Bitmap bm = e.EndGetImage(ar);
        ShowBitmap(bm);
      } catch (ElibException ex){
        MessageBox.Show("The connection is damaged. Try reconnect reconnect to e-Puck. \n Reason: "+ex.Message);
      } 
    }

    public static void ShowBitmap(Bitmap bm) {
      //GUI
      Application.EnableVisualStyles();
      //Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1(bm));
      //end GUI
    }
    class Form1 : Form {
      private PictureBox pictureBox1;
      public Form1(Bitmap content) {
        InitializeComponent();
        pictureBox1.Image = content;//all you need to show the picture
      }

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
          components.Dispose();
        }
        base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent() {
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // pictureBox1
        // 
        this.pictureBox1.Location = new System.Drawing.Point(12, 12);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(268, 242);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.pictureBox1.TabIndex = 0;
        this.pictureBox1.TabStop = false;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(292, 266);
        this.Controls.Add(this.pictureBox1);
        this.Name = "Form1";
        this.Text = "Form1";
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

      }

      #endregion
    }

    #endregion ShowImage

  }
}

