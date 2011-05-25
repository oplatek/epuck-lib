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
    // delegate == typed variable for functions
    delegate void updateUIDelegate();
    // Dispatcher updates gui controls via functions pased as updateUIDelegate variables
    Dispatcher guid = Dispatcher.CurrentDispatcher;

    // Thread for continuous updating of sensors
    Thread t = null;
    // Synchronisation for "endless" loop that performes continuous update of sensors
    EventWaitHandle canRefresh = null;
    // signals whether connection was interrupted
    volatile bool closed = false;

    // filename for logging commands and answers
    string filename;

    // to - timeout - maximum time for waiting to an answer
    const double to = 0.4;
    const double imgto = 1.0;

    // Type of image requested from e-Puck
    volatile bool colorful = false;
    
    // IRLight class connects gui representation and values find out from e-Puck
    IRLight[] array;
    const int arrayLenght = 8;
    IRLight bodylight_;
    IRLight frontlight_;

    // Epuck class from Elib.dll wraps communication with e-Puck
    Epuck ep;

    #region Constructor and startup stuff

    // Wraper of ep
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
    
    // constructor    
    public Window1() {      
      InitializeComponent();
      myInitializeEpuckLights();
      setAktivState(false);
    }
    
    // conects visualisation and data of epuck lights and irs    
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

    #endregion Constructor and start up staff

    #region Turning on and off session with e-Puck

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
        try {
            IAsyncResult ar;
            ar = Ep.BeginStop(to, null, null);
            Ep.EndFtion(ar);
            Zoom z = Zoom.Small;
            CamMode m = CamMode.Color;
            switch (mode.SelectedIndex) {
                case 0: m = CamMode.BaW; break;
                case 1: m = CamMode.Color; break;
            }
            switch (zoom.SelectedIndex) {
                case 0: z = Zoom.Small; break;
                case 1: z = Zoom.Medium; break;
                case 2: z = Zoom.Big; break;
            }
            ar = Ep.BeginSetCam(int.Parse(width.Text), int.Parse(height.Text), z, m, to, null, null);
            Ep.EndFtion(ar);
            ar = Ep.BeginLightX(8, Turn.Off, to, null, null);
            Ep.EndFtion(ar);
        } catch (NullReferenceException) {
            notConfirmedCommand(this);
        } catch (ElibException) {
            notConfirmedCommand(this);
        }
    }

    private void EpClose_Click(object sender, RoutedEventArgs e) {
        updateUIDelegate d = delegate { refresh.IsChecked = false; LogBool.IsChecked = false; };
        guid.Invoke(DispatcherPriority.Send, d);
        if (Ep != null)
            Ep.StopLogging();

        if (t != null)
            t.Abort();
        if (Ep != null)
            Ep.Dispose();
        Ep = null;
    }

    static void notConfirmedCommand(Window1 win) {
        MessageBox.Show("Command has not been confirmed in timeout! Probably you are connected to another device or e-Puck has low batery");
        win.AbortSession();
    }
    
    /*
    // Raises the System.Windows.Window.Closed event
    // stop refreshing if refreshing sensor values is enabled and calls EpClose_Click(null, null)
    // that shut down the session with e-puck.    
    protected override void OnClosed(EventArgs e) {
        if (!closed) {
            closed = true;
            EpClose_Click(null, null);
            canRefresh.Set();
            Thread.Sleep(50);
            canRefresh = null;            
        }
    }
    */

    // Calls EpClose_Click(null, null) and stop refreshing    
    public void AbortSession() {
        if (!closed) {
            closed = true;
            EpClose_Click(null, null);
            canRefresh.Set();
            Thread.Sleep(50);
            canRefresh = null;
        }
    }

    private void setAktivState(bool ak) {      
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

    #endregion Turning on and off session with e-Puck

    #region Sending commands

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
        try {
            IAsyncResult ar = Ep.BeginGetImage(imgto, null, null);
            Bitmap bmp = Ep.EndGetImage(ar);
            pic.Source = Convert2BitmapSource(bmp, colorful);
        } catch (ElibException) {
            notConfirmedCommand(this);
        }
        //RunningCall--;
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

    static string ArrToString(int[] arr) {
        string r = "";
        for (int i = 0; i < arr.Length; ++i)
            r += arr[i].ToString() + " ";
        return r;
    }

#endregion Sending commands

    /// <summary>
    /// Connects gui and inner representation of IR sennsors and lights,
    /// which are located on e-Puck's perimeter almost on identical places. 
    /// </summary>
    public class IRLight : INotifyPropertyChanged {
        int senProximity;
        string senName;
        bool isShining;
        Window1 win;
    
        public IRLight(string name, int val, bool isShining_, Window1 win_) {
            senName = name;
            senProximity = val;
            isShining = isShining_;
            win = win_;
        }
        // Occurs when a property value changes.      
        public event PropertyChangedEventHandler PropertyChanged;
        // Notifies the specified property name.      
        protected void Notify(string PropName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropName));
        }
        public int SenProximity {
            get { return senProximity; }
            set { senProximity = value; Notify("SenLight"); Notify("SenGraph"); }
        }
        // binds the senProximity and its gui representation
        public int SenGraph {
            get { return (senProximity * 3) / 5; }
        }
        // sensor name
        public string SenName {
            get { return senName; }
            set { senName = value; Notify("SenName"); }
        }
        // we put together in one control IR sensors and LEDs, because they are placed on similar places on e-Puck
        public bool IsShining {
            get { return isShining; }
            set { isShining = value; setLedLight(win, senName, value ? Turn.On : Turn.Off); Notify("IsShining"); }
        }
    } //end of IRLight class    

  }//end of Window class

}//end of namespace
