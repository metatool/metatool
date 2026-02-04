using System;
using System.Collections.Generic;
using System.Windows;

namespace KeyMouse
{
    public static class KeyGenerator
    {
        public static Dictionary<string, Rect> GetKeyPointPairs(List<Rect> rects, string keys)
        {
            var keyPointPairs = new Dictionary<string, Rect>();

            var count = rects.Count;
            if (count == 0) return keyPointPairs;

            int N = keys.Length;
            int length = 1;
            long capacity = N;
            while (capacity < count)
            {
                length++;
                capacity *= N;
            }

            int i = 0;
            foreach (var rect in rects)
            {
                string key = GenerateKey(i++, keys, N, length);
                keyPointPairs[key] = rect;
            }
            return keyPointPairs;
        }

        private static string GenerateKey(int index, string keys, int N, int length)
        {
            char[] buffer = new char[length];
            int temp = index;
            for (int j = length - 1; j >= 0; j--)
            {
                buffer[j] = keys[temp % N];
                temp /= N;
            }

            return new string(buffer);
        }
    }
}