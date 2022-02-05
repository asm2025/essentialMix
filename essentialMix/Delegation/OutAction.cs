﻿namespace essentialMix.Delegation;

public delegate void OutAction<TOut>(out TOut result);
public delegate void OutAction<in T, TOut>(T arg, out TOut result);
public delegate void OutAction<in T1, in T2, TOut>(T1 arg1, T2 arg2, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, TOut>(T1 arg1, T2 arg2, T3 arg3, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out TOut result);
public delegate void OutAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, TOut>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out TOut result);