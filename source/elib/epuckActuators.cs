#define LOGGING
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Elib {
  public partial class Epuck  {

    private void checkArgs(object okf, object kof, double to) {
      if (disposed)
        throw new  UnconnectedException("Epuck is already disposed, you can't send a command to it");
      if (okf == null || kof == null)
        throw new ArgsException("okf and kof callbacks must not be not be null!");
      if (to < 0)
        throw new ArgsException("timeout has to be a positive number!");
    }
    #region actuators

    /// <summary>
    /// A format of functions, which are called if the command for an actuator is successfully confirmed in timeout.
    /// </summary>
    public delegate void OkfActuators(object data);
    
    private void actuators(string c, OkfActuators okf, KofCallback kof, object state, double to, string method) {
      checkArgs(okf, kof, to);
      logf(Action.call, method);
      ser.Write(c,
        (ans, data) => { okf(data); logf(Action.ok, method);},
        (data) => { kof(data); logf(Action.ko, method);}          
        , state, to );      
    }
    ////////////////////////////////////////single functions ///////////////////////////////////
    /// <summary>    
    /// Sets encoders values.
    /// One revolution corresponds to 1000 steps.
    /// </summary>
    /// <param name="leftMotor"></param>
    /// <param name="rightMotor"></param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void SetEncoders(int leftMotor, int rightMotor, OkfActuators okf, KofCallback kof, object state, double timeout) {      
      actuators(Commands.c_SetMotorPosition(leftMotor,rightMotor), okf, kof, state, timeout, "SetEncoders(..)");
    }
    /// <summary>
    /// Calibrates proximity IR sensors, which 
    /// makes IR sensors more accurate for measuring proximity.
    /// Calibration adapts sensor for different reflection of IR light 
    /// in the current environment.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    ///<remarks>E-Puck needs around 1s to calibrate its IR sensors.</remarks>
    public void CalibrateIRSensors(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_CalibrateIR(), okf, kof, state, timeout, "CalibrateIRSensors(..)");
    }
    /// <summary> It stops e-Puck and turn off leds. </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void Stop(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Stop(),okf,kof, state, timeout, "Stop(..)");
    }
    /// <summary> It stops e-Puck, turn off LEDS, restars e-Puck and calibrate IR sensors.</summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    ///<remarks>E-Puck needs around 1.5s to restart.</remarks>
    public void Reset(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Reset(), okf, kof, state, timeout, "Reset()");
    }
    /// <summary>
    /// Sets Left and Right Motor speed. Acceptable values are from -1 to 1. 
    /// Value 1 corresponds to 1 revolution per second.
    /// Wheels have perimeter of 12,88 mm.
    /// </summary>
    /// <param name="leftMotor">The left motor.</param>
    /// <param name="rightMotor">The right motor.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void Motors(double leftMotor, double rightMotor, OkfActuators okf, KofCallback kof, object state, double timeout) {
      int LM = (int)(leftMotor * 1000);
      int RM = (int)(rightMotor * 1000);
      actuators(Commands.c_Move(LM, RM), okf,kof, state, timeout, "Motors(..)");
    }
    /// <summary>
    /// Sets a LED with number n on,off or into inverse state. Acceptable values are 0..7(resp. 8).
    /// Value 8 represents all diodes at once.
    /// </summary>
    /// <param name="num">The nummber of the LED.</param>
    /// <param name="how">Off/On/InverseState.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void LightX(int num, Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
      actuators(Commands.c_LedX(num,how), okf, kof, state, timeout, "LightX(..)");      
    }
    /// <summary>
    /// Sets Body led on, off or into an inverse state.
    /// </summary>
    /// <param name="how">Off/On/InverseState.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void BodyLight(Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
        actuators(Commands.c_LedBody(how), okf, kof, state, timeout, "BodyLight(..)");      
    }
    /// <summary>
    /// Sets Front led on, off or into an inverse state. It can produce enough light for capturing close obstacles with e-Puck's camera.
    /// </summary>
    /// <param name="how">Off/On/InverseState.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void FrontLight(Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
        actuators(Commands.c_LedFront(how), okf, kof, state, timeout, "FrontLight(..)");      
    }
    /// <summary>
    /// It sets the parameters of a camera. Maximum size of a picture can be 3200 bytes.
    /// The picture size S = width*height  bytes; for black and white mode  
    /// S = width*height*2 bytes; for colourful mode.    
    /// </summary>
    /// <param name="width">The width of picture in pixels.</param>
    /// <param name="height">The height of picture in pixels</param>
    /// <param name="zoom">The zoom of a cam. The lowest is the most useful.</param>
    /// <param name="mode">The mode. Can be black and white or colour.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void SetCam(int width, int height, Zoom zoom, CamMode mode, OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_SetCamPar(height, width, mode, zoom), okf, kof, state, timeout, "SetCam(..)");
    }
    /// <summary>
    /// It begins to play sound. Values 0-5 are for predefined sounds. 6 turns speaker off
    /// </summary>
    /// <param name="SoundNum">The SoundNum can be between 0 and 6. 6 turns speakers off. Other numbers play a sound.</param>
    /// <param name="okf">A function called after receiving the confirmation answer.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation answer has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[seconds] set how long are you willing to wait for the command confirmation answer.</param>
    public void PlaySound(int SoundNum, OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Play(SoundNum), okf, kof, state, timeout, "PlaySound(..)");
    }

    #endregion Actuators

  }
}
