using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

#if(UNITY_EDITOR || UNITY_EDITOR_OSX)
using UnityEditor;

/*Future Additions
 * ====================
 * Re-Scale UI
 * Save Messages in output file for built executables
 * - Seperate data from logic
 * Change font and display colors
*/

public class DebugWindow : EditorWindow
{
    #region Static Fields
    //0: Windows 1: OSX 2: Linux
    public static bool[] OS_Toggles = new bool[] { true, false, false }; 
    //Track of all objects that have sent messages and a list of messages it has sent  
    private static Dictionary<string, List<LogData>> LogMessages = new Dictionary<string, List<LogData>>();
    //Keeps track of the objects we want to see messages from
    private static List<bool> SenderMessageToggles = new List<bool>();
    #endregion
    
    #region Fields
    //Keeps track of what message type we want to see. 0:Message 1:Warning 2:Error
    private bool[] ThreatLevels = new bool[3];
    //Default UI color
    private Color DefaultColor;
    //Position of the scroll view is at for the Objects that have seen messages
    private Vector2 ObjectViewScrollPosition;
    //Position of the scroll view is at for the Messages that have been sent
    private Vector2 MessageViewScrollPosition;
    //Rect that is used to draw all the Option menu items
    private Rect OptionsRect;
    //Rect that is used to draw all of the Objects that have sent messages
    private Rect ObjectRect = new Rect(55, 40, 10, 500);
    //Rect of the last message drawn to the screen
    private Rect LogRect;
    //Name of the object we are searching for
    private string ObjectSearch;
    //Message detail we are searching for
    private string MessageSearch;
    #endregion

    #region Static Methods
    /// <summary>
    /// Creates the window and checks what operating system the user is currently running
    /// </summary>
    [UnityEditor.MenuItem("Tools/Debugger Window")]
    private static void Init()
    {      
        var window = ScriptableObject.CreateInstance<DebugWindow>();
        window.Show();
        OS_Toggles[0] = (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT);
        OS_Toggles[1] = (System.Environment.OSVersion.Platform == System.PlatformID.Unix);
        OS_Toggles[2] = !(OS_Toggles[0] || OS_Toggles[1]);
    }

    /// <summary>
    /// Adds a new message to the log. If a message is received by a new object that is not in the system the
    /// object will be added for it, otherwise the message is added to the objects list of messages sent.
    /// If duplicate messages are sent by the same object the messages duplicate count will increase.
    /// 
    /// References: [file] : [function] : [line number]
    /// Log.cs : InternalWrite : 52
    /// Log.cs : InternalWrite : 65
    /// </summary>
    /// <param name="entry"></param>
    public static void AddMessage(LogData entry)
    {
        if (!LogMessages.ContainsKey(entry.SenderType))
        {
            LogMessages.Add(entry.SenderType, new List<LogData>() { entry });
            SenderMessageToggles.Add(false);
            return;
        }
        LogData foundMessage = CheckForDuplicateMessage(entry);
        if (object.ReferenceEquals(foundMessage, null))
            LogMessages[entry.SenderType].Add(entry);
        else
            foundMessage.Duplicates++;
    }

    /// <summary>
    /// Checks to see if the same message has sent by an object all ready. If it has been seen all ready the
    /// message all ready in the log is returned other wise null is returned.
    /// </summary>
    /// <param name="msg">Message we are trying to find</param>
    /// <returns>If the message is found return the message all ready in the log otherwise returns null</returns>
    private static LogData CheckForDuplicateMessage(LogData msg)
    {
        if (LogMessages[msg.SenderType].Count == 0)
            return null;
        foreach (var log in LogMessages[msg.SenderType])
        {
            if (log == msg)
                return log;
        }
        return null;
    }
    #endregion

    #region Draw Methods
    /// <summary>
    /// Draws the message to the UI. Depending on the messages threat level the color of the box drawn to the
    /// screen will change.
    /// Using the search bar in the message view the user can specify the messages they are looking for
    /// Message = black
    /// Warning = yellow
    /// Error = red
    /// </summary>
    /// <param name="msg">Message we want to draw to the screen</param>
    /// <param name="rect">Rect that will be used to draw to the screen</param>
    private void DrawMessage(LogData msg, Rect rect)
    {
        if (ThreatLevels[0] && msg.ThreatLevel == 1)
        {
            //GUI.color = Color.black;
            GUI.Box(rect, msg.ToString());            
        }
        if (ThreatLevels[1] && msg.ThreatLevel == 2)
        {
            GUI.color = Color.yellow;
            GUI.Box(rect, msg.ToString());            
        }
        if (ThreatLevels[2] && msg.ThreatLevel == 3)
        {
            GUI.color = Color.red;
            GUI.Box(rect, msg.ToString());
        }
        GUI.color = DefaultColor;
    }
    /// <summary>
    /// Displays all of the objects that exist in the system to the Object display menu.
    /// The user can check which objects they would like to receive messages from.
    /// Using the search bar the user can filter objects by name
    /// </summary>
    private void DrawObjectSelection()
    {
        Rect searchBox = new Rect(0, 40, ObjectRect.position.x - 5, 25);
        ObjectSearch = GUI.TextField(searchBox, ObjectSearch);
        if (ObjectSearch == null)
            ObjectSearch = "";
        ObjectRect.position = new Vector2(200, 40);
        ObjectRect.size = new Vector2(ObjectRect.size.x, this.position.size.y - 50);
        GUI.Box(ObjectRect, "");
        ObjectViewScrollPosition = GUI.BeginScrollView(new Rect(0, 80, ObjectRect.position.x - 5, this.position.size.y - 100), ObjectViewScrollPosition, new Rect(0, 0, 300, LogMessages.Count * 20));
        Rect previousItem = new Rect(new Vector2(10, 0), new Vector2(500, 15));
        int index = 0;
        string[] foundObjects = SearchObjects(ObjectSearch);
        if (foundObjects.Length == 0)
            foreach (var log in LogMessages)
            {
                string messageCount = (CountMessages(log.Key) < 100) ? CountMessages(log.Key).ToString() : "99+";
                SenderMessageToggles[index] = GUI.Toggle(previousItem, SenderMessageToggles[index], log.Key + "(" + messageCount + ")");
                previousItem.position += new Vector2(0, 20);
                index++;
            }
        else
            for (int i = 0; i < foundObjects.Length; i++)
            {
                string messageCount = (CountMessages(foundObjects[i]) < 100) ? CountMessages(foundObjects[i]).ToString() : "99+";
                SenderMessageToggles[index] = GUI.Toggle(previousItem, SenderMessageToggles[index], foundObjects[i] + "(" + messageCount + ")");
                previousItem.position += new Vector2(0, 20);
                index++;
            }
        GUI.EndScrollView();
    }
    /// <summary>
    /// Draws all of the options the user can change or interact with
    /// The user can clear the entire log(messages and objects) or add all files in the project to the object
    /// menu.
    /// The user can also toggle which message types they would like to see
    /// </summary>
    private void DrawOptions()
    {
        Rect clearMessageRect = new Rect(OptionsRect.position.x + 5, OptionsRect.position.y + 5, 75, 25);
        if (GUI.Button(clearMessageRect, "Clear All"))
        {
            ClearLog();
        }

        Rect updateObjects = new Rect(clearMessageRect.position + new Vector2(85, 0), new Vector2(55, 25));
        if (GUI.Button(updateObjects, "Update"))
        {
            PollObjects();
        }
        Rect ToggleMessageLevel1 = new Rect(updateObjects.position.x + 60,
            updateObjects.position.y, 100, 25);
        ThreatLevels[0] = GUI.Toggle(ToggleMessageLevel1, ThreatLevels[0], "Message Lvl1");
        Rect ToggleMessageLevel2 = new Rect(ToggleMessageLevel1.position.x + 110,
            clearMessageRect.position.y, 100, 25);
        ThreatLevels[1] = GUI.Toggle(ToggleMessageLevel2, ThreatLevels[1], "Message Lvl2");
        Rect ToggleMessageLevel3 = new Rect(ToggleMessageLevel2.position.x + 110,
            clearMessageRect.position.y, 100, 25);
        ThreatLevels[2] = GUI.Toggle(ToggleMessageLevel3, ThreatLevels[2], "Message Lvl3");
    }
    /// <summary>
    /// Goes through every message received and if the message meets the criteria the user is searching for or has
    /// the preferences to view it, it will be drawn to the screen.
    /// </summary>
    private void DrawMessages()
    {
        Rect clearButton = new Rect(ObjectRect.position.x + 15, 40, this.position.size.x - ObjectRect.position.x - 15, 25);
        if (GUI.Button(clearButton, "Clear Messages"))
        {
            ClearLog(false);
        }
        Rect searchBar = new Rect(ObjectRect.position.x + 15, 70, this.position.size.x - ObjectRect.position.x - 15, 25);
        MessageSearch = GUI.TextField(searchBar, MessageSearch);
        LogData[] messages;
        if (MessageSearch == null)
            MessageSearch = "";
        if (MessageSearch != "" || MessageSearch != null)
        {
            messages = SearchMessages(MessageSearch);
        }
        else
        {
            messages = new List<LogData>().ToArray();
        }
        Rect previousLog = new Rect(new Vector2(0, 0), new Vector2(this.position.size.x - 225, 20));
        MessageViewScrollPosition = GUI.BeginScrollView(new Rect(ObjectRect.position.x + 20, 100,
            this.position.size.x - 225, this.position.size.y - 120),
            MessageViewScrollPosition,
            new Rect(0, 0, 0, LogRect.position.y));
        int index = 0;
        foreach (var log in LogMessages)
        {
            if (SenderMessageToggles[index])
            {
                if (log.Value == null)
                    continue;
                if (MessageSearch != "")
                {
                    for (int i = 0; i < messages.Length; i++)
                    {
                        DrawMessage(messages[i], previousLog);
                        if (previousLog.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.type == EventType.MouseDown)
                                OpenEditor(messages[i]);
                        }
                        previousLog.position += new Vector2(0, 25);
                    }
                }
                else
                {
                    foreach (var message in log.Value)
                    {
                        DrawMessage(message, previousLog);
                        if (previousLog.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.type == EventType.MouseDown)
                                OpenEditor(message);
                        }
                        previousLog.position += new Vector2(0, 20);
                    }
                }
            }
            index++;
        }
        GUI.EndScrollView();
        LogRect = previousLog;
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Removes all spaces from file paths and replaces them with ('\ ').
    /// This is needed so that VS Code will properly open files from the commands being sent to it.
    /// </summary>
    /// <param name="path">path we are removing spaces from</param>
    /// <returns>Returns the path with spaces removed/modified</returns>
    private string FixPathSpaces(string path)
    {
        string[] pathSplit = path.Split(' ');
        string fixedPath = "";
        for (int i = 0; i < pathSplit.Length; i++)
        {
            fixedPath += (i != pathSplit.Length - 1) ? pathSplit[i] + "\" " : pathSplit[i];
        }
        return fixedPath;
    }
    /// <summary>
    /// Removes all messages or all objects and messages from the system depending on the value of clearAll when
    /// invoked.
    /// If the argument is true all data in the system is deleted otherwise just the messages are deleted.
    /// </summary>
    /// <param name="clearAll">Tells the function weather or not to clear all the data or just messages</param>
    private void ClearLog(bool clearAll = true)
    {        
        if (clearAll)
        {
            LogMessages.Clear();
            SenderMessageToggles.Clear();
        }
        else
        {
            foreach (var log in LogMessages)
            {
                log.Value.Clear();
            }
        }
    }
    /// <summary>
    /// Mimics the behavior of a Toggle group that you would see in other applications.
    /// Ensure the user can only have one option selected in a group of objects
    /// </summary>
    /// <param name="collection">Collection of items in the group</param>
    /// <param name="index">Index we are changing the value of</param>
    private void ToggleGroup(bool[] collection, int index)
    {
        collection[index] = !collection[index];
        for (int i = 0; i < collection.Length; i++)
        {
            if (i == index)
                continue;
            collection[i] = !collection[index];
        }
    }
    /// <summary>
    /// Get the total messages sent by an object in the system
    /// </summary>
    /// <param name="file">File we want to know the number of messages sent from</param>
    /// <returns></returns>
    private int CountMessages(string file)
    {
        int messages = 0;
        for (int i = 0; i < LogMessages[file].Count; i++)
        {
            messages += LogMessages[file][i].Duplicates + 1;
        }
        return messages;
    }
    /// <summary>
    /// Tells the OS to open the users desired editor as specified by the Unity editor preferences.
    /// ONLY SUPPORTS VISUAL STUDIO AND VS CODE
    /// </summary>
    /// <param name="log">Message the user clicked on and is trying to open the source of</param>
    void OpenEditor(LogData log)
    {        
        var path = log.FilePath;
        if (OS_Toggles[0])
        {
            
            string editor = EditorPrefs.GetString("kScriptsDefaultApp_h2657262712");
            if (editor.Contains("devenv"))
            {
                var proc = Process.GetProcesses();
                for (int i = 0; i < proc.Length; i++)
                {
                    if (proc[i].ProcessName.Contains("devenv"))
                    {
                        Process.Start("devenv", "/Edit \"" + path);
                        return;
                    }
                }
                var rootDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                var sln = rootDir.GetFiles("*.sln");
                Process.Start("devenv", "/UseEnv \"" + sln[0].FullName);
            }
            else
            {
                string fixedPath = FixPathSpaces(path);
                string slnPath = FixPathSpaces(Directory.GetCurrentDirectory());
                Process.Start("code", "-g " + fixedPath + ":" + log.LineNumber);
                if (Process.GetProcessesByName("code") == null)
                    Process.Start("code", "--add " + slnPath);
            }
        }
        else if (OS_Toggles[1])
        {
#if UNITY_EDITOR_OSX
            string fixedPathOSX = FixPathSpaces(path);
            ProcessStartInfo s = new ProcessStartInfo()
            {
                FileName = "/Applications/Visual Studio Code.app/Contents/MacOS/Electron",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = "-g " + fixedPathOSX + ":" + log.LineNumber
            };

            string slnPathOSX = FixPathSpaces(Directory.GetCurrentDirectory());
            s.Arguments += " --add " + slnPathOSX;
            Process.Start(s);
            return;
#endif

            string fixedPathLinux = FixPathSpaces(path);
            string slnPathLinux = FixPathSpaces(Directory.GetCurrentDirectory());
            Process.Start("code", "-g " + fixedPathLinux + ":" + log.LineNumber);
            if (Process.GetProcessesByName("code") == null)
                Process.Start("code", "--add " + slnPathLinux);
        }     
    }
    #endregion

    #region Search Methods
    /// <summary>
    /// Adds all the files in the project to the system
    /// </summary>
    private void PollObjects()
    {
        string[] objects = GetProjectObjects().ToArray();
        LogMessages.Clear();
        SenderMessageToggles.Clear();
        for (int i = 0; i < objects.Length; i++)
        {
            if (!LogMessages.ContainsKey(objects[i]))
            {
                LogMessages.Add(objects[i], new List<LogData>());
                SenderMessageToggles.Add(false);
            }
        }
    }
    /// <summary>
    /// Returns a list of the names of the files in the project so that they can be added to the system
    /// </summary>
    /// <returns></returns>
    private List<string> GetProjectObjects()
    {
        List<string> objects = new List<string>();

        string[] files = Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "/Assets/Scripts/", "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Contains(".meta"))
            {
                string[] split = files[i].Split('/');
                if (split[split.Length - 1].Contains("\\"))
                {
                    string[] subSplit = split[split.Length - 1].Split('\\');
                    objects.Add(subSplit[subSplit.Length - 1]);
                }
                else
                    objects.Add(split[split.Length - 1]);
            }
        }
        return objects;
    }
    /// <summary>
    /// Returns a list of all the objects in the system that name's contain the value of the argument passed in
    /// </summary>
    /// <param name="name">Object we are looking for</param>
    /// <returns>List of all objects that match the argument passed in</returns>
    private string[] SearchObjects(string name)
    {
        List<string> objs = new List<string>();
        foreach (var log in LogMessages)
        {
            if (log.Key.Contains(name))
                objs.Add(log.Key);
        }
        return objs.ToArray();
    }
    /// <summary>
    /// Returns a list messages in the system that contain the value of the argument passed in
    /// </summary>
    /// <param name="message">Message we are looking for</param>
    /// <returns>Lit of messages that match the argument</returns>
    private LogData[] SearchMessages(string message)
    {
        List<LogData> messages = new List<LogData>();
        int index = 0;
        foreach (var log in LogMessages)
        {
            if (SenderMessageToggles[index])
            {
                for (int i = 0; i < log.Value.Count; i++)
                {
                    if (log.Value[i].Message.Contains(message))
                        messages.Add(log.Value[i]);
                }
            }
        }

        return messages.ToArray();
    }
    #endregion

    private void OnGUI()
    {
        OS_Toggles[0] = (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT);
        OS_Toggles[1] = (System.Environment.OSVersion.Platform == System.PlatformID.Unix);
        OS_Toggles[2] = !(OS_Toggles[0] || OS_Toggles[1]);
        DefaultColor = Color.white;
        //Test Code DO NOT DELETE
        Log.Write(Random.Range(0,100).ToString(), 3);
        DrawOptions();        
        GUI.color = Color.black;
        GUI.Box(new Rect(0, 35, this.position.size.x, 2), "");
        GUI.color = Color.white;
        DrawObjectSelection();
        DrawMessages();        
        Repaint();
    }
}
#endif