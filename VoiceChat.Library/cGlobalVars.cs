using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class cGlobalVars
{

    public static bool IsFinished = false;

    public static List<string> LogMessages = new List<string>();


    public static void AddLogChat(string message) {

        lock (LogMessages)
        {
              LogMessages.Add(message);
        }

    }


}
