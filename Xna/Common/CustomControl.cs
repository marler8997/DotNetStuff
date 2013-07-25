using System;

using Microsoft.Xna.Framework.Input;

namespace Marler.Xna.Common
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


    public class TrinaryInt32Control
    {
        public Keys subtractKey, addKey;
        public Int32 diff;
        public Int32 min, max;
        public Boolean roll;

        public TrinaryInt32Control(Keys subtractKey, Keys addKey, Int32 diff, Int32 min, Int32 max, Boolean roll)
        {
            this.subtractKey = subtractKey;
            this.addKey = addKey;
            this.diff = diff;
            this.min = min;
            this.max = max;
            this.roll = roll;
        }
        public void Update(KeyboardState keyboardState, ref Int32 input)
        {
            Boolean subtractKeyDown = keyboardState.IsKeyDown(subtractKey);
            Boolean addKeyDown = keyboardState.IsKeyDown(addKey);

            if (subtractKeyDown && !addKeyDown)
            {
                input -= diff;
                if (input < min)
                {
                    input = roll ? max : min;
                }
            }
            else if (!subtractKeyDown && addKeyDown)
            {
                input += diff;
                if (input > max)
                {
                    input = roll ? min : max;
                }
            }
        }
        public Boolean UpdateAndReturnTrueIfChanged(KeyboardState keyboardState, ref Int32 input)
        {
            Boolean subtractKeyDown = keyboardState.IsKeyDown(subtractKey);
            Boolean addKeyDown = keyboardState.IsKeyDown(addKey);

            if (subtractKeyDown && !addKeyDown)
            {
                input -= diff;
                if (input < min)
                {
                    if (roll)
                    {
                        input = max;
                        return true;
                    }
                    if (input + diff > min)
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
                if (input > max)
                {
                    if (roll)
                    {
                        input = min;
                        return true;
                    }
                    if (input - diff < max)
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
        public Keys subtractKey, addKey;
        public float diff;
        public float min,max;

        public TrinaryFloatControl(Keys subtractKey, Keys addKey, float diff, float min, float max)
        {
            this.subtractKey = subtractKey;
            this.addKey = addKey;
            this.diff = diff;
            this.min = min;
            this.max = max;
        }
        public void Update(KeyboardState keyboardState, ref float input)
        {
            Boolean subtractKeyDown = keyboardState.IsKeyDown(subtractKey);
            Boolean addKeyDown      = keyboardState.IsKeyDown(addKey);

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
