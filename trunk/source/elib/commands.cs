using System;
using System.Collections.Generic;
using System.Text;
namespace Elib {
  /// <summary>
  /// An enumaration of possible action performed with LED on e-Puck.
  /// </summary>
  public enum Turn { 
    /// <summary>
    /// Turns LED on.
    /// </summary>
    On=0 ,
    /// <summary>
    /// Turns LED off.
    /// </summary>
    Off=1,
    /// <summary>
    /// If the LED is on, turn it off and vice versa.
    /// </summary>
    Inv=2 };
  /// <summary>
  /// E-Puck's camera can be in two colour modes.
  /// </summary>
  public enum CamMode { 
    /// <summary>
    /// Black and white mode. One pixel corresponds to one byte.
    /// </summary>
    BaW = 0, 
    /// <summary>
    /// Colour mode. One pixel corresponds to 2 bytes.
    /// </summary>
    Color = 1 };
  /// <summary>
  /// E-Puck's camera has 3 possible states of zoom.
  /// </summary>
  public enum Zoom {
    /// <summary>
    /// The biggest zoom. Usefull for linear camera.
    /// </summary>
    Big = 1, 
    /// <summary>
    /// The medium zoom.
    /// </summary>
    Medium = 4, 
    /// <summary>
    /// The smallest zoom. Good for analyzing pictures.
    /// </summary>
    Small = 8 };

  /// <summary>
  /// List of functions, which return commands for BTCom 1.1.3 with correct parameters.
  /// </summary>
  static class Commands {    
    public static readonly List<char> Commands_BigChars = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','N',
      'O','P','Q','R','S','U','V','T'};    
    static string Light(string Command, Turn how) {
      StringBuilder ans = new StringBuilder();
      ans.Append(Command); ans.Append(',');
      switch (how) {
        case Turn.Off: ans.Append(0); break;
        case Turn.On: ans.Append(1); break;
        case Turn.Inv: ans.Append(2); break;
      }
      ans.Append("\r");
      return ans.ToString();
    }

    /// <summary>
    ///c_LedBody(how) returns command to set Body led on,off,inv.
    /// </summary>
    public static string c_LedBody(Turn how) {
      return Light("B", how); 
    }


    /// <summary>
    /// Returns a command to set front led on, off or into inverse state.
    /// </summary>
    /// <param name="how">The how (on, off, inverse).</param>
    /// <returns></returns>
    public static string c_LedFront(Turn how) {
      return Light("F", how);
    }

    /// <summary>
    /// Returns a command to set a LED with number n on,off or into inverse state. Acceptable values are 0..7(resp. 8).
    /// Value 8 represents all diodes at once.
    /// </summary>
    /// <param name="n">The number of LED.</param>
    /// <param name="how">The how(on, off, inverse).</param>
    public static string c_LedX(int n, Turn how) {
      if ((n < 0) || (n > 8))
        throw new CommandArgsException("///c_LedOf returns command to set led with number n of. Acceptable values are 0..8");
      return Light("L," + n, how);
    }
    
    /// <summary>
    ///Returns command to gets motors speed left,right.
    /// </summary>
    public static string c_GetSpeed() {
      return "E\r";
    }
    /// <summary>
    ///Returns command to get Accelerometer values.
    /// </summary>
    public static string c_GetAccelerometr() {
      return "A\r";
    }
    /// <summary>
    ///Returns command to get selector position.
    /// </summary>
    public static string c_SelectorPos() {
      return "C\r";
    }
    /// <summary>
    ///Returns command to get array of IR.
    /// </summary>
    public static string c_IrData() {
      return "G\r";
    }
       
    /// <summary>
    ///Returns command to Calibrate proximity sensors.
    /// </summary>
    public static string c_CalibrateIR() {
      return "K\r";
    }

    /// <summary>
    /// Returns a command to set e-Puck's camera parameters.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="mode">The mode can be 0 or 1.</param>
    /// <param name="zoom">The zoom can be 1,4,8.</param>
    ///<remarks> Last parametr is immutable and has to be set 3200 viz http://www.dailyenigma.org/e-puck-cam.shtml.
    ///</remarks>
    public static string c_SetCamPar(int width, int height, CamMode mode, Zoom zoom) {
      const int MAX_CAM_RESOLUTION = 3200;            
      int size = (((int)mode)+1) * height * width;
      
      if (size > MAX_CAM_RESOLUTION)
        throw new CommandArgsException("///c_SetCamPar returns command to set cam parameters. Width, height can be only positive and width*height*mode<" + MAX_CAM_RESOLUTION.ToString());      
      return "J," + ((int)mode).ToString() + "," + width.ToString() + "," + height.ToString() + "," + ((int)zoom).ToString() + "\r";      
    }
    
    /// <summary>
    ///Returns a command to get camera parametrs.
    /// </summary>
    public static string c_GetCamPar() {
      return "I\r";
    }
    /// <summary>
    ///Returns a command to get camera parametrs.
    /// </summary>
    public static string c_GetImage() {
      return "-I\r"; //works in unmanaged code with type signed char like in c++. In c# is char unicode character and there is no - operator
    }
    /// <summary>
    ///Returns a command to get help.
    /// </summary>
    public static string c_Help() {      
      return "H\r";
    }
    /// <summary>
    ///Returns a command to stop e-Puck and turn off leds.
    /// </summary>
    public static string c_Stop() {      
      return "S\r";
    }
    /// <summary>
    /// Returns a command to set encoders values.
    /// One revolution corresponds to 1000 steps.
    /// </summary>
    /// <param name="LM">The encoder of the left wheel.</param>
    /// <param name="RM">The encoder fo the right wheel.</param>
    public static string c_SetMotorPosition(int LM, int RM) {      
      return "P," + LM.ToString() + "," + RM.ToString() + "\r";
    }
    /// <summary>
    ///c_getMotorPosition() returns command to get left and right values
    ///about how many times left and right wheel spinned*1000.
    /// </summary>
    public static string c_GetMotorPosition() {
      return "Q\r";
    }
    /// <summary>
    ///Returns a command to get 8 values from sensors, which detect ambient light. The more light, the lower the values. 
    ///Usual values above 3000 max 5000.
    /// </summary>
    public static string c_Light() {
      return "O\r";
    }
    /// <summary>
    ///Returns a command to get 8 values from proximity sensors. Values range from aprox. 570 to 4000. 
    ///Values start increasing from about 16 cm, up to 3900 at 0.5 cm.
    /// </summary>
    public static string c_Proximity() {
      return "N\r";
    }

    /// <summary>
    /// Returns command to set Left and Right Motor speed. Acceptable values are from -1000 to 1000.
    /// </summary>
    /// <param name="LM">The left motor speed.</param>
    /// <param name="RM">The right motor speed.</param>
    public static string c_Move(int LM, int RM) {      
      string command;
      if ((LM > 1000) || (LM < -1000) || (RM > 1000) || (RM < -1000))
        throw new CommandArgsException("c_Move(LM,RM) returns command to set Left and Right Motor speed. Acceptable values are LM==RM==<-1000,1000> ");
      command = "D," + LM.ToString() + "," + RM.ToString() + "\r";
      return command;
    }
    /// <summary>
    /// Returns a command to restart e-Puck.
    /// </summary>
    public static string c_Reset() {      
      return "R\r";
    }
    /// <summary>
    ///Returns a command to get version of E-puck BTcom.
    /// </summary>
    public static string c_Version() {
      return "V\r";
    }
    /// <summary>
    ///Returns a command to get strenght of 3 speakers.
    /// </summary>
    public static string c_Microphones() {      
      return "U\r";
    }
    /// <summary>
    /// Returns command to play sound. It turns speaker on.
    /// </summary>
    /// <param name="OTo6">From 0 to 5 for predefined sounds. 6 is for turning the microphones off.</param>
    /// <returns></returns>
    public static string c_Play(int OTo6) {
      if ((OTo6 < 0) || (OTo6 > 6))
        throw new CommandArgsException("///c_Play(OTo6) returns command to play sound. 6 turns speaker off");
      return "T," + OTo6.ToString() + "\r";

    }
  }
}