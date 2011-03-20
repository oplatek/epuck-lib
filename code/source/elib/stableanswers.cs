using System;
using System.Collections.Generic;
using System.Text;

namespace Elib {
  partial class Sercom{
    static Dictionary<char, string> stableAnsCommands = new Dictionary<char, string>();
    static Sercom(){
     stableAnsCommands.Add('B',"b\r\n");
     stableAnsCommands.Add('D',"d\r\n");
     stableAnsCommands.Add('F',"f\r\n");
     stableAnsCommands.Add('J',"j\r\n");
     stableAnsCommands.Add('H',Help);
     stableAnsCommands.Add('K',"k, Starting calibration - Remove any object in sensors range\r\nk, Calibration finished\r\n");
     stableAnsCommands.Add('P',"p\r\n");
     stableAnsCommands.Add('R',RestartMessage);
     stableAnsCommands.Add('S',"s\r\n");
     stableAnsCommands.Add('V',Version);      
    }
    /// <summary>
    /// A textual commands, which have always an identical answer.
    /// </summary>
    public static readonly List<char> StableAnsCommandsChars = new List<char>( stableAnsCommands.Keys);

    /// <summary>
    /// A textual help about BTCom protocol, which is stored on e-Puck. 
    /// </summary>
    /// <remarks>We suppose, that Elib is used with BTCom version 1.1.3, so we know how the help looks like.</remarks>
    public const string Help = 
"\n\"A\"         Accelerometer\r\n\"B,#\"       Body led 0=off 1=on 2=inverse\r\n\"C\"         Selector position\r\n\"D,#,#\"     Set motor speed left,right\r\n\"E\"         Get motor speed left,right\r\n\"F,#\"       Front led 0=off 1=on 2=inverse\r\n\"G\"         IR receiver\r\n\"H\"\t     Help\r\n\"I\"         Get camera parameter\r\n\"J,#,#,#,#\" Set camera parameter mode,width,heigth,zoom(1,4 or 8)\r\n\"K\"         Calibrate proximity sensors\r\n\"L,#,#\"     Led number,0=off 1=on 2=inverse\r\n\"N\"         Proximity\r\n\"O\"         Light sensors\r\n\"P,#,#\"     Set motor position left,right\r\n\"Q\"         Get motor position left,right\r\n\"R\"         Reset e-puck\r\n\"S\"         Stop e-puck and turn off leds\r\n\"T,#\"       Play sound 1-5 else stop sound\r\n\"U\"         Get microphone amplitude\r\n\"V\"         Version of SerCom\r\n";
    /// <summary>
    /// A message, which is sent from e-Puck after a restart.
    /// </summary>
    /// <remarks>We suppose, that Elib is used with BTCom version 1.1.3, so we know how the help looks like.</remarks>
    public const string RestartMessage =
              "r\r\f\aWELCOME to the SerCom protocol on e-Puck\r\nthe EPFL education robot type \"H\" for help\r\n";
    /// <summary>
    /// <remarks>We suppose, that Elib is used with BTCom version 1.1.3, so we know the version.</remarks>
    /// </summary>
    public const string Version="v,Version 1.1.3 September 2006\r\n";
  }
}
