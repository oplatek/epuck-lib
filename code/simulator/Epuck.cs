using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator {
  class Epuck {
    public enum Zoom { basic = 1, twice = 2, fourtimes = 4, eighttimes = 8 };
    int[] ir_sensors;//proximity sensors usually 100-400
    int[] light_sen;//values under 5000 ussually around 3000
    bool[] lights;
    bool front_light;
    bool bottom_light;
    int LM;
    int RM;
    int[] accelerators;
    int[] mircrophones;
    int speedL;
    int speedR;
    object image;//wtf?
    int i_width;
    int i_height;
    Zoom zoom;
    int sonar;//todo ?exists
  }
}
