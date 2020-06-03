using System;

namespace PBase.Utility
{
    public static class MathHelper
    {
        /// <summary>Clamps a value to a defined range, setting the value to the nearest available value if out of range.</summary>
        /// <param name="val">The value to be clamped.</param>
        /// <param name="min">The minimum bound of the defined range.</param>
        /// <param name="max">The maximum bound of the defined range.</param>
        /// <returns>The value clamped within the defined range.</returns>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if(max.CompareTo(min) < 0) 
                throw new System.ArgumentException("The maximum (max) cannot be less than the minimum (min)");

            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }
    }
}