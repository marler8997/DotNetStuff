using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Marler.Common;

//
// This protocol needs documentation.
// Also in the future it should implement syntax to represent tables
//
// Sos Primitives:
//
//     Boolean     true|false
//
//   NumberTypes:
//     SByte                        -128 through 127
//     Byte                            0 through 255
//
//     Int16                     -32,768 through 32,767
//     UInt16                          0 through 65,535
//
//     Int32               2,147,483,648 through 2,147,483,647
//     UInt32                          0 through 4,294,967,295
//
//     Int64  -9,223,372,036,854,775,808 through 9,223,372,036,854,775,807
//     UInt64                          0 through 18,446,744,073,709,551,615
//
//     Single              -3.402823E+38 through 3.402823E+38
//     Double     -1.79769313486232E+308 through 1.79769313486232E+308
//
//   StringTypes:
//     Char     '<char>'
//     String   "<some-string"
//
// Enum Types:
//     <EnumName> [a-zA-Z_]
//     An enum type has
//        1. An enum name
//        2. A set of enum values, each of which has a name and an explicit value
//        
//
// Sos Combination Types (used to combine other Sos types to make more complex types):
//     Sos Array  '[' (value) [ ',' (value) ] ']'
//         Example: [1,2,3,4]
//
//     Sos Object '{' (field-name) ':' (field-value) [ ',' (field-name) ':' (field-value) ]* '}'
//         Example: {MyBoolean:false,MyInteger:3,MyString:"hello",AnotherObject:{LittleInteger:-1},SomeNumbers:[1,2,3]}
//
//     Sos Table  '<' (header-name) [ ',' (header-name) ]* [ ':' (object-value) [ ',' (object-value) ] ] '>'
//         Example: <Name,Age,Gender:"Joe Smith",23,Male:"Amy White",42,Female>
//         (Note: This feature is not implemented yet but is a part of the specification)


namespace Marler.Net
{
    public class SosTypeSerializationVerifier
    {
        readonly HashSet<Type> inRecursionTree = new HashSet<Type>();
        public SosTypeSerializationVerifier()
        {
        }
        public String CannotBeSerializedBecause(Type type)
        {
            //Console.Write("   Debug: ");
            //Console.Write(new String(' ', inRecursionTree.Count));
            //Console.WriteLine(type.FullName);

            //
            // IsAbstract must be checked before type == typeof(Enum) because enum types can be serialized but the
            // actual typeof(Enum) type cannot
            //
            if (type.IsAbstract) return String.Format("Class '{0}' cannot be serialized because it is abstract", type.FullName);

            //
            // This must be checked before IsPrimitive because IntPtr and UIntPtr are primitive types
            //
            if (type == typeof(IntPtr) || type == typeof(UIntPtr))
                return String.Format("{0} cannot be serialized because it is a pointer", type.Name);

            if (type.IsPrimitive || type == typeof(String) || type == typeof(Enum)) return null;

            if (type.IsArray) return CannotBeSerializedBecause(type.GetElementType());

            //
            // Check other conditions
            //
            if (type.IsGenericTypeDefinition) return String.Format("Class '{0}' cannot be serialized because it is a generic type definition", type.FullName);
            if (type.IsPointer) return String.Format("Class '{0}' cannot be serialized because it is a pointer", type.FullName);

            //
            // Check the type name
            //
            String fullTypeName = type.SosTypeName();
            for (int i = 0; i < fullTypeName.Length; i++)
            {
                Char c = fullTypeName[i];
                if (
                    (c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c != '.') &&
                    (c < '0' || c > '9') &&
                    (c != '_'))
                {
                    return String.Format("Class '{0}' cannot be serialized because it's name contains invalid characters", fullTypeName);
                }
            }

            //
            // Check that each field can be serialized
            //
            if (inRecursionTree.Contains(type)) return null;
            inRecursionTree.Add(type);

            String message = null;

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfos != null && fieldInfos.Length > 0)
            {
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    message = CannotBeSerializedBecause(fieldInfos[i].FieldType);
                    if (message != null) break;
                }
            }
            inRecursionTree.Remove(type);

            return message;
        }
    }

    public static class Sos
    {
        static readonly Type[] StringParam = new Type[] { typeof(String) };


        // Used to create a user friendly string describing the difference
        // between the given expected object and the actual object
        public static String Diff(this Object expected, Object actual)
        {
            if (expected == null) return (actual == null) ? null : String.Format("Expected <null> but got '{0}'", actual);
            if (actual == null) return String.Format("Expected '{0}' but got <null>", expected);

            Type type = expected.GetType();
            Type actualType = actual.GetType();

            if (type != actualType) return String.Format("Expected object to be of type '{0}' but actual type is '{1}'", type.FullName, actualType.FullName);

            if (type.IsPrimitive || type == typeof(Enum)) return expected.Equals(actual) ? null :
                String.Format("Expected '{0}' but got '{1}'", expected, actual);

            if (type == typeof(String)) return expected.Equals(actual) ? null :
                String.Format("Expected \"{0}\" but got \"{1}\"", expected, actual);

            if (type.IsArray)
            {
                Array expectedArray = (Array)expected;
                Array actualArray = (Array)actual;

                if (expectedArray.Length != actualArray.Length)
                    return String.Format("Expected array of length {0} but actual length is {1}",
                        expectedArray.Length, actualArray.Length);

                for (int i = 0; i < expectedArray.Length; i++)
                {
                    Object expectedElement = expectedArray.GetValue(i);
                    Object actualElement = actualArray.GetValue(i);

                    String diffMessage = Diff(expectedElement, actualElement);
                    if (diffMessage != null) return String.Format("At index {0}: {1}", i, diffMessage);
                }

                return null;
            }

            //
            // Check that each field can be serialized
            //
            FieldInfo[] expectedFieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (expectedFieldInfos == null || expectedFieldInfos.Length <= 0)
            {
                FieldInfo[] actualFieldInfos = actualType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return (actualFieldInfos == null || actualFieldInfos.Length <= 0) ? null :
                    String.Format("Expected object to have no fields but actual object has {0} fields", actualFieldInfos.Length);
            }
            for (int i = 0; i < expectedFieldInfos.Length; i++)
            {
                FieldInfo expectedFieldInfo = expectedFieldInfos[i];

                Object expectedField = expectedFieldInfo.GetValue(expected);
                Object actualField = expectedFieldInfo.GetValue(actual);

                String diffMessage = Diff(expectedField, actualField);
                if (diffMessage != null) return String.Format("Field '{0}': {1}", expectedFieldInfo.Name, diffMessage);
            }

            return null;
        }

        // An Sos Object always starts with 0-9, a-z, A-Z, -, ", [, {, or '
        public static Boolean IsValidStartOfSosString(Char c)
        {
            return
                (c >= '0' && c <= '9') || // Decimal digit
                (c >= 'a' && c <= 'z') || // Lower case letter
                (c >= 'A' && c <= 'Z') || // Upper case letter
                (c == '-') ||
                (c == '"') ||
                (c == '[') ||
                (c == '{') ||
                (c == '\'');
        }

        // Returns the index of the next whitespace that is not part of an Sos string or character.
        public static Int32 NextNonQuotedWhitespace(String str, Int32 offset)
        {
            //
            // Make sure Sos object starts with valid character
            //
            Char c = str[offset];

            while (true)
            {
                // Started
                if (c == '"')
                {
                    while (true)
                    {
                        offset++;
                        if (offset >= str.Length) throw new FormatException("Found double quote that was never closed");
                        c = str[offset];
                        if (c == '"')
                        {
                            // Make sure the quote is not escaped

                            // Get the first previous character that is not a '\'
                            Int32 back;
                            for (back = 1; str[offset - back] == '\\'; back++) ;

                            Boolean endOfString = (back % 2) == 1;

                            if (endOfString) break;
                            continue;
                        }
                    }
                }
                else if (c == '\'')
                {
                    offset++;
                    if (offset >= str.Length) throw new FormatException("Found single quote that was not closed");
                    c = str[offset];
                    if (c == '\'') throw new FormatException("Found empty character");

                    while (true)
                    {
                        offset++;
                        if (offset >= str.Length) throw new FormatException("Found single quote that was not closed");
                        c = str[offset];
                        if (c == '\'') break;
                    }
                }

                offset++;
                if (offset >= str.Length) return offset;

                c = str[offset];
                if (Char.IsWhiteSpace(c)) return offset;
            }
        }


        //
        // Boolean Type
        //
        public static String Serialize(this Boolean value)
        {
            return value ? "true" : "false";
        }
        //
        // Integer Types
        //
        public static String Serialize(this SByte value)
        {
            return value.ToString();
        }
        public static String Serialize(this Byte value)
        {
            return value.ToString();
        }
        public static String Serialize(this Int16 value)
        {
            return value.ToString();
        }
        public static String Serialize(this UInt16 value)
        {
            return value.ToString();
        }
        public static String Serialize(this Int32 value)
        {
            return value.ToString();
        }
        public static String Serialize(this UInt32 value)
        {
            return value.ToString();
        }
        public static String Serialize(this Int64 value)
        {
            return value.ToString();
        }
        public static String Serialize(this UInt64 value)
        {
            return value.ToString();
        }
        //
        // Float Types
        //
        public static String Serialize(this Single value)
        {
            return value.ToString("r");
        }
        public static String Serialize(this Double value)
        {
            return value.ToString("r");
        }
        //
        // Character Types
        //
        public static String Serialize(this Char value)
        {
            if (value >= ' ' && value <= '~')
            {
                if (value == '\\') return "'\\\\'";
                if (value == '\'') return "'\\''";
                return "'" + value + "'";
            }

            if (value == '\n') return @"'\n'";
            if (value == '\r') return @"'\r'";
            if (value == '\t') return @"'\t'";
            if (value == '\0') return @"'\0'";

            UInt32 valueAsUInt32 = (UInt32)value;
            if (valueAsUInt32 <= 0xFF) return @"'\x" + valueAsUInt32.ToString("X2") + "'";
            if (valueAsUInt32 <= 0xFFFF) return @"'\u" + valueAsUInt32.ToString("X4") + "'";
            throw new InvalidOperationException(String.Format("The character '{0}' (code = {1}) has a code that cannot be represented by 2 bytes, which is currently not supported", value, valueAsUInt32));
        }
        public static String Serialize(this String value)
        {
            //return String.Format("\"{0}\"", value.Replace(@"\", @"\\").Replace("\"", "\\\""));
            return "\"" + value.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\""; // Faster
        }
        public static String Serialize(this Object obj)
        {
            StringBuilder builder = new StringBuilder();
            Serialize(obj, builder);
            return builder.ToString();
        }

        //
        // Right now it does not handle circular references
        //
        public static void Serialize(this Object obj, StringBuilder builder)
        {
            if (obj == null)
            {
                builder.Append("null");
                return;
            }

            Type type = obj.GetType();

            //
            // Handle primitive types
            //
            if (type.IsPrimitive)
            {
                if (type == typeof(Boolean))
                {
                    builder.Append(((Boolean)obj).Serialize());
                    return;
                }
                if (type == typeof(Char))
                {
                    builder.Append(((Char)obj).Serialize());
                    return;
                }
                if (type == typeof(Single) || type == typeof(Double))
                {
                    // The 'r' format specifier stands for "Round Trip"
                    // Which means the number should be printed in a way that the value
                    // can be parsed from the string without losing information.
                    if (type == typeof(Single))
                    {
                        builder.Append(((Single)obj).ToString("r"));
                        return;
                    }
                    else
                    {
                        builder.Append(((Double)obj).ToString("r"));
                        return;
                    }
                }
                builder.Append(obj.ToString());
                return;
            }

            if (type == typeof(String))
            {
                builder.Append(((String)obj).Serialize());
                return;
            }

            //
            // Handle Arrays
            //
            if (type.IsArray)
            {
                builder.Append('[');
                Array objectAsArray = (Array)obj;
                for (int i = 0; i < objectAsArray.Length; i++)
                {
                    if (i > 0) builder.Append(',');
                    Serialize(objectAsArray.GetValue(i), builder);
                }
                builder.Append(']');
                return;
            }

            //
            // Handle enums
            //
            if (type.IsEnum)
            {
                builder.Append(obj.ToString());
                return;
            }

            //
            // Serialize it as a an object of primitive types
            //
            builder.Append('{');
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                builder.Append('}');
                return;
            }

            FieldInfo fieldInfo = fieldInfos[0];

            builder.Append(fieldInfo.Name);
            builder.Append(":");
            Serialize(fieldInfo.GetValue(obj), builder);

            for (int i = 1; i < fieldInfos.Length; i++)
            {
                fieldInfo = fieldInfos[i];
                builder.Append(',');
                builder.Append(fieldInfo.Name);
                builder.Append(":");
                Serialize(fieldInfo.GetValue(obj), builder);
            }
            builder.Append('}');
        }

        //
        // Deserialization Methods
        //
        public static Int32 HexValue(this Char c)
        {
            if (c >= '0' && c <= '9') return (c - '0');
            if (c >= 'A' && c <= 'F') return (c - 'A') + 10;
            if (c >= 'a' && c <= 'f') return (c - 'a') + 10;
            throw new FormatException(String.Format("Expected 0-9, A-F, or a-f but got '{0}' (charcode={1})", c, (UInt32)c));
        }
        public static Int32 EnumValueLength(String str, Int32 offset, Int32 offsetLimit)
        {
            //
            // Check if it is a number version
            //
            Int32 numberLength = WholeNumberLength(str, offset, offsetLimit);
            if (numberLength > 0) return numberLength;

            Int32 originalOffset = offset;

            while (true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if ((check < 'a' || check > 'z') &&
                    (check < 'A' || check > 'Z' ) &&
                    (check < '0' || check > '9') &&
                    check != '_') return offset - originalOffset;

                offset++;
            }
        }
        public static Int32 WholeNumberLength(String str, Int32 offset, Int32 offsetLimit)
        {
            Int32 originalOffset = offset;
            if (str[offset] == '-') offset++;
            while (true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if ((check < '0' || check > '9')) return offset - originalOffset;

                offset++;
            }
        }
        public static Int32 FloatLength(String str, Int32 offset, Int32 offsetLimit)
        {
            Int32 originalOffset = offset;
            if(str[offset] == '-') offset++; // By checking it first we are speeding up the performance in the average case

            while(true)
            {
                if (offset >= offsetLimit) return offset - originalOffset;

                Char check = str[offset];
                if(
                    (check < '0' || check > '9') &&
                    check != '.' &&
                    check != 'E' &&
                    check != 'e' &&
                    check != '-' &&
                    check != '+') return offset - originalOffset;

                offset++;
            }
        }
        public static Int32 Deserialize(out Object obj, Type type, String value, Int32 offset, Int32 offsetLimit)
        {
            //
            // Handle primitive types
            //
            if (type.IsPrimitive)
            {
                if (type == typeof(Char))
                {
                    if (value[offset] != '\'') throw new FormatException(String.Format(
                        "A character must begin with a single quote (') but it began with ({0}) (charcode={1})",
                         value[offset], (UInt32)value[offset]));

                    offset++;
                    if(offset >= offsetLimit) throw new FormatException("Unexpected end of input after single-quote marking the start of a character");


                    Char charValue = value[offset];
                    if (charValue != '\\')
                    {
                        obj = charValue;
                        offset++;
                    }
                    else
                    {
                        offset++;
                        if (offset >= offsetLimit) throw new FormatException("Expected an escape character but reached end of string");

                        charValue = value[offset];
                        if (charValue == '\\')
                        {
                            obj = '\\';
                        }
                        else if (charValue == '\'')
                        {
                            obj = '\'';
                        }
                        else if (charValue == 'n')
                        {
                            obj = '\n';
                        }
                        else if (charValue == 't')
                        {
                            obj = '\t';
                        }
                        else if (charValue == 'r')
                        {
                            obj = '\r';
                        }
                        else if (charValue == '0')
                        {
                            obj = '\0';
                        }
                        else if (charValue == 'x')
                        {
                            if(offset + 2 >= offsetLimit) throw new FormatException("The \\x escape sequence must be followed by exactly two hex characters but reached end of input"); 
                            
                            obj = (Char)((value[offset + 1].HexValue() << 4) + value[offset + 2].HexValue());
                            
                            offset += 2;
                        }
                        else if (charValue == 'u')
                        {
                            if (offset + 4 >= offsetLimit) throw new FormatException("The \\u escape sequence must be followed by exactly four hex characters but reached end of input");
                            obj = (Char)( 
                                (value[offset + 1].HexValue() << 12) +
                                (value[offset + 2].HexValue() <<  8) +
                                (value[offset + 3].HexValue() <<  4) +
                                (value[offset + 4].HexValue()      ) );
                            offset += 4;
                        }
                        else if (charValue == 'a')
                        {
                            obj = '\a';
                        }
                        else if (charValue == 'b')
                        {
                            obj = '\b';
                        }
                        else if (charValue == 'f')
                        {
                            obj = '\f';
                        }
                        else if (charValue == 'v')
                        {
                            obj = '\v';
                        }
                        else
                        {
                            throw new FormatException(String.Format("Unknown escape sequence '\\{0}'", charValue));
                        }
                        offset++;
                    }
                    
                    if (value[offset] != '\'') throw new FormatException(String.Format(
                        "Expected a single-quote (') to end a character but found ({0}) (charcode={1})",
                         value[offset], (UInt32)value[offset]));

                    return offset + 1;
                }

                if (type == typeof(Boolean))
                {
                    if (
                        value[offset    ] == 'f' &&
                        value[offset + 1] == 'a' &&
                        value[offset + 2] == 'l' &&
                        value[offset + 3] == 's' &&
                        value[offset + 4] == 'e')
                    {
                        obj = false;
                        return offset + 5;
                    }
                    else if (
                        value[offset    ] == 't' &&
                        value[offset + 1] == 'r' &&
                        value[offset + 2] == 'u' &&
                        value[offset + 3] == 'e')
                    {
                        obj = true;
                        return offset + 4;
                    }

                    throw new FormatException(String.Format(
                        "Expected boolean value to be 'true' or 'false' but was '{0}'...?",
                        value.Substring(offset, 4)));
                }
                if (type == typeof(Single) || type == typeof(Double))
                {
                    //
                    // Check for NaN
                    //
                    if (
                        (value[offset    ] == 'N' || value[offset    ] == 'n') &&
                        (value[offset + 1] == 'a' || value[offset + 1] == 'A') &&
                        (value[offset + 2] == 'N' || value[offset + 2] == 'n'))
                    {
                        if (type == typeof(Single))
                        {
                            obj = Single.NaN;
                        }
                        else
                        {
                            obj = Double.NaN;
                        }
                        return offset + 3;
                    }

                    //
                    // Check for Infinity
                    //
                    Int32 infinityOffset = offset + ((value[offset] == '-') ? 1 : 0);
                    if (
                        (value[infinityOffset    ] == 'I' || value[infinityOffset    ] == 'i') &&
                        (value[infinityOffset + 1] == 'n' || value[infinityOffset + 1] == 'N') &&
                        (value[infinityOffset + 2] == 'f' || value[infinityOffset + 2] == 'F') &&
                        (value[infinityOffset + 3] == 'i' || value[infinityOffset + 3] == 'I') &&
                        (value[infinityOffset + 4] == 'n' || value[infinityOffset + 4] == 'N') &&
                        (value[infinityOffset + 5] == 'i' || value[infinityOffset + 5] == 'I') &&
                        (value[infinityOffset + 6] == 't' || value[infinityOffset + 6] == 'T') &&
                        (value[infinityOffset + 7] == 'y' || value[infinityOffset + 7] == 'Y'))
                    {
                        if (infinityOffset == offset)
                        {
                            if (type == typeof(Single))
                            {
                                obj = Single.PositiveInfinity;
                            }
                            else
                            {
                                obj = Double.PositiveInfinity;
                            }
                            return offset + 8;
                        }
                        if (type == typeof(Single))
                        {
                            obj = Single.NegativeInfinity;
                        }
                        else
                        {
                            obj = Double.NegativeInfinity;
                        }
                        return offset + 9;
                    }
                }

                //
                // Get number characters
                //
                Int32 numberLength = FloatLength(value, offset, offsetLimit);
                if(numberLength <= 0) throw new FormatException(String.Format(
                    "Expected number but got '{0}'", value.Substring(offset)));

                String numberString = value.Substring(offset, numberLength);
                offset += numberLength;


                //
                // Slower way using reflection
                //
                //MethodInfo parseMethod = type.GetMethod("Parse", stringParam);
                //obj = parseMethod.Invoke(null, new Object[] { numberString });

                //
                // Faster way using If-ElseIf
                //
                if (type == typeof(SByte))
                {
                    obj = SByte.Parse(numberString);
                }
                else if (type == typeof(Byte))
                {
                    obj = Byte.Parse(numberString);
                }
                else if (type == typeof(Int16))
                {
                    obj = Int16.Parse(numberString);
                }
                else if (type == typeof(UInt16))
                {
                    obj = UInt16.Parse(numberString);
                }
                else if (type == typeof(Int32))
                {
                    obj = Int32.Parse(numberString);
                }
                else if (type == typeof(UInt32))
                {
                    obj = UInt32.Parse(numberString);
                }
                else if (type == typeof(Int64))
                {
                    obj = Int64.Parse(numberString);
                }
                else if (type == typeof(UInt64))
                {
                    obj = UInt64.Parse(numberString);
                }
                else if (type == typeof(Single))
                {
                    obj = Single.Parse(numberString);
                }
                else if (type == typeof(Double))
                {
                    obj = Double.Parse(numberString);
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unknown Type '{0}'", type.Name));
                }

                return offset;
            }

            //
            // Handle enums
            //
            if (type.IsEnum)
            {
                Int32 enumValueLength = EnumValueLength(value, offset, offsetLimit);
                if (enumValueLength > 0)
                {
                    String enumValueString = value.Substring(offset, enumValueLength);
                    obj = Enum.Parse(type, enumValueString, true);
                    return offset + enumValueLength;
                }
                throw new FormatException(String.Format("Expected an enum value name or a number but got '{0}'",
                    value.Substring(offset)));
            }

            //
            // Check if it is null
            //
            if (
                (offset + 3 < offsetLimit) &&
                value[offset    ] == 'n' &&
                value[offset + 1] == 'u' &&
                value[offset + 2] == 'l' &&
                value[offset + 3] == 'l')
            {
                obj = null;
                return offset + 4;
            }

            if (type == typeof(String))
            {
                if (value[offset] != '"')
                    throw new FormatException(String.Format("Expected string to start with '\"' but started with '{0}'", value[offset]));

                StringBuilder builder = new StringBuilder();

                Int32 start = offset;
                while (true)
                {
                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Missing closing quote for string");

                    if (value[offset] == '"')
                    {
                        obj = builder.ToString();
                        return offset + 1;
                    }

                    if (value[offset] != '\\')
                    {
                        builder.Append(value[offset]);
                        continue;
                    }

                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Missing closing quote for string");

                    if (value[offset] == '\\')
                    {
                        builder.Append('\\');
                    }
                    else if (value[offset] == '"')
                    {
                        builder.Append('"');
                    }
                    else if (value[offset] == 'n')
                    {
                        builder.Append('\n');
                    }
                    else
                    {
                        throw new FormatException(String.Format("Unknown escape sequence '\\{0}'", value[offset]));
                    }
                }
            }

            //
            // Handle Arrays
            //
            if (type.IsArray)
            {
                if (value[offset] != '[')
                    throw new FormatException(String.Format("Expected array string to start with '[' but it started with '{0}' (charcode={1})", value[offset], (UInt32)value[offset]));

                offset++;
                if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                
                Type elementType = type.GetElementType();

                //
                // Check if array is empty
                //
                if (value[offset] == ']')
                {
                    offset++;
                    obj = Array.CreateInstance(elementType, 0);
                    return offset;
                }

                ArrayBuilder arrayBuilder = new ArrayBuilder(elementType);

                while (true)
                {
                    Object nextElement;
                    offset = Deserialize(out nextElement, elementType, value, offset, offsetLimit);
                    arrayBuilder.Add(nextElement);

                    if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                    if (value[offset] == ']')
                    {
                        offset++;
                        obj = arrayBuilder.Build();
                        return offset;
                    }
                    if (value[offset] != ',')
                        throw new FormatException(String.Format("Expected ',' to delimit an array element but got '{0}' (charcode={1})", value[offset], (UInt32)value[offset]));
                    offset++;
                    if (offset >= offsetLimit) throw new FormatException(String.Format("Missing closing ']' for array"));
                }
            }

            //
            // Serialize it as a an object of primitive types
            //
            obj = FormatterServices.GetUninitializedObject(type);
            //obj = Activator.CreateInstance(type);

            return DeserializeObject(obj, value, offset, offsetLimit);
        }


        private static Int32 DeserializeObject(Object obj, String value, Int32 offset, Int32 offsetLimit)
        {
            /*
            // Skip till '{'
            while (true)
            {
                if (offset >= offsetLimit) throw new FormatException(String.Format(
                     "Expected '{{' but did not find one in '{0}'",
                     value.Substring(offset, offsetLimit - offset)));
                //Encoding.UTF8.GetString(bytes, offsetLimit, offsetLimit - offsetLimit)));
                if (value[offset] == '{') break;
                offset++;
            }

            offset++;
            if (offset >= offsetLimit) throw new FormatException("Missing ending '}'");
            */

            if(offset >= offsetLimit) throw new FormatException("Object string is empty");

            if (value[offset] != '{') throw new FormatException(String.Format(
                "Expected object to start with '{{' but it started with '{0}', the rest of the string is '{1}'",
                value[offset], value.Substring(offset)));
            offset++;

            if (offset >= offsetLimit) throw new FormatException("Missing ending '}'");


            Type type = obj.GetType();

            while (true)
            {
                Int32 fieldNameStart = offset;

                // Skip till ':'
                while (true)
                {
                    Char c = value[offset];

                    //
                    // End of Object '}'
                    //
                    if (c == '}')
                    {
                        // Check that there is only whitespace
                        while (fieldNameStart < offset)
                        {
                            if (value[fieldNameStart] != ' ' && value[fieldNameStart] != '\t' && value[fieldNameStart] != '\n')
                                throw new FormatException(string.Format("Found end of object '}' but it was preceded by '{0}'",
                                    value.Substring(fieldNameStart, offset - fieldNameStart)));
                            fieldNameStart++;
                        }
                        return offset + 1;
                    }

                    //
                    // At Field Value ':'
                    //
                    if (c == ':') break;

                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Expected ':' but reached end of string'");
                }

                offset++;
                if (offset >= offsetLimit) throw new FormatException("Expected value after ':' but reached end of string");

                //
                // Deserialize the field
                //
                String fieldName = value.Substring(fieldNameStart, offset - fieldNameStart - 1).Trim();
                FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo == null) throw new InvalidOperationException(String.Format("Could not find field '{0}' in type {1}", fieldName, type.Name));

                Type fieldType = fieldInfo.FieldType;


                Object fieldValue;
                offset = Deserialize(out fieldValue, fieldType, value, offset, offsetLimit);
                fieldInfo.SetValue(obj, fieldValue);


                //
                // Check for ',' or '}'
                //
                if (offset >= offsetLimit) throw new FormatException("Missing ending '}'");
                Char next = value[offset];
                if (next == ',')
                {
                    offset++;
                    if (offset >= offsetLimit) throw new FormatException("Expected field name but reached end of string");
                    continue;
                }
                if (next == '}') return offset + 1;
                throw new FormatException(String.Format("Expected ',' or '}}' but got '{0}' (charcode={1})", next, (UInt32)next));
            }
        }
    }
}
