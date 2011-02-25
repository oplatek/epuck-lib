//#define MYDEBUG
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Elib {
  /// <summary>
  /// Sercom wraps serial communication with epuck. Main qoal is to keep application responsive, although serial communication could be very irresponsive.
  /// </summary>
  /// <remarks>
  /// More commands doesn't mean better control. Consider following example. We want to accelerate fluently from 0 to 1 speed in 100steps(0,01;0,02;....;0,98;1,00)
  /// using Epuck.Motors(double,double,OkfActuators,KofCallback,object,double). 
  /// We send commands to BTCom program on e-Puck.
  /// <see cref="Sercom"/> puts the commands in queue and the commands will be sent in with gaps greater than L, because the commands wait until the previous command is confirmed. 
  /// Therefor the acceleration lasts more than 100*L. If Epuck.Motors(double,double,OkfActuators,KofCallback,object,double) is called
  /// with gaps smaller than L, the acceleration is slower than it should be.
  /// Luckily most of commands are confirmed even with low battery until 0.1 seconds, but calibration of IR sensors, reseting e-Puck and shooting an image takes
  /// much more time. Calibration of IR sensors takes more than 1 second. Reseting e-Puck last more than 1.5 s and capturing a colourful image 40 * 40 takes around 0.5 s.
  /// </remarks>
  public partial class Sercom:IDisposable {
    #region public functions:

    /// <summary>
    /// Initializes a new instance of the <see cref="Sercom"/> class.
    /// </summary>
    /// <param name="portName">Name of the serial port.</param>
    /// <param name="serialPortWriteTimeout">The serial port write time out.</param>
    /// <param name="serialPortReadTimeout">The serial port read time out.</param>
    public Sercom(string portName, int serialPortWriteTimeout, int serialPortReadTimeout) {
      try {
        port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
        port.DiscardNull = false;
        port.ReceivedBytesThreshold = 1;
        port.DataReceived += new SerialDataReceivedEventHandler(Read);
        if (serialPortWriteTimeout < 0)
          port.WriteTimeout = SerialPort.InfiniteTimeout;
        else
          port.WriteTimeout = serialPortWriteTimeout;
        if (serialPortReadTimeout < 0)
          port.ReadTimeout = SerialPort.InfiniteTimeout;
        else
          port.ReadTimeout = serialPortReadTimeout;

      } catch (Exception e) {
        throw new SerialPortException("Opening port problem: " + portName, e);
      }
      
      //interesting part
      chN = new Thread(CheckNS);
      chN.IsBackground = true;            
      notSended.NonEmpty += new EventHandler(Send);      
      chS = new Thread(checkSD);
      chS.IsBackground = true;
      Received += new EventHandler(Send); 
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sercom"/> class with default values.
    /// </summary>
    /// <param name="portName">Name of the serial port.</param>
    public Sercom(string portName) :     
      this(portName, defWriteTimeout, defReadTimeout) { }
        
    /// <summary>
    /// Opens the Bluetooth connection and turn the BTCom protocol on.
    /// </summary>
    public void Start() {
      port.Open();  
      while (!port.IsOpen)
        Thread.Sleep(10);            
      port.Write(Commands.c_Stop());
      Thread.Sleep(100);
      chS.Start();
      chN.Start();
      ready = true;
#if MYDEBUG
      this.Write(Commands.c_Stop(), (ans, nth) => Console.WriteLine("start ok2"), null, null, 10); 
      Thread.Sleep(20);
      this.Write(Commands.c_Stop(), (ans,nth)=>Console.WriteLine("start ok2"),null, null,10);
      Thread.Sleep(20);
#else      
      this.Write(Commands.c_Stop(), null,null, null,0.09);
      Thread.Sleep(110);
#endif
    }

    /// <summary>
    /// Writes the specified command to notSended queue until it can be sent over serial connection.
    /// Each command waits until the commands before him arer sent and confirmed or
    /// their timeout elaps.
    /// ///
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="okf">The received callback.</param>
    /// <param name="kof">The not received callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="timeout">The timeout after, which is called notrecieved callback and the command is considered undelivered.</param>
    /// <remarks>writes tests if input command is in range of characters for sercom protocol,
    /// but it doesn't ensures good structure after first character! </remarks>
    public void Write(string command, OkfCallback okf, KofCallback kof,object state, double timeout) {
      if (!disposed) {
        if (timeout == double.NaN || timeout == double.PositiveInfinity)
          throw new TimeoutElibException("Use timeout smaller than positive infinity!");
        if (!ready)
          throw new UnconnectedException("You can't use instance of e-puck, if it is not started");
        ansGuard p = new ansGuard(new OkfCallbackDef(okf, state), new KofCallbackDef(kof, state), command);        
        node np = new node(p);
        lock (notSendedLock) {
          p.timeout = timeout + Stamp.Get();
          p.nSST = notSendedSTS.Enqueue(np);
          p.nS = notSended.Enqueue(np);          
          NotAnswered++;                        
        }
        checkNSwhSet();       
      }
    } 
    
    #region IDisposable Members

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="Sercom"/> is reclaimed by garbage collection.
    /// </summary>
    ~Sercom() { Dispose(false);  }
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// It takes under 0.5 s for e-Puck.
    /// </summary>
    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    
    #endregion
    

    #region Properties
    /// <summary>
    /// The width of last taken picture.
    /// </summary>
    public int WidthImg { get { return width; } }
    /// <summary>
    /// The height of last taken picture. 
    /// </summary>
    public int HeightImg { get { return height; } }
    /// <summary>
    /// The colour mode of last taken picture. It can be black and white or colourful.
    /// </summary>
    public int ModeImg { get { return mode; } }
    /// <summary>
    /// If an image in LastImg is set to <c>true</c>, <see cref="Sercom.FullImgBytes"/> stores image. If it returns <c>false</c> , 
    /// a picture has not been taken yet or
    /// <see cref="Sercom.FullImgBytes"/> are saving a picture right now.
    /// </summary>
    public bool FullImgBytes { get { return fullimage; } }
    /// <summary>
    /// Bytes from bitmap of last taken picture.
    /// </summary>
    public byte[] LastImgBytes { get { byte[] pom = null; Interlocked.Exchange<byte[]>(ref pom, pics); return pom; } }
    /// <summary>
    /// Number of commands waiting to be sent over serial connection.
    /// </summary>
    public int NotSended { get { return notSended.Count; } }
    /// <summary>
    /// Number of commands waiting to be sent or to an answer of the sent command.
    /// </summary>
    public int NotAnswered {
      private set { notAnswered = value; if (notAnswered == 0) OnFree(this, null); else OnWorking(this, null); }
      get { return notAnswered; }
    }
    /// <summary>
    /// Sercom offers events Free, which is raised, if Sercom processed some commands and
    /// now he has no command to process.
    /// </summary>
    public static event EventHandler Free;
    /// <summary>
    /// Sercom offers events Working, which is raised, if Sercom he has had no command to process 
    /// and now is processing some commands.    
    /// </summary>
    public static event EventHandler Working;
    /// <summary>
    /// Raises Free event, if some handler is set.
    /// </summary>
    /// <param name="sender">Parameter for the event</param>
    /// <param name="ev">Parameter for the event</param>
    protected void OnFree(object sender, EventArgs ev) {
      if (Free != null)
        Free(sender, ev);
    }
    /// <summary>
    /// Raises Working event, if some handler is set.
    /// </summary>
    /// <param name="sender">Parameter for the event.</param>
    /// <param name="ev">Paramter for the event.</param>
    protected void OnWorking(object sender, EventArgs ev) {
      if (Working != null)
        Working(sender, ev);
    }


    /// <summary>
    /// Writes the characteristic of underlying SerialPort connection.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() {    
      StringBuilder w = new StringBuilder();
      w.AppendFormat("port isOpen=={0}", port.IsOpen);
      w.AppendFormat("SerialPort read buffer {0}", port.ReadBufferSize);
      w.AppendFormat("SerialPort write buffer {0}", port.WriteBufferSize, ", by default is 2 second");
      w.AppendFormat("SerialPort write timeout {0}", port.WriteTimeout, ", by default is unlimited");
      w.AppendFormat("SerialPort read timeout {0}", port.ReadTimeout);
      return w.ToString();
    }

    #endregion //properties

    #endregion //public functions
  }
}
