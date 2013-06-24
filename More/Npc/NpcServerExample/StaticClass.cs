using System;
using System.Collections.Generic;
using System.Text;

public static class StaticClass
{
    public static void BlankCall()
    {
    }

    static Int32 int32Value;
    public static void SetInt32(Int32 value)
    {
        int32Value = value;
    }
    public static Int32 GetInt32()
    {
        return int32Value;
    }
}