﻿/*#define checkSDlog
#define testModeCalllog
#define SendAsyncCalllog
#define CheckNSlog
*/
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Elib {
  public partial class Sercom {
    #region Protected and private fce
    delegate void SendAsync();
    void Send(object Sender, EventArgs ev) {
      SendAsync asyncCaller = new SendAsync(SendAsyncCall);
      asyncCaller.BeginInvoke(null, null);      
    }
    void SendAsyncCall() {
      ansGuard command = null;
      lock (notSendedLock) {
        if (notSended.Peek != null && Interlocked.Equals(hshake_sended, null)) {
          command = notSended.Peek.Elem;
          notSendedSTS.Remove(notSended.Peek);
          notSended.Dequeue();          
        }
      }
      if (command != null) {         
        if (Stamp.Get() > command.timeout) {
          callKof(command);
#if SendAsyncCalllog
          Console.WriteLine("on ansGuard {0} was called kof in SendAsyncCall(..)", command);
#endif
        }
        else {
          //wakes the thread that keeps an eye on every hshake_sended ansCommand if it is not expired
          hshake_sendedUpdateWh.Reset();
          checkSDwh.Set();
          lock (hsshake_sendedLock) {              
            hshake_sended = command;
            ans.Remove(0, ans.Length);
            if (stableAnsCommands.ContainsKey(command.command[0]))
              stableans = true;
            else
              stableans = false;
          }
          hshake_sendedUpdateWh.Set();            
          try {
            if (command.command == Commands.c_GetImage()) {
              getImage();           
            }else{
              port.Write(command.command);
            }
#if SendAsyncCalllog
            Console.WriteLine("Comand {0} was sended in SendAsyncCall(..)", command.command[0]);
#endif
          }
          catch (Exception e) {
            //IMPORTANT hshake_sended!=null  
            Interlocked.Exchange<ansGuard>(ref hshake_sended, null);
            throw new SerialPortException("Serial port.write() is not responding in time for current timeout", e);
          }
        }//end if commad is late                      
      }
    }//end of hSend()     
     
    /// <summary>
    /// Use out of notSendedLock !
    /// </summary>
    void checkNSwhSet() {
      while (!sleeping) { Thread.Sleep(10); }
      Thread.Sleep(10);
      checkNSwh.Set();       
    }
    void CheckNS() {      
      double now;    
      myQueue kofs = new myQueue();
      double nextTO;
      while (!stopped_send) {                
        lock (notSendedLock) {
          now = Stamp.Get();
          while (notSendedSTS.Peek != null && now > notSendedSTS.Peek.Elem.timeout) {
            kofs.Enqueue(new node(notSendedSTS.Peek.Elem));
            notSended.Remove(notSendedSTS.Peek.Elem.nS);
            notSendedSTS.Dequeue();
          }
        }
        foreach (node n in kofs) {
          if (n.Elem.command == Commands.c_GetImage())
            Interlocked.Exchange<byte[]>(ref pics, null);
          callKof(n.Elem);
#if CheckNSlog
          Console.WriteLine("in CheckNS on ansguard {0} was called kof", n.Elem);
          Console.WriteLine("hshake_sended in CheckNS {0}", hshake_sended);
#endif
        }
        kofs.Clear();

        //wake/sleep management (cooperates only if new element is added to notSended if deleted it wakes according its plan)
        lock (notSendedLock) {
          if (notSendedSTS.Peek == null)
            nextTO = -1;
          else
            nextTO = notSendedSTS.Peek.Elem.timeout;
        }
        sleeping = true;
        if (nextTO < 0)
            checkNSwh.WaitOne();
        else if (nextTO < Stamp.Get() + 0.01){
              Thread.Sleep(10);
		} else{
			checkSDwh.WaitOne((int)((nextTO - Stamp.Get()) * 1000),true);
		}
        sleeping = false;
      }//end while(!stopped_send)
    }//end of checkNS

    void checkSD() {
      double nextTO = -1;
      while (!stopped_confirm) {
        if (text_mode) {                    
          nextTO = -1;
          lock (hsshake_sendedLock) {
            if (hshake_sended != null) {
              if (Stamp.Get() > hshake_sended.timeout) {
                callKof(hshake_sended);
#if checkSDlog
                Console.WriteLine("in checkSD was on hsshake_sended=={0} called kof in time {1} with to {2}", hshake_sended,stamp.Elapsed(),hshake_sended.timeout);
#endif
                //nextTo==-1 and checkSD will wait until next ansGuard will be sended
                hshake_sended = null;
                Received(null, null);
              } else
                nextTO = hshake_sended.timeout;
            }
          }//endlock                              
        }//end if(text mode)
        //wait/sleepmanagement
        if (nextTO == -1) {
          checkSDwh.WaitOne();
        } else if (Stamp.Get() + 0.01 < nextTO) {
			    checkSDwh.WaitOne((int)((nextTO - Stamp.Get()) * 1000),true);
        }
        hshake_sendedUpdateWh.WaitOne();//has to set up if you set up hshake_sended
      }//end while loop      
    }//end of checkSD

    delegate void textModeReadCall(string ans);
    void Read(object sender, SerialDataReceivedEventArgs e) {
      if (ready) {
        if (text_mode)          
          textModeCall();
        else          
          binaryModeRead();                         
      }else
#if MYDEBUG
        Console.WriteLine("READY=false: {0}",port.ReadExisting());
#else
        port.ReadExisting();
#endif
    }
    void binaryModeRead() {
      ansGuard a=null;      
      Interlocked.Exchange<ansGuard>(ref a, hshake_sended);
      double to = a.timeout;
      if (Stamp.Get() >= a.timeout)
        callKof(a);
      else {
        //It is not convenient call kof during sending a picture
        int readBytes = 0;
        int newBytes;
        int size = 3;
        byte[] pom = new byte[size];
        while ((readBytes += newBytes = port.Read(pom, readBytes, size - readBytes)) < size) {
          if (newBytes > 0)
            to = Stamp.Get();
          else if (Stamp.Get() - to > 0.1) { 
            //we have not received any new byte in 0.1s
            callKof(a);
            endBinaryMode();
            return;
          }
        }
        //mode is 1,2  -> RGB565 image ->*2 else mode is 0 ->grey scale picture-> *1
        mode = pom[0];
        width = pom[1];
        height = pom[2];
        size = height * width;
        if (pom[0] >= 1)
          size *= 2;
        pom = new byte[size];
        readBytes = 0;
        while ((readBytes += newBytes = port.Read(pom, readBytes, size - readBytes)) < size) {
          if (newBytes > 0)
            to = Stamp.Get();
          else if (Stamp.Get() - to > 0.1) { 
            //we have not received any new byte in 0.1s
            callKof(a);
            endBinaryMode();
            return;
          }
        }
        Interlocked.Exchange<byte[]>(ref pics, pom);
        fullimage = (size == readBytes);
        if (Stamp.Get() >= a.timeout) 
          callKof(a);
        else
          callOkf(a, "-I ok\r\n");
      }
      endBinaryMode();         
    }
    void endBinaryMode() {
      //very important, if omitting it blocks this thread 
      text_mode = true;
      //very important, if omitting it blocks this thread 
      lock (hsshake_sendedLock) {
        hshake_sended = null;
      }
      Received(null, null);
    }
    void textModeCall() {      
      string r = port.ReadExisting();
      lock (hsshake_sendedLock) {
        //we want answer only if someone is receiving
        if (hshake_sended != null) {
          for (int i = 0; i < r.Length; ++i) {
            if (stableans) {
              ans.Append(r[i]);
              foreach (String s in stableAnsCommands.Values) {
                if (ans.Length == s.Length && ans.ToString() == s) {
                  weHaveAnswer();
                  break;//answer are different 
                }
              }              
            } else {
              //we throw away \r if we are not in longtext mode
              if (r[i] != '\r') {
                if (r[i] != '\n') {
                  ans.Append(r[i]);
                } else {
                  //we have answer(\n ends an answer), we have thrown away \r\n
                  if (ans.Length>0 && hshake_sended.command[0] == Char.ToUpper(ans[0]) && (Stamp.Get() < hshake_sended.timeout)) 
                    weHaveAnswer();
                }//end if (we have answer)
              }
            }
          }//end for loop
        }
      }
    }
    void weHaveAnswer() {
      callOkf(hshake_sended, ans.ToString());
      hshake_sended = null;
      Received(null, null);    
    }
    
    void callKof(ansGuard a) {
      NotAnswered--;
      if (a.kof.f != null) {
        a.kof.f.BeginInvoke(a.kof.data, null, null);
      }
    }//end of callKof
    void callOkf(ansGuard a, string ans) {
      NotAnswered--;
      if (a.okf.f != null) {
        a.okf.f.BeginInvoke(ans, a.okf.data, null, null);
      }
    }//end of callOkf(..)

    private void getImage() {
      Interlocked.Exchange<byte[]>(ref pics, null);      
      fullimage = false;
      //in Read(..) (different thread)after reading image in binary mode the text_mode is set to true.
      text_mode = false; 
      //183 == -'I' in c++ and BTCom interpret -'Char' commands in binary mode 
      port.Write(new byte[]{183,0},0,2);      
    }



    /// <summary>
    /// Releases unmanaged (close serial port) and - optionally - managed resources (stop threads in nice way).
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
      if (!disposed) {
        disposed = true;
        stopped_send = true;       
        stopped_confirm = true;
        hshake_sendedUpdateWh.Set();
        checkSDwh.Set();        
        checkNSwh.Set();
        
        if (disposing) {
          Thread.Sleep(20);
          //if the check threads were controlling during the first call of checkSDwh.Set(); ..
          checkSDwh.Set();
          checkNSwh.Set();
          chN.Join(20);          
          chS.Join(20);         
          lock (notSendedLock) {
#if MYDEBUG
            Console.WriteLine("Dispose notSendedn Count {0}", notSended.Count);
#endif
            
            foreach (node n in notSended) {
              callKof(n.Elem);                
            }
            notSended.Clear();
          }
          
        }//end if(disposing)

        chN.Abort();//ends the thread that checks for run out commands in notSended
        chS.Abort();

        checkNSwh.Close();
        checkSDwh.Close();
        
        if (port.IsOpen)
          port.Close();
        port = null; //to null resources helps GC to release faster this resources
      }
    }

    #endregion
  }
  /// <summary>
  /// A wraper, which converts a system clock ticks to seconds. 
  /// </summary>
  public static class Stamp {
    /// <summary>
    /// Gets the number of seconds from turning on the computer. 
    /// </summary>
    /// <returns></returns>
    public static double Get() {
      return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
    }
  }

}
