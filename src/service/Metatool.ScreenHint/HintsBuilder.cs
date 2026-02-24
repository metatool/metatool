using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using Metatool.Service;
using Metatool.UIElementsDetector;

namespace Metatool.ScreenPoint;

public class HintsBuilder : IHintsBuilder
{
	public string HintKeys { get; set;} = "ASDFQWERZXCVTGBHJKLYUIOPNM";

	private Dictionary<string, IUIElement> GetKeyPointPairs(List<IUIElement> rects, string keyChars)
	{
		var keyPointPairs = new Dictionary<string, IUIElement>();

		var count = rects.Count;
		var keyLen = keyChars.Length;
		var dimensions = (int)Math.Ceiling(Math.Log(count, keyLen));

        var lowDimCount = 0;
        var usedInLowDim = 0;
        var notUsedInLowDim = 0;
        if (dimensions > 1)
        {
            lowDimCount = (int)Math.Pow(keyLen, dimensions - 1);
            usedInLowDim = (int)Math.Ceiling(((double)(count - lowDimCount)) / (dimensions - 1));//(int) Math.Ceiling(((double) count) / lowDimCount);
            notUsedInLowDim = lowDimCount - usedInLowDim;
        }


		static string getKeyOfDimension(int index, int dimension, string keys)
		{
			var ii = index;
			var len = keys.Length;
			var sb = new StringBuilder();
			do
			{
				var i = ii % len;
				sb.Insert(0, keys[i]);
				ii = ii / len;
			} while (ii > 0);

			var r = sb.ToString();
			return r.PadLeft(dimension, keys[0]);
		}

		string getKey(int index)
		{
			if (index < notUsedInLowDim)
			{
				return getKeyOfDimension(index + usedInLowDim, dimensions - 1, keyChars);
			}

			return getKeyOfDimension(index - notUsedInLowDim, dimensions, keyChars);
		}

		for (var i = 0; i < count; i++)
		{
			var key = getKey(i);
			keyPointPairs.Add(key, rects[i]);
		}

		return keyPointPairs;
	}


	public Dictionary<string, IUIElement> GenerateKeys(List<IUIElement> elementRects)
	{
#if DEBUG
		var w = new Stopwatch();
		w.Start();
#endif
		var eles = GetKeyPointPairs(elementRects, HintKeys);
#if DEBUG
		Debug.WriteLine("GetKeyPointPairs:" + w.ElapsedMilliseconds);
#endif
		return eles;
	}
}