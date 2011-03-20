﻿using System;
using System.Drawing;
namespace Elib {
  /// <summary>
  /// A virtual representation of e-Puck, which allows control the robot with its methods.
  /// </summary>
  /// <remarks>
  /// It uses <see cref="Sercom">Sercom</see> internally. 
  /// Epuck can logg its commands. It has two interfaces(basic inteface 
  /// based on <see cref="Sercom">Sercom</see> and <see cref="T:IAsyncResult">IAsyncResult interface</see></remarks>
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
    /// It waits synchronously until the function, which created 
    /// an instance of<see cref="AsyncResultNoResult">AsyncNoResult</see> <paramref name="ar"/>.
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
    /// gets the desired answer or timeout elapses. It uses <paramref name="ar"/> ir order to get the <see langword="int"/> values.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns an array of <c>int</c>.</returns>
    public int[] EndGetFtion(IAsyncResult ar) { return EndSensors<int[]>(ar); }
    /// <summary>
    /// It waits synchronously until a function, which asked for sensors with <see langword="string"/> values,
    /// gets the desired answer or timeout elapses. It uses <paramref name="ar"/> ir order to get the <see langword="string"/> values.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGetInfo... function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns a <c>string</c>.</returns>
    public string EndInfoFtion(IAsyncResult ar) { return EndSensors<string>(ar); }
    /// <summary>
    /// It waits synchronously until a function, which asked for an image,
    /// gets the <see cref="System.Drawing.Bitmap"/> or timeout elapses. It uses <paramref name="ar"/> ir order to get the <see cref="System.Drawing.Bitmap"/>.</summary>
    /// <param name="ar"> Instance of AsyncResult serves to connect the  BeginGet. function with <see cref="EndGetFtion(IAsyncResult)"/> method</param>
    /// <returns>Returns array of <c>int</c>.</returns>
    public Bitmap EndGetImage(IAsyncResult ar) { return EndSensors<Bitmap>(ar); }
    
    // ///////////////////////////////////// each single actuator and sensor function//////////////////////////////////////////////////////////////////////
    ///<summary>
    /// It calibrates the IR sensors. Usufull for proximity measurement.
    /// </summary>
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
    /// It sets the speed of e-Puck's motors.
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
    /// <summary> It gets the current amplitude of sound.(Sound strenght}   </summary>
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
    /// <summary>It gets ambient light from IR sensors. The smaller values the greater light.</summary>
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
    /// <summary> It stops e-Puck. </summary>
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
    /// <summary> It resets e-Puck. </summary>
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
    /// <summary> It sets the front LED on or off. </summary>
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
    /// <summary> It sets the state of LED on or off </summary>
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
    /// <summary>It changes state of the body light. </summary>
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
    /// It gets the IR data in in array of 6 ints with following meaning
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
    /// <summary> It shows Epuck's help. </summary>
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
    /// <summary>It sets the parameters of e-Puck's cam. Maximum size of a picture can be 3200 bytes and is computed width*height in black and white mode and width*height*2 in colourful mode. </summary>
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
    /// <summary> It begins to play sound. </summary>
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
    /// <summary> It gets the BTCom version. </summary>
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
    /// <summary>  It gets the proximity from IR sensors. Obstacle can be recongnized up to 4 cm.</summary>
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
    /// <summary>  It returns vector of values, which indicates the slant of e-Puck</summary>
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
    /// <summary>    It returns a selector position</summary>
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
    /// <summary> It gets the current speed of both wheels. Speed on a wheel is from -1 to 1 </summary>
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
    /// <summary> It gets current camera settings</summary>
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
    /// <summary>It gets a current state of encoders. It is measured in steps. It is nulled if the e-Puck resets.</summary>
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
    /// <summary>   
    /// It gets a picture. It can take a long time. E.g. piture 40*40 in colour takes more than 0.4 s under good light conditions
    /// and with battery fully charged.
    /// </summary>
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