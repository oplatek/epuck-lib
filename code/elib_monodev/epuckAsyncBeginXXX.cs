using System;
using System.Drawing;
namespace Elib {
  /// <summary>
  /// A virtual representation of a e-Puck, which allows control the robot with its function.
  /// </summary>
  /// <remarks>It uses <see cref="Sercom"/> internally. <see cref="Epuck"/> can logg its commands. It has two interfaces(basic inteface based on <see cref="T:Sercom"/> and <see cref="T:IAsyncResult"/></remarks>
  partial class Epuck {
    static void received(object asyncNoResult) {
      AsyncResultNoResult ar = (AsyncResultNoResult)asyncNoResult; ar.SetAsCompleted(null, false); 
    }
    static void receivedSensors<T>(T b, object asyncResIntArr) {
      AsyncResult<T> a = (AsyncResult<T>)asyncResIntArr;
      a.SetAsCompleted(b, false);
    }
    
    static void failed(object asyncNoResult) {
      AsyncResultNoResult ar = (AsyncResultNoResult)asyncNoResult;
      ar.SetAsCompleted(new TimeoutElibException(ar.Name + " command hasn't been confirmed in timeout"), false); 
    }

    static void failedBitmap(Bitmap pic,object asyncResult) {
      if (pic != null) {
        AsyncResult<Bitmap> ar = (AsyncResult<Bitmap>)asyncResult;        
        ar.SetAsCompleted(pic, false, new TimeoutElibException(ar.Name + " commad has not been confirmed in timeout, but picture is still available in AsyncResult<Bitmap>"));
      } else
        failed(asyncResult);
    }

    /// <summary>
    /// Waits until the function, which created <see cref="AsyncResultNoResult">ar</see>.
    /// </summary>
    /// <param name="ar">The ar.</param>
    public void EndFtion(IAsyncResult ar) {
      AsyncResultNoResult a = (AsyncResultNoResult)ar;
      a.EndInvoke();
    }

    static T EndSensors<T>(IAsyncResult ar) {
      AsyncResult<T> a = (AsyncResult<T>)ar;
      return a.EndInvoke();
    }
    /// <summary>
    /// Waits synchronously until a function, which asked for sensors with <c>int</c> values, gets the desired answer or timeout elapses.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns an array of <c>int</c>.</returns>
    public int[] EndGetFtion(IAsyncResult ar) { return EndSensors<int[]>(ar); }
    /// <summary>
    /// Waits synchronously until a function, which asked for textual information, gets the desired answer or timeout elapses.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGetInfo... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns a <c>string</c>.</returns>
    public string EndInfoFtion(IAsyncResult ar) { return EndSensors<string>(ar); }
    /// <summary>
    /// Waits synchronously until a function, which asked for an image, gets the desired answer or timeout elapses.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet. function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns array of <c>int</c>.</returns>
    public Bitmap EndGetImage(IAsyncResult ar) { return EndSensors<Bitmap>(ar); }
    
    /////////////////////////////////////// each single actuator and sensor function//////////////////////////////////////////////////////////////////////
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginCalibrateIRSensors(double timeout,AsyncCallback callback, Object state) {
      AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginCalibrateIRSensors(..)"]);
      CalibrateIRSensors(received,failed,a,timeout);
      return a;
    }
    /// <summary>
    /// Begins the motors.
    /// </summary>
    /// <param name="leftMotor">Sets the left motor speed.</param>
    /// <param name="rightMotor">Sets the right motor speed.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginMotors(double leftMotor, double rightMotor, double timeout, AsyncCallback callback,Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginMotors(..)"]);
      Motors(leftMotor, rightMotor, received, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetMikes(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetMikes(..)"]);
      GetMikes(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetLight(double timeout, AsyncCallback callback, object state) { 
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetLight(..)"]);
      GetLight(receivedSensors<int[]>, failed, a, timeout);
      return a;      
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginStop(double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginStop(..)"]);
      Stop(received, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginReset(double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginReset(..)"]);
      Reset(received, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Sets the front LED.
    /// </summary>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns>Returns an instance of AsyncNoResult.</returns>
    public IAsyncResult BeginFrontLight(Turn how, double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginFrontLight(..)"]);
      FrontLight(how, received, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Sets the state of LED.
    /// </summary>
    /// <param name="num">Number of LED, which is changed.</param>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns>Returns an instance of AsyncNoResult.</returns>
    public IAsyncResult BeginLightX(int num, Turn how,double timeout,AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginLightX(..)"]);
      LightX(num,how, received,failed,a,timeout);
      return a;
    }
    /// <summary>
    /// Change state of the body light.
    /// </summary>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns>Returns an instance of AsyncNoResult.</returns>
    public IAsyncResult BeginBodyLight(Turn how, double timeout,AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginBodyLight(..)"]);
      BodyLight(how, received,failed,a,timeout);
      return a;
    }
    /// <summary>
    /// Gets the IR data in in array of 6 ints with following meaning
    /// g IR check : 0x%x, address : 0x%x, data : 0x%x
    /// </summary>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetIRData(double timeout, AsyncCallback callback, Object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state,"GetIRData(..)");
      GetIRData(receivedSensors<int[]> , failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetInfoHelp(double timeout, AsyncCallback callback, Object state) {
            AsyncResult<string> a = new AsyncResult<string>(callback, state,logFunctionNames["BeginGetInfoHelp(..)"]);
      GetHelpInfo(receivedSensors<string>, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Sets the parameters of a cam. Maximum size can be 3200 bytes and is computed width*height in black and white mode and width*height*2 in colourful mode.    
    /// </summary>
    /// <param name="width">The width of picture in pixels.</param>
    /// <param name="height">The heightof picture in pixels</param>
    /// <param name="zoom">The zoom of a cam. The lowest is the most usefull.</param>
    /// <param name="mode">The mode. Can be black and white or colourful.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginSetCam(int width, int height, Zoom zoom, CamMode mode, double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginSetCam(..)"]);
      SetCam(width,height,zoom,mode, received, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Begins to play sound.
    /// </summary>
    /// <param name="SoundNum">The sound num can be between 0 and 6. 6 turns speakers off other numbers plays a sound.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function called after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginPlaySound(int SoundNum,double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginPlaySound(..)"]);
      PlaySound(SoundNum,received, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function called after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>        
    public IAsyncResult BeginGetInfoVersion(double timeout, AsyncCallback callback, Object state) {
            AsyncResult<string> a = new AsyncResult<string>(callback, state,logFunctionNames["BeginGetInfoVersion(..)"]);
      GetVersionInfo(receivedSensors<string>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetIR(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetIR(..)"]);
      GetIR(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetAccelerometer(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetAccelerometer(..)"]);
      GetAccelerometer(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetSelector(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetSelector(..)"]);
      GetSelector(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetSpeed(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetSpeed(..)"]);
      GetSpeed(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetCamParams(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetCamParams(..)"]);
      GetCamParams(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetEncoders(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetEncoders(..)"]);
      GetEncoders(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetImage(double timeout, AsyncCallback callback, object state) {
      AsyncResult<Bitmap> a = new AsyncResult<Bitmap>(callback, state,logFunctionNames["BeginGetImage(..)"]);
      GetImage(receivedSensors<Bitmap>,failedBitmap,a,timeout);
      return a;
    }

  }
}