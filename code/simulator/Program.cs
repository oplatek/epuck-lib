using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace Simulator {
  class Program {
    static void Main(string[] args) {      
      if (args.Length != 3)
        throw new ArgumentException("You have to fill 3 arguments. configure.vspe, port to simulator (\"COM1\"), port to your application (\"COM2\")");      
      Process v = null;
      try {
        ProcessStartInfo si = new ProcessStartInfo(args[0]);
        si.WindowStyle = ProcessWindowStyle.Minimized;        
        v = Process.Start(si);
      } catch (Win32Exception ex) {
        Console.WriteLine(ex.Message);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
      Thread.Sleep(500);      
      EpuckS ep = new EpuckS(args[1],Console.Out);
      Thread.Sleep(1000);
      try {
        ep.Run();
        Console.WriteLine("Simulator is connected to emulatad {0}",args[1]);
        Console.WriteLine("there is {0} ready for your e-Puck application",args[2]);
      } catch (IOException) {
        Console.WriteLine("If VSPE is running just try again, because sercom {0} is not open!;)",args[1]);
      }
            
      Console.WriteLine("Press enter to end the simulation....");
      Console.ReadLine();
            
      ep.Dispose();      
      //closing opened application
      if (v != null) {
        if(!v.HasExited)
          v.Kill();
        v.Close();
      }
    }    
  }
}
