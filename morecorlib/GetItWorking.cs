

using System;

namespace System
{
}
namespace System
{
}
namespace System
{
}
namespace System
{
}
namespace System.IO
{
}
namespace System.Text
{
}
namespace System.Runtime
{
}
namespace System.Runtime.ConstrainedExecution
{
}
namespace System.Runtime.Versioning
{
}
namespace System.Runtime.CompilerServices
{
}
namespace System.Runtime.Remoting
{
}
namespace System.Threading
{
}
namespace System.Security
{
}
namespace System.Security.Permissions
{
}
namespace System.Security.Policy
{
}
namespace System.Diagnostics
{
}
namespace System.Diagnostics.Contracts
{
}
namespace System.Globalization
{
}
namespace System.Collections.Generic
{
}
namespace System.Reflection
{
    public interface IReflect
    {
    }
}
namespace System.Runtime.Serialization
{
    public class SerializableAttribute : Attribute
    {
    }
}
namespace System.Diagnostics.Contracts
{
    /// <summary>
    /// Methods and classes marked with this attribute can be used within calls to Contract methods. Such methods not make any visible state changes.
    /// </summary>
    [Conditional("CONTRACTS_FULL")]
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class PureAttribute : Attribute
    {
    }
}



namespace Microsoft
{
}
namespace Microsoft.Win32
{
}






/*
using System;
using System.Runtime.InteropServices;

namespace System
{
    // Summary:
    //     Indicates that a class can be serialized. This class cannot be inherited.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    [ComVisible(true)]
    public sealed class SerializableAttribute : Attribute
    {
        // Summary:
        //     Initializes a new instance of the System.SerializableAttribute class.
        public SerializableAttribute()
        {
        }
    }

    // Summary:
    //     Specifies the application elements on which it is valid to apply an attribute.
    [Serializable]
    [ComVisible(true)]
    //[Flags]
    public enum AttributeTargets
    {
        // Summary:
        //     Attribute can be applied to an assembly.
        Assembly = 1,
        //
        // Summary:
        //     Attribute can be applied to a module.
        Module = 2,
        //
        // Summary:
        //     Attribute can be applied to a class.
        Class = 4,
        //
        // Summary:
        //     Attribute can be applied to a structure; that is, a value type.
        Struct = 8,
        //
        // Summary:
        //     Attribute can be applied to an enumeration.
        Enum = 16,
        //
        // Summary:
        //     Attribute can be applied to a constructor.
        Constructor = 32,
        //
        // Summary:
        //     Attribute can be applied to a method.
        Method = 64,
        //
        // Summary:
        //     Attribute can be applied to a property.
        Property = 128,
        //
        // Summary:
        //     Attribute can be applied to a field.
        Field = 256,
        //
        // Summary:
        //     Attribute can be applied to an event.
        Event = 512,
        //
        // Summary:
        //     Attribute can be applied to an interface.
        Interface = 1024,
        //
        // Summary:
        //     Attribute can be applied to a parameter.
        Parameter = 2048,
        //
        // Summary:
        //     Attribute can be applied to a delegate.
        Delegate = 4096,
        //
        // Summary:
        //     Attribute can be applied to a return value.
        ReturnValue = 8192,
        //
        // Summary:
        //     Attribute can be applied to a generic parameter.
        GenericParameter = 16384,
        //
        // Summary:
        //     Attribute can be applied to any application element.
        All = 32767,
    }
}
namespace System
{
    // Summary:
    //     Specifies the usage of another attribute class. This class cannot be inherited.
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    [ComVisible(true)]
    public sealed class AttributeUsageAttribute : Attribute
    {
        readonly AttributeTargets validOn;
        Boolean allowMultiple = false;
        Boolean inherited = true;
        // Summary:
        //     Initializes a new instance of the System.AttributeUsageAttribute class with
        //     the specified list of System.AttributeTargets, the System.AttributeUsageAttribute.AllowMultiple
        //     value, and the System.AttributeUsageAttribute.Inherited value.
        //
        // Parameters:
        //   validOn:
        //     The set of values combined using a bitwise OR operation to indicate which
        //     program elements are valid.
        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            this.validOn = validOn;
        }

        // Summary:
        //     Gets or sets a Boolean value indicating whether more than one instance of
        //     the indicated attribute can be specified for a single program element.
        //
        // Returns:
        //     true if more than one instance is allowed to be specified; otherwise, false.
        //     The default is false.
        public bool AllowMultiple {
            get { return allowMultiple; }
            set { allowMultiple = value; }
        }
        //
        // Summary:
        //     Gets or sets a Boolean value indicating whether the indicated attribute can
        //     be inherited by derived classes and overriding members.
        //
        // Returns:
        //     true if the attribute can be inherited by derived classes and overriding
        //     members; otherwise, false. The default is true.
        public bool Inherited
        {
            get { return inherited; }
            set { inherited = value; }
        }
        //
        // Summary:
        //     Gets a set of values identifying which program elements that the indicated
        //     attribute can be applied to.
        //
        // Returns:
        //     One or several System.AttributeTargets values. The default is All.
        public AttributeTargets ValidOn { get { return this.validOn; } }
    }
}

namespace System.Runtime.InteropServices
{
    // Summary:
    //     Identifies the type of class interface that is generated for a class.
    [Serializable]
    [ComVisible(true)]
    public enum ClassInterfaceType
    {
        // Summary:
        //     Indicates that no class interface is generated for the class. If no interfaces
        //     are implemented explicitly, the class can only provide late-bound access
        //     through the IDispatch interface. This is the recommended setting for System.Runtime.InteropServices.ClassInterfaceAttribute.
        //     Using ClassInterfaceType.None is the only way to expose functionality through
        //     interfaces implemented explicitly by the class.
        None = 0,
        //
        // Summary:
        //     Indicates that the class only supports late binding for COM clients. A dispinterface
        //     for the class is automatically exposed to COM clients on request. The type
        //     library produced by Type Library Exporter (Tlbexp.exe) does not contain type
        //     information for the dispinterface in order to prevent clients from caching
        //     the DISPIDs of the interface. The dispinterface does not exhibit the versioning
        //     problems described in System.Runtime.InteropServices.ClassInterfaceAttribute
        //     because clients can only late-bind to the interface.
        AutoDispatch = 1,
        //
        // Summary:
        //     Indicates that a dual class interface is automatically generated for the
        //     class and exposed to COM. Type information is produced for the class interface
        //     and published in the type library. Using AutoDual is strongly discouraged
        //     because of the versioning limitations described in System.Runtime.InteropServices.ClassInterfaceAttribute.
        AutoDual = 2,
    }

    // Summary:
    //     Controls accessibility of an individual managed type or member, or of all
    //     types within an assembly, to COM.
    [ComVisible(true)]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public sealed class ComVisibleAttribute : Attribute
    {
        readonly bool visibility;

        // Summary:
        //     Initializes a new instance of the ComVisibleAttribute class.
        //
        // Parameters:
        //   visibility:
        //     true to indicate that the type is visible to COM; otherwise, false. The default
        //     is true.
        public ComVisibleAttribute(bool visibility)
        {
            this.visibility = visibility;
        }

        // Summary:
        //     Gets a value that indicates whether the COM type is visible.
        //
        // Returns:
        //     true if the type is visible; otherwise, false. The default value is true.
        public bool Value { get { return this.visibility; } }
    }
    // Summary:
    //     Indicates the type of class interface to be generated for a class exposed
    //     to COM, if an interface is generated at all.
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
    [ComVisible(true)]
    public sealed class ClassInterfaceAttribute : Attribute
    {
        ClassInterfaceType classInterfaceType;
        // Summary:
        //     Initializes a new instance of the System.Runtime.InteropServices.ClassInterfaceAttribute
        //     class with the specified System.Runtime.InteropServices.ClassInterfaceType
        //     enumeration member.
        //
        // Parameters:
        //   classInterfaceType:
        //     One of the System.Runtime.InteropServices.ClassInterfaceType values that
        //     describes the type of interface that is generated for a class.
        public ClassInterfaceAttribute(ClassInterfaceType classInterfaceType)
        {
            this.classInterfaceType = classInterfaceType;
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Runtime.InteropServices.ClassInterfaceAttribute
        //     class with the specified System.Runtime.InteropServices.ClassInterfaceType
        //     enumeration value.
        //
        // Parameters:
        //   classInterfaceType:
        //     Describes the type of interface that is generated for a class.
        public ClassInterfaceAttribute(short classInterfaceType)
        {
            this.classInterfaceType = (ClassInterfaceType)classInterfaceType;
        }

        // Summary:
        //     Gets the System.Runtime.InteropServices.ClassInterfaceType value that describes
        //     which type of interface should be generated for the class.
        //
        // Returns:
        //     The System.Runtime.InteropServices.ClassInterfaceType value that describes
        //     which type of interface should be generated for the class.
        public ClassInterfaceType Value { get { return classInterfaceType; } }
    }
}
*/