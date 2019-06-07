﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.Input;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class CombinationExtetions
    {
        public static IDisposable Down(this ICombination combination, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            return Keyboard.Add(combination, new KeyAction(actionId,description,action));
        }
        
        public static IDisposable Up(this ICombination combination, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            return Keyboard.Add(new Combination(combination.TriggerKey,combination.Chord,KeyEventType.Up), new KeyAction(actionId,description,action));
        }
        public static IDisposable Map(this ICombination key, Keys target, Predicate<ICombination> predicate = null)
        {
            return Keyboard.Map(key as Combination, new Combination(target), predicate);
        }
        public static IDisposable Map(this ICombination key, ICombination target, Predicate<ICombination> predicate = null)
        {
            return Keyboard.Map(key as Combination, target, predicate);
        }
    }
}
