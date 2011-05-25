using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Elib;
using System.Drawing;

namespace WpfEpuckLayout {
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Window1 : Window {
    const double to = 0.2;
    const double imgto = 0.6;
    volatile bool colorful = false;
    volatile bool closed = false;
    /// <summary>
    /// Represents a logic of IR sennsors and lights,
    /// which are located on e-Puck's perimeter almost on identical places. 
    /// </summary>
    public class IRLight : INotifyPropertyChanged {
      int senProximity;
      string senName;
      bool isShining;
      Window1 win;
      /// <summary>
      /// Initializes a new instance of the <see cref="IRLight"/> class.
      /// </summary>
      /// <param name="name">The name.</param>
      /// <param name="val">The val.</param>
      /// <param name="isShining_">if set to <c>true</c> [is shining_].</param>
      /// <param name="win_">The win_.</param>
      public IRLight(string name, int val, bool isShining_,Window1 win_) {
        senName = name;
        senProximity = val;
        isShining = isShining_;
        win = win_;
      }
      /// <summary>
      /// Occurs when a property value changes.
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;
      /// <summary>
      /// Notifies the specified prop name.
      /// </summary>
      /// <param name="PropName">Name of the prop.</param>
      protected void Notify(string PropName) {
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs(PropName));
      }
      /// <summary>
      /// Gets or sets the sensors light.
      /// </summary>
      /// <value>The sen light.</value>
      public int SenProximity {
        get { return senProximity; }
        set { senProximity = value; Notify("SenLight"); Notify("SenGraph"); }
      }
      /// <summary>
      /// Gets the sen graph.
      /// </summary>
      /// <value>The sen graph.</value>
      public int SenGraph {
        get { return (senProximity * 3) / 5; }
      }
      /// <summary>
      /// Gets or sets the name of the sen.
      /// </summary>
      /// <value>The name of the sen.</value>
      public string SenName {
        get { return senName; }
        set { senName = value; Notify("SenName"); }
      }
      /// <summary>
      /// Gets or sets a value indicating whether this instance is shining.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is shining; otherwise, <c>false</c>.
      /// </value>
      public bool IsShining {
        get { return isShining; }
        set { isShining = value; setLedLight(win,senName,value?Turn.On:Turn.Off); Notify("IsShining"); }
      }
    }
    delegate void updateUIDelegate();

    /// <summary>
    /// array representing ir sensors and lights
    /// </summary>
    IRLight[] array;
    const int arrayLenght=8;
    /// <summary>
    /// representing center ir sensor and light
    /// </summary>
    IRLight bodylight_;
    IRLight frontlight_;
    Epuck ep;
    //sercom_prop
    /// <summary>
    /// Gets or sets the epuck instance.
    /// </summary>
    /// <value>The ep.</value>
    public Epuck Ep {
      set {
        ep = value;
        if (value != null) {
          setAktivState(true);
          epuckIniActions();
        } else
          setAktivState(false);
      }
      get { return ep; }
    }
    string filename;
    Thread t = null;
    EventWaitHandle canRefresh = null;
    //volatile int runningCall = 0;

    /// <summary>
    /// Avoid closing application if another thread is communication wit e-Puck. Initial state true does not have to block= nothing is sending
    /// </summary>
    /*
    EventWaitHandle running = null;
    object runningLock = new object();
    int RunningCall { 
      set {
          lock (runningLock)
          {
              runningCall = value;
              if (running != null)
              {
                  if (runningCall == 0) running.Set(); else running.Reset();
              }
          }
      }
        get { lock (runningLock) { return runningCall; } }
    }
     */
    Dispatcher guid = Dispatcher.CurrentDispatcher;

    /************************************* end of field members **************************/
    /// <summary>
    /// Initializes a new instance of the <see cref="Window1"/> class.
    /// </summary>
    public Window1() {      
      InitializeComponent();
      myInitializeEpuckLights();
      setAktivState(false);
    }

    /// <summary>
    /// conects visualisation and data of epuck lights and irs
    /// </summary>
    private void myInitializeEpuckLights() {
      string name = "ir";
      array = new IRLight[arrayLenght];
      const int iniValue = 50;
      for (int i = 0; i < 8; ++i) {
        array[i] = new IRLight(name + i.ToString(), iniValue, false, this);
      }
      //conects visualisation and data
      this.ir0v.DataContext = array[0];
      this.ir1v.DataContext = array[1];
      this.ir2v.DataContext = array[2];
      this.ir3v.DataContext = array[3];
      this.ir4v.DataContext = array[4];
      this.ir5v.DataContext = array[5];
      this.ir6v.DataContext = array[6];
      this.ir7v.DataContext = array[arrayLenght - 1];//should be arrayLenght -1

      bodylight_ = new IRLight("bodylight", 0, false, this);
      this.bodylight.DataContext = bodylight_;
      frontlight_ = new IRLight("frontlight", 0, false, this);
      frontlight.DataContext = frontlight_;
      LogBool.IsEnabled = false;
    }    
    /// <summary>
    /// Raises the <see cref="E:System.Windows.Window.Closed"/> event
    /// stop refreshing if refreshing sensor values is enabled and calls EpClose_Click(null, null), which 
    /// shut down the session with e-puck.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
    protected override void OnClosed(EventArgs e) {
        if (!closed)
        {
            closed = true;
            EpClose_Click(null, null);
            canRefresh.Set();
            Thread.Sleep(50);
            canRefresh = null;
            //lock (runningLock) { running = null; }
            //base.OnClosed(e);
        }
    }
    private void setAktivState(bool ak) {
      //nevim jakou vlastnost mam deaktivovat aby prvky nebyl "aktivni"
      BConnect.IsEnabled = !ak;
      rightPanel.IsEnabled = ak;
      JoystickPanel.IsEnabled = ak;
      LightPanel.IsEnabled = ak;
      refresh.IsEnabled = ak;
      getIR.IsEnabled = ak;
      BClose.IsEnabled = ak;
      LogBool.IsEnabled = ak;
      LogBool.IsChecked = false;
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e) {
      Microsoft.Win32.OpenFileDialog opf = new Microsoft.Win32.OpenFileDialog();
      Nullable<bool> result = opf.ShowDialog();
      if (result == true) 
        filename = opf.FileName;                
    }
    
    private void LogBool_Change(object sender, RoutedEventArgs e) {
      if (Ep != null) {
        if ((bool)LogBool.IsChecked) {
          if (filename != null) {
            if(Ep.LogStream!=null)
              Ep.LogStream.Close();
            Ep.LogStream = new StreamWriter(filename);
            Ep.StartLogging();            
          } else {
            MessageBox.Show("Specify a log file before attempting to log Elib Joystick commands");
            LogBool.IsChecked = false;
          }
        } 
        else
          Ep.StopLogging();
      } else {
        MessageBox.Show("For E-Puck logging has to be e-Puck connected");
        LogBool.IsChecked = false;
      }        
      
    }
    /// <summary>
    /// Handler for joystick panel, which gets the speed for wheels from joystick
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void Connect_Click(object sender, RoutedEventArgs e) {
        try {
            //running = new EventWaitHandle(true, EventResetMode.ManualReset);
            canRefresh = new EventWaitHandle(false, EventResetMode.ManualReset);
            string port = PortName.Text;
            Ep = new Epuck(port, "Epuck Monitor");

            closed = false;
            t = new Thread(updateAllSensors);
            t.Start();
            SetDefaultValues();
        } catch (ElibException) {
            notConfirmedCommand(this);
        }
    }
    private void SetDefaultValues() {
        try
        {
            IAsyncResult ar;
            ar = Ep.BeginStop(to, null, null);
            Ep.EndFtion(ar);
            Zoom z = Zoom.Small;
            CamMode m = CamMode.Color;
            switch (mode.SelectedIndex)
            {
                case 0: m = CamMode.BaW; break;
                case 1: m = CamMode.Color; break;
            }
            switch (zoom.SelectedIndex)
            {
                case 0: z = Zoom.Small; break;
                case 1: z = Zoom.Medium; break;
                case 2: z = Zoom.Big; break;
            }
            ar = Ep.BeginSetCam(int.Parse(width.Text), int.Parse(height.Text), z, m, to, null, null);
            Ep.EndFtion(ar);
            ar = Ep.BeginLightX(8, Turn.Off, to, null, null);
            Ep.EndFtion(ar);
        }
        catch (NullReferenceException) {
            notConfirmedCommand(this);
        }
        catch (ElibException)
        {
            notConfirmedCommand(this);
        }
    }
    private void EpClose_Click(object sender, RoutedEventArgs e) {
      updateUIDelegate d = delegate { refresh.IsChecked = false; LogBool.IsChecked = false; };
      guid.Invoke(DispatcherPriority.Send, d);
      if(Ep!=null)
        Ep.StopLogging();

      if(t!=null)
        t.Abort();            
      if (Ep != null)
        Ep.Dispose();
      Ep = null;
    }
    private void updateIR() {
      if (Ep != null) {
        try {
          IAsyncResult ar = Ep.BeginGetIR(to, null, null);
          int[] proxies = Ep.EndGetFtion(ar);
          for (int i = 0; i < 8; ++i) //range of SenProximity 0-100
            array[i].SenProximity = (proxies[i]) / 35;
        } catch (ElibException) {
          notConfirmedCommand(this);
        }
      }
    }
    private void getIR_Click(object sender, RoutedEventArgs e) {
      updateIR();
    }
    private static void setLedLight(Window1 win,string name, Turn how) {      
        try {
          IAsyncResult ar;
          switch (name) { 
            case "bodylight":ar = win.Ep.BeginBodyLight(how, to, null, null);
              break;
            case "frontlight":ar=win.Ep.BeginFrontLight(how,to,null,null);
              break;
            default:
                    int num = int.Parse(name.Substring(2));
            ar = win.Ep.BeginLightX(num, how, to, null, null);
            break;
          }                 
          win.Ep.EndFtion(ar);
        }
        catch (NullReferenceException) {
          //epuck is already null because of previous error or is uninitialized
          notConfirmedCommand(win);
        }
        catch (ElibException) {
          notConfirmedCommand(win);
        }      
    }
 
    private void refresh_Checked(object sender, RoutedEventArgs e) {
        if (canRefresh != null)
        {
            if (true == refresh.IsChecked)         
                canRefresh.Set();        
            else
                canRefresh.Reset();
        }
    }
    private void updateAllSensors() {      
      while(true){
        updateUIDelegate d1 = new updateUIDelegate(updateSen);
        updateUIDelegate d2 = new updateUIDelegate(updateImg);
        if (canRefresh != null)
        {
            canRefresh.WaitOne();
            Thread.Sleep(100);
            guid.Invoke(DispatcherPriority.Normal, d1);
            Thread.Sleep(100);
            guid.Invoke(DispatcherPriority.Normal, d2);
        }     
      }
    }
    private void updateSen(){
      //RunningCall++;
      GetAccelerometer_Click(null,null);
      getIR_Click(null,null);
      getir_Click(null,null);
      getmotors_Click(null,null);
      getmikes_Click(null,null);
      //RunningCall--;
    }
    private void updateImg() {
      //RunningCall++;
        try
        {
            IAsyncResult ar = Ep.BeginGetImage(imgto, null, null);
            Bitmap bmp = Ep.EndGetImage(ar);
            pic.Source = Convert2BitmapSource(bmp, colorful);
        }
        catch (ElibException) {
            notConfirmedCommand(this);
        }
      //RunningCall--;
    }

    static void notConfirmedCommand(Window1 win) {
      MessageBox.Show("Command has not been confirmed in timeout! Probably you are connected to another device or e-Puck has low batery");
      win.OnClosed(null);
    }


    private void getir_Click(object sender, RoutedEventArgs e) {
      if(Ep!=null)
      try {
        IAsyncResult ar = Ep.BeginGetIRData(to, null, null);
        string res = ArrToString(Ep.EndGetFtion(ar));
        irinfo.Content = res;
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }

    private void getmotors_Click(object sender, RoutedEventArgs e) {
      if(Ep!=null)
      try {
        IAsyncResult ar = Ep.BeginGetSpeed(to, null, null);
        int[] res = Ep.EndGetFtion(ar);
        leftmotor.Content = res[0];
        rightmotor.Content = res[1];
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }

    private void Motors_Click(object sender, RoutedEventArgs e) {/*pro button o rozmerech 200*200 a label stop 34*34  */
      if (Ep != null) {
        System.Windows.Point p = Mouse.GetPosition((Button)sender);
        double leftMotor = (p.X - 100) / 100;
        double rightMoror = (100 - p.Y) / 100;
        if (leftMotor > 1)
          leftMotor = 1;
        if (rightMoror > 1)
          rightMoror = 1;
        if (leftMotor < -1)
          leftMotor = -1;
        if (rightMoror < -1)
          rightMoror = -1;
        const double stop = 0.17;
        if ((-stop < leftMotor) & (leftMotor < stop) & (-stop < rightMoror) & (rightMoror < stop)) {
          leftMotor = 0;
          rightMoror = 0;
        }
        try {
          IAsyncResult ar = Ep.BeginMotors(leftMotor, rightMoror, to, null, null);
          Ep.EndFtion(ar);
        } catch (ElibException) {
          notConfirmedCommand(this);
        }
      }
    }
    private void getmikes_Click(object sender, RoutedEventArgs e) {
      if(Ep!=null)
      try {
        IAsyncResult ar = Ep.BeginGetMikes(to, null, null);
        int[] res = Ep.EndGetFtion(ar);
        string r = ArrToString(res);        
        mikes.Content = r;
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }
    static string ArrToString(int[] arr) {
      string r="";
      for (int i = 0; i < arr.Length; ++i)
        r += arr[i].ToString() + " ";
      return r;
    }
    private void setcam_Click(object sender, RoutedEventArgs e) {
      if(Ep!=null)
      try {
        Zoom z=Zoom.Small;
        CamMode m=CamMode.Color;
        switch(mode.SelectedIndex){
          case 0:m=CamMode.BaW;break;
          case 1:m=CamMode.Color;break;
        }
        switch(zoom.SelectedIndex){
          case 0:z=Zoom.Small;break;
          case 1:z=Zoom.Medium;break;
          case 2:z=Zoom.Big;break;
        }
        IAsyncResult ar = Ep.BeginSetCam(int.Parse(width.Text),int.Parse(height.Text),z,m, to, null, null);
        Ep.EndFtion(ar);
        colorful = CamMode.Color == m;
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }

    private void sound_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if(Ep!=null)
      try {
        IAsyncResult ar = Ep.BeginPlaySound(sound.SelectedIndex, to, null, null);
        Ep.EndFtion(ar);
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }

    private static BitmapSource Convert2BitmapSource(Bitmap bmp, bool color) {
      if (!color) {
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
      }
      //this transformation flip the bitmap for black and white pictures
      return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());                 
    }

    private void getpic_Click(object sender, RoutedEventArgs e) {
        updateImg();
    }
      

    private void epuckIniActions() {
      try {
        IAsyncResult ar = Ep.BeginGetSelector(to, null, null);
        selector.Content=Ep.EndGetFtion(ar)[0];
      } catch (ElibException) {
        notConfirmedCommand(this);
      }
    }

    private void GetAccelerometer_Click(object sender, RoutedEventArgs e) {
      if (Ep != null) {
        try {
          IAsyncResult ar = Ep.BeginGetAccelerometer(to, null, null);
          int[] v = Ep.EndGetFtion(ar);
          acc.Content = ArrToString(v);
        } catch (ElibException) {
          notConfirmedCommand(this);
        }
      }
    }
  }//end of Window
}
