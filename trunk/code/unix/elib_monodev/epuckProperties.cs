using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;


namespace Elib {
  public partial class Epuck {

    #region Properties
    /// <summary>
    /// Gets the number of unconfirmed commads called on Epuck instance. 
    /// </summary>
    /// <value>The working.</value>
    public int Working { get { return ser.NotAnswered; } }
    
    /// <summary>
    /// Gets the number of waiting commands in notSended queue to be send via Serial Communication(Bluetooth).
    /// </summary>
    /// <value>The waiting commands.</value>
    public int NotSended { get { return ser.NotSended; } }

    /// <summary>
    /// Gets the name specified in a constructor.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get { return name; } }

    /// <summary>
    /// Gets the port specified in a constructor.
    /// </summary>
    /// <value>The port.</value>
    public string Port { get { return port; } }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// A user defined robot name and <see cref="Sercom"/> parameters are returned.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() { return "Epuck" + name + ": "+ser.ToString(); }

    /// <summary>
    /// Return a <c>bool</c> flag, which indicates whether logging is on.
    /// </summary>
    protected bool Log {       
      get { return log; } 
    }

    /// <summary>
    /// <exception cref="T:ElibException"> is thrown when <see cref="Epuck.LogStream">LogStream</see> is <c>null</c>. </exception>
    /// </summary>
    public void StartLogging() { 
      if (w == null) 
        throw new ElibException("Specify LogStream before attempting to turn logging on!");
      log = true; 
    }

    /// <summary>
    /// Disables logging.
    /// </summary>
    public void StopLogging() { log = false; Thread.Sleep(20); }

    /// <summary>
    /// Enables sets or get TextWriter of e-Puck, where all actions of e-Puck are logged if loggin is turned on.
    /// </summary>
    public TextWriter LogStream {
      set { if (log) throw new ElibException("Stop logging before changing LogStream"); w = value; }
      get { return w; }
    }

    #endregion          
  }
}
