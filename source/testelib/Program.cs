/*////////////////////////////////////////////////////////////////////
//       Elib library and TestElib console application              //
//        for a remote control of e-Puck robot.                    //
//               As a bachelor study                                //
//              Ondrej Platek (c),2010                              //
//                      and                                         //
//           rndr. Frantisek Mraz(c), csc. (Supervisor)             //
//        Please use the code and application freely,               //
//           but add a reference to this work.                      //
//////////////////////////////////////////////////////////////////////
///////////////////// Main begins at 76 ////////////////////////////*/
using System;
using Elib;
using System.Threading;
using System.Drawing;
using System.Text;
using System.IO;

namespace TestElib {  
  partial class Program {
    /*////////////////////////////////// Main function ///////////////////////////////////////////////////*/

    /// <summary>
    /// The Main function presents all prepared examples, which introduce Elib dictionary.
    /// The examples are listed from the easiest to the more complex.
    /// The key section implements different behaviours.
    /// Section tips and tricks presents not so common, but useful code.
    /// Keep in mind, that  <see cref="Program.to"/>,<see cref="Program.toImg"/>
    /// etc. significantly influence the behaviour of functions!
    /// </summary>
    static void Main() {      
      //if your e-Puck moves everything is OK.
      TestPortTurnAround("/dev/rfcomm0");
      
      //see startEpuck and how easy is to make connection
      Epuck ada = startEpuck("/dev/rfcomm0","Ada");
      // Do you want to see, where the limit of timeout is? 
      // Set your timeout and runs this simple functions below!
      ConsoleTestActuatorsTimeout(ada, to);//wait to answer before, in order to measure time (logging can measure time in much more convenient way.)
      ConsoleTestSensorsTimeout(ada, to); //same example for Sensors
      ConsoleAsynchronousSensors(ada, to);
      Console.WriteLine("Close the new windows in order to continue with the TestElib program");
      // Show the biggest image, which can e-Puck capture.
      ShowImage(ada);
      
      // /////////////////////// Behaviours //////////////////////////////
      
      // See Chapter 6 of Ondrej Platek bachelor thesis and the code for detail info behaviours.
      // Press enter to end it and go to next action
      // IAsyncResult interface usage
      Bull(ada);//infinite behaviour
      Billiard(ada);//infinite behaviour

      Console.WriteLine("The behaviour last about 10 seconds pressing an enter cause to stop it after it finishes the square.");
      //Can throw an exception.
      GoAndTurn(ada);//finite

      Go2Light(ada);//infinite behaviour
      // Epuck basic interface usage
      // KofGoXcm(ada,20,0.2);

      // //////////////////// Tips and tricks //////////////////////////
      // See Chapter 5 of bachelor thesis and the code for detail info.
      
      // Example of filtering red colour from image. Try it on e-Puck's box. Capture e-Puck logo;-).
       
      ShowProcessedImage(ada);
      
      // Runs TestEpuck.ConsoleTestSensorsTimeout twice with different timeouts and logs to log file
      // LoggingExample(ada,"log");//If you do not specify your path, log is located in Debug or Release folder of this solution.
      
      //Appropriate end of session. (But not completely necessary.)
      endEpuckSession(ada);      
    }


    /// <summary>
    /// Creates an Epuck instance and starts the session with real e-Puck.
    /// </summary>
    /// <returns></returns>
    /// <remarks>The ports are specific according settings of your computer. The two lines below shows the typical format.
    /// <c>string port="/dev/rfcomm0";</c> is an example of port under Linux. Under windows looks serial port name like <c>string port = "COM4";</c>.
    /// </remarks>
    static Epuck startEpuck(string port,string name) {
      // Typical port name under Windows is COM4, Unix is /dev/rfcomm0
      // Instanciation of Epuck can take a while (under 500ms)
      //Name it. It is useful for logging and debugging while working with multiple robots
      return new Epuck(port,name);
    }
    /// <summary>
    /// Terminates the session with e-Puck in a nice way.
    /// </summary>
    /// <param name="robot">An <see cref="Elib.Epuck"/> instance.</param>
    static void endEpuckSession(Epuck robot) {
      // e-Puck should be stopped at the end
      try {
        IAsyncResult ar = robot.BeginStop(2 * to, null, null);
        robot.EndFtion(ar);
      }
      catch (TimeoutElibException) {
        Console.WriteLine("Catch the robot!");
      }
      // disposal can take a while (under 500ms)
      robot.Dispose();
    }

    /// <summary>
    /// Timeout for the command, which sets CamParameters
    /// </summary>
    static double toSetCam = 0.3;//0.3 sec is common
    /// <summary>
    /// Timeout for the command, which asks for picture
    /// </summary>
    static double toImg = 0.7;//0.7 sec is common(depends on light condition)
    /// <summary>
    /// Timeout for the command, which resets e-Puck
    /// </summary>
    static double toReset = 1.5;//1.5 sec is common
    /// <summary>
    /// Timeout for common commands
    /// </summary>
    static double to = 0.2;
  }
}
