using System;
using System.Collections.Generic;
using System.Text;

namespace et {
  class SelectFromColection : Action {
    string[] collections;
    public SelectFromColection(int Column, string[] Collections)
      : base(Column) {
      collections = Collections;
    }
    protected override void LineAction(string line) {
      if (collections == null)
        return;
      if (Contains<string>(collections, (line.Split(sep)[column])))
        w.WriteLine(line);
    }

    /// <summary>
    /// very simple linear search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    static bool Contains<T>(T[] arr, T x) {
      bool result = false;
      foreach (T i in arr) {
        if (x.Equals(i)) {
          result = true;
          break;
        }
      }
      return result;
    }
  }

}
