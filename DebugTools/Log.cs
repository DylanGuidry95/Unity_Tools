using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

// This singleton handles all logging in order to make it simple
// to reroute text output to different sinks. This also sets the groundwork
// to split logs into various channels that can be enabled or disabled.
// for public builds, we should make sure that we are not sending any text
// to Log.Write()
public class Log
{
    //When the user invokes the Write function they will send in a value for threat level of the message
    //1 = Normal debug message will not stop execution of game in the editor
    //2 = Warning message does not stop execution of the game unless set in the editor
    //3 = Error will stop execution of the game in editor mode.
    public enum LogType
    {
        message = 1, warning = 2, error = 3
    }

    public bool ShowMessages;
    public bool ShowWarnings;
    public bool ShowErrors;

    [SerializeField]
    private static Dictionary<string, List<LogData>> Logs = new Dictionary<string, List<LogData>>();

    public static void PuplishLog()
    {        
        foreach (var log in Logs)
        {
            foreach (var msg in log.Value)
            {
                var data = JsonUtility.ToJson(msg);
                
                File.AppendAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DebugLog.json",
                data);
            }
        }
    }

    /// <summary>
    /// Precondition check to ensure there is an instance of the Log class in the scene before trying to send 
    /// messages to the output source
    /// </summary>
    /// <param name="s">Message to be logged</param>
    /// <param name="threatLevel">Level of threat to the application default is 1 which means the message will
    /// not impact the execution process of the game</param>
    public static void Write(string s, int threatLevel = 1)
    {
        InternalWrite(s, (LogType)threatLevel);
    }

    private static LogData CheckForDuplicateMessage(LogData msg)
    {
        if (Logs[msg.SenderType].Count == 0)
            return null;
        foreach (var log in Logs[msg.SenderType])
        {
            if (log == msg)
                return log;
        }
        return null;
    }

    public static void GenerateLog(LogData log)
    {
        if (!Logs.ContainsKey(log.SenderType))
            Logs.Add(log.SenderType, new List<LogData>() { log });
        else
        {
            LogData msg = CheckForDuplicateMessage(log);
            if (ReferenceEquals(msg, null))
            {
                Logs[log.SenderType].Add(log);
            }
            else
            {
                msg.Duplicates++;
            }
        }    
    }

    private static void InternalWrite(string s, LogType threatLevel)
    {
#if (UNITY_EDITOR || UNITY_EDITOR_OSX)
        var stack = new StackFrame(2, true);
        string[] fileName = stack.GetFileName().Split(Path.DirectorySeparatorChar);
        LogData entry = new LogData()
        {
            Message = s,
            ThreatLevel = (int)threatLevel,
            LineNumber = stack.GetFileLineNumber(),
            SenderType = fileName[fileName.Length - 1],
            FilePath = stack.GetFileName()
        };
        GenerateLog(entry);

        DebugWindow.AddMessage(entry);
#endif
    }
}

[System.Serializable]
public class LogData
{
    public string Message;
    public int ThreatLevel;
    public int Duplicates;
    public int LineNumber;
    public string FilePath;
    public string SenderType;

    public override string ToString()
    {
        string data = SenderType + " || " + Message + " : " + "Line: " + LineNumber + " (" + Duplicates + ")";
        return data;
    }

    public static bool operator ==(LogData rhs, LogData lhs)
    {
        if (object.ReferenceEquals(rhs, null) || object.ReferenceEquals(lhs, null))
            return false;
        return rhs.Message == lhs.Message &&
            rhs.ThreatLevel == lhs.ThreatLevel &&
            rhs.LineNumber == lhs.LineNumber &&
            rhs.FilePath == lhs.FilePath &&
            rhs.SenderType == lhs.SenderType;
    }

    public static bool operator !=(LogData rhs, LogData lhs)
    {
        if (object.ReferenceEquals(rhs, null) || object.ReferenceEquals(lhs, null))
            return false;
        return rhs.Message != lhs.Message &&
            rhs.ThreatLevel != lhs.ThreatLevel &&
            rhs.LineNumber != lhs.LineNumber &&
            rhs.FilePath != lhs.FilePath &&
            rhs.SenderType != lhs.SenderType;
    }
}