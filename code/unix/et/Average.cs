using System;
using System.Collections.Generic;
using System.Text;

namespace et {
  class Average : Action {
    long count;
    double avg;
    public Average(int Column)
      : base(Column) {
      count = 0;
      avg = 0;
    }
    protected override void LineAction(string line) {
      count++;
      avg *= (count - 1) / (double)count;
      try {
        avg += (Double.Parse((line.Split(sep)[column])) / count);
      } catch (FormatException e) {
        Console.WriteLine("Avg has to be done from Integer. Error: " + e.Message);
        avg = double.NaN;
      }
    }
    protected override void LastAction() {
      w.WriteLine("{0:F8}", avg);
    }
  }
}
