using System;

using OpenTK.Input;

namespace More.OpenTK
{
    public abstract class CustomControl
    {
        public enum Type
        {
            Pulse         = 0,
            BinarySwitch  = 1,
            TrinarySwitch = 2,
        }

        public readonly Type type;

        public CustomControl(Type type)
        {
            this.type = type;
        }
    }



    public class TrinaryControl
    {
        public Key negativeKey, positiveKey;
        public TrinaryControl(Key negativeKey, Key positiveKey)
        {
            this.negativeKey = negativeKey;
            this.positiveKey = positiveKey;
        }
        // return negative for 
        public Int32 GetControl(KeyboardDevice keyboard)
        {
            Boolean negativeKeyDown = keyboard[negativeKey];
            Boolean positiveKeyDown = keyboard[positiveKey];

            if (positiveKeyDown && !negativeKeyDown)
            {
                return 1;
            }
            if (negativeKeyDown && !positiveKeyDown)
            {
                return -1;
            }
            return 0;
        }
    }


    public class TrinaryInt32Control
    {
        public Key subtractKey, addKey;
        public Int32 min, max;
        public Boolean roll;

        public TrinaryInt32Control(Key subtractKey, Key addKey, Int32 min, Int32 max, Boolean roll)
        {
            this.subtractKey = subtractKey;
            this.addKey = addKey;
            this.min = min;
            this.max = max;
            this.roll = roll;
        }
        public void Update(KeyboardDevice keyboard, ref Int32 input)
        {
            Boolean subtractKeyDown = keyboard[subtractKey];
            Boolean addKeyDown = keyboard[addKey];

            if (subtractKeyDown && !addKeyDown)
            {
                input--;
                if (input < min)
                {
                    input = roll ? max : min;
                }
            }
            else if (!subtractKeyDown && addKeyDown)
            {
                input++;
                if (input > max)
                {
                    input = roll ? min : max;
                }
            }
        }
        public Boolean UpdateAndReturnTrueIfChanged(KeyboardDevice keyboard, ref Int32 input)
        {
            Boolean subtractKeyDown = keyboard[subtractKey];
            Boolean addKeyDown = keyboard[addKey];

            if (subtractKeyDown && !addKeyDown)
            {
                input--;
                if (input < min)
                {
                    if (roll)
                    {
                        input = max;
                        return true;
                    }
                    if (input + 1 > min)
                    {
                        input = min;
                        return true;
                    }
                    input = min;
                    return false;
                }

                return true;
            }
            if (!subtractKeyDown && addKeyDown)
            {
                input++;
                if (input > max)
                {
                    if (roll)
                    {
                        input = min;
                        return true;
                    }
                    if (input - 1 < max)
                    {
                        input = max;
                        return true;
                    }
                    input = max;
                    return false;
                }

                return true;
            }
            return false;
        }
    }


    public class TrinaryFloatControl
    {
        public Key subtractKey, addKey;
        public float diff;
        public float min,max;

        public TrinaryFloatControl(Key subtractKey, Key addKey, float diff, float min, float max)
        {
            this.subtractKey = subtractKey;
            this.addKey = addKey;
            this.diff = diff;
            this.min = min;
            this.max = max;
        }
        public void Update(KeyboardDevice keyboard, ref float input)
        {
            Boolean subtractKeyDown = keyboard[subtractKey];
            Boolean addKeyDown = keyboard[addKey];

            if (subtractKeyDown && !addKeyDown)
            {
                input -= diff;
                if(input < min)
                {
                    input = min;
                }
            }
            else if (!subtractKeyDown && addKeyDown)
            {
                input += diff;
                if(input > max)
                {
                    input = max;
                }
            }
        }
        public Boolean UpdateAndReturnTrueIfChanged(KeyboardState keyboardState, ref float input)
        {
            Boolean subtractKeyDown = keyboardState.IsKeyDown(subtractKey);
            Boolean addKeyDown      = keyboardState.IsKeyDown(addKey);

            if (subtractKeyDown && !addKeyDown)
            {
                input -= diff;
                if(input < min)
                {
                    if(input + diff > min)
                    {
                        input = min;
                        return true;
                    }
                    input = min;
                    return false;
                }

                return true;
            }
            if (!subtractKeyDown && addKeyDown)
            {
                input += diff;

                if(input > max)
                {
                    if(input - diff < max)
                    {
                        input = max;
                        return true;
                    }
                    input = max;
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
