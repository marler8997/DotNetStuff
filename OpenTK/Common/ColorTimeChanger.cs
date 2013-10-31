using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK.Graphics;

namespace More.OpenTK
{
    public class ColorTimeChanger
    {
        Color4 currentColor;

        Color4 changeColor;
        Int64 changeColorStartTimeMicros;
        Int32 changeColorDurationMicros;



        public ColorTimeChanger(Color4 initialColor)
        {
            this.currentColor = initialColor;
            this.changeColorStartTimeMicros = 0;
        }


        public Boolean InColorChange()
        {
            return changeColorStartTimeMicros != 0;
        }
        /*
        public Int32 TimeLeftInColorChange(Int64 nowMicros)
        {
            if (changeColorStartTimeMicros <= 0) return 0;
            return (Int32)(nowMicros + changeColorStartTimeMicros - changeColorStartTimeMicros);
        }
        */
        public void SetColorChange(Int64 startTimeMicros, Int32 durationMicros, Color4 newColor)
        {
            this.changeColor = newColor;
            this.changeColorStartTimeMicros = startTimeMicros;
            this.changeColorDurationMicros = durationMicros;
        }
        public Color4 GetColor(Int64 nowMicros)
        {
            if (InColorChange())
            {
                Int64 diffStartTime = nowMicros - changeColorStartTimeMicros;
                if (diffStartTime > 0)
                {
                    Int32 timeLeftInColorChange = (Int32)(changeColorDurationMicros - diffStartTime);
                    if (timeLeftInColorChange <= 0)
                    {
                        changeColorStartTimeMicros = 0;
                        currentColor = changeColor;
                    }
                    else
                    {
                        float diffRed = changeColor.R - currentColor.R;
                        float diffGreen = changeColor.G - currentColor.G;
                        float diffBlue = changeColor.B - currentColor.B;

                        float colorChangePercentage = 1 - ((float)(100 * timeLeftInColorChange / changeColorDurationMicros) / 100);

                        return new Color4(
                            (currentColor.R + (diffRed * colorChangePercentage)),
                            (currentColor.G + (diffGreen * colorChangePercentage)),
                            (currentColor.B + (diffBlue * colorChangePercentage)),
                            0);
                    }
                }
            }
            return currentColor;
        }

    }
}
