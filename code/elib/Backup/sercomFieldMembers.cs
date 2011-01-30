using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Text;


namespace Elib {
  public partial class Sercom {
    SerialPort port;
    Thread chN;
    Thread chS;
    
    myQueue notSent = new myQueue();
    priorQueue notSentSTS=new priorQueue() ;//locks at not at notSendedLock
    object notSentLock=new object();

    EventWaitHandle checkNSwh = new EventWaitHandle(false, EventResetMode.AutoReset);
    volatile bool sleeping = true;

    event EventHandler Received;
    EventWaitHandle checkSDwh = new EventWaitHandle(false, EventResetMode.ManualReset);
    EventWaitHandle hshake_sentUpdateWh = new EventWaitHandle(true, EventResetMode.ManualReset);
    ansGuard hshake_sent=null;
    object hshake_sentLock = new object();
    
    int notAnswered=0;
    volatile bool stableAns = false;
    volatile bool disposed=false;
    volatile bool stoppedSend=false;
    volatile bool stoppedConfirm=false;
    
    StringBuilder ans = new StringBuilder();
    volatile bool text_mode=true;
    volatile bool fullimage=false;
    volatile int mode=-666; 
    volatile int width=-666;
    volatile int height=-666;
    volatile bool ready=false;
    byte[] pics=null;

    /// <summary>
    /// A receiving of image is treated differently.
    /// The receiving of image continues if the timeout ellapses,
    /// but it is necessary to check whether new bytes are still receiving.
    /// Constant imgTo is specifies maximum gap in seconds between the time
    /// when last bytes are received and the new bytes are received.
    /// <see cref="Sercom.binaryModeRead()"/> method for detail info.
    /// </summary>
    const double imgTo = 0.1;

    /// <summary>
    /// Maximum of miliseconds, which are spent on unsuccessfull writing to serial port.
    /// Default value in ms, which is set to SerialPort. If serial port can not write for defWriteTimeout in ms, than Elib exception is raised.
    /// </summary>
    public const int defWriteTimeout = 2000;

    /// <summary>
    /// Default value. It is infinite, because the serial port do not have to contain any answer.
    /// </summary>
    public const int defReadTimeout = SerialPort.InfiniteTimeout;    
    

    /******* Sercom privateData structure    ******/
    /// <summary>
    /// ansGuard stores OkfCallback, KofCallback design especially for commands
    /// and uid for better identification and debugig
    /// </summary>
    sealed class ansGuard:IComparable<ansGuard> {
      public ansGuard(OkfCallbackDef okDf,KofCallbackDef koDf, string command_) {
        okf = okDf;
        kof = koDf;        
        command = command_;
      }
      public OkfCallbackDef okf=null;
      public KofCallbackDef kof=null;
      public node nS=null;
      public node nSST=null;
      public double timeout;
      public readonly string command;
      /// <summary>
      /// Returns a <see cref="System.String"/>command name that represents this instance.
      /// </summary>
      /// <returns>
      /// A <see cref="System.String"/> that represents this instance.
      /// </returns>
      public override string ToString() {
        string c;
        if(command!=null)
          if(command==Commands.c_GetImage())
            c="-I";
          else
            c=command[0].ToString();
        else
          c="null";
        return c;
      }
      public int CompareTo(ansGuard other) {
        return (int) (timeout - other.timeout);
      }
    }    
        
    /// <summary>
    /// wraps the pair of Kofcallback and the arguments for Kofcallback functions 
    /// </summary>
    sealed class KofCallbackDef {

      ///<summary>KofCallback function</summary>
      public KofCallback f;

      ///<summary>arguments  for f</summary>
      public object data;

      /// <summary>
      /// Initializes a new instance of the <see cref="KofCallbackDef"/> class.
      /// </summary>
      /// <param name="f_">The f_.</param>
      /// <param name="data_">The data_.</param>
      public KofCallbackDef(KofCallback f_, object data_) {
        f = f_;
        data = data_;
      }
    }
    
    /// <summary>
    /// wraps the pair of Rcallback "f" and the arguments "data" for Rcallback functions
    /// </summary>
    sealed class OkfCallbackDef {
      ///<summary>Okfcallback function</summary>
      public OkfCallback f;
      ///<summary>arguments  for f</summary>
      public object data;
      /// <summary>
      /// Initializes a new instance of the <see cref="OkfCallbackDef"/> class.
      /// </summary>
      /// <param name="f_">The f_.</param>
      /// <param name="data_">The data_.</param>
      public OkfCallbackDef(OkfCallback f_, object data_) {
        f = f_;
        data = data_;
      }
    }

  }

  #region Elib public delegates and delagetes data structure
    /// <summary>
    /// OkfCallback, KofCallback are delegates for methods, which should be
    /// passed together with commands to Sercom. These methods should implement
    /// how to react if sended command was confirmed(OkfCallback) or the time 
    /// for answer elapsed(KofCallback).
    /// </summary>
   
    public delegate void OkfCallback(string answer, object Data);

    /// <summary>
    /// OkfCallback, KofCallback are delegates for methods, which should be
    /// passed together with commands to Sercom. These methods should implement
    /// how to react if sended command was confirmed(OkfCallback) or the time 
    /// for answer elapsed(KofCallback).
    /// </summary>
    public delegate void KofCallback(object Data);
 #endregion

}
