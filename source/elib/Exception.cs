using System;
using System.Collections.Generic;
using System.Text;

namespace Elib {

  /// <summary>
  /// The ElibEception is thrown, if an unusual situation happens in Elib.
  /// It wraps all other exceptions, which are thrown from Elib.
  /// </summary>
  public class ElibException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="ElibException"/> class.
    /// </summary>
    public ElibException() : base() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="ElibException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ElibException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="ElibException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ElibException(string message, Exception innerException) : base(message, innerException) { }
  }

  /// <summary>
  /// The TimeoutElibException is thrown if the "End" function implementing IAsyncResult was called 
  /// and indicates that the answer to command has not been delivered in time.
  /// </summary>
  public class TimeoutElibException : ElibException {
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutElibException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TimeoutElibException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutElibException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TimeoutElibException(string message, Exception innerException) : base(message, innerException) { }
  }

  /// <summary>
  /// If SerialPort class throws any exception, than this exception wraps the original exception. After that the SerialPortException is thrown.
  /// </summary>
  public class SerialPortException : ElibException {
    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SerialPortException (string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortException"/> class.
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="innerException">The inner exception.</param>
    public SerialPortException(string message, Exception innerException) : base(message, innerException) { } 
  }

  /// <summary>
  /// ArgsException is thrown if wrong arguments are passed to function in Elib.
  /// </summary>
  public class ArgsException : ElibException {

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgsException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ArgsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgsException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ArgsException(string message, Exception innerException) : base(message, innerException) { }
  }

  /// <summary>
  /// Thrown if command to e-Puck has nonsense values.
  /// </summary>
  public class CommandArgsException : ArgsException {

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgsException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public CommandArgsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgsException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CommandArgsException(string message, Exception innerException) : base(message, innerException) { }
  }

  /// <summary>
  /// Thrown if session with e-Puck has not started or has already ended.
  /// </summary>
  public class UnconnectedException : ElibException {

    /// <summary>
    /// Initializes a new instance of the <see cref="UnconnectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public UnconnectedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnconnectedException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public UnconnectedException(string message, Exception innerException) : base(message, innerException) { }
  }
}
