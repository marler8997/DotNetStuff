using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JBuild
{
    public class JBuildType
    {
        public JBuildPrimitive primitive;
        public JBuildType(JBuildPrimitive primitive)
        {
            this.primitive = primitive;
        }
    }

    public enum JBuildPrimitive
    {
        Number,
            Unsigned,
            Signed,
            Decimal,
        String,
        FileSystemObject,
            Directory,
            File,
        Object,
    }

    public enum JBuildSetType
    {
        Single,
        List,
        Set,
    }

    public class Output
    {
        public readonly Property[] properties;
        public Output(Property[] properties)
        {
            this.properties = properties;
        }
    }

    public class Property
    {
        public readonly JBuildType type;
        public readonly String name;

        public Property(JBuildType type, String name)
        {
            this.type = type;
            this.name = name;
        }
    }

    public class BuildFunction
    {
        public readonly String name;
        public readonly UInt32 outputCount; // A value of 0 means variable number of outputs
        public readonly UInt32 inputCount; // A value of 0 means variable number of inputs
        public readonly Property[] properties;

        public BuildFunction(String name, UInt32 outputCount, UInt32 inputCount, Property[] properties)
        {
            this.name = name;
            this.outputCount = outputCount;
            this.inputCount = inputCount;
            this.properties = properties;
        }        
    }
}
