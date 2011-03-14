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
    /// Sets the values of the encoder to its motors.
    /// </summary>
    /// <param name="leftMotor"></param>
    /// <param name="rightMotor"></param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void SetEncoders(int leftMotor, int rightMotor, OkfActuators okf, KofCallback kof, object state, double timeout) {      
      actuators(Commands.c_SetMotorPosition(leftMotor,rightMotor), okf, kof, state, timeout, "SetEncoders(..)");
    }
    /// <summary>
    /// Calibrates the IR sensors. Usufull for proximity measurement.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    ///<remarks>E-Puck needs around 1s to calibrate its IR sensors.</remarks>
    public void CalibrateIRSensors(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_CalibrateIR(), okf, kof, state, timeout, "CalibrateIRSensors(..)");
    }
    /// <summary>
    /// Stops e-Puck.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void Stop(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Stop(),okf,kof, state, timeout, "Stop(..)");
    }
    /// <summary>
    /// Resets e-Puck.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    ///<remarks>E-Puck needs around 1.5s to restart.</remarks>
    public void Reset(OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Reset(), okf, kof, state, timeout, "Reset()");
    }
    /// <summary>
    /// Set the speed of e-Puck's motors.
    /// </summary>
    /// <param name="leftMotor">The left motor.</param>
    /// <param name="rightMotor">The right motor.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void Motors(double leftMotor, double rightMotor, OkfActuators okf, KofCallback kof, object state, double timeout) {
      int LM = (int)(leftMotor * 1000);
      int RM = (int)(rightMotor * 1000);
      actuators(Commands.c_Move(LM, RM), okf,kof, state, timeout, "Motors(..)");
    }
    /// <summary>
    /// Turn on or off one of 8 lights of e-Puck. 
    /// </summary>
    /// <param name="num">The num.</param>
    /// <param name="how">The how.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void LightX(int num, Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
      actuators(Commands.c_LedX(num,how), okf, kof, state, timeout, "LightX(..)");      
    }
    /// <summary>
    /// Turn off and on the body light.
    /// </summary>
    /// <param name="how">The how.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void BodyLight(Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
        actuators(Commands.c_LedBody(how), okf, kof, state, timeout, "BodyLight(..)");      
    }
    /// <summary>
    /// Front light turns off and on the front LED. It is useful for picture capturing.
    /// </summary>
    /// <param name="how">The how.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void FrontLight(Turn how, OkfActuators okf, KofCallback kof, object state, double timeout) {      
        actuators(Commands.c_LedFront(how), okf, kof, state, timeout, "FrontLight(..)");      
    }
    /// <summary>
    /// Sets the parameters of a cam. Maximum size can be 3200 bytes and is computed width*height in black and white mode and width*height*2 in colourful mode.    
    /// </summary>
    /// <param name="width">The width of picture in pixels.</param>
    /// <param name="height">The heightof picture in pixels</param>
    /// <param name="zoom">The zoom of a cam. The lowest is the most usefull.</param>
    /// <param name="mode">The mode. Can be black and white or colourful.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void SetCam(int width, int height, Zoom zoom, CamMode mode, OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_SetCamPar(height, width, mode, zoom), okf, kof, state, timeout, "SetCam(..)");
    }
    /// <summary>
    /// Begins to play sound.
    /// </summary>
    /// <param name="SoundNum">The sound num can be between 0 and 6. 6 turns speakers off other numbers plays a sound.</param>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ s ] set how long are you willing to wait for the command confirmation answer.</param>
    public void PlaySound(int SoundNum, OkfActuators okf, KofCallback kof, object state, double timeout) {
      actuators(Commands.c_Play(SoundNum), okf, kof, state, timeout, "PlaySound(..)");
    }

    #endregion Actuators

  }
}
