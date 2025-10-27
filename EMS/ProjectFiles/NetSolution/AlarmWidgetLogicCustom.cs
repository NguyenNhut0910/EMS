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
using FTOptix.DataLogger;
using FTOptix.Store;
using FTOptix.InfluxDBStoreLocal;
using FTOptix.MQTTClient;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.InfluxDBStore;
using FTOptix.Core;
using FTOptix.SerialPort;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
using FTOptix.ODBCStore;
using FTOptix.MQTTBroker;
using FTOptix.Report;
#endregion

public class AlarmWidgetLogicCustom : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
