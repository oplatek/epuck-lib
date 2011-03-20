using System;
using System.Collections.Generic;
using System.Text;

namespace et {
  class SelectFromNumInterval : Action {
    double from;
    double to;
    public SelectFromNumInterval(int Column, double From, double To)
      : base(Column) {
      from = From;
      to = To;
    }
    protected override void LineAction(string line) {
      double x = Double.Parse(line.Split(sep)[column]);
      if (from <= x && x < to)
        w.WriteLine(line);
    }
  }

}
