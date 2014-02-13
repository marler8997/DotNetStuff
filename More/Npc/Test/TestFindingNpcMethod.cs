using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RegressionTest()
        {
            NpcReflector executor = new NpcReflector(new Object[] {
                new DirectNpcAttribute.Class(),
                new NpcAttributeFromParent.Class(),
                new NpcAttributeFromComplexInterface.Class(),
                new NpcAttributeFromParentInterfaceParent.Class(),
            });

            executor.PrintInformation(Console.Out);
        }
    }
    namespace DirectNpcAttribute
    {
        [NpcInterface]
        interface ClassNpcInterface
        {
            void DirectNpcMethod();
        }
        class Class : ClassNpcInterface
        {
            public void DirectNpcMethod()
            {
            }
        }
    }
    namespace NpcAttributeFromParent
    {
        [NpcInterface]
        interface ParentNpcInterface
        {
            void NpcAttributeFromParentMethod();
        }
        class Parent : ParentNpcInterface
        {
            public void NpcAttributeFromParentMethod()
            {
            }
        }
        class Class : Parent
        {
        }
    }
    namespace NpcAttributeFromComplexInterface
    {
        [NpcInterface]
        interface BaseInterface
        {
            void NpcAttributeFromComplexInterfaceMethod();
        }
        interface ChildInterface : BaseInterface
        {
        }
        class Class : ChildInterface
        {
            public void NpcAttributeFromComplexInterfaceMethod()
            {
            }
        }
    }
    namespace NpcAttributeFromParentInterfaceParent
    {
        [NpcInterface]
        interface BaseInterface
        {
            void NpcAttributeFromParentInterfaceParentMethod();
        }
        interface ChildInterface : BaseInterface
        {
        }
        abstract class Parent : ChildInterface
        {
            public abstract void NpcAttributeFromParentInterfaceParentMethod();
        }
        class Class : Parent
        {
            public override void NpcAttributeFromParentInterfaceParentMethod()
            {
            }
        }
    }
}
