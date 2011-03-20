using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace Simulator {
  public partial class EpuckS {
    bool disposed;    
    TextWriter w;
    SerialPort p;
    object commandsLock;
    StringBuilder commands;
    volatile bool end;
    EventWaitHandle read;    

    public EpuckS(string port,TextWriter log) {
      end = true;      
      w = log;
      disposed = false;
      commands = new StringBuilder();
      commandsLock = new object();
      end = false;
      read = new EventWaitHandle(false,EventResetMode.ManualReset);
      p = new SerialPort(port);      
      p.ReceivedBytesThreshold = 1;      
    }
  
    public void Run() {
      p.Open();
      end = false;
      p.DataReceived += new SerialDataReceivedEventHandler(Read);
    }

    void Read(object sender, SerialDataReceivedEventArgs e) {
      lock (commandsLock) {
        string n = p.ReadExisting();
        for (int i = 0; i < n.Length; ++i) {
          if (n[i] != '\n') {//throws away '\n'
            commands.Append(n[i]);
            if (n[i] == '\r' && commands.ToString() != "r\r") {
              answer(commands.ToString());
              commands.Remove(0, commands.Length);
            }
          }
        }//end for loop
      }
    }

    int OneArg(string command)  {
      int arg;
      if (command.Length != 4 || !Int32.TryParse(command[2].ToString(), out arg))
        throw new ArgumentException();
       return arg;
    }
    int[] TwoArgs(string command){
      string[] ans=command.Split(new char[]{','});
      int[] args=new int[2];
      if (ans.Length != 3 || !Int32.TryParse(ans[1], out args[0]) ||
        !Int32.TryParse(ans[2], out args[1]))
        throw new ArgumentException();            
      return args;
    }    
    
    private void answer(string command) {      
      string ans;
      int[] args;
      int arg;
      command = command.ToUpper();
      if(command.Length>0){
        switch (command[0]) {
          case 'B': arg = OneArg(command); 
            if (arg < 0 || arg > 2) throw new ArgumentException(); 
            ans = BodyLightAction(arg); break;
          case 'F': arg=OneArg(command);
            if (arg < 0 || arg > 2) throw new ArgumentException(); 
            ans=FrontLightAction(arg);break;
          case 'E': ans=GetSpeed();break;
          case 'A': ans=GetAcccelerometr();break;
          case 'C': ans=GetSelectorPos();break;
          case 'G': ans=GetIRInfo();break;
          case 'L': args = TwoArgs(command); 
            if (args[1] < 0 || args[1] > 2||args[0]<0||args[0]>8) throw new ArgumentException(); 
            ans = Lights(args); break;
          case 'K': ans = CalibrateIR(); break;
          case 'I': ans = GetCamParams(); break;
          case 'H': ans = GetHelp(); break;
          case 'S': ans = Stop(); break;
          case 'P': args = TwoArgs(command); ans = SetMotorsPos(args); break;
          case 'Q': ans = GetMotorPos(); break;
          case 'O': ans = GetAmbientLight(); break;
          case 'N': ans = GetProximity(); break;
          case 'D': args = TwoArgs(command);
            if (args[1] < -1000 || args[1] > 1000 || args[0] < -1000 || args[0] > 1000) throw new ArgumentException(); 
            ans = Move(args); break;
          case 'R': ans = Reset(); break;
          case 'V': ans = GetVersion(); break;
          case 'U': ans = GetMicrophones(); break;
          case 'T': arg=OneArg(command); 
            if (arg < 0 || arg > 6) throw new ArgumentException();
            ans = Play(arg); break;
          case 'J': throw new NotImplementedException();//todo setCamParams and '-I' get Image
          default: ans = ""; throw new ArgumentException();                            
        }
        p.Write(ans);
        if(w!=null){          
          w.WriteLine(command);
          w.WriteLine(ans);          
        }
      }//end if(command.Length>0)      
    }

  

    #region Disposing            
    public void Dispose(){ Dispose(true); }

    ~EpuckS() { Dispose(false); }

    protected void Dispose(bool disposing) {
      if (!disposed) {
        disposed = true;
        end = true;        
        if (disposing) {
          commands = null;
          //free manage memberes
        }
        //free unmanage resources
        p.Close();
      }
    }
    #endregion Disposing
    
  }

}
