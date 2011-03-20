using System;
using System.Threading;
namespace Elib {
  /// <summary>
  /// Class used in <see cref="System.IAsyncResult"/> for example in <see cref="Epuck.EndFtion"/>
  /// It does not allow to return an answer.
  /// </summary>
  public class AsyncResultNoResult : IAsyncResult {
    // Fields set at construction which never change while 
    // operation is pending
    readonly AsyncCallback m_AsyncCallback;
    readonly Object m_AsyncState;    

    // Fields set at construction which do change after 
    // operation completes
    const Int32 c_sp = 0;//StatePending
    const Int32 c_scs= 1;// StateCompletedSynchronously
    const Int32 c_sca = 2;//StateCompletedAsynchronously
    Int32 m_CompletedState = c_sp;
    
    // Field that may or may not get set depending on usage
     ManualResetEvent m_AsyncWaitHandle;

    // Fields set when operation completes
     Exception m_exception;

    string name;
    /// <summary>
    /// Gets or sets the name.
    /// It allows easy debugging and determine, which command is using <see cref="T:AsyncResultNoResult"/>
    /// </summary>
    /// <value>The name of function, which called the 'Begin' function.</value>
    public string Name { get { return (name != null) ? name : ""; } set { name = value; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncResultNoResult"/> class.
    /// </summary>
    /// <param name="asyncCallback">The async callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="name_">The name_.</param>
    public AsyncResultNoResult(AsyncCallback asyncCallback, Object state, string name_) {
      m_AsyncCallback = asyncCallback;
      m_AsyncState = state;
      name = name_;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncResultNoResult"/> class.
    /// </summary>
    /// <param name="asyncCallback">The async callback.</param>
    /// <param name="state">The state.</param>
    public AsyncResultNoResult(AsyncCallback asyncCallback, Object state) : this(asyncCallback, state, null) { }

    /// <summary>
    /// Used for signalling, that an exception has been thrown during waiting to end of operation.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
    public void SetAsCompleted(Exception exception, Boolean completedSynchronously) {
      // Passing null for exception means no error occurred. 
      // This is the common case
      m_exception = exception;

      // The m_CompletedState field MUST be set prior calling the callback
      Int32 prevState = Interlocked.Exchange(ref m_CompletedState,
         completedSynchronously ? c_scs :
         c_sca);
      if (prevState != c_sp)
        throw new InvalidOperationException(
            "You can set a result only once");

      // If the event exists, set it
      if (m_AsyncWaitHandle != null) m_AsyncWaitHandle.Set();

      // If a callback method was set, call it
      if (m_AsyncCallback != null) m_AsyncCallback(this);
    }

    /// <summary>
    /// Synchronously wait until the operation is pending. 
    /// </summary>
    public void EndInvoke() {
      // This method assumes that only 1 thread calls EndInvoke 
      // for this object
      if (!IsCompleted) {
        // If the operation isn't done, wait for it
        AsyncWaitHandle.WaitOne();
        AsyncWaitHandle.Close();
        m_AsyncWaitHandle = null;  // Allow early GC
      }      
      // Operation is done: if an exception occured, throw it
      if (m_exception != null) throw m_exception;
    }

    #region Implementation of IAsyncResult
    /// <summary>
    /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
    /// </summary>
    /// <value></value>
    /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
    public Object AsyncState { get { return m_AsyncState; } }

    /// <summary>
    /// Gets an indication of whether the asynchronous operation completed synchronously.
    /// </summary>
    /// <value></value>
    /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
    public Boolean CompletedSynchronously {
      get { return Thread.VolatileRead(ref m_CompletedState) ==c_scs; }
    }

    /// <summary>
    /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
    /// </summary>
    /// <value></value>
    /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
    public WaitHandle AsyncWaitHandle {
      get {
        if (m_AsyncWaitHandle == null) {
          Boolean done = IsCompleted;
          ManualResetEvent mre = new ManualResetEvent(done);
          if (Interlocked.CompareExchange(ref m_AsyncWaitHandle,
             mre, null) != null) {
            // Another thread created this object's event; dispose 
            // the event we just created
            mre.Close();
          } else {
            if (!done && IsCompleted) {
              // If the operation wasn't done when we created 
              // the event but now it is done, set the event
              m_AsyncWaitHandle.Set();
            }
          }
        }
        return m_AsyncWaitHandle;
      }
    }

    /// <summary>
    /// Gets an indication whether the asynchronous operation has completed.
    /// </summary>
    /// <value></value>
    /// <returns>true if the operation is complete; otherwise, false.</returns>
    public Boolean IsCompleted {
      get { return Thread.VolatileRead(ref m_CompletedState) != c_sp; }
    }
    #endregion
  }
  
  /// <summary>
  /// Class used in <see cref="System.IAsyncResult"/> when the "End" function
  /// returns an answer. E.g  <see cref="M:Epuck.EndGetFtion(IAsyncResult)"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  public class AsyncResult<TResult> : AsyncResultNoResult { 
    // Field set when operation completes
    TResult m_result = default(TResult);

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncResult&lt;TResult&gt;"/> class.
    /// </summary>
    /// <param name="asyncCallback">The async callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="name">The name.</param>
    public AsyncResult(AsyncCallback asyncCallback, Object state,string name) :
      base(asyncCallback, state,name) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncResult&lt;TResult&gt;"/> class.
    /// </summary>
    /// <param name="asyncCallback">The async callback.</param>
    /// <param name="state">The state.</param>
    public AsyncResult(AsyncCallback asyncCallback, Object state) : this(asyncCallback, state, null) { }

    /// <summary>
    ///Sets a result or an Exception.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
    public void SetAsCompleted(TResult result,
       Boolean completedSynchronously) {
      // Save the asynchronous operation's result
      m_result = result;

      // Tell the base class that the operation completed 
      // sucessfully (no exception)
      base.SetAsCompleted(null, completedSynchronously);
    }

    /// <summary>
    /// Sets as completed.
    /// Added for <see cref="M:BeginGetImage(IAsyncResult)"/>. Allows to set both the exception and the result if there is any.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
    /// <param name="exception">The exception.</param>
    public void SetAsCompleted(TResult result, Boolean completedSynchronously, Exception exception) {
      m_result = result;
      base.SetAsCompleted(exception, completedSynchronously);
    }

    /// <summary>
    /// <see cref="M:AsyncResultNoResult.EndInvoke()"/>
    /// </summary>
    /// <returns>Returns the desired value</returns>
    new public TResult EndInvoke() {
      base.EndInvoke(); // Wait until operation has completed 
      return m_result;  // Return the result (if above didn't throw)
    }
  }
 }