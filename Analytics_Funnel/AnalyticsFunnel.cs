using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Diagnostics;

class AnalyticsFunnel : MoreMountains.Tools.PersistentSingleton<AnalyticsFunnel>
{
    private static List<EventPackage> Packages;

    private static DateTime GameLaunchTime; //Time the game was launched    

    [AnalyticsName("App_Focus_Lost", "Time_of_Focus_Lost")]
    private DateTime AppSoftClosed;
    [AnalyticsName("App_Focused", "Time_Spent_Not_in_Focus")]
    private int TimeSpentSoftClosed;

    private long SessionCount;

    [SerializeField]
    private int AutoSendIntervalMinutes;

    private float TimeSinceLastPublish;

    public string AppVersion = "Closed_Beta";
    static string ApplicationVersion;

    private void Awake()
    {        
        //ApplicationVerison = Int32.Parse(Application.version.Replace(".", ""));
        if (AppVersion == "" || AppVersion == null)
            ApplicationVersion = "Internal";
        else
            ApplicationVersion = AppVersion;
        Packages = new List<EventPackage>();
        GameLaunchTime = DateTime.UtcNow;
        SessionCount = UnityEngine.Analytics.AnalyticsSessionInfo.sessionCount;
        if (UnityEngine.Analytics.AnalyticsSessionInfo.sessionFirstRun)
        {
            PublishInternalEvent("First_Play", new Dictionary<string, object>()
            {
                { "Launch_Time", GameLaunchTime },
                { "Platform",  Environment.OSVersion.Platform }
            });
        }
        else
        {
            PublishInternalEvent("Application_Launched", new Dictionary<string, object>()
            {
                { "Launch_Time", GameLaunchTime },
                { "Session_Count", SessionCount },
                { "Device",  SystemInfo.deviceModel },
                { "Operating System", SystemInfo.operatingSystem },
                { "Shader Level", SystemInfo.graphicsShaderLevel },
                { "Screen_Dimensions", "(" + Screen.width + "," + Screen.height + ")"}
            });
        }
    }

    private void Update()
    {
        TimeSinceLastPublish += Time.deltaTime;
        if (TimeSinceLastPublish > (AutoSendIntervalMinutes * 60))
        {
            PublishAllEvents();
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            AppSoftClosed = DateTime.UtcNow;
            PublishAllEvents();
        }
        else
        {
            TimeSpentSoftClosed = (DateTime.UtcNow - GameLaunchTime).Seconds;
            EventPackage sendPackage = new EventPackage("App_Focused", this, false, ApplicationVersion + " :app_focused");
        }
    }

    private void OnApplicationQuit()
    {
        PublishInternalEvent("App_Closed",
            new Dictionary<string, object>() {
                { "Session_Length", (System.DateTime.UtcNow - GameLaunchTime).Seconds },
                { "Active_Scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name } });
        PublishAllEvents();
    }

    /// <summary>
    /// All events currently in the system that have not been published to the Unity Analytics service are
    /// published and the list is blown out
    /// </summary>
    public void PublishAllEvents()
    {
        foreach (EventPackage package in Packages)
        {
            if(!Application.isEditor)
                UnityEngine.Analytics.AnalyticsEvent.Custom(package.EventName, package.Data);
        }
        Packages = new List<EventPackage>();
        TimeSinceLastPublish = 0;
    }

    /// <summary>
    /// As non-urgent events are sent to the system
    /// </summary>
    /// <param name="package"></param>
    public static void SaveEventPackage(EventPackage package)
    {
        Packages.Add(package);
    }

    /// <summary>
    /// When you need to publish an event instantly you can use this function and manually create the dictionary
    /// with all the data. 
    /// </summary>
    /// <param name="eventName">Name of the event we are creating</param>
    /// <param name="data">All the data that is to be published with the event</param>
    public static void PublishEvent(string eventName, Dictionary<string, object> data)
    {
        if(!Application.isEditor)
            UnityEngine.Analytics.AnalyticsEvent.Custom(ApplicationVersion + ":" + eventName, data);
        var result = UnityEngine.Analytics.AnalyticsEvent.Custom(ApplicationVersion + ":" + eventName, data);
        Log.Write(eventName + " " + result.ToString());
    }

    /// <summary>
    /// Used when an event is raised and the event needs to be published to the analytics server instantly
    /// </summary>
    /// <param name="package">Event package we are sending</param>
    public static void PublishEvent(EventPackage package)
    {
        var result = UnityEngine.Analytics.AnalyticsEvent.Custom(ApplicationVersion + ":" + package.EventName, package.Data);
        if(result != UnityEngine.Analytics.AnalyticsResult.Ok)
        {
            UnityEngine.Debug.LogError(package.EventName + " Failed: " + result.ToString());
            Log.Write(package.EventName + " Failed: " + result.ToString());
        }
#if(UNITY_EDITOR || UNITY_EDITOR_OSX)
        if(Application.isEditor)
        {
            //var sendName = ApplicationVersion + ":" + package.EventName + "(" + result.ToString() + ")";
            //package.EventName = sendName;
            EventValidator.RecieveEvent(package, result);
            //ConvertToJSON(package);
        }
#endif
    }

    /// <summary>
    /// Only used when this class needs to send an event manually about data that is not stored in this object
    /// </summary>
    /// <param name="eventName">name of the event</param>
    /// <param name="data">data being sent with the event</param>
    private void PublishInternalEvent(string eventName, Dictionary<string, object> data)
    {
        if(!Application.isEditor)
            UnityEngine.Analytics.AnalyticsEvent.Custom(ApplicationVersion + ":" + eventName, data);
    }
}

public class EventPackage
{
    public string EventName;
    public Dictionary<string, object> Data;

    public EventPackage(string eventName, System.Object sender, bool sendImeditate = false,
        string UniqueEventName = "", bool IncludeRoom = false)
    {
        Type senderType = sender.GetType();
        Data = new Dictionary<string, object>();
        Data.Add("In Game Time", UnityEngine.Analytics.AnalyticsSessionInfo.sessionElapsedTime / 1000);
        Data.Add("World Time", DateTime.UtcNow);
        EventName = eventName;
        System.Reflection.MemberInfo[] fields = senderType.GetFields(BindingFlags.NonPublic | BindingFlags.Public
            | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            System.Object[] fieldAttributes = fields[i].GetCustomAttributes(true);
            for (int j = 0; j < fieldAttributes.Length; j++)
            {
                if (fieldAttributes[j].GetType() == typeof(AnalyticsNameAttribute))
                {
                    AnalyticsNameAttribute atb = fieldAttributes[j] as AnalyticsNameAttribute;
                    if (atb.BindingEvent == eventName)
                    {
                        FieldInfo f = senderType.GetField(fields[i].Name, BindingFlags.NonPublic | BindingFlags.Public
            | BindingFlags.Instance);
                        Data.Add(atb.Name, f.GetValue(sender));
                    }
                }
            }
        }
        if (UniqueEventName != "")
            EventName = UniqueEventName;
        else
        {
            
            if(IncludeRoom)
            {
                EventName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "." + LevelManager.Instance.CurrentLauncher + ":" + EventName;
            }
            else
                EventName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ":" + EventName;
        }
        if (!sendImeditate)
            AnalyticsFunnel.SaveEventPackage(this);
        else
            AnalyticsFunnel.PublishEvent(this);
    }

    //Type GetSenderType(System.Object sender)
    //{
    //    var stack = new StackFrame(2, true);
    //    string[] senderFileSplit = stack.GetFileName().Split('\\');
    //    string typeName = senderFileSplit[senderFileSplit.Length - 1].Split('.')[0];
    //    return sender.GetComponent(typeName).GetType();
    //}
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class AnalyticsNameAttribute : Attribute
{
    public AnalyticsNameAttribute(string bindingEvent, string name) { Name = name; BindingEvent = bindingEvent; }
    public string Name;
    public string BindingEvent;
}