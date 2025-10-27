#region Using directives
using UAManagedCore;
using FTOptix.UI;
using FTOptix.NetLogic;
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
#endregion

public class TimeShelveLogic : BaseNetLogic
{
    public override void Start()
    {
        presetTimeButton = Owner.Get<ToggleButton>("TimedShelve/Layout/DurationButtonsLayout/PresetShelveDurationButton");
        customTimeButton = Owner.Get<ToggleButton>("TimedShelve/Layout/DurationButtonsLayout/CustomTime/CustomTimeShelveButton");

        PresetShelveDurationButtonPressed();
    }

    [ExportMethod]
    public void PresetShelveDurationButtonPressed()
    {
        presetTimeButton.Active = true;
        customTimeButton.Active = false;
    }

    [ExportMethod]
    public void CustomTimeShelveButtonPressed()
    {
        presetTimeButton.Active = false;
        customTimeButton.Active = true;
    }

    private ToggleButton presetTimeButton;
    private ToggleButton customTimeButton;
}
