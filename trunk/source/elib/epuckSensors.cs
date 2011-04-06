using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Elib {
  public partial class Epuck {

    #region IRSensors
    /// <summary>
    /// A format of functions, which are  called when a command requiring an array of <c>int</c> is confirmed in timeout.
    /// </summary>
    public delegate void OkfIntsSensors(int[] ans, object data);
    /// <summary>
    /// A format of functions, which are  called when a command requiring a <c>string</c> is confirmed in timeout.
    /// </summary>
    public delegate void OkfStringSensors(string ans, object data);
    /// <summary>
    /// A format of functions, which are  called when a command requiring a <c>Bitmap</c> is confirmed in timeout.
    /// </summary>
    public delegate void OkfKofCamSensor(Bitmap ans, object data);
    
    static Bitmap parsingBitmap(byte[] pixs, int width, int height, int mode) {
      //IMPORTANT!!!: THE PICTURE FROM CAMERA IS ROTATED 
      if (pixs == null || width <= 0 || height <= 0 || !(mode == 0 || mode == 1))
        return null;
      //gray level == 1 byte
      int pixNum;
      int line;
      int col;
      System.Drawing.Color clr;
      //creating bitmap and turning picture around
      Bitmap res = new Bitmap(height, width); 
      if (mode == 0) {//gray level
        int px;
        pixNum = height * width;
        for (int i = 0; i < pixNum; i++) { 
          line = i / width;
          col = i - line * width;
          px = (int)pixs[i];
          clr = System.Drawing.Color.FromArgb(px, px, px);
          res.SetPixel(line, col, clr);
        }
      } else { //RGB~565 image
        int redPix, greenPix, bluePix, index;
        byte pix1, pix2;

        for (line = 0; line < height; ++line) {
          for (col = 0; col < width; ++col) {
            index = line * 2 * width + 2 * col;
            pix1 = pixs[index];
            pix2 = pixs[index + 1];
            redPix = (int)((pix1 & 0xF8));
            greenPix = (int)(((pix1 & 0x07) << 5) | ((pix2 & 0xE0) >> 3));
            bluePix = (int)((pix2 & 0x1F) << 3);
            if (redPix > 255)
              redPix = 255;
            if (greenPix > 255)
              greenPix = 255;
            if (bluePix > 255)
              bluePix = 255;
            clr = System.Drawing.Color.FromArgb(redPix, greenPix, bluePix);
            res.SetPixel(line, width - 1 - col, clr);
          }
        }

      }//end if(mode==0)

      return res;
    }    

    int[] parseAns(int size,string ans){
      string[] args = ans.Split(new char[] { ',' });
      int[] data = new int[size];
      if (args.Length != size+1)
        throw new ElibException("Sercom is returning wrong answer! Please restart applications");
      for (int i = 1; i < size+1; ++i) {
        if (!Int32.TryParse(args[i], out data[i - 1]))
          throw new ElibException("Sercom is returning wrong answer! Please restart applications");
      }
      return data;
    }

    void intArrSensors(string c,int arrSize, OkfIntsSensors okf, KofCallback kof, object state, double to,string method) {
      checkArgs(okf, kof, to);
      logf(Action.call, method);
      ser.Write(c,
        (ans, data) => {
          okf(parseAns(arrSize, ans), data); //arrSize is here expected size of returned array e.g. arrSize for IRSensors is 8
          logf(Action.ok, method);
        },
        (data) => {
          kof(data);
          logf(Action.ko, method);
        }, state, to);      
    }

    void stringSensors(string c, OkfStringSensors okf, KofCallback kof, object state, double to, string method) { 
      checkArgs(okf, kof, to);
      logf(Action.call, method);
      ser.Write(c,
        (ans, data) => {
          okf(ans,data);
          logf(Action.ok,method);
        },
        (data) => {
          kof(data);
          logf(Action.ko, method);
        }, state, to);      
    }


    /////////////////////////////////////single functions //////////////////////////
    /// <summary>   
    /// It gets a picture. It can take a long time. E.g. picture 40*40 in colour takes more than 0.4 sec under good light conditions
    /// and with battery fully charged.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    ///<remarks>In contrary to other methods the <paramref name="kof"/> callback can be called later than <paramref name="timeout"/> elapses.
    ///<paramref name="kof"/> is delayed when data are still sending and the <paramref name="timeout"/> has already elapsed.</remarks>
    public void GetImage(OkfKofCamSensor okf, OkfKofCamSensor kof, object state, double timeout) {
      checkArgs(okf, kof, timeout);
      string fname="GetImage(..)";
      logf(Action.call, fname);
      ser.Write(
        Commands.c_GetImage(),
        (ans, data) => {
          okf(parsingBitmap(ser.LastImgBytes, ser.WidthImg, ser.HeightImg, ser.ModeImg), data);
          logf(Action.ok, fname);
        },
        (data) => {
          if (ser.FullImgBytes)//kof is defined and a whole image was captured
            kof(parsingBitmap(ser.LastImgBytes, ser.WidthImg, ser.HeightImg, ser.ModeImg), data);
          else
            kof(null, data);//image is damaged
          logf(Action.ko, fname);
        },
        state, timeout);
    }

    /// <summary>  It gets the proximity from IR sensors. Obstacle can be recognized up to 4 cm.</summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetIR(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_Proximity(),8,okf,kof,state,timeout,"GetIR(..)");
    }
    /// <summary>  It returns vector of values, which indicates the slant of e-Puck</summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param> 
    public void GetAccelerometer(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_GetAccelerometr(), 3, okf, kof, state, timeout, "GetAccelerometer(..)");
    }
    /// <summary>    It returns a selector position</summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetSelector(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_SelectorPos(), 1, okf, kof, state, timeout, "GetSelector(..)");
    }
    /// <summary> It gets the current speed of both wheels. Speed on a wheel is from -1 to 1. 
    /// Value 1 corresponds to 1 revolution per second.
    /// Wheels have perimeter of 12,88 mm.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[ sec ] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetSpeed(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_GetSpeed(), 2, okf, kof, state, timeout, "GetSpeed(..)");
    }
    /// <summary> It gets current camera settings.
    /// The picture size S = width*height, black or white mode and zoom.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetCamParams(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_GetCamPar(), 5, okf, kof, state, timeout, "GetCamParams(..)");
    }
    /// <summary>
    /// Returns a command to get the array of integers from IR sensors. 
    /// The more ambient light, the lower the values. Usual values are above 3000.
    /// Maximal value is 5000.
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetLight(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_Light(), 8, okf, kof, state, timeout, "GetLight(..)");
    }
    /// <summary>It gets a current state of encoders. It is measured in steps. One forward revolution corresponds to +1000 steps.It is nulled if the e-Puck resets.</summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetEncoders(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_GetMotorPosition(), 2, okf, kof, state, timeout, "GetEncoders(..)");
    }
    /// <summary> It gets the current amplitude of sound from e-Puck's 3 speakers. </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetMikes(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      intArrSensors(Commands.c_Microphones(), 3, okf, kof, state, timeout, "GetMikes(..)");
    }
    

    //string answered sensors
    //In fact it there are not actuators, although there are in a actuators region. 
    //Following functions look like sensor functions for an user. 
    //However they return text values, so they act like actuator function if you processing answers. 
    /// <summary>
    /// It gets the IR data in in array of 3 integers converted from hex number with following meaning.
    /// IR check : 0x%x, address : 0x%x, data : 0x%x
    /// </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetIRData(OkfIntsSensors okf, KofCallback kof, object state, double timeout) {
      string method="GetIRData(..)";
      checkArgs(okf, kof, timeout);
      logf(Action.call, method);
      ser.Write(Commands.c_IrData(),
        (ans, data) => {
          okf(parseGetIRData(ans), data); //arrSize is here expected size of returned array e.g. arrSize for IRSensors is 8
          logf(Action.ok, method);
        },
        (data) => {
          kof(data);
          logf(Action.ko, method);
        }, state, timeout);      
    }
    
    private int[] parseGetIRData(string ans) {
      //g IR check : 0x%x, address : 0x%x, data : 0x%x
      int[] res = null;
      try {
        string[] parts = ans.Split(new char[] { ',' });
        parts[0] = parts[0].Substring(13);
        parts[1] = parts[1].Substring(11);
        parts[2] = parts[2].Substring(8);
        res = new int[parts.Length];
        for (int i = 0; i < parts.Length; ++i) {
          string[] p = parts[i].Split(new char[] { 'x' });          
          res[i] = int.Parse(p[1],System.Globalization.NumberStyles.HexNumber);
        }
      } catch (Exception) {
        throw new ElibException("Answer has a bad format");
      }
      return res;
    }

    /// <summary> It shows Epuck's help sent from e-Puck. </summary>
    /// <param name="okf">The okf.</param>
    /// <param name="kof">The kof.</param>
    /// <param name="state">The state.</param>
    /// <param name="timeout">The timeout.</param>
    public void GetHelpInfo(OkfStringSensors okf, KofCallback kof, object state, double timeout) {
      stringSensors(Commands.c_Help(), okf, kof, state, timeout, "GetHelpInfo(..)");
    }
    /// <summary> It gets the BTCom version. </summary>
    /// <param name="okf">A function called after the confirmation answer is received.</param>
    /// <param name="kof">A function called after the timeout is elapsed if the confirmation of command has not been received.</param>
    /// <param name="state">An instance of any class, which is passed to the callback function as an argument.</param>
    /// <param name="timeout">Timeout[sec] set how long are you willing to wait for the command confirmation answer.</param>
    public void GetVersionInfo(OkfStringSensors okf, KofCallback kof, object state, double timeout) {
      stringSensors(Commands.c_Version(), okf, kof, state, timeout, "BTComVersion(..)");
    }
        
    #endregion
    
  }
}
