﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Metatool.Service;

public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
}

// System.Collections.Specialized.OrderedDictionary is NOT generic.
// This class is essentially a generic wrapper for OrderedDictionary.
public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
{
	private readonly OrderedDictionary _internalDictionary;

	public OrderedDictionary()
	{
		_internalDictionary = new OrderedDictionary();
	}

	public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
	{
		if (dictionary == null) return;
		_internalDictionary = new OrderedDictionary();

		foreach (var pair in dictionary)
		{
			_internalDictionary.Add(pair.Key, pair.Value);
		}
	}

	public int Count => _internalDictionary.Count;

	public bool IsReadOnly => false;

	public TValue this[TKey key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (_internalDictionary.Contains(key))
			{
				return (TValue) _internalDictionary[(object) key];
			}
			else
			{
				throw new KeyNotFoundException("Cannot find key " + key);
			}
		}

		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			_internalDictionary[(object) key] = value;
		}
	}

	public ICollection<TKey> Keys
	{
		get
		{
			var keys = new List<TKey>(_internalDictionary.Count);
			keys.AddRange(_internalDictionary.Keys.Cast<TKey>());

			// Keys should be put in a ReadOnlyCollection,
			// but since this is an internal class, for performance reasons,
			// we choose to avoid creating yet another collection.

			return keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			var values = new List<TValue>(_internalDictionary.Count);
			values.AddRange(_internalDictionary.Values.Cast<TValue>());

			// Values should be put in a ReadOnlyCollection,
			// but since this is an internal class, for performance reasons,
			// we choose to avoid creating yet another collection.

			return values;
		}
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		_internalDictionary.Add(key, value);
	}

	public void Clear()
	{
		_internalDictionary.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if ((item.Key == null) || !(_internalDictionary.Contains(item.Key)))
		{
			return false;
		}
		else
		{
			return _internalDictionary[(object) item.Key].Equals(item.Value);
		}
	}

	public bool ContainsKey(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		return _internalDictionary.Contains(key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}

		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}

		if ((array.Rank                  > 1)             ||
		    (arrayIndex                  >= array.Length) ||
		    ((array.Length - arrayIndex) < _internalDictionary.Count))
		{
			throw new Exception("Fx.Exception.Argument('array', SRCore.BadCopyToArray)");
		}

		int index = arrayIndex;

		foreach (DictionaryEntry entry in _internalDictionary)
		{
			array[index] = new KeyValuePair<TKey, TValue>((TKey) entry.Key, (TValue) entry.Value);
			++index;
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		foreach (DictionaryEntry entry in _internalDictionary)
		{
			yield return new KeyValuePair<TKey, TValue>((TKey) entry.Key, (TValue) entry.Value);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (Contains(item))
		{
			_internalDictionary.Remove(item.Key);
			return true;
		}

		return false;
	}

	public bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		if (_internalDictionary.Contains(key))
		{
			_internalDictionary.Remove(key);
			return true;
		}

		return false;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		var keyExists = _internalDictionary.Contains(key);
		value = keyExists ? (TValue) _internalDictionary[(object) key] : default(TValue);

		return keyExists;
	}
}