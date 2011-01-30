using System;
using System.Text;
using Elib;
using System.Threading;
using System.Drawing;

namespace TestElib {
    partial class Program{
      //allows to end the asynchronous programming model in Behaviours in nice way
      static volatile bool endf;
      static EventWaitHandle endconfirmed;
      static void startBehaviour() {
        endf = false;
        endconfirmed = new EventWaitHandle(false, EventResetMode.ManualReset);
      }
      static void endBehaviour() {
        if (!endf) {
          //If you press an key endf is set to true and all functions accessing Epuck instance runs only if endf==false
          Console.WriteLine("Press enter to quit..");
          //Behaviours are usually infinite loops, they run and run.
          Console.ReadLine();
          //You press any the behaviour is still running=>endf==false, or it has already ended endf==true
          if (!endf) {
            // Blocks invoking next function
            endf = true;
            //Wait until current function is finished for long time.
            endconfirmed.WaitOne((int)(1000 * (toReset + toImg + to)),false);
          }
        }
        //releasing EventWaitHandle
        endconfirmed.Close();
      }

      #region BullBehaviour
      /// <summary>
      /// Switch between 3 behaviour. In first mode it goes randomly.
      /// Second behaviour search for something red or orange. Third action goes to the red colour.
      /// If exception occurs, it turns off possible side actions with endf=true. The side action
      /// is ending of BullBehaviour itself. It turns Off Console.ReadLine in endBehaviour function.
      /// </summary>
      /// <param name="ada"></param>
      public static void Bull(Epuck ada) {
        startBehaviour();
        //modes are Big(not very usefull),Medium(useful big zoom),Small(most common lowest) zoom
        //not nice: trimmed pictures are from top left area.
        //best choice is zoom 8 a picture 40*40 because it can quite good se on a floor.      
        int width = 40, height = 40;
        try {
          IAsyncResult ar = ada.BeginSetCam(width, height, Zoom.Small, CamMode.Color, toSetCam, null, null);
          ada.EndFtion(ar);
          ada.BeginGetImage(toImg, searchAround, new CountDown(ada, 3));
          endBehaviour();
        } catch (ElibException e) {
          exceptionOccured_End("Exception was thrown in Bull(..)", e);
        }
      }

      ///<summary>Turn off sending the Stop command in endBehaviour</summary>
      static void exceptionOccured_End(string source, ElibException e) {
        Console.WriteLine(source + "\n" + e.Message);
        endf = true;
      }
      static void howRed(Bitmap bm, out double procentage, out double avgRedWidth) {
        double rdown = 1, rup = 1, gdown = 0, gup = 0.7, bdown = 0.0, bup = 0.7;
        double red, green, blue;
        double w = 0;
        int count = 0;
        for (int i = 0; i < bm.Width; ++i) {
          for (int j = 0; j < bm.Height; ++j) {
            Color c = bm.GetPixel(i, j);
            int darkness = Math.Max(Math.Max(c.R, c.G), c.B);
            red = c.R / (double)darkness; green = c.G / (double)darkness; blue = c.B / (double)darkness;
            if (rdown <= red && red <= rup && gdown <= green && green <= gup && bdown <= blue && blue <= bup) {
              count++;
              w += i;
            }
          }
        }
        procentage = count / (double)(bm.Width * bm.Height);
        avgRedWidth = (w / count) / bm.Width;
      }
      struct CountDown {
        public Epuck e;
        public int left;
        public CountDown(Epuck e_, int left_) { e = e_; left = left_; }
      }
      static void searchAround(IAsyncResult ar_) {
        if (!endf) {
          CountDown cd = (CountDown)ar_.AsyncState;
          Epuck ada = cd.e;
          double redl;
          double dirl = redl = 0.3;
          int degrees = 20;
          AsyncResult<Bitmap> ar = (AsyncResult<Bitmap>)ar_;
          double red, dir;
          try {
            howRed(ada.EndGetImage(ar), out red, out dir);
            if (red > redl) {
              if (dir < dirl)
                turnAround(cd.e, 1, -degrees, to);
              if (dir > 0.6)
                turnAround(cd.e, 1, degrees, to);
              cd.e.BeginMotors(1, 1, to, agressive, cd.e);
            } else {
              turnAround(cd.e, 0.2, 90, to);
              if (cd.left > 0)
                cd.e.BeginGetImage(toImg, searchAround, new CountDown(cd.e, --cd.left));
              else
                cd.e.BeginMotors(0.1, 0.1, to, wander, cd.e);
            }
          } catch (ElibException e) {
            exceptionOccured_End("Exception was thrown in searchAround(..)", e);
          }
        } else
          endconfirmed.Set();
      }
      static void agressive(IAsyncResult ar_) {
        if (!endf) {
          double redl = 0.2;
          double red, dir;
          Epuck ada = (Epuck)ar_.AsyncState;
          try {
            IAsyncResult ar = ada.BeginGetImage(toImg, null, null);
            Bitmap a = ada.EndGetImage(ar);
            howRed(a, out red, out dir);
            Console.WriteLine(red);
            if (red > redl) {
              ada.BeginMotors(0.4, 0.4, to, agressive, ada);
            } else {
              ar = ada.BeginStop(to, null, null);
              ada.EndFtion(ar);
              ada.BeginGetImage(toImg, searchAround, new CountDown(ada, 3));
            }
          } catch (ElibException e) {
            exceptionOccured_End("Exception was thrown in Bull(..)", e);
          }
        } else
          endconfirmed.Set();
      }
      static void wander(IAsyncResult ar_) {
        if (!endf) {
          double rlim = 0.03;
          int irlim = 500;
          double speed = 0.1; double spdn = 3;
          Epuck ada = (Epuck)ar_.AsyncState;
          Random r = new Random();
          try {
            if (r.NextDouble() < rlim) {
              ada.BeginGetImage(toImg + to, searchAround, new CountDown(ada, 3));
            } else {
              IAsyncResult ar = ada.BeginGetIR(0.1, null, null);
              int[] ir = ada.EndGetFtion(ar);
              if (ir[0] + ir[1] + ir[7] + ir[6] < (4 * irlim)) {
                double L = speed + (r.NextDouble() / spdn); double R = speed + (r.NextDouble() / spdn);
                ada.BeginMotors(L, R, to, wander, ada);
              } else {
                int ind = -1, min = 1000;
                for (int i = 0; i < ir.Length; ++i) {
                  if (ir[i] < irlim && ir[i] < min) {
                    min = ir[i];
                    ind = i;
                  }
                }
                if (ind != -1) {
                  if (Epuck.IRSensorsDegrees[ind] < 180)
                    turnAround(ada, speed, Epuck.IRSensorsDegrees[ind], to);
                  else
                    turnAround(ada, speed, -(360 - Epuck.IRSensorsDegrees[ind]), to);
                  //ada.BeginPlaySound(5, to, wander, ada);
                } else {
                  Console.WriteLine(ada.ToString() + " get stacked");
                  ar = ada.BeginStop(to, null, null);
                  ada.EndFtion(ar);
                }
              }
            }
          } catch (ElibException e) {
            exceptionOccured_End("Exception was thrown in wander(..)", e);
          }
        } else
          endconfirmed.Set();
      }

      #endregion BullBehaviour

      #region Billiardball
      /// <summary>
      /// A behaviour, where e-Puck bahaves like billiardball. It goes straight until it is very near an obstacle 
      /// and then it bounce under same angle to other side.
      /// If an <exception cref="TimeoutElibException">TimeoutElibException</exception> is thrown, than it invokes the function again.
      /// </summary>
      /// <param name="ada">An <see cref="Elib.Epuck"/> instance.</param>
      public static void Billiard(Epuck ada) {
        startBehaviour();
        try {
          IAsyncResult ar = ada.BeginStop(to, null, null);
          ada.EndFtion(ar);
          ada.BeginGetIR(to, go2wall, ada);
        } catch (ElibException) {
          Console.WriteLine("Stop command has not been confirmed in time in Billiard starting function. Behaviour ends.");
          endf = true;
        }
        endBehaviour();
      }

      static void go2wall(IAsyncResult ar_) {
        //value to decide if an obstacle is near enough
        int frontLimit = 1000;
        //does not throw ElibException
        Epuck ada = (Epuck)ar_.AsyncState;
        if (!endf) {
          AsyncResult<int[]> ar = (AsyncResult<int[]>)ar_;
          try {
            //doesn't create EventWaitHandle because the action has already completed synchronously.
            //Can throw an TimeoutElibException
            int[] ir = ar.EndInvoke();
            if (ir[0] + ir[7] > frontLimit)
              ada.BeginStop(to, rebound, ada);
            else {
              //Does not use EndFtion, it safes the EventWaitHandle. We suppose, that it succeeds now or in next rounds.
              ada.BeginMotors(0.2, 0.2, to, null, null);
              //The BeginGetIR command is enqueued in the same mommen as BeginMotors, therefor 2*to.
              ada.BeginGetIR(2 * to, go2wall, ada);
            }
          } catch (ElibException e) {
            Console.WriteLine("Billiard restarted in go2wall, because of exception:\n" + e.Message);
            //Invokes go2wall function again. It needs to be invoked by BeginGetIR command, because it expects ar_ with IR values.
            ada.BeginGetIR(to, go2wall, ada);
          }
        } else
          endconfirmed.Set();
      }
      static void rebound(IAsyncResult ar_) {
        if (!endf) {
          //Does not throw an ElibException
          Epuck ada = (Epuck)ar_.AsyncState;
          try {
            IAsyncResult ar = ada.BeginGetIR(to, null, null);
            int[] ir = ada.EndGetFtion(ar);
            int[] index = new int[ir.Length]; for (int i = 0; i < index.Length; ++i) index[i] = i;
            Array.Sort(ir, index);
            Array.Reverse(ir);
            Array.Reverse(index);
            int limit = 50;
            int sumDegrees = 0;
            int byObstacle = 0;
            for (int i = 0; i < index.Length; ++i) {
              if (ir[0] - limit < ir[i]) {
                if (index[i] == 2 || index[i] == 3) {
                  //because in front of e-Puck (measured in go2wall(..)) is an obstacle and behind too.
                  ar = ada.BeginPlaySound(4, to, null, null);
                  ada.EndFtion(ar);
                  Console.WriteLine("e-Puck is stacked");
                  Console.WriteLine("Behaviour has finished. Do not wait to its end. Press enter!");
                  endf = true;
                  return;
                } else {
                  byObstacle++;
                  sumDegrees += Epuck.IRSensorsDegrees[index[i]];
                }
              } else
                break;
            }
            sumDegrees /= byObstacle;
            //has to be smaller than 90
            if (sumDegrees < 180)
              sumDegrees = -(180 - (2 * sumDegrees));
            //has to greater than 270
            else
              sumDegrees = 180 - 2 * (360 - sumDegrees);
            turnAround(ada, 0.2, sumDegrees, to);
            //Go again to next wall
            ada.BeginGetIR(to, go2wall, ada);
          } catch (ElibException e) {
            Console.WriteLine("Billiard restarted in go2wall, because of exception:\n" + e.Message);
            //Invokes rebound function again. It needs to be invoked by BeginStop command, but it never fails on in, because no GetFtion is
            //called after BeginStop. If the "stop" command is unconfirmed it continues going and than suddendly turnd around, if the connection has been recovered.
            ada.BeginStop(to, rebound, ada);
          }
        } else
          endconfirmed.Set();
      }
      #endregion Billiardball

      #region GoAndTurn
      /// <summary>
      /// Simple behaviour, which let e-Puck make a square.
      /// If an exception occured nothing is done.
      /// </summary>
      /// <param name="ada"></param>
      public static void GoAndTurn(Epuck ada) {
        startBehaviour();
        //stepper motors has resolution 0.13mm in one step, so speed 0.1==100steps should be 1,3cm per second

        //in 3 measurement it has gone exactly 13cm +-0.5mm with settings goXmiliseconds(ada, 0.1, 0.1, 10000, 0.1);
        //results goXmiliseconds(ada, 0.2, 0.2, 10000, 0.1) the results are: 26.1cm, 26cm, 26.1cm
        //results goXcm(ada, 1, 20); ..20.5cm,20cm
        //results goXcm(ada, 1, 10); ..11cm,11cm,11.1cm

        //has to be called if int ada.Working==0 
        //select one of these behaviours

        //goXmiliseconds(ada, 0.2, 0.2, 10000, 0.1);
        //goXcm(ada, 1, 20);
        //turnAround(ada, 1, 720,0.1);
        square(ada, 15, 0.2);
        Console.WriteLine("Behaviour has finished. Do not wait to its end. Press enter!");
        endBehaviour();
      }

      static void square(Epuck e, double cm, double speed) {
        for (int i = 0; i < 3; i++) {
          goXcm(e, speed, cm);
          turnAround(e, speed, 90, to);
        }
        goXcm(e, speed, cm);
      }
      static void turnAround(Epuck e, double speed, int degrees, double timeout) {
        /* turn around is quite accurate following snippet let e-Puck spin 360 degrees and results were -363,368,365,366,361,368,3,363,368,365 degrees
         * for(int i=0;i<10;++i)
         *   turnAround(ada, 1, 30, to);
         */
        /*perch(distance between wheels) of e-Puck is 5.3cm      
         *The weels diameter is about 41 mm. The distance between the wheels is about 53 mm. Perimeter is about 128.8mm.
         * The maximum speed of the wheels is about 1000 steps / s, which corresponds to one wheel revolution per second. 
         */
        double ms = Math.Abs(Epuck.Perch * Math.PI * ((double)degrees / 360) / (speed * Epuck.MaxSpeed) * 1000);
        speed *= degrees < 0 ? -1 : 1;
        goXmiliseconds(e, speed, -speed, (int)ms, timeout);
      }
      static void goXcm(Epuck e, double speed, double cm) {
        int ms = (int)((cm / (Epuck.MaxSpeed * speed)) * 1000);
        goXmiliseconds(e, speed, speed, ms, to);
      }
      static void goXmiliseconds(Epuck e, double L, double R, int milisec, double addto) {
        if (!endf) {
          int x = e.Working;
          if (x != 0)
            throw new ElibException("It would be extremely inaccurate if commadns are still waiting to be sent");
          double st0 = Stamp.Get();
          IAsyncResult ar = e.BeginMotors(L, R, addto, null, null);
          //debugging
          AsyncResultNoResult pom = (AsyncResultNoResult)ar;
          //debugging
          pom.Name += "goXms";
          e.EndFtion(ar);
          double st1 = Stamp.Get();
          //heuristic
          milisec -= (int)((st1 - st0) / 2);
          Thread.Sleep(milisec);
          ar = e.BeginStop(addto, null, null);
          e.EndFtion(ar);
          // debugging Console.WriteLine("{0} {1}", st0, st1);
        } else
          endconfirmed.Set();
      }
      #endregion GoAndTurn

      #region Go2Light
      /// <summary>Let the robot go to source of infra red light. E.g. a candle or lighter.
      ///After five unconfirmed commands, which are signaled by <exception cref="TimeoutElibException">TimeoutElibException</exception>,
      ///this behavoiour ends.
      /// </summary>
      /// <param name="ada">An <see cref="Elib.Epuck"/> instance.</param>
      public static void Go2Light(Epuck ada) {
        startBehaviour();
        restarts = restarts_startingValue;
        try {
          ada.BeginStop(to, recGotoLight, ada);
        } catch (TimeoutElibException) {
          exceptionOccured_Restart(ada);
        }
        endBehaviour();
      }

      static int restarts_startingValue = 5;
      static volatile int restarts;
      static void exceptionOccured_Restart(Epuck ada) {
        //Reconnect again to e-Puck.
        ada.Dispose();
        ada = new Epuck(ada.Port, ada.Name);
        restarts--;
        if (restarts >= 0) {
          Console.WriteLine("Remaining " + restarts.ToString() + " restart. Press enter to continue");
          ada.BeginStop(to, recGotoLight, ada);
        } else {
          Console.WriteLine("End of Go2Light, because all " + restarts_startingValue.ToString() + " has been used.");
          Console.WriteLine("Behaviour has finished. Press enter to perform next actions");
          endf = true;
        }
      }
      static void recGotoLight(IAsyncResult ar_) {
        if (!endf) {
          double speed = 0.3;
          int limits = 30;
          Epuck ada = (Epuck)ar_.AsyncState;
          IAsyncResult ar;
          try {
            ar = ada.BeginGetLight(to, null, null);
            int[] light = ada.EndGetFtion(ar);
            Console.WriteLine("Light Values in Go2Light: " + ArrayToString<int>(light));
            int diff_lr = (light[0] + light[1] + light[2]) - (light[6] + light[7] + light[5]);
            diff_lr += 100;//correction
            int diff_fb = light[0] + light[7] - (light[3] + light[4]);
            diff_fb += 5;//correction
            Console.WriteLine(diff_fb);
            if (diff_fb > 0) {
              if (light[2] < light[5]) {
                Console.WriteLine("turn around right {0}", diff_lr);
                ar = ada.BeginMotors(speed, 0, to, recGotoLight, ada);
              } else {
                Console.WriteLine("turn around left");
                ar = ada.BeginMotors(0, speed, to, recGotoLight, ada);
              }
            } else if (diff_lr < -limits || limits < diff_lr) {
              if (diff_lr > 0) {
                Console.WriteLine("turn left {0}", diff_lr);
                ar = ada.BeginMotors(0, speed, to, recGotoLight, ada);
              } else {
                Console.WriteLine("turn right {0}", diff_lr);
                ar = ada.BeginMotors(speed, 0, to, recGotoLight, ada);
              }
            } else {
              Console.WriteLine("Go straight {0}", diff_lr);
              ar = ada.BeginMotors(speed, speed, to, recGotoLight, ada);
            }
            ada.EndFtion(ar);
          } catch (TimeoutElibException) {
            exceptionOccured_Restart(ada);
          }
        }
      }
      #endregion Go2Light

      #region KofGoXcm
      class RobotAndTime{
        /// <summary>
        /// Time of ride in one direction. In seconds.
        /// </summary>
        public double StartTime;
        public Epuck E;
        double cm;
        public double Cm {
          set { Thread.VolatileWrite(ref cm, value); }
          get { return Thread.VolatileRead(ref cm); } 
        }
        double speed;
        /// <summary>
        /// Allows Thread safe change of speed.
        /// </summary>
        public double Speed {
          set { Thread.VolatileWrite(ref speed, value); }
          get { return Thread.VolatileRead(ref speed); }
        }
        public volatile int stopKof=0;
        public volatile int goKof = 0;
        public ManualResetEvent End;
      }
      /// <summary>
      /// Go x Cm and if it pass the destination, it returns back.
      /// Start it from stopped state. If the stopKof callback is called more 5 times, it tries to reconnect to e-Puck.
      /// </summary>
      /// <param name="e"></param>
      /// <param name="cm"></param>
      /// <param name="speed"></param>
      public static void KofGoXcm(Epuck e,double cm,double speed) {
        RobotAndTime x = new RobotAndTime();
        x.Speed = speed;
        x.E = e; 
        x.Cm=cm;
        x.StartTime = Stamp.Get();
        x.End = new ManualResetEvent(false);
        Console.WriteLine("KofGoXcm beginns");
        e.Motors(x.Speed, x.Speed, goOkf, goKof, x, to);
        //It is an asynchronous invocation next function will be immediatly, we have to wait in order to avoid mixture of behaviours.
        x.End.WaitOne();
      }

      static void goOkf(object robotAndTime) {
        Console.WriteLine("goOkf was called");
        RobotAndTime x = (RobotAndTime)robotAndTime;
        double time = Stamp.Get() - x.StartTime;
        while (travelled(time,x.Speed) < (x.Cm-0.05)) {
          //We do not need to send any command.     
          Thread.Sleep(2);
          time = Stamp.Get() - x.StartTime;
        }
        x.E.Stop(stopOkf, stopKof, x, to);
      }
      static void goKof(object robotAndIime) { 
        //we are stopped
        Console.WriteLine("goKof was called");
        RobotAndTime x = (RobotAndTime)robotAndIime;
        x.goKof++;
        if (x.goKof > 5) {
          if (!reconnect(x))
            return;
          else {
            x.goKof = 0;
            x.StartTime = Stamp.Get();
          }
        }
        x.E.Motors(x.Speed, x.Speed, goOkf, goKof, x, to);        
      }
      static bool reconnect(RobotAndTime x) {
        Console.WriteLine("5 KofStop callbacks was called. Press \"r\" to reconnect or any other key to end.");
        if ("r" == Console.ReadLine()) {
          x.E.Dispose();
          x.E = new Epuck(x.E.Port, x.E.Name);
          return true;
        } else {
          x.End.Set();
          return false;
        }
      }
      static void stopOkf(object robotAndTime) {
        Console.WriteLine("Behaviour has ended.");
        RobotAndTime x = (RobotAndTime)robotAndTime;
        x.End.Set();
      }
      static void stopKof(object robotAndTime) {
        Console.WriteLine("stopKof was called.");
        RobotAndTime x = (RobotAndTime)robotAndTime;
        x.stopKof++;
        if (x.stopKof > 5) {
          if (!reconnect(x))
            return;
          else
            x.stopKof = 0;
        }
        double time = Stamp.Get() - x.StartTime;
        if (travelled(time,x.Speed) < (x.Cm+0.05))
          x.E.Stop(stopOkf, stopKof,x,to);
        else {
          Console.WriteLine("We passed the destination spot, we returns back, Try repair the connection.");
          // Thread safe read and write to x.Speed
          x.Cm = travelled(x.StartTime,x.Speed) - x.Cm;
          x.Speed = -x.Speed;
          x.StartTime = Stamp.Get();
          x.E.Motors(x.Speed, x.Speed, goOkf, goKof, x, to);
        }
      }
      static double travelled(double time,double speed) {
        return time * (speed * Epuck.WheelDiameter * Math.PI);
      }
      #endregion KofGoXcm


      #region Simulation Kof callback using IAsyncResult interface
      /// <summary>
      /// An simpe example, that Kof callbacks can be simulated with IAsyncResult interface.
      /// Good for theory, useless for common usage.
      /// </summary>
      /// <param name="ada"></param>
      public static void SimulatingKof_over_IAsResult(Epuck ada) {
        startBehaviour();
        //the timeout is too small intentionally!
        ada.BeginGetImage(0.001, okf, ada);
        endBehaviour();
      }

      static void okf(IAsyncResult ar_) {
        if (!endf) {
          //it can be called only as a callback of BeginGetImage in order to match EndGetImage ftion 3 rows below
          Epuck ada = (Epuck)ar_.AsyncState;
          Console.WriteLine("Okf has been called");
          try {
            //no EventWaitHandle created
            Bitmap b = ada.EndGetImage(ar_);
            //simulate some work
            IAsyncResult ar = ada.BeginMotors(-1, 1, 0.1, null, null);
            //simulate image processig
            Thread.Sleep(20);
            ada.EndFtion(ar);

            //the timeout is too small!
            ada.BeginGetImage(0.01, okf, ada);
          } catch (TimeoutElibException) {
            //has to be fixed in kof
            ada.BeginStop(0.1, kof, ada);
          }
        } else
          endconfirmed.Set();
      }
      static void kof(IAsyncResult ar_) {
        if (!endf) {
          Epuck ada = (Epuck)ar_.AsyncState;
          //it can be called from any function because we call only EndFtion can be applied to every IAsyncResult in Elib
          ada.EndFtion(ar_);
          Console.WriteLine("Kof has been executed");
          try {
            //do the repair actions
            IAsyncResult ar = ada.BeginStop(to, null, null);
            ada.EndFtion(ar);
            Console.WriteLine("The problem is fixed. Lets call okf!");
            ada.BeginGetImage(0.1, okf, ada);
          } catch (TimeoutElibException) {
            ada.BeginStop(0.1, kof, ada);
          }
        } else
          endconfirmed.Set();
      }

      #endregion Simulation Kof callback using IAsyncResult interface
    }
}
