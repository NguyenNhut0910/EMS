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
using FTOptix.DataLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.InfluxDBStoreLocal;
using FTOptix.MQTTClient;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.InfluxDBStore;
using FTOptix.SerialPort;
using FTOptix.Core;
using FTOptix.ODBCStore;
using FTOptix.MQTTBroker;
using FTOptix.Report;
#endregion

[CustomBehavior]
public class HistoryBehavior : BaseNetBehavior
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined behavior is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined behavior is stopped
    }

#region Auto-generated code, do not edit!
    protected new History Node => (History)base.Node;
#endregion
}
