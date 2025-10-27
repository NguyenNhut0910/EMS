using System;
using UAManagedCore;

//-------------------------------------------
// WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
//-------------------------------------------

[MapType(NamespaceUri = "EMS", Guid = "a5a38fb9a2450f2bde1a896f7f37acbc")]
public class AlarmWidgetConfiguration : UAObject
{
#region Children properties
    //-------------------------------------------
    // WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
    //-------------------------------------------
    public FTOptix.NetLogic.NetLogicObject AlarmWidgetGenerateDefaultFiltersToggle
    {
        get
        {
            return (FTOptix.NetLogic.NetLogicObject)Refs.GetObject("AlarmWidgetGenerateDefaultFiltersToggle");
        }
    }
    public FiltersConfiguration FiltersConfiguration
    {
        get
        {
            return (FiltersConfiguration)Refs.GetObject("FiltersConfiguration");
        }
    }
    public UAManagedCore.NodeId AlarmWidgetComponents
    {
        get
        {
            return (UAManagedCore.NodeId)Refs.GetVariable("AlarmWidgetComponents").Value.Value;
        }
        set
        {
            Refs.GetVariable("AlarmWidgetComponents").SetValue(value);
        }
    }
    public FTOptix.Core.NodePointer AlarmWidgetComponentsVariable
    {
        get
        {
            return (FTOptix.Core.NodePointer)Refs.GetVariable("AlarmWidgetComponents");
        }
    }
#endregion
}
