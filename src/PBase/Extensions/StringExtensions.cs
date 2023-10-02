﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PBase.Extensions
{
    public static class StringExtensions
    {
        public static string PadCenter(this string str, int length)
        {
            int spaces = length - str.Length;
            int padLeft = spaces / 2 + str.Length;
            return str.PadLeft(padLeft).PadRight(length);
        }
    }
}
