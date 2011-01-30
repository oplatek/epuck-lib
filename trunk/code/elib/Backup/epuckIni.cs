using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Elib {
  //Epuck class should be subclassable
  public partial class Epuck:IDisposable{
    /// <summary>
    /// units: [cm]
    /// </summary>
    public const double WheelDiameter = 4.1;
    /// <summary>
    /// Distance between wheels of an e-Puck in cm.
    /// </summary>
    public const double Perch = 5.3;
    /// <summary>
    /// 13cm/s is a maximum speed of e-Puck. In Elib 13cm/s corresponds to 1.00. From -1.00 to 1.00 is the speed linearly growing.
    /// </summary>
    public const double MaxSpeed = 13;
    /// <summary>
    /// Eight Infra Red sensors are placed on the perimeter of e-Puck, which can be obtained on the 
    /// instance e of e-Puck by <see cref="Epuck.BeginGetIRSensors(IAsyncResult)">e.BeginGetIRSensors(..)</see> method
    /// or by <c>e.GetIRSensors(..)</c> method.
    /// IrSensorsDegrees describes the degrees measured from front(There is a cam.) As you can see most of the sensors are on the front side of e-Puck.
    /// </summary>
    public static readonly int[] IRSensorsDegrees = new int[8] { 10, 30, 90, 170, 190, 270, 330, 350 };
    static Dictionary<string, string> logFunctionNames = new Dictionary<string, string>();
    /// <summary>It gets the BTCom version, which is a static property. </summary>
    /// <value>The BTCom version.</value>
    public static string BTComVersion { get { return Sercom.Version; } }
    /// <summary> It gets the BTCom help. </summary>
    /// <value>The BTCom help from static property.</value>
    public static string BTComHelp { get { return Sercom.Help; } }
    static Epuck() {
      logFunctionNames.Add("SetEncoders(..)", "SetEncoders(..)");
      logFunctionNames.Add("CalibrateIRSensors(..)","CalibrateIRSensors(..)");
      logFunctionNames.Add("Stop(..)","Stop(..)" );
      logFunctionNames.Add("Reset()","Reset()" );
      logFunctionNames.Add("Motors(..)","Motors(..)" );
      logFunctionNames.Add("LightX(..)","LightX(..)" );
      logFunctionNames.Add("BodyLight(..)","BodyLight(..)" );
      logFunctionNames.Add("FrontLight(..)","FrontLight(..)" );
      logFunctionNames.Add("SetCam(..)","SetCam(..)" );
      logFunctionNames.Add("PlaySound(..)","PlaySound(..)" );
      logFunctionNames.Add("GetHelpInfo(..)", "GetHelpInfo(..)");
      logFunctionNames.Add("BTComVersion(..)","BTComVersion(..)" );
      logFunctionNames.Add("GetImage(..)","GetImage(..)" );
      logFunctionNames.Add("IrSensors(..)","IrSensors(..)" );
      logFunctionNames.Add("GetIR(..)","GetIR(..)" );
      logFunctionNames.Add("GetIRData(..)", "GetIRData(..)");
      logFunctionNames.Add("GetAccelerometer(..)","GetAccelerometer(..)" );
      logFunctionNames.Add("GetSelector(..)","GetSelector(..)" );
      logFunctionNames.Add("GetSpeed(..)","GetSpeed(..)" );
      logFunctionNames.Add("GetCamParams(..)","GetCamParams(..)" );
      logFunctionNames.Add("GetLight(..)","GetLight(..)" );
      logFunctionNames.Add("GetEncoders(..)", "GetEncoders(..)");
      logFunctionNames.Add("GetMikes(..)", "GetMikes(..)");
      logFunctionNames.Add("BeginCalibrateIRSensors(..)","BeginCalibrateIRSensors(..)" );
      logFunctionNames.Add("BeginMotors(..)","BeginMotors(..)" );
      logFunctionNames.Add("BeginGetMikes(..)","BeginGetMikes(..)" );
      logFunctionNames.Add("BeginGetLight(..)","BeginGetLight(..)" );
      logFunctionNames.Add("BeginStop(..)","BeginStop(..)" );
      logFunctionNames.Add("BeginReset(..)","BeginReset(..)" );
      logFunctionNames.Add("BeginFrontLight(..)","BeginFrontLight(..)" );
      logFunctionNames.Add("BeginLightX(..)","BeginLightX(..)" );
      logFunctionNames.Add("BeginBodyLight(..)","BeginBodyLight(..)" );
      logFunctionNames.Add("BeginGetIRData(..)","BeginGetIRData(..)" );
      logFunctionNames.Add("BeginEpuckHelp(..)","BeginEpuckHelp(..)" );
      logFunctionNames.Add("BeginSetCam(..)","BeginSetCam(..)" );
      logFunctionNames.Add("BeginPlaySound(..)","BeginPlaySound(..)" );
      logFunctionNames.Add("BeginGetInfoVersion(..)","BeginGetInfoVersion(..)" );
      logFunctionNames.Add("BeginGetIR(..)", "BeginGetIR(..)");      
      logFunctionNames.Add("BeginGetAccelerometer(..)","BeginGetAccelerometer(..)");
      logFunctionNames.Add("BeginGetSelector(..)","BeginGetSelector(..)");
      logFunctionNames.Add("BeginGetSpeed(..)", "BeginGetSpeed(..)");
      logFunctionNames.Add("BeginGetCamParams(..)","BeginGetCamParams(..)");
      logFunctionNames.Add("BeginGetEncoders(..)","BeginGetEncoders(..)");
      logFunctionNames.Add("BeginGetImage(..)","BeginGetImage(..)");            
    }

    
    volatile bool log;
    TextWriter w;
    string name;
    string port;
    Sercom ser;
    bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Epuck"/> class.
    /// </summary>
    /// <param name="Port">The port.</param>
    /// <param name="Name">Choose a name for your robot! It is usefull for logging.</param>
    public Epuck(string Port, string Name) {
      disposed = false;
      name = Name;
      port = Port;
      ser = new Sercom(Port, Sercom.defWriteTimeout, Sercom.defReadTimeout);
      ser.Start();     
    }

    enum Action { call, ok, ko };
    void logf(Action a, string method) {
      if (log)
        w.WriteLine("{0:F8} {1} {2} {3} {4} {5}", Stamp.Get(), name, a.ToString(), logFunctionNames[method], ser.NotSent, ser.NotAnswered);
    }

    /// <summary>
    /// WriteToLogStream adds to LogStream a comment. (Before passed strings puts '#'.)
    /// </summary>
    /// <param name="comment"></param>
    public void WriteToLogStream(string comment) { 
      if(w==null)
        throw new ElibException("Specify LogStream before attempting to log!");      
      w.WriteLine("#"+comment);
    }
    

    #region Dispose_fce

    /// <summary>
    /// It releases unmanaged(Serial Port in <see cref="Sercom"/> and access to log file) and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
      if (!disposed) {
        if (w != null) {
          w.Flush();
          w.Close();
        }
        disposed = true;
        ser.Dispose();
        ser=null;
        /*if (wlog != null)
          wlog.Close();
        if(disp!=null)
          disp.Close();
         */ 
        if (disposing) { //nth to dispose for subclasses
          //todo do I miss to dispose some  managed objects
        }
      }
    }
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// It takes under 0.5 s.
    /// </summary>
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="Epuck"/> is reclaimed by garbage collection.
    /// </summary>
    ~Epuck() {
      Dispose(false);
    }
    #endregion
   
  }
  
}