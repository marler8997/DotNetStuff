using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace More
{
    [TestClass]
    public class TestOverloadableMethod
    {
        [NpcInterface]
        interface ITestMethods
        {
            void NoArgument();
            void OneArgument(Boolean b);
            void TwoArguments(Boolean b, Int32 i);
            void ThreeArguments(UInt32 u, Int32 i, String str);
        }
        class ClassWithTestMethods : ITestMethods
        {
            public void NoArgument() { }
            public void OneArgument(Boolean b) { }
            public void TwoArguments(Boolean b, Int32 i) { }
            public void ThreeArguments(UInt32 u, Int32 i, String str) { }
        }
        [TestMethod]
        public void RegressionTest()
        {
            ClassWithTestMethods testClass = new ClassWithTestMethods();
            NpcExecutionObject executionObject = new NpcExecutionObject(testClass);


            NpcMethodInfo[] methodArray = new NpcMethodInfo[4];

            methodArray[0] = new NpcMethodInfo(executionObject.type.GetMethod("NoArgument"));
            NpcMethodOverloadable methods = new NpcMethodOverloadable(executionObject, methodArray[0]);

            methodArray[1] = new NpcMethodInfo(executionObject.type.GetMethod("OneArgument"));
            methods.AddOverload(methodArray[1]);

            methodArray[2] = new NpcMethodInfo(executionObject.type.GetMethod("TwoArguments"));
            methods.AddOverload(methodArray[2]);

            methodArray[3] = new NpcMethodInfo(executionObject.type.GetMethod("ThreeArguments"));
            methods.AddOverload(methodArray[3]);

            Int32 index = 0;
            foreach (NpcMethodInfo method in methods)
            {
                Assert.AreSame(methodArray[index], method, String.Format("Index {0} method={1}, methodArray={2}", index, method.methodInfo, methodArray[index]));
                index++;
            }
        }
        [NpcInterface]
        interface ITwoMethodsSameParameterCount
        {
            void Method(Boolean b);
            void Method(Char c);
        }
        class ClassWithTwoMethodsSameParameterCount : ITwoMethodsSameParameterCount
        {
            public void Method(Boolean b) { }
            public void Method(Char c) { }
        }
        [TestMethod]
        public void TestOverloadsWithSameParameterCount()
        {
            ClassWithTwoMethodsSameParameterCount testClass = new ClassWithTwoMethodsSameParameterCount();
            NpcExecutionObject executionObject = new NpcExecutionObject(testClass);

            NpcMethodOverloadable methods = new NpcMethodOverloadable(executionObject,
                new NpcMethodInfo(executionObject.type.GetMethod("Method", new Type[] { typeof(Boolean) })));

            try
            {
                methods.AddOverload(new NpcMethodInfo(executionObject.type.GetMethod("Method", new Type[] { typeof(Char) })));
                Assert.Fail("Expected NotSupportedException");
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine("Caught expected exception {0}", e.Message);
            }
        }
    }
}
