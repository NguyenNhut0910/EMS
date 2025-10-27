using System;
using UAManagedCore;

//-------------------------------------------
// WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
//-------------------------------------------

[MapType(NamespaceUri = "EMS", Guid = "4043625fdc194a509c90422483250c87")]
public class PowerData : UAObject
{
#region Children properties
    //-------------------------------------------
    // WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
    //-------------------------------------------
    public double Voltage
    {
        get
        {
            return (double)Refs.GetVariable("Voltage").Value.Value;
        }
        set
        {
            Refs.GetVariable("Voltage").SetValue(value);
        }
    }
    public IUAVariable VoltageVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Voltage");
        }
    }
    public double Current
    {
        get
        {
            return (double)Refs.GetVariable("Current").Value.Value;
        }
        set
        {
            Refs.GetVariable("Current").SetValue(value);
        }
    }
    public IUAVariable CurrentVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Current");
        }
    }
    public double Power
    {
        get
        {
            return (double)Refs.GetVariable("Power").Value.Value;
        }
        set
        {
            Refs.GetVariable("Power").SetValue(value);
        }
    }
    public IUAVariable PowerVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Power");
        }
    }
    public double Frequency
    {
        get
        {
            return (double)Refs.GetVariable("Frequency").Value.Value;
        }
        set
        {
            Refs.GetVariable("Frequency").SetValue(value);
        }
    }
    public IUAVariable FrequencyVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Frequency");
        }
    }
#endregion
}
