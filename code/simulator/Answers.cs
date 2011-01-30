using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator {
  public partial class EpuckS {

    private string BodyLightAction(int arg) {
      return "b\r\n";
    }

    private string Play(int arg) {
      return "s\r\n";
    }

    private string GetMicrophones() {
      return "u,10,6,17\r\n";
    }

    private string GetVersion() {
      return "v, E-Puck simulator 0.1\r\n";
    }

    private string Reset() {
      return "r\r\x0c\x07WELCOME to the SerCom protocol on e-Puck\r\nthe EPFL education robot type H for help\r\n";
    }

    private string Move(int[] args) {
      return "d\r\n";
    }

    private string GetProximity() {
      return "n,313,113,391,178,127,198,108,666\r\n";
    }

    private string GetAmbientLight() {
      return "o,3520,3568,3772,3749,3862,3784,3766,3451\r\n";
    }

    private string GetMotorPos() {
      return "q,0,0\r\n";//77 from this command mean one cm, but if e-puck straight goes the values are q,a,-a. 
      //which means that value from left wheel is positive and from right wheel is negative
    }

    private string SetMotorsPos(int[] args) {
      return "p\r\n";
    }
    
    private string Stop() {
      return "s\r\n";
    }

    private string GetHelp() {
      return @"A         Accelerometer\r\nB,#       Body led 0=off 1=on 2=inverse\r\nC         Selector position\r\nD,#,#
              Set motor speed left,right\r\nE         Get motor speed left,right\r\nF,#       Front led 0=off 1=on 2=inverse\r\n
              G         IR receiver\r\nH\t     Help\r\nI         Get camera parameter\r\nJ,#,#,#,# Set camera parameter mode,w
              idth,heigth,zoom(1,4 or 8)\r\nK         Calibrate proximity sensors\r\nL,#,#     Led number,0=off 1=on 2=inverse\r\n
              N         Proximity\r\nO         Light sensors\r\nP,#,#     Set motor position left,right\r\nQ         Get motor
               position left,right\r\nR         Reset e-puck\r\nS         Stop e-puck and turn off leds\r\nT,#       Play sound
              1-5 else stop sound\r\nU         Get microphone amplitude\r\nV         Version of SerCom\r\n";
    }

    private string GetCamParams() {
      return "i,1,40,40,8,3200\r\n";
    }

    private string CalibrateIR() {
      return "k, Starting calibration - Remove any object in sensors range\r\nk, Calibration finished\r\n";
    }

    private string Lights(int[] args) {
      return "l\r\n";
    }

    private string GetIRInfo() {
      return "g IR check : 0x2, address : 0x0, data : 0x0\r\n";
    }

    private string GetSelectorPos() {
      return "c,2\r\n";//selector position for BTCom
    }

    private string GetAcccelerometr() {
      return "a,1892,2206,2800\r\n";//staying on table
    }

    private string GetSpeed() {
      return "e,-19565,-1297\r\n";//don't know what it means, but if the e-Puck is cruising its ok.
    }

    private string FrontLightAction(int arg) {
      return "f\r\n";
    }
  }
}
