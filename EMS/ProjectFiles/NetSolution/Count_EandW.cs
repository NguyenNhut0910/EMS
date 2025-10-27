#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.EventLogger;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.Alarm;
using FTOptix.DataLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.InfluxDBStoreLocal;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.InfluxDBStore;
using FTOptix.SerialPort;
using FTOptix.Core;
using FTOptix.ODBCStore;
using FTOptix.MQTTClient;
using FTOptix.MQTTBroker;
using FTOptix.Report;
#endregion

public class Count_EandW : BaseNetLogic
{
    private PeriodicTask periodicTask;
    public override void Start()
    {
        periodicTask = new PeriodicTask(ComputeSum, 1000, LogicObject); // 1000ms = 1 giây
        periodicTask.Start();
    }

    private void ComputeSum()
    {
        // int a = LogicObject.GetVariable("a").Value;
        // int b = LogicObject.GetVariable("b").Value;
        // Severity 0
        // int Alarm_Digital = LogicObject.GetVariable("Alarm_Digital_State").Value;
        int Connect_Digital = LogicObject.GetVariable("Connect_Digital_State").Value;
        // Severity 1
        int V_AnalogAlarm = LogicObject.GetVariable("V_AnalogAlarm_State").Value;
        int A_AnalogAlarm = LogicObject.GetVariable("A_AnalogAlarm_State").Value;
        int F_AnalogAlarm = LogicObject.GetVariable("F_AnalogAlarm_State").Value;
        int P_AnalogAlarm = LogicObject.GetVariable("P_AnalogAlarm_State").Value;
        // LogicObject.GetVariable("c0").Value = Alarm_Digital + Connect_Digital;
        LogicObject.GetVariable("c0").Value = Connect_Digital;
        LogicObject.GetVariable("c1").Value = V_AnalogAlarm + A_AnalogAlarm + F_AnalogAlarm + P_AnalogAlarm;
        
    }

    public override void Stop()
    {
        if (periodicTask != null)
            periodicTask.Dispose();
    }
}
