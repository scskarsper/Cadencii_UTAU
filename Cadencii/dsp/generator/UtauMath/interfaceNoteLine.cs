using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.dsp.generator.UtauMath
{

    public interface interfaceNoteLine
    {
        double getPoint(double X);
        double getPointUnLimit(double X);
        bool isInArea(double X);
    }
    public class NoteLineClass
    {
        public static interfaceNoteLine getNoteLineMath(double startX, double endX, double startY, double endY)
        {
            return new DirectLine(startX,endX,startY,endY);
        }
    }
    public class RiseFallLine : interfaceNoteLine
    {
        double basel = 0;
        double startX = 0;
        double endX = 0;
        double startLater = 0;

        double a = 0;
        double b = 0;

        public RiseFallLine(double startX, double endX, double BaseLine, double Top)
        {
            this.startX = startX;
            this.endX = endX;
            this.basel = BaseLine;

            double k = Top;
            double l = endX - startX;
            double m = l / 2;
            a = k / (m * m - l * m);
            b = -a * l;
        }
        public void setAreaEnd(double endX)
        {
            this.endX = endX;
        }
        public void setAreaStartLater(double StartLaterLen)
        {
            this.startLater = StartLaterLen;
        }
        public bool isInArea(double X)
        {
            if (X < startX) return false;
            if (X >= endX + startLater) return false;
            return true;
        }
        public double getPoint(double X)
        {
            if (X < startX) return basel;
            if (X > endX + startLater) return basel;
            double x = X - startX-startLater;
            double y = a * x * x + b * x;
            return y + basel;
            //return a*(X-m)+k;
        }
        public double getPointUnLimit(double X)
        {
            double x = X - startX-startLater;
            double y = a * x * x + b * x;
            return y + basel;
            //return a*(X-m)+k;
        }
    }
    public class DirectLine:interfaceNoteLine
    {
        double a = 0;
        double b = 0;
        double startX = 0;
        double endX = 0;
        double startY = 0;
        double endY = 0;
        public DirectLine(double startX, double endX, double startY, double endY)
        {
            this.startX = startX;
            this.endX = endX;
            this.startY = startY;
            this.endY = endY;
            a = (startY - endY) / (startX - endX);
            b = endY - a * (endX);
        }
        public bool isInArea(double X)
        {
            if (X < startX) return false;
            if (X > endX) return false;
            return true;
        }
        public double getPoint(double X)
        {
            if (X < startX) return startY;
            if (X>endX) return endY;
            return a * X + b;
        }
        public double getPointUnLimit(double X)
        {
            return a * X + b;
        }
    }
}
