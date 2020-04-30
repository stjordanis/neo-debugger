﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Neo.VM;
using Neo.VM.Types;
using NeoFx;
using Newtonsoft.Json.Linq;

namespace NeoDebug
{
    static class Extensions
    {
        public static ImmutableArray<string> GetReturnTypes(this LaunchArguments arguments)
        {
            IEnumerable<string> GetReturnTypes()
            {
                if (arguments.ConfigurationProperties.TryGetValue("return-types", out var returnTypes))
                {
                    foreach (var returnType in returnTypes)
                    {
                        yield return Helpers.CastOperations[returnType.Value<string>()];
                    }
                }
            }

            return GetReturnTypes().ToImmutableArray();
        }

        public static bool TryPopInterface<T>(this RandomAccessStack<StackItem> stack, [NotNullWhen(true)] out T? value)
            where T : class
        {
            if (stack.Pop() is InteropInterface @interface)
            {
                var t = @interface.GetInterface<T>();
                if (t != null)
                {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryPopAdapter<T>(this RandomAccessStack<StackItem> stack, [NotNullWhen(true)] out T? adapter)
            where T : ModelAdapters.AdapterBase
        {
            if (stack.Pop() is T _adapter)
            {
                adapter = _adapter;
                return true;
            }
            adapter = null;
            return false;
        }

        public static bool TryAdapterOperation<T>(this ExecutionEngine engine, Func<T, bool> func)
            where T : ModelAdapters.AdapterBase
        {
            if (engine.CurrentContext.EvaluationStack.TryPopAdapter<T>(out var adapter))
            {
                return func(adapter);
            }
            return false;
        }

        public static bool TryToArray(this UInt160 uInt160, [NotNullWhen(true)] out byte[]? value)
        {
            var buffer = new byte[UInt160.Size];

            if (uInt160.TryWrite(buffer))
            {
                value = buffer;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryToArray(this UInt256 uInt256, [NotNullWhen(true)] out byte[]? value)
        {
            var buffer = new byte[UInt256.Size];

            if (uInt256.TryWrite(buffer))
            {
                value = buffer;
                return true;
            }

            value = default;
            return false;
        }

        public delegate StackItem WrapStackItem<T>(in T item);

        public static StackItem[] WrapStackItems<T>(this ReadOnlyMemory<T> memory, WrapStackItem<T> wrapItem)
        {
            var items = new StackItem[memory.Length];
            for (int i = 0; i < memory.Length; i++)
            {
                items[i] = wrapItem(memory.Span[i]);
            }
            return items;
        }
    }
}
