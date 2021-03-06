﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using InlineIL;
using JetBrains.Annotations;
using static InlineIL.IL.Emit;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Utils
{
    internal static class TypeUtil
    {
        private static readonly Func<IntPtr, Type> _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

        public static IntPtr GetTypeHandleSlow(Type type)
            => type?.TypeHandle.Value ?? IntPtr.Zero;

        public static Type GetTypeFromHandle(IntPtr typeHandle)
            => _getTypeFromHandleFunc?.Invoke(typeHandle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<TTo>();
        }

        private static Func<IntPtr, Type> BuildGetTypeFromHandleFunc()
        {
            var method = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(IntPtr) }, null);
            if (method == null)
                return null;

            var param = Parameter(typeof(IntPtr));

            return Lambda<Func<IntPtr, Type>>(
                Call(method, param),
                param
            ).Compile();
        }
    }

    internal static class TypeUtil<T>
    {
        public static readonly IntPtr TypeHandle = TypeUtil.GetTypeHandleSlow(typeof(T));
        public static readonly bool IsEnum = typeof(T).IsEnum;
    }

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal static class TypeUtilNullable<T>
    {
        // Nullable-specific properties, initializing this type will allocate

        [CanBeNull]
        private static readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static readonly bool IsNullableEnum = _underlyingType?.IsEnum == true;
        public static readonly IntPtr UnderlyingTypeHandle = TypeUtil.GetTypeHandleSlow(_underlyingType);
        public static readonly TypeCode UnderlyingTypeCode = Type.GetTypeCode(_underlyingType);
    }
}
