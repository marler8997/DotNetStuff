using System;
using System.IO;

namespace More.Pdl
{
    public interface ICodeGenerator
    {
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, IntegerTypeReference type);
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, EnumOrFlagsTypeReference type);
        void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, ObjectTypeReference type);
    }
}