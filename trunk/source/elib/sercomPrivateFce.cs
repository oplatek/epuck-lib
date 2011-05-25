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
      lock (notSentLock) {
        // if( we have command to send && we are not waiting to confirmation answer of previous ) { ..
        if (notSent.Peek != null && Interlocked.Equals(hshake_sent, null)) {
          command = notSent.Peek.Elem;
          notSentSTS.Remove(notSent.Peek);
          notSent.Dequeue();          
        }
      }
      if (command != null) {         
        if (Stamp.Get() > command.timeout) {
          callKof(command);
        }
        else {
          //wakes the thread that keeps an eye on every hshake_sent ansCommand if it is not expired
          hshake_sentUpdateWh.Reset();
          checkSDwh.Set();
          lock (hshake_sentLock) {              
            // necessary for
            hshake_sent = command;
            ans.Remove(0, ans.Length);
            if (stableAnsCommands.ContainsKey(command.command[0]))
              stableAns = true;
            else
              stableAns = false;
          }
          hshake_sentUpdateWh.Set();            
          try {
            if (command.command == Commands.c_GetImage()) {
              getImage();           
            }else{
              port.Write(command.command);
            }
          }
          catch (Exception e) {
            //IMPORTANT hshake_sent!=null  
            Interlocked.Exchange<ansGuard>(ref hshake_sent, null);
            throw new SerialPortException("Serial port.write() is not responding in time for current timeout", e);
          }
        }//end if command is late                      
      }
    }//end of SendAsyncCall()     
     
    void checkNSwhSet() {
      while (!sleeping) { Thread.Sleep(10); }
      Thread.Sleep(10);
      checkNSwh.Set();       
    }

    void CheckNS() {      
      double now;    
      myQueue kofs = new myQueue();
      double nextTO;
      while (!stoppedSend) {                
        lock (notSentLock) {
          now = Stamp.Get();
          while (notSentSTS.Peek != null && now > notSentSTS.Peek.Elem.timeout) {
            kofs.Enqueue(new node(notSentSTS.Peek.Elem));
            notSent.Remove(notSentSTS.Peek.Elem.nS);
            notSentSTS.Dequeue();
          }
        }
        foreach (node n in kofs) {
          if (n.Elem.command == Commands.c_GetImage())
            Interlocked.Exchange<byte[]>(ref pics, null);
          callKof(n.Elem);
        }
        kofs.Clear();

        //wake/sleep management (cooperates only if new element is added to notSent if deleted it wakes according its plan)
        lock (notSentLock) {
          if (notSentSTS.Peek == null)
            nextTO = -1;
          else
            nextTO = notSentSTS.Peek.Elem.timeout;
        }
        sleeping = true;
        if (nextTO < 0)
            checkNSwh.WaitOne();
        else if (nextTO < Stamp.Get() + 0.01){
              Thread.Sleep(10);
		} else{
			checkSDwh.WaitOne((int)((nextTO - Stamp.Get()) * 1000),false);
		}
        sleeping = false;
      }//end while(!stopped_send)
    }//end of checkNS

    void checkSD() {
      double nextTO = -1;
      while (!stoppedConfirm) {
        if (text_mode) {                    
          nextTO = -1;
          lock (hshake_sentLock) {
            if (hshake_sent != null) {
              if (Stamp.Get() > hshake_sent.timeout) {
                callKof(hshake_sent);
                //nextTo==-1 and checkSD will wait until next ansGuard will be sent
                hshake_sent = null;
                Received(null, null);
              } else
                nextTO = hshake_sent.timeout;
            }
          }//endlock                              
        }//end if(text mode)
        //wait/sleepmanagement
        if (nextTO == -1) {
          checkSDwh.WaitOne();
        } else if (Stamp.Get() + 0.01 < nextTO) {
			    checkSDwh.WaitOne((int)((nextTO - Stamp.Get()) * 1000),false);
        }
        hshake_sentUpdateWh.WaitOne();//has to set up if you set up hshake_sent
      }//end while loop      
    }//end of checkSD

    delegate void textModeReadCall(string ans);
    void Read(object sender, SerialDataReceivedEventArgs e) {
      if (ready) {
        if (text_mode)          
          textModeCall();
        else          
          binaryModeRead();                         
      } else {
        port.ReadExisting();
      }
    }

    void binaryModeRead() {
      ansGuard a=null;      
      Interlocked.Exchange<ansGuard>(ref a, hshake_sent);
      double to = a.timeout;
      if (Stamp.Get() >= a.timeout)
        callKof(a);
      else {
          try {
              //It is not convenient call kof during sending a picture
              int readBytes = 0;
              int newBytes;
              int size = 3;
              byte[] pom = new byte[size];
              while ((readBytes += newBytes = port.Read(pom, readBytes, size - readBytes)) < size) {
                  if (newBytes > 0)
                      to = Stamp.Get();
                  else if (Stamp.Get() - to > imgTo) {
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
                  else if (Stamp.Get() - to > imgTo) {
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
          } catch (IOException e) { 
            callKof(a);
            throw new SerialPortException("Serial port.Read() is not responding in time for current Readtimeout", e);
          }          
      }
      endBinaryMode();         
    }

    void endBinaryMode() {
      //very important, if omitting it blocks this thread 
      text_mode = true;
      //very important, if omitting it blocks this thread 
      lock (hshake_sentLock) {
        hshake_sent = null;
      }
      Received(null, null);
    }

    void textModeCall() {
        try {
            string r = port.ReadExisting();
            lock (hshake_sentLock) {
                //we want answer only if someone is receiving
                if (hshake_sent != null) {
                    for (int i = 0; i < r.Length; ++i) {
                        if (stableAns) {
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
                                    if (ans.Length > 0 && hshake_sent.command[0] == Char.ToUpper(ans[0]) && (Stamp.Get() < hshake_sent.timeout))
                                        weHaveAnswer();
                                }//end if (we have answer)
                            }
                        }
                    }//end for loop
                }
            }
        } catch (IOException e) {            
            throw new SerialPortException("Serial port.Read() is not responding in time for current Readtimeout", e);          
        }
    }

    void weHaveAnswer() {
      callOkf(hshake_sent, ans.ToString());
      hshake_sent = null;
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
      try {
          port.Write(new byte[] { 183, 0 }, 0, 2);
      } catch (IOException e) {           
        throw new SerialPortException("Serial port.Write() is not responding in time for current Write timeout", e);          
      }
    }


    /// <summary>
    /// Releases unmanaged (close serial port) and - optionally - managed resources (stop threads in nice way).
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
    /// <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
      if (!disposed) {
        disposed = true;
        stoppedSend = true;       
        stoppedConfirm = true;
        hshake_sentUpdateWh.Set();
        checkSDwh.Set();        
        checkNSwh.Set();
        
        if (disposing) {
          Thread.Sleep(20);
          //if the check threads were controlling during the first call of checkSDwh.Set(); ..
          checkSDwh.Set();
          checkNSwh.Set();
          chN.Join(20);          
          chS.Join(20);         
          lock (notSentLock) {           
            foreach (node n in notSent) {
              callKof(n.Elem);                
            }
            notSent.Clear();
          }
          
        }//end if(disposing)

        chN.Abort();//ends the thread that checks for run out commands in notSent
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
  /// A wrapper, which converts a system clock ticks to seconds. 
  /// </summary>
  public static class Stamp {
    /// <summary>
    /// Gets the number of seconds from turning on the computer. Do not handle overflow of the variable with tick count, 
    /// so it is restarted to 0 after long processor run .
    /// </summary>
    /// <returns></returns>
    public static double Get() {
      return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
    }
  }

}
