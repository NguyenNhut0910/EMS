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
using FTOptix.Report;
using FTOptix.CommunicationDriver;
using FTOptix.Modbus;
#endregion

public class CreateAlarmCountDB : BaseNetLogic
{
    private PeriodicTask periodicTask;

    public override void Start()
    {
        periodicTask = new PeriodicTask(Export, 5000, LogicObject); // 1000ms = 1 giây
        periodicTask.Start();
    }
    [ExportMethod]
    public void Export()
    {
        try
        {
            // Thêm mảng
            string[] AlarmDate = new string[15];
            int[] Severity0 = new int[15];
            int[] Severity1 = new int[15];
            int dayIndex = 0;
            // Lấy bảng AlarmsEventLogger1 (qua biến "Table")
            var alarmTable = GetTable();
            var storeObject = GetStoreObject(alarmTable);
            var fifteenDaysAgo = DateTime.Now.AddDays(-15).ToString("yyyy-MM-dd");
            var selectQuery = $@"
                SELECT 
                    LocalTime,
                    Severity
                FROM AlarmsEventLogger1
                WHERE ActiveState_Id = 1
                    AND ConfirmedState_Id = 1
                    AND AckedState_Id = 0
                    AND LocalTime >= '{fifteenDaysAgo}'
                ORDER BY LocalTime";
            storeObject.Query(selectQuery, out string[] header, out object[,] resultSet);
            if (header == null || resultSet == null)
                throw new Exception("Query trả về dữ liệu rỗng hoặc sai định dạng");
            int rowCount = resultSet.GetLength(0);
            var summary = new Dictionary<string, Dictionary<string, int>>();
            for (int r = 0; r < rowCount; r++)
            {
                string localTimeStr = resultSet[r, 0]?.ToString() ?? "";
                string severityStr = resultSet[r, 1]?.ToString() ?? "";
                if (DateTime.TryParse(localTimeStr, out DateTime localTime))
                {
                    string date = localTime.ToString("yyyy-MM-dd");
                    if (!summary.ContainsKey(date))
                        summary[date] = new Dictionary<string, int>();
                    if (!summary[date].ContainsKey(severityStr))
                        summary[date][severityStr] = 0;
                    summary[date][severityStr]++;
                }
                else
                {
                    Log.Warning("GenericTableExporter", $"Không parse được LocalTime: {localTimeStr}");
                }
            }

            // Lấy bảng đích AlarmSummary qua biến "TableSummary"
            var summaryTableVar = LogicObject.GetVariable("TableSummary");
            if (summaryTableVar == null)
                throw new Exception("Không tìm thấy biến TableSummary");

            var summaryTableNodeId = summaryTableVar.Value;
            var summaryTable = InformationModel.Get(summaryTableNodeId) as Table;
            if (summaryTable == null)
                throw new Exception("TableSummary không phải là Table hợp lệ");

            // Xoá dữ liệu cũ trong bảng AlarmSummary
            string deleteQuery = "DELETE FROM AlarmSummary";
            storeObject.Query(deleteQuery, out _, out _);
            // Ghi dữ liệu tổng hợp vào bảng
            foreach (var dateEntry in summary)
            {
                string date = dateEntry.Key;

                // Thêm dữ liệu vào mảng
                if (dayIndex < 15)
                {
                    AlarmDate[dayIndex] = dateEntry.Key;
                    Severity0[dayIndex] = dateEntry.Value.ContainsKey("0") ? dateEntry.Value["0"] : 0;
                    Severity1[dayIndex] = dateEntry.Value.ContainsKey("1") ? dateEntry.Value["1"] : 0;
                    dayIndex++;
                }

                //
                foreach (var sevEntry in dateEntry.Value)
                {
                    string severity = sevEntry.Key;
                    int count = sevEntry.Value;

                    // Log.Info("GenericTableExporter", $"Ghi dữ liệu: Ngày={date}, Severity={severity}, Số lượng={count}");

                    // Convert Severity về short
                    if (!short.TryParse(severity, out short severityValue))
                        severityValue = -1;

                    string[] columnNames = new string[]
                    {
                        "AlarmDate",
                        "Severity",
                        "Count"
                    };

                    object[,] values = new object[,]
                    {
                        { date, severityValue, count }
                    };

                    summaryTable.Insert(columnNames, values);

                }
            }

            // Gán vào các biến UI trong NetLogic
            var alarmDateVar = LogicObject.GetVariable("AlarmDate");
            var severity0Var = LogicObject.GetVariable("Severity0");
            var severity1Var = LogicObject.GetVariable("Severity1");

            if (alarmDateVar == null || severity0Var == null || severity1Var == null)
                throw new Exception("Không tìm thấy biến AlarmDate hoặc Severity0 hoặc Severity1");

            alarmDateVar.Value = AlarmDate;
            severity0Var.Value = Severity0;
            severity1Var.Value = Severity1;

            try // Thử lọc và tạo bảng lưu 10 lỗi có tần suất xuất hiện nhiều nhất trong ngày
            {
                // Thêm mảng
                string[] ConditionName = new string[10];
                int[] OccurrenceCount = new int[10];

                var sevenDaysAgo = DateTime.Now.AddDays(0).ToString("yyyy-MM-dd");

                var selectQueryAO = $@"
                    SELECT 
                        ConditionName,
                        COUNT(*) AS OccurrenceCount
                    FROM 
                        AlarmsEventLogger1
                    WHERE 
                        ActiveState_Id = 1
                        AND AckedState_Id = 0
                        AND ConfirmedState_Id = 1
                        AND LocalTime >= '{sevenDaysAgo}'
                    GROUP BY 
                        ConditionName
                    ORDER BY 
                        OccurrenceCount DESC
                    LIMIT 10";

                storeObject.Query(selectQueryAO, out string[] headerAO, out object[,] resultSetAO);
                if (headerAO == null || resultSetAO == null)
                    throw new Exception("Query trả về dữ liệu rỗng hoặc sai định dạng");

                rowCount = resultSetAO.GetLength(0);

                // Lấy bảng đích AlarmOccurrenceCount
                var AlarmCOTableVar = LogicObject.GetVariable("TableAlarmOccurrence");
                if (AlarmCOTableVar == null)
                    throw new Exception("Không tìm thấy biến TableAlarmOccurrence");

                var AlarmCOTableNodeId = AlarmCOTableVar.Value;
                var AlarmCOTable = InformationModel.Get(AlarmCOTableNodeId) as Table;
                if (AlarmCOTable == null)
                    throw new Exception("TableAlarmOccurrence không phải là Table hợp lệ");

                // Xoá dữ liệu cũ
                deleteQuery = "DELETE FROM AlarmOccurrenceCount";
                storeObject.Query(deleteQuery, out _, out _);

                // Ghi dữ liệu mới
                for (int i = 0; i < rowCount && i < 10; i++)
                {
                    string condition = resultSetAO[i, 0]?.ToString() ?? "Unknown";
                    int count = Convert.ToInt32(resultSetAO[i, 1]);

                    ConditionName[i] = condition;
                    OccurrenceCount[i] = count;

                    string[] columnNames = new string[] { "ConditionName", "OccurrenceCount" };
                    object[,] values = new object[,] { { condition, count } };

                    AlarmCOTable.Insert(columnNames, values);
                }
  
            }
            catch (Exception ex)
            {
                Log.Error("GenericTableExporter", "Lỗi khi export: " + ex.Message);
            }




        }
        catch (Exception ex)
        {
            Log.Error("GenericTableExporter", "Lỗi khi export: " + ex.Message);
        }
    }

    private Table GetTable()
    {
        var tableVar = LogicObject.GetVariable("Table");
        if (tableVar == null)
            throw new Exception("Không tìm thấy biến Table");

        var tableNodeId = tableVar.Value;
        var table = InformationModel.Get(tableNodeId) as Table;
        if (table == null)
            throw new Exception("Biến Table không trỏ tới một Table hợp lệ");

        return table;
    }

    private Store GetStoreObject(Table tableNode)
    {
        return tableNode.Owner.Owner as Store;
    }

    private string GetQuery()
    {
        var queryVariable = LogicObject.GetVariable("Query");
        if (queryVariable == null)
            throw new Exception("Query variable not found");

        string query = queryVariable.Value;
        if (String.IsNullOrEmpty(query))
            throw new Exception("Query variable is empty");

        return query;
    }

    public override void Stop()
    {
        if (periodicTask != null)
            periodicTask.Dispose();
    }
}
