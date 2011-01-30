using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace et {
  abstract class Action {
    const string Comment = "#";
    protected int column;
    protected TextReader r;
    public TextReader R { get { return r; } set { r = value; } }
    protected TextWriter w;
    public TextWriter W { get { return w; } set { w = value; } }
    protected char[] sep;
    public char[] Separators { get { return sep; } set { sep = value; } }
    public Action(int Column) {
      column = Column;
    }
    public void DoAction() {
      string line = null;
      while ((line = r.ReadLine()) != null) {
        if (!line.StartsWith(Comment))
          LineAction(line);
      }
      LastAction();
    }
    protected virtual void LastAction() { /*usually does nothing and is called after all lines are processed*/}
    protected abstract void LineAction(string line);
  }

}
