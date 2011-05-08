/*This is just a wrapper for System.IO.Ports, which reimplements DataReceived event, because the Elib library
  should be platform independent and mono 2.10 still does not implement the DataReceived event. */
using System;
using System.Text;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;

namespace OP {

  /// <summary>
  /// This is just a wrapper for System.IO.Ports, which reimplements DataReceived event, because the Elib library
  /// should be platform independent and mono 2.10 still does not implement the DataReceived event. 
  /// </summary>
  class WrapSerialPort:SerialPort {
      SerialDataReceivedEventHandler dataReceived;
      List<byte> buffer;
      Object mylock;
      Thread t;
      volatile bool end = false;

      public new SerialDataReceivedEventHandler DataReceived {
          get { return dataReceived; }
          set { dataReceived = value; }
      }

      public WrapSerialPort(string portName):base(portName, 115200, Parity.None, 8, StopBits.One){
          mylock = new object();
          buffer = new List<byte>();
          t = new Thread(readout);
          t.IsBackground = true;
          t.Priority = ThreadPriority.Highest;
                  }

      private void myDispose(bool disposing) {
          end = true;
          Thread.Sleep(10);
          t.Abort();
          base.Close();
      }

      public new void Close() { myDispose(false); GC.SuppressFinalize(this); }

      public new void Dispose() { myDispose(false); GC.SuppressFinalize(this); }

      ~WrapSerialPort() { Dispose(true); }
              
      public new void Open(){
        t.Start();
        base.Open();                    
      }
              
      public new string ReadExisting() {
          string s = null;
          lock (mylock)
          {
              s=this.Encoding.GetString(buffer.ToArray());
              buffer.Clear();
          }            
          return s; 
      }
      
      public new void Read(byte[] array, int Offset, int Count) {
          if(array!=null)
              throw new ArgumentException("Can not assign to null array!");
          if(Offset+Count+1>array.Length)
              throw new ArgumentException("Offset has to be smaller thant array.Length and Offset + Count has to be array.Length");

          int read=0;
          while (read < Count)
          {                
              lock (mylock)
              {
                  if (this.BytesToRead > 0)
                  {
                      array[Offset + read] = (byte)this.ReadByte();
                      read++;
                  }
              }                
          }
      }

      private void readout() {
        while (!end) {
          lock (mylock) {
            while (this.IsOpen && this.BytesToRead > 0) {
              buffer.Add((byte)this.ReadByte());
              dataReceived(this, null);
            }
          }
        }
      }        
  }
}
