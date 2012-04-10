using System;
using System.Collections.Generic;
using System.IO;

namespace Marler.Hmd
{

    public class HmdIDCounter
    {
        public readonly HmdIDProperties idProperties;
        private UInt32 count;
        public UInt32 Count { get { return count; } }

        public HmdIDCounter(HmdIDProperties idProperties)
        {
            this.idProperties = idProperties;
            this.count = 1;
        }

        public void Increment()
        {
            count++;
        }

        public void Validate()
        {
            if (!idProperties.CountProperty.IsValidCount(count))
            {
                throw new FormatException(String.Format("\"{0}\" appeared {1} times, but it's count property is {2}",
                    idProperties.idOriginalCase, count, idProperties.CountProperty));
            }

        }
    }

    public class HmdBlockValidator
    {
        public readonly HmdBlockIDProperties blockIDProperties;
        public List<HmdIDCounter> childrenCountList;

        public HmdBlockValidator(HmdBlockIDProperties blockIDProperties)
        {
            this.blockIDProperties = blockIDProperties;
        }

        public void NewChild(HmdIDProperties idProperties)
        {
            if (childrenCountList == null)
            {
                childrenCountList = new List<HmdIDCounter>();
            }
            for (int i = 0; i < childrenCountList.Count; i++)
            {
                if (childrenCountList[i].idProperties == idProperties)
                {
                    childrenCountList[i].Increment();
                    return;
                }
            }
            childrenCountList.Add(new HmdIDCounter(idProperties));
        }

        public void ValidateChildren(HmdProperties hmdProperties)
        {
            //
            // Validate Children Counts (make sure there's not too many)
            //
            if (childrenCountList != null)
            {
                for (int i = 0; i < childrenCountList.Count; i++)
                {
                    childrenCountList[i].Validate();
                }
            }

            //
            // Make sure there are none missing
            //

            foreach (HmdIDProperties childIDProperties in blockIDProperties)
            {
                if(childIDProperties.CountProperty.MinCount > 0)
                {
                    Boolean childAppears = false;
                    for(int i = 0; i < childrenCountList.Count; i++)
                    {
                        if(childIDProperties.idLowerCase.Equals(childrenCountList[i].idProperties.idLowerCase, StringComparison.CurrentCultureIgnoreCase))
                        {
                            childAppears = true;
                            break;
                        }
                    }

                    if (!childAppears)
                    {
                        throw new FormatException(String.Format("Block \"{0}\" is missing the \"{1}\" ID",
                            blockIDProperties.idOriginalCase, childIDProperties.idOriginalCase));
                    }
                }

            }
        }

    }

    public class HmdValidator
    {
        private HmdBlockValidator currentBlock;
        private Stack<HmdBlockValidator> blockStack;
        private TextWriter debugOutput;

        public HmdValidator()
        {
        }

        public static void ValidateStatic(HmdBlockID root, HmdProperties hmdProperties)
        {
            new HmdValidator().Validate(root, hmdProperties);
        }

        public void Validate(HmdBlockID root, HmdProperties hmdProperties)
        {
            debugOutput = HmdDebug.DebugOutput;
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            if (hmdProperties == null)
            {
                throw new ArgumentNullException("hmdProperties");
            }

            hmdProperties.ResolveChildParentReferences();

            debugOutput.WriteLine("[Validating HMD file...]");

            hmdProperties.PrintEnums(debugOutput);

            currentBlock = null;
            blockStack = new Stack<HmdBlockValidator>();

            ValidateBlockID(root, hmdProperties.root, hmdProperties);

            debugOutput.WriteLine("[Done validating HMD file]");
        }

        private void ValidateValueID(HmdValueID valueID, HmdValueIDProperties valueIDProperties, HmdProperties hmdProperties)
        {
            //
            // Check that the current parent is valid
            //
            debugOutput.Write(blockStack.Count, "Checking that \"{0}\" has \"{1}\" as a valid parent...", valueID.idOriginalCase, currentBlock.blockIDProperties.idOriginalCase);
            if (!valueIDProperties.IsValidParent(currentBlock.blockIDProperties))
            {
                throw new FormatException(String.Format("Value ID \"{0}\" appeared in Block \"{1}\", but this is not allowed with the current properties",
                    valueID.idOriginalCase, currentBlock.blockIDProperties.idOriginalCase));
            }
            debugOutput.WriteLine("Pass.");

            if (valueID.value != null)
            {
                debugOutput.Write(blockStack.Count, "Checking the Value Type for \"{0}\"...", valueID.idOriginalCase);
                if (!valueIDProperties.IsValidValue(valueID.value, hmdProperties))
                {
                    throw new FormatException(String.Format("Value ID \"{0}\" of type {1}, had an invalid value of \"{2}\"",
                        valueID.idOriginalCase, valueIDProperties.hmdType.ToHmdTypeString(), valueID.value));
                }
                debugOutput.WriteLine("Pass.");
            }


            // Add ID to current block validator
            currentBlock.NewChild(valueIDProperties);
        }

        private void ValidateBlockID(HmdBlockID blockID, HmdBlockIDProperties blockIDProperties, HmdProperties hmdProperties)
        {
            //
            // Check that the current parent is valid
            //
            if (currentBlock == null)
            {
                debugOutput.Write(blockStack.Count, "Checking that \"{0}\" is the root...", blockID.idOriginalCase);
                if (!blockIDProperties.IsRoot)
                {
                    throw new FormatException(String.Format("Block ID \"{0}\" was expected to be the root, but it wasn't?",
                        blockIDProperties.idOriginalCase));

                }
                debugOutput.WriteLine("Pass.");
            }
            else
            {
                debugOutput.Write(blockStack.Count, "Checking that \"{0}\" has \"{1}\" as a valid parent...", blockID.idOriginalCase, currentBlock.blockIDProperties.idOriginalCase);
                if (!blockIDProperties.IsValidParent(currentBlock.blockIDProperties))
                {
                    throw new FormatException(String.Format("Block ID \"{0}\" appeared in Block \"{1}\", but this is not allowed with the current properties",
                        blockIDProperties.idOriginalCase, currentBlock.blockIDProperties.idOriginalCase));
                }
                debugOutput.WriteLine("Pass.");

                // Add ID to current block validator
                currentBlock.NewChild(blockIDProperties);
            }


            //
            // Verify the Children of the Block ID
            //
            debugOutput.WriteLine(blockStack.Count, "{");

            blockStack.Push(currentBlock);
            currentBlock = new HmdBlockValidator(blockIDProperties);

            blockID.Iterate(hmdProperties, ValidateValueID, ValidateBlockID);

            // Validate Children
            debugOutput.Write(blockStack.Count, "Checking counts of all children for \"{0}\"...", blockID.idOriginalCase);
            currentBlock.ValidateChildren(hmdProperties);
            debugOutput.WriteLine("Pass.");

            currentBlock = blockStack.Pop();
            debugOutput.WriteLine(blockStack.Count, "}} (end of \"{0}\")", blockID.idOriginalCase);
        }

    }
}
