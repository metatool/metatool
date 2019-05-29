using System;
using System.Collections.Generic;

namespace Metaseed.DataStructures
{
    /// <summary>
    /// Interface to be implemented by a data structure 
    /// which allows adding values <see cref="TValue"/> associated with keys.
    /// The interface allows retrieveal of multiple values 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface ITrie<TKey,TValue>
    {
        IEnumerable<TValue> Get(IList<TKey> query);
        void Add(IList<TKey> query, TValue value);
        bool Remove(IList<TKey> query, Predicate<TValue> predicate);
        bool Remove(IList<TKey> query);

    }
}