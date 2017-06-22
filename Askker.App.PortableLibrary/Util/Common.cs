using System;
using System.Collections.Generic;

namespace Askker.App.PortableLibrary.Util
{
    public static class Common
    {
        public static List<T> Randomize<T>(List<T> list)
        {
            List<T> randomizedList = new List<T>();
            Random rnd = new Random();
            while (list.Count > 0)
            {
                int index = rnd.Next(0, list.Count); //pick a random item from the master list
                randomizedList.Add(list[index]); //place it at the end of the randomized list
                list.RemoveAt(index);
            }
            return randomizedList;
        }

        public static string FormatNumberAbbreviation(long number)
        {
            if(number <= 0)
            {
                return "0";
            }
            // Ensure number has max 3 significant digits (no rounding up can happen)
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(number) - 2));
            number = number / i * i;

            if (number >= 1000000000)
                return (number / 1000000000D).ToString("0.##") + "B";
            if (number >= 1000000)
                return (number / 1000000D).ToString("0.##") + "M";
            if (number >= 1000)
                return (number / 1000D).ToString("0.##") + "K";

            return number.ToString("#,0");
        }
    }
}
