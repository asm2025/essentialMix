using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer;

public delegate TaskResult ExecuteCallbackDelegates<T>([NotNull] IProducerConsumer<T> queue, T item);
public delegate bool ResultCallbackDelegates<T>([NotNull] IProducerConsumer<T> queue, T item, TaskResult result, Exception exception);
public delegate bool ScheduledCallbackDelegates<T>(T item);
public delegate void FinalizeCallbackDelegates<T>(T item);
public delegate void CallbackDelegates<T>([NotNull] IProducerConsumer<T> queue);