#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.WebUI;
using FTOptix.Alarm;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.Report;
using FTOptix.MQTTClient;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.SerialPort;
using FTOptix.Core;
#endregion

public class CheckStatus : BaseNetLogic
{
    private PeriodicTask periodicTask;

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        periodicTask = new PeriodicTask(CheckFunction, 5000, LogicObject); // 1000ms = 1 giây
        periodicTask.Start();
    }

    // 
    public void CheckFunction()
    {
        bool check = true;
        string timestampStr = LogicObject.GetVariable("timestamp").Value;
        DateTime parsedTimestamp;

        if (DateTime.TryParse(timestampStr, out parsedTimestamp))
        {
            // Chuyển timestamp về UTC để so sánh với UtcNow
            DateTime timestampUtc = parsedTimestamp.ToUniversalTime();
            DateTime now = DateTime.UtcNow;

            TimeSpan difference = now - timestampUtc;

            if (Math.Abs(difference.TotalSeconds) > 30)
            {
                check = false;
                Log.Info("CheckStatus", $"Lệch thời gian: {difference.TotalSeconds} giây");
            }
        }
        else
        {
            check = false;
            // Log.Info("CheckStatus", $"Không parse được timestamp: {timestampStr}");
        }

        LogicObject.GetVariable("Status").Value = check;
    }


    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        if (periodicTask != null)
            periodicTask.Dispose();
    }
}
