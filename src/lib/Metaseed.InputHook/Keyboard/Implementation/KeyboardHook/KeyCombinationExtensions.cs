// This code is distributed under MIT license.
// Copyright (c) 2010-2018 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections.Generic;
using System.Linq;
using Metaseed.Input;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook;

namespace Metaseed.KeyboardHook
{

    using Combination = Metaseed.Input.MouseKeyHook.Combination;
    using Sequence = Metaseed.Input.MouseKeyHook.Sequence;
    /// <summary>
    ///     Extension methods to detect key combinations
    /// </summary>
    public static class KeyCombinationExtensions
    {
        /// <summary>
        ///     Detects a key or key combination and triggers the corresponding action.
        /// </summary>
        /// <param name="source">
        ///     An instance of Global or Application hook. Use <see cref="Hook.GlobalEvents" /> or <see cref="Hook.AppEvents" /> to
        ///     create it.
        /// </param>
        /// <param name="map">
        ///     This map contains the list of key combinations mapped to corresponding actions. You can use a dictionary initilizer
        ///     to easily create it.
        ///     Whenever a listed combination will be detected a corresponding action will be triggered.
        /// </param>
        /// <param name="reset">
        ///     This optional action will be executed when some key was pressed but it was not part of any wanted combinations.
        /// </param>
        public static void ProcessCombination(this IKeyboardEvents source,
            IEnumerable<KeyValuePair<Combination, Action<Metaseed.Input.KeyEventArgsExt>>> map, Action<KeyEventArgsExt> reset = null)
        {
            var watchlists = map.GroupBy(k => k.Key.TriggerKey)
                .ToDictionary(g => g.Key, g => g.ToArray());
            source.KeyDown += (sender, e) =>
            {
                KeyValuePair<Combination, Action<Input.KeyEventArgsExt>>[] element;
                var found = watchlists.TryGetValue(e.KeyCode, out element);
                if (!found)
                {
                    reset?.Invoke(e as KeyEventArgsExt);
                    return;
                }

                var state = KeyboardState.GetCurrent();
                Action<Input.KeyEventArgsExt> action = null;
                var maxLength = 0;
                foreach (var current in element)
                {
                    var matches = current.Key.Chord.All(state.IsDown);
                    if (!matches) continue;
                    if (maxLength > current.Key.ChordLength) continue;
                    maxLength = current.Key.ChordLength;
                    action = current.Value;
                }

                if (action == null)
                    reset?.Invoke(e as KeyEventArgsExt);
                else
                    action.Invoke(e as KeyEventArgsExt);
            };
        }


        /// <summary>
        ///     Detects a key or key combination sequence and triggers the corresponding action.
        /// </summary>
        /// <param name="source">
        ///     An instance of Global or Application hook. Use <see cref="Hook.GlobalEvents" /> or
        ///     <see cref="Hook.AppEvents" /> to create it.
        /// </param>
        /// <param name="map">
        ///     This map contains the list of sequences mapped to corresponding actions. You can use a dictionary initilizer to
        ///     easily create it.
        ///     Whenever a listed sequnce will be detected a corresponding action will be triggered. If two or more sequences match
        ///     the longest one will be used.
        ///     Example: sequences may A,B,C and B,C might be detected simultanously if user pressed first A then B then C. In this
        ///     case only action corresponding
        ///     to 'A,B,C' will be triggered.
        /// </param>
        public static void ProcessSequence(this IKeyboardEvents source, IEnumerable<KeyValuePair<Sequence, Action<Metaseed.Input.KeyEventArgsExt>>> map)
        {
            var actBySeq = map.ToArray();
            var endsWith = new Func<Queue<Combination>, Sequence, bool>((chords, sequence) =>
            {
                var skipCount = chords.Count - sequence.Length;
                return skipCount >= 0 && chords.Skip(skipCount).SequenceEqual(sequence);
            });

            var max = actBySeq.Select(p => p.Key).Max(c => c.Length);
            var min = actBySeq.Select(p => p.Key).Min(c => c.Length);
            var buffer = new Queue<Combination>(max);

            var wrapMap = actBySeq.SelectMany(p => p.Key).Select(c => new KeyValuePair<Combination, Action<Input.KeyEventArgsExt>>(c, e =>
            {
                buffer.Enqueue(c);
                if (buffer.Count > max) buffer.Dequeue();
                if (buffer.Count < min) return;
                //Invoke action corresponding to the longest matching sequence
                actBySeq
                    .Where(pair => endsWith(buffer, pair.Key))
                    .OrderBy(pair => pair.Key.Length)
                    .Select(pair => pair.Value)
                    .LastOrDefault()
                    ?.Invoke(e);
            }));

            ProcessCombination(source, wrapMap, _ => buffer.Clear());
        }
    }
}