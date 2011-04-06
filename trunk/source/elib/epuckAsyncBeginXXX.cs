using System;
using System.Drawing;
namespace Elib {
  /// <summary>
  /// A virtual representation of e-Puck, which allows control the robot with its methods.
  /// </summary>
  /// <remarks>
  /// It uses <see cref="Sercom">Sercom</see> internally. 
  /// Epuck class can log its commands. It has two interfaces
  /// (basic interface  based on <see cref="Sercom">Sercom</see> 
  /// and <see cref="T:IAsyncResult">IAsyncResult interface</see>
  /// </remarks>
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
        ar.SetAsCompleted(pic, false, new TimeoutElibException(ar.Name + " command has not been confirmed in timeout, but picture is still available in AsyncResult<Bitmap>"));
      } else
        failed(asyncResult);
    }

    /// <summary>
    /// It waits synchronously until the function, which created 
    /// the instance of<see cref="AsyncResultNoResult">AsyncNoResult</see> <paramref name="ar"/> finished.
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
    /// It waits synchronously until a function, which asked for sensors with <see langword="int"/> values,
    /// gets the desired answer or timeout elapses. It uses <paramref name="ar"/> in order to get the <see langword="int"/> values.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns an array of <c>int</c>.</returns>
    public int[] EndGetFtion(IAsyncResult ar) { return EndSensors<int[]>(ar); }
    /// <summary>
    /// It waits synchronously until a function, which asked for sensors with <see langword="string"/> values,
    /// gets the desired answer or timeout elapses. It uses <paramref name="ar"/> in order to get the <see langword="string"/> values.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGetInfo... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns a <c>string</c>.</returns>
    public string EndInfoFtion(IAsyncResult ar) { return EndSensors<string>(ar); }
    /// <summary>
    /// It waits synchronously until a function, which asked for an image,
    /// gets the <see cref="System.Drawing.Bitmap"/> or timeout elapses. It uses <paramref name="ar"/> in order to get the <see cref="System.Drawing.Bitmap"/>.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet. function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns array of <c>int</c>.</returns>
    public Bitmap EndGetImage(IAsyncResult ar) { return EndSensors<Bitmap>(ar); }
    
    // ///////////////////////////////////// each single actuator and sensor function//////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Sets Left and Right Motor Encoder value.  
    /// </summary>
    /// <param name="leftMotor">Sets the left motor value.</param>
    /// <param name="rightMotor">Sets the right motor value.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginSetEncoders(int leftTicks, int rightTicks, double timeout, AsyncCallback callback,Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginSetEncoders(..)"]);
      SetEncoders(leftTicks, rightTicks, received, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Calibrates proximity IR sensors, which 
    /// makes IR sensors more accurate for measuring proximity.
    /// Calibration adapts sensor for different reflection of IR light 
    /// in the current environment.
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// Sets Left and Right Motor speed. Acceptable values are from -1 to 1. 
    /// Value 1 corresponds to 1 revolution per second.
    /// Wheels have perimeter of 12,88 mm.
    /// </summary>
    /// <param name="leftMotor">Sets the left motor speed.</param>
    /// <param name="rightMotor">Sets the right motor speed.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginMotors(double leftMotor, double rightMotor, double timeout, AsyncCallback callback,Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginMotors(..)"]);
      Motors(leftMotor, rightMotor, received, failed, a, timeout);
      return a;
    }
    /// <summary> It gets the current amplitude of sound from e-Puck's 3 speakers. </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetMikes(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetMikes(..)"]);
      GetMikes(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary>
    /// Returns a command to get the array of integers from IR sensors. 
    /// The more ambient light, the lower the values. Usual values are above 3000.
    /// Maximal value is 5000.
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetLight(double timeout, AsyncCallback callback, object state) { 
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetLight(..)"]);
      GetLight(receivedSensors<int[]>, failed, a, timeout);
      return a;      
    }
    /// <summary> It stops e-Puck and turn off leds. </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginStop(double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginStop(..)"]);
      Stop(received, failed, a, timeout);
      return a;
    }
    /// <summary> It restarts e-Puck. </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// Sets Front led on, off or into an inverse state. It can produce enough light for capturing close obstacles with e-Puck's camera.
    /// </summary>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// Sets a LED with number n on,off or into inverse state. Acceptable values are 0..7(resp. 8).
    /// Value 8 represents all diodes at once.
    /// </summary>
    /// <param name="num">Number of the LED which is changed.</param>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// Sets Body led on, off or into an inverse state.
    /// </summary>
    /// <param name="how"><see cref="Turn"/> change state of the LED? </param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// It gets the IR data in in array of 6 integers with following meaning
    ///  IR check : 0x%x, address : 0x%x, data : 0x%x
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetIRData(double timeout, AsyncCallback callback, Object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state,"GetIRData(..)");
      GetIRData(receivedSensors<int[]> , failed, a, timeout);
      return a;
    }
    /// <summary> It shows Epuck's help sent from e-Puck. </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// It sets the parameters of a camera. Maximum size of a picture can be 3200 bytes.
    /// The picture size S = width*height  bytes; for black and white mode  
    /// S = width*height*2 bytes; for colourful mode.    
    /// </summary>
    /// <param name="width">The width of the picture in pixels.</param>
    /// <param name="height">The height of the picture in pixels</param>
    /// <param name="zoom">The zoom of a cam. The lowest is the most useful.</param>
    /// <param name="mode">The mode. Can be black and white or colourful.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
    /// It begins to play sound. Values 0-5 are for different sounds. 6 turns speaker off.
    /// </summary>
    /// <param name="SoundNum">The SoundNum can be between 0 and 6. 6 turns speakers off other numbers plays a sound.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function called after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginPlaySound(int SoundNum,double timeout, AsyncCallback callback, Object state) {
            AsyncResultNoResult a = new AsyncResultNoResult(callback, state,logFunctionNames["BeginPlaySound(..)"]);
      PlaySound(SoundNum,received, failed, a, timeout);
      return a;
    }
    /// <summary> It gets the BTCom version from e-Puck.</summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function called after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>        
    public IAsyncResult BeginGetInfoVersion(double timeout, AsyncCallback callback, Object state) {
            AsyncResult<string> a = new AsyncResult<string>(callback, state,logFunctionNames["BeginGetInfoVersion(..)"]);
      GetVersionInfo(receivedSensors<string>, failed, a, timeout);
      return a;
    }
    /// <summary>  It gets the proximity from IR sensors. Obstacle can be recognized up to 4 cm.</summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetIR(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetIR(..)"]);
      GetIR(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary>  It returns vector of values, which indicates the slant of e-Puck</summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetAccelerometer(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetAccelerometer(..)"]);
      GetAccelerometer(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary>    It returns a selector position</summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetSelector(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetSelector(..)"]);
      GetSelector(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary> It gets the current speed of both wheels. Speed on a wheel is from -1 to 1. 
    /// Value 1 corresponds to 1 revolution per second.
    /// Wheels have perimeter of 12,88 mm.
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetSpeed(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetSpeed(..)"]);
      GetSpeed(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary> It gets current camera settings.
    /// The picture size S = width*height, black or white mode and zoom.
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetCamParams(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetCamParams(..)"]);
      GetCamParams(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary>It gets a current state of encoders. It is measured in steps. One forward revolution corresponds to +1000 steps.It is nulled if the e-Puck resets.</summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
    /// If the confirmation does not arrived until timeout exception is raised</param>
    /// <param name="callback">A function which is called, after the confirmation answer is received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <returns></returns>
    public IAsyncResult BeginGetEncoders(double timeout, AsyncCallback callback, object state) {
      AsyncResult<int[]> a = new AsyncResult<int[]>(callback, state, logFunctionNames["BeginGetEncoders(..)"]);
      GetEncoders(receivedSensors<int[]>, failed, a, timeout);
      return a;
    }
    /// <summary>   
    /// It gets a picture. It can take a long time. E.g. picture 40*40 in colour takes more than 0.4 sec under 
    /// good light conditions and with battery fully charged.
    /// </summary>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.
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
