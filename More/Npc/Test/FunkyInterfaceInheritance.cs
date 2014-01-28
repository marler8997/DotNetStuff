using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

[NpcInterface]
interface IRoot
{
    void RootMethod();
}
interface ISubRoot : IRoot
{
    void SubRootMethod();
}

class InheritSubRoot : ISubRoot
{
    public void RootMethod() { }
    public void SubRootMethod() { }
}
class InheritRootAndSubRoot : IRoot, ISubRoot
{
    public void RootMethod() { }
    public void SubRootMethod() { }
}

[TestClass]
public class FunkyInterfaceInheritance
{
    [TestMethod]
    public void TestMethod1()
    {
        NpcExecutionObject executionObject = new NpcExecutionObject(new InheritSubRoot());
    }
    [TestMethod]
    public void TestMethod2()
    {
        NpcExecutionObject executionObject = new NpcExecutionObject(new InheritRootAndSubRoot());
    }
}