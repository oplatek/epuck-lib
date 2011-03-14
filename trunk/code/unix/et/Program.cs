using System;
using System.Collections.Generic;
using System.Text;
using  System.IO;
using System.Collections;

namespace et {
  class Program {
    static char[] sep = new char[1] { ' ' };
    //if user doesn't specified right commands it doesn't matter if he uses only one command, because parse sep would not produce any action
    const string parDestination = "-d";
    const string parSource = "-s";
    const string commandAvg = "Avg";
    const string commandCollectionFromTo = "FromTo";
    const string commandStringCollection = "FromCollection";
    const string parHelp = "-h";

    static void Main(string[] args) {
      string source = null, destination = null;
      Action action = parseArguments(args, out source, out destination);
      TextReader r = null;
      TextWriter w = null;
      try {
        if (source == null)
          r = Console.In;
        else
          r = new StreamReader(source);
        if (destination == null)
          w = Console.Out;
        else
          w = new StreamWriter(destination);

        if (action != null) {
          action.R = r;
          action.W = w;
		  action.Separators=sep;
          action.DoAction();
        }
      } catch (IOException) {
        throw new ArgumentException("Can't choose source file or destination file");
      } catch (ArgumentException e) { Console.WriteLine("Wrong number of arguments passed or wrong arguments passed!" + e.Message); }
      catch (IndexOutOfRangeException e) { Console.WriteLine("Wrong number of arguments passed or wrong arguments passed!" + e.Message);}      
      finally {
        if (r != null) r.Close();
        if (w != null) w.Close();
      }
    }//end Main

    struct Command {
      public string CommandName;
      public string[] Args;
      public Command(string command, string[] args) { CommandName = command; Args = args; }
    }

    static void InnerSwitch(string command, ref string source, ref string destination, List<string> arguments, out Action action) {
      action = null;
      if (command != null) {
        switch (command) {
          case parDestination:
            if (arguments.Count != 1 && destination != null)
              throw new ArgumentException("Can't choose destination file");
            destination = arguments[0];
            break;
          case parSource:
            if (arguments.Count != 1 && source != null)
              throw new ArgumentException("Can't choose source file");
            source = arguments[0];
            break;
          case commandAvg:
            try {
              action = new Average(int.Parse(arguments[0]));
            } catch (Exception) { throw new ArgumentException("avg can have only one argument which select a column of numbers."); }
            break;
          case commandCollectionFromTo:
            try {
              action = new SelectFromNumInterval(int.Parse(arguments[0]), int.Parse(arguments[1]), int.Parse(arguments[2]));
            } catch (Exception) { throw new ArgumentException("selectFrom commands needs arguments for column number and borders of intervals"); }
            break;
          case commandStringCollection:
            try {
              action = new SelectFromColection(int.Parse(arguments[0]), arguments.GetRange(1, arguments.Count - 1).ToArray());
            } catch (Exception) { throw new ArgumentException("selectFrom commands needs one argument for column number and rest for collections"); }
            break;
        }
      }

    }

    static Action parseArguments(string[] args, out string source, out string destination) {
      source = null;
      destination = null;
      Action action = null;
      List<string> arguments = new List<string>();
      string command = null;
      if (args.Length == 1 && args[0] == parHelp) {
//todo resource file				
        Console.WriteLine(@"
et (Elib Tools) USAGE:
et is a console application for parsing log from Elib library.
Elib log has 7 columns separated with a space.
'#' starts the commented line.
SEE LOG EXAMPLE:

536.22003579 Ada call SetLight(..) 0 1
536.26506609 Ada ko SetLight(..) 0 1
#Commented line
536.28355000 Ada call Stop() 0 1
536.29590912 Ada call Microphones(..) 1 2



et offers FOLLOWING COMMANDS:
... Count a average of a column: et Avg
... Select rows from a number interval: et FromTo
... Select rows from a collection of values: et FromCollection

EXAMPLES OF USAGE:
et -h
...Get this help
et [-s source file] [-s destination file] avg [x]
...Compute average from the x column of source file and writes it to destination file avg from source file.
et [-s source file] [-s destination file] FromTo c x y
...Select rows which in column c has a numerical value between x and y.
et [-s source file] [-s destination file] FromCollection c first second etc
...Select rows which in column c  has a string 'first', 'second' or 'etc'");
        action = null;   
      }

      for (int i = 0; i < args.Length; ++i) {
        switch (args[i]) {
          //a command with its arguments has ended because another starts
          case parDestination:
          case parSource:
          case commandAvg:
          case commandCollectionFromTo:
          case commandStringCollection:
            InnerSwitch(command, ref source, ref destination, arguments, out action);
            if (action != null)
              return action;
            command = args[i];
            arguments.Clear();
            break;
          //args[i] is argument of a command  
          default:
            arguments.Add(args[i]);
            break;
        }//end of switch
      }//end of for loop      
      //a command with its arguments has ended because all commandline arguments for this program has been processed
      InnerSwitch(command, ref source, ref destination, arguments, out action);
      return action;
    }
  }
 

  

  

}
