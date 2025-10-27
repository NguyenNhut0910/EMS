#region Using directives
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
using FTOptix.SerialPort;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
using FTOptix.DataLogger;
using FTOptix.InfluxDBStoreLocal;
using FTOptix.InfluxDBStore;
using FTOptix.ODBCStore;
using FTOptix.MQTTClient;
using FTOptix.MQTTBroker;
using FTOptix.Report;
using FTOptix.NativeUI;
#endregion

public class AlarmWidgetGenerateDefaultFiltersToggle : BaseNetLogic
{
    [ExportMethod]
    public void GenerateDefaultFiltersToggle()
    {
        var filterConfiguration = AlarmFilterEditModelLogic.GetEditModel(AlarmWidgetConfiguration, filtersConfigurationBrowseName);

        AlarmFilterEditModelLogic.DeleteEditModel(AlarmWidgetConfiguration);
        AlarmFilterEditModelLogic.CreateEditModel(AlarmWidgetConfiguration, filterConfiguration);
    }

    [ExportMethod]
    public void UpdateDefaultFiltersToggle()
    {
        var filterConfiguration = AlarmFilterEditModelLogic.GetEditModel(AlarmWidgetConfiguration, filtersConfigurationBrowseName);

        AlarmFilterEditModelLogic.CreateEditModel(AlarmWidgetConfiguration, filterConfiguration);
    }

    public IUANode AlarmWidgetConfiguration
    {
        get
        {
            var alarmWidgetConfiguration = InformationModel.Get(Owner.NodeId);
            return alarmWidgetConfiguration ?? throw new CoreConfigurationException($"{alarmWidgetConfigurationBrowseName} not found");
        }
    }

    private readonly string alarmWidgetConfigurationBrowseName = "AlarmWidgetConfiguration";
    private readonly string filtersConfigurationBrowseName = "FiltersConfiguration";
}
