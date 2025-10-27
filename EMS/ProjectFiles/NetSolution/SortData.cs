#region Using directives
using System;
using System.IO;
using System.Text;

using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.EventLogger;
using FTOptix.OPCUAServer;
using FTOptix.UI;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.Core;
using System.Collections.Generic;
using FTOptix.DataLogger;
using FTOptix.InfluxDBStoreLocal;
using FTOptix.InfluxDBStore;
using FTOptix.ODBCStore;
using System.Text.RegularExpressions;
using FTOptix.MQTTClient;
using FTOptix.MQTTBroker;
using FTOptix.Report;
#endregion

public class SortData : BaseNetLogic
{
public bool IsValidDateTimeFormat(string input)
{
    if (input == null || input.Length != 19)
        return false;

    string[] split = input.Split(' ');
    if (split.Length != 2)
        return false;

    string[] d = split[0].Split('-');
    string[] t = split[1].Split(':');
    if (d.Length != 3 || t.Length != 3)
        return false;

    int y, m, day, h, min, s;
    if (!int.TryParse(d[0], out y)) return false;
    if (!int.TryParse(d[1], out m)) return false;
    if (!int.TryParse(d[2], out day)) return false;
    if (!int.TryParse(t[0], out h)) return false;
    if (!int.TryParse(t[1], out min)) return false;
    if (!int.TryParse(t[2], out s)) return false;

    if (m < 1 || m > 12) return false;
    if (day < 1 || day > 31) return false;
    if (h < 0 || h > 23) return false;
    if (min < 0 || min > 59) return false;
    if (s < 0 || s > 59) return false;

    return true;
}



    [ExportMethod]
    public void QuerySelectedColumns()
    
    {
        Log.Info("NEW RUN ROI NE PA OI");
        string fromVar = Project.Current.GetVariable("Model/History/Select/FromTime").Value;
        string toVar = Project.Current.GetVariable("Model/History/Select/ToTime").Value;
        bool isFromValid = IsValidDateTimeFormat(fromVar);
        bool isToValid = IsValidDateTimeFormat(toVar);


        // Dùng đúng định dạng có dấu '-' cho ngày tháng
        DateTime fromDateTime = DateTime.ParseExact(fromVar.Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime toDateTime = DateTime.ParseExact(toVar.Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        // Chuyển sang định dạng ISO 8601 (UTC) với hậu tố 'Z'
        string fromDateISO = fromDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string toDateISO = toDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        

        string[] NameEnergy = { "voltage", "current", "power", "frequency" };
        string[] BoolVarNamesEnergy = { "VoltageSelected", "CurrentSelected", "PowerSelected", "FrequencySelected" };
        var selectedColumnsEnergy = new List<string>();
        
        // Lấy các cột đã được chọn
        for (int i = 0; i < NameEnergy.Length; i++)
        {
            var varNode = Project.Current.GetVariable($"Model/History/Select/Energy/{BoolVarNamesEnergy[i]}");
            if (varNode == null)
            {
                Log.Warning($"Không tìm thấy biến: {BoolVarNamesEnergy[i]}");
                continue;
            }

            if ((bool)varNode.Value)
                selectedColumnsEnergy.Add(NameEnergy[i]);
        }

        string columnStringEnery = string.Join(",", selectedColumnsEnergy);
        if (!string.IsNullOrEmpty(columnStringEnery))
        {
            columnStringEnery = "," + columnStringEnery;
        }


        string[] NameDevice = { "1", "2", "3", "4" };
        string[] BoolVarNameDevice = { "DeviceSelected1", "DeviceSelected2", "DeviceSelected3", "DeviceSelected4" };
        var selectedColNameDevice = new List<string>();

        for (int i = 0; i < NameDevice.Length; i++)
        {
            var varNode = Project.Current.GetVariable($"Model/History/Select/Device/{BoolVarNameDevice[i]}");
            if (varNode == null)
            {
                Log.Warning($"Không tìm thấy biến: {BoolVarNameDevice[i]}");
                continue;
            }

            if ((bool)varNode.Value)
                selectedColNameDevice.Add($"{NameDevice[i]}");
        }

        string columnStringDevice = selectedColNameDevice.Count > 0 ? string.Join(" ,", selectedColNameDevice) : "";

        string[] Mode = { "hour", "day", "week", "month" };  
        string[] BoolVarMode = { "Hour", "Day", "Week", "Month" };

        string selectedMode = "hour";  

        for (int i = 0; i < Mode.Length; i++)
        {
            var varNode = Project.Current.GetVariable($"Model/History/Select/Mode/{BoolVarMode[i]}");
            if (varNode == null)
            {
                Log.Warning($"Không tìm thấy biến: {BoolVarMode[i]}");
                continue;
            }

            if ((bool)varNode.Value)
            {
                selectedMode = Mode[i];
                break;  
            }
        }

        if (selectedMode != null)
        {
            Log.Info($"Chế độ được chọn: {selectedMode}");
        }
        else
        {
            Log.Warning("Không có chế độ nào được chọn.");
        }

        string query;
        if(columnStringDevice.Length>=1){
            query = $"SELECT LocalTimestamp,DeviceID,Status{columnStringEnery} FROM Device WHERE DeviceID = {columnStringDevice}";
            if(isFromValid && isToValid){
                query = $"SELECT LocalTimestamp,DeviceID,Status{columnStringEnery} FROM Device WHERE DeviceID = {columnStringDevice}  AND LocalTimestamp BETWEEN '{fromVar}' AND '{toVar}'";
            }
            
        }else{
            query = $"SELECT LocalTimestamp,DeviceID,Status{columnStringEnery} FROM Device";
            if(isFromValid && isToValid){
                query = $"SELECT LocalTimestamp,DeviceID,Status{columnStringEnery} FROM Device WHERE LocalTimestamp BETWEEN '{fromVar}' AND '{toVar}'";
            }
        }

        LogicObject.GetVariable("QueryReturn").Value = query;

        LogicObject.GetVariable("device").Value = columnStringDevice;
        LogicObject.GetVariable("startTime").Value = fromVar;
        LogicObject.GetVariable("endTime").Value = toVar;
        LogicObject.GetVariable("enery").Value =  $"Timestamp,DeviceID{columnStringEnery}";
        LogicObject.GetVariable("limitData").Value = "100";
        LogicObject.GetVariable("mode").Value = selectedMode;

        string projectPath = ResourceUri.FromProjectRelativePath("").Uri;
        string dataPath = Path.Combine(projectPath, "eCharts", "History", "Bar", "mix-line-bar.js");
        string filePath = Path.Combine(projectPath, "eCharts", "History", "Bar", "chart-bar-line.js");
       
        string text = File.ReadAllText(dataPath);
        // Log.Info("[eChart] dataPath path: " + dataPath);
        // Log.Info("[eChart] filePath path: " + filePath);
        // Log.Info("[eChart] text path: " + text);
        // Log.Info("columnStringDevice: " + columnStringDevice);
        text = text.Replace("$01",columnStringDevice);
        text = text.Replace("$02",fromDateISO);
        text = text.Replace("$03",toDateISO);
        string jsObjectString = "timestamp,device_id" + columnStringEnery;
        Log.Info("columnStringDevice: " + columnStringDevice);
        text = text.Replace("$04",jsObjectString);
        text = text.Replace("$06",selectedMode);
        Log.Info("text: " + text);

        File.WriteAllText(filePath, text);

        // Owner.Get<WebBrowser>("TestAppEnerg/UI/Pages/History/VerticalLayout1/Summary/Mixed Line and Bar/WebBrowser1").Refresh();
        Log.Info("asds"+query);
    }


}
