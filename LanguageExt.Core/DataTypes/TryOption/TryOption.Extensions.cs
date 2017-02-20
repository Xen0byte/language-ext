﻿using System;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;
using static LanguageExt.TypeClass;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Reactive.Linq;
using LanguageExt.TypeClasses;
using LanguageExt.ClassInstances;
using System.ComponentModel;
using System.Collections.Generic;

/// <summary>
/// Extension methods for the Try monad
/// </summary>
public static class TryOptionExtensions
{
    /// <summary>
    /// Invoke a delegate if the Try returns a value successfully
    /// </summary>
    /// <param name="Some">Delegate to invoke if successful</param>
    public static Unit IfSome<A>(this TryOption<A> self, Action<A> Some)
    {
        var res = TryOptionExtensions.Try(self);
        if (!res.IsFaulted && res.Value.IsSome)
        {
            Some(res.Value.Value);
        }
        return unit;
    }

    /// <summary>
    /// Invoke a delegate if the Try is in a Fail or None state
    /// </summary>
    /// <param name="None">Delegate to invoke if successful</param>
    public static Unit IfNone<A>(this TryOption<A> self, Action None)
    {
        var res = TryOptionExtensions.Try(self);
        if (res.IsFaulted || res.Value.IsNone)
        {
            None();
        }
        return unit;
    }

    /// <summary>
    /// Return a default value if the Try fails
    /// </summary>
    /// <param name="defaultValue">Default value to use on failure</param>
    /// <returns>failValue on failure, the result of the Try otherwise</returns>
    [Pure]
    public static A IfNone<A>(this TryOption<A> self, A defaultValue)
    {
        if (isnull(defaultValue)) throw new ArgumentNullException(nameof(defaultValue));

        var res = TryOptionExtensions.Try(self);
        if (res.IsFaulted || res.Value.IsNone)
            return defaultValue;
        else
            return res.Value.Value;
    }

    /// <summary>
    /// Invoke a delegate if the Try fails
    /// </summary>
    /// <param name="None">Delegate to invoke on failure</param>
    /// <returns>Result of the invocation of Fail on failure, the result of the Try otherwise</returns>
    [Pure]
    public static A IfNone<A>(this TryOption<A> self, Func<A> None)
    {
        var res = TryOptionExtensions.Try(self);
        if (res.IsFaulted || res.Value.IsNone)
            return None();
        else
            return res.Value.Value;
    }

    /// <summary>
    /// Invoke delegates based on None or Failed stateds
    /// </summary>
    /// <typeparam name="T">Bound value type</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="None">Delegate to invoke if the result is None</param>
    /// <param name="Fail">Delegate to invoke if the result is Fail</param>
    /// <returns>Success value, or the result of the None or Failed delegate</returns>
    [Pure]
    public static A IfNoneOrFail<A>(
        this TryOption<A> self,
        Func<A> None,
        Func<Exception, A> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        if (res.Value.IsNone)
            return None();
        else if (res.IsFaulted)
            return Fail(res.Exception);
        else
            return res.Value.Value;
    }

    /// <summary>
    /// Provides a fluent exception matching interface which is invoked
    /// when the Try fails.
    /// </summary>
    /// <returns>Fluent exception matcher</returns>
    [Pure]
    public static ExceptionMatch<Option<A>> IfFail<A>(this TryOption<A> self)
    {
        var res = TryOptionExtensions.Try(self);
        if (res.IsFaulted)
            return res.Exception.Match<Option<A>>();
        else
            return new ExceptionMatch<Option<A>>(res.Value);
    }

    /// <summary>
    /// Pattern matches the two possible states of the Try computation
    /// </summary>
    /// <typeparam name="R">Type of the resulting bound value</typeparam>
    /// <param name="Some">Delegate to invoke if the Try computation completes successfully</param>
    /// <param name="Fail">Delegate to invoke if the Try computation fails</param>
    /// <returns>The result of either the Succ or Fail delegates</returns>
    [Pure]
    public static R Match<A, R>(this TryOption<A> self, Func<A, R> Some, Func<R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Some(res.Value.Value);
    }

    /// <summary>
    /// Pattern matches the two possible states of the Try computation
    /// </summary>
    /// <typeparam name="R">Type of the resulting bound value</typeparam>
    /// <param name="Some">Delegate to invoke if the Try computation completes successfully</param>
    /// <param name="Fail">Delegate to invoke if the Try computation fails</param>
    /// <returns>The result of either the Succ or Fail delegates</returns>
    [Pure]
    public static R Match<A, R>(this TryOption<A> self, Func<A, R> Some, Func<R> None, Func<Exception, R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : None();
    }

    /// <summary>
    /// Pattern matches the two possible states of the Try computation
    /// </summary>
    /// <typeparam name="R">Type of the resulting bound value</typeparam>
    /// <param name="Some">Delegate to invoke if the Try computation completes successfully</param>
    /// <param name="Fail">Default value to use if the Try computation fails</param>
    /// <returns>The result of either the Succ delegate or the Fail value</returns>
    [Pure]
    public static R Match<A, R>(this TryOption<A> self, Func<A, R> Some, R Fail)
    {
        if (isnull(Fail)) throw new ArgumentNullException(nameof(Fail));

        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Fail
            : Some(res.Value.Value);
    }

    /// <summary>
    /// Pattern matches the two possible states of the Try computation
    /// </summary>
    /// <param name="Some">Delegate to invoke if the Try computation completes successfully</param>
    /// <param name="Fail">Delegate to invoke if the Try computation fails</param>
    public static Unit Match<A>(this TryOption<A> self, Action<A> Some, Action Fail)
    {
        var res = TryOptionExtensions.Try(self);

        if (res.IsFaulted || res.Value.IsNone)
            Fail();
        else
            Some(res.Value.Value);

        return Unit.Default;
    }

    /// <summary>
    /// Pattern matches the two possible states of the Try computation
    /// </summary>
    /// <param name="Some">Delegate to invoke if the Try computation completes successfully</param>
    /// <param name="Fail">Delegate to invoke if the Try computation fails</param>
    public static Unit Match<A>(this TryOption<A> self, Action<A> Some, Action None, Action<Exception> Fail)
    {
        var res = TryOptionExtensions.Try(self);

        if (res.IsFaulted)
            Fail(res.Exception);
        else if (res.Value.IsNone)
            None();
        else
            Some(res.Value.Value);

        return Unit.Default;
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, Task<R>> Some, Func<R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted || res.Value.IsNone
            ? Task.FromResult(Fail())
            : Some(res.Value.Value));
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, Task<R>> Some, Func<R> None, Func<Exception, R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted
            ? Task.FromResult(Fail(res.Exception))
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : Task.FromResult(None()));
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, Task<R>> Some, Func<Task<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Some(res.Value.Value));
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, Task<R>> Some, Func<Task<R>> None, Func<Exception, Task<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : None());
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, R> Some, Func<Task<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Task.FromResult(Some(res.Value.Value)));
    }

    public static async Task<R> MatchAsync<A, R>(this TryOption<A> self, Func<A, R> Some, Func<Task<R>> None, Func<Exception, Task<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return await (res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Task.FromResult(Some(res.Value.Value))
                : None());
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, IObservable<R>> Some, Func<R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Observable.Return(Fail())
            : Some(res.Value.Value);
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, IObservable<R>> Some, Func<R> None, Func<Exception, R> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? Observable.Return(Fail(res.Exception))
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : Observable.Return(None());
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, IObservable<R>> Some, Func<IObservable<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Some(res.Value.Value);
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, IObservable<R>> Some, Func<IObservable<R>> None, Func<Exception, IObservable<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : None();
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, R> Some, Func<IObservable<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Observable.Return(Some(res.Value.Value));
    }

    public static IObservable<R> MatchObservable<A, R>(this TryOption<A> self, Func<A, R> Some, Func<IObservable<R>> None, Func<Exception, IObservable<R>> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Observable.Return(Some(res.Value.Value))
                : None();
    }

    /// <summary>
    /// Memoise the try
    /// </summary>
    public static TryOption<A> Memo<A>(this TryOption<A> self)
    {
        var res = TryOptionExtensions.Try(self);
        return () =>
        {
            if (res.IsFaulted) throw new InnerException(res.Exception);
            return res.Value;
        };
    }

    [Pure]
    public static Option<A> ToOption<A>(this TryOption<A> self)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? None
            : res.Value;
    }

    [Pure]
    public static Try<A> ToTry<A>(this TryOption<A> self) => () => 
        self.IfFailThrow();

    [Pure]
    public static A IfFailThrow<A>(this TryOption<A> self)
    {
        try
        {
            return self().Value.Value;
        }
        catch (Exception e)
        {
            TryConfig.ErrorLogger(e);
            throw;
        }
    }

    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TryOption<U> Select<A, U>(this TryOption<A> self, Func<A, U> select) => () => 
        self().Value.Map(select);

    /// <summary>
    /// Apply Try values to a Try function of arity 2
    /// </summary>
    /// <param name="self">Try function</param>
    /// <param name="arg1">Try argument</param>
    /// <param name="arg2">Try argument</param>
    /// <returns>Returns the result of applying the Try arguments to the Try function</returns>
    public static Unit Iter<A>(this TryOption<A> self, Action<A> action) =>
        self.IfSome(action);

    /// <summary>
    /// Counts the number of bound values.  
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <param name="self">TrTry computation</param>
    /// <returns>1 if the Try computation is successful, 0 otherwise.</returns>
    [Pure]
    public static int Count<A>(this TryOption<A> self)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? 0
            : 1;
    }

    /// <summary>
    /// Tests that a predicate holds for all values of the bound value T
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="pred">Predicate to test the bound value against</param>
    /// <returns>True if the predicate holds for the bound value, or if the Try computation
    /// fails.  False otherwise.</returns>
    [Pure]
    public static bool ForAll<A>(this TryOption<A> self, Func<A, bool> pred)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? false
            : pred(res.Value.Value);
    }

    /// <summary>
    /// Folds Try value into an S.
    /// https://en.wikipedia.org/wiki/Fold_(higher-order_function)
    /// </summary>
    /// <param name="self">Try to fold</param>
    /// <param name="state">Initial state</param>
    /// <param name="folder">Fold function</param>
    /// <returns>Folded state</returns>
    [Pure]
    public static S Fold<A, S>(this TryOption<A> self, S state, Func<S, A, S> folder)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? state
            : folder(state, res.Value.Value);
    }

    /// <summary>
    /// Folds Try value into an S.
    /// https://en.wikipedia.org/wiki/Fold_(higher-order_function)
    /// </summary>
    /// <param name="self">Try to fold</param>
    /// <param name="state">Initial state</param>
    /// <param name="Some">Fold function for Success</param>
    /// <param name="Fail">Fold function for Failure</param>
    /// <returns>Folded state</returns>
    [Pure]
    public static S BiFold<A, S>(this TryOption<A> self, S state, Func<S, A, S> Some, Func<S, S> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? Fail(state)
            : Some(state, res.Value.Value);
    }

    /// <summary>
    /// Folds Try value into an S.
    /// https://en.wikipedia.org/wiki/Fold_(higher-order_function)
    /// </summary>
    /// <param name="self">Try to fold</param>
    /// <param name="state">Initial state</param>
    /// <param name="Some">Fold function for Success</param>
    /// <param name="Fail">Fold function for Failure</param>
    /// <returns>Folded state</returns>
    [Pure]
    public static S TriFold<A, S>(this TryOption<A> self, S state, Func<S, A, S> Some, Func<S, S> None, Func<S, Exception, S> Fail)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted
            ? Fail(state, res.Exception)
            : res.Value.IsSome
                ? Some(state, res.Value.Value)
                : None(state);
    }

    /// <summary>
    /// Tests that a predicate holds for any value of the bound value T
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="pred">Predicate to test the bound value against</param>
    /// <returns>True if the predicate holds for the bound value.  False otherwise.</returns>
    [Pure]
    public static bool Exists<A>(this TryOption<A> self, Func<A, bool> pred)
    {
        var res = TryOptionExtensions.Try(self);
        return res.IsFaulted || res.Value.IsNone
            ? false
            : pred(res.Value.Value);
    }

    /// <summary>
    /// Maps the bound value
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <typeparam name="R">Resulting bound value type</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="mapper">Delegate to map the bound value</param>
    /// <returns>Mapped Try computation</returns>
    [Pure]
    public static TryOption<R> Map<A, R>(this TryOption<A> self, Func<A, R> mapper) =>
        () => self().Value.Map(mapper);

    /// <summary>
    /// Maps the bound value
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <typeparam name="R">Resulting bound value type</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="Some">Delegate to map the bound value</param>
    /// <param name="Fail">Delegate to map the exception to the desired bound result type</param>
    /// <returns>Mapped Try computation</returns>
    [Pure]
    public static TryOption<R> BiMap<A, R>(this TryOption<A> self, Func<A, R> Some, Func<R> Fail)
    {
        return () =>
        {
            var res = self.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Some(res.Value.Value);
        };
    }

    /// <summary>
    /// Maps the bound value
    /// </summary>
    /// <typeparam name="T">Type of the bound value</typeparam>
    /// <typeparam name="R">Resulting bound value type</typeparam>
    /// <param name="self">Try computation</param>
    /// <param name="Some">Delegate to map the bound value</param>
    /// <param name="Fail">Delegate to map the exception to the desired bound result type</param>
    /// <returns>Mapped Try computation</returns>
    [Pure]
    public static TryOption<R> TriMap<A, R>(this TryOption<A> self, Func<A, R> Some, Func<R> None, Func<Exception, R> Fail)
    {
        return () =>
        {
            var res = self.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome
                    ? Some(res.Value.Value)
                    : None();
        };
    }

    /// <summary>
    /// Partial application map
    /// </summary>
    /// <remarks>TODO: Better documentation of this function</remarks>
    [Pure]
    public static TryOption<Func<B, R>> ParMap<A, B, R>(this TryOption<A> self, Func<A, B, R> func) =>
        self.Map(curry(func));

    /// <summary>
    /// Partial application map
    /// </summary>
    /// <remarks>TODO: Better documentation of this function</remarks>
    [Pure]
    public static TryOption<Func<B, Func<C, R>>> ParMap<A, B, C, R>(this TryOption<A> self, Func<A, B, C, R> func) =>
        self.Map(curry(func));

    [Pure]
    public static TryOption<A> Filter<A>(this TryOption<A> self, Func<A, bool> pred)
    {
        return () =>
        {
            var res = self().Value;
            return res.IsSome && pred(res.Value)
                ? res
                : Option<A>.None;
        };
    }

    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TryOption<A> Where<A>(this TryOption<A> self, Func<A, bool> pred) =>
        self.Filter(pred);

    [Pure]
    public static TryOption<R> Bind<A, R>(this TryOption<A> self, Func<A, TryOption<R>> binder)
    {
        return () =>
        {
            var opt = self().Value;
            return opt.IsSome
                ? binder(opt.Value)().Value
                : Option<R>.None;
        };
    }

    [Pure]
    public static TryOption<R> BiBind<A, R>(this TryOption<A> self, Func<A, TryOption<R>> Some, Func<TryOption<R>> Fail)
    {
        return () =>
        {
            var res = self().Value;
            return res.IsNone
                ? Fail()().Value
                : Some(res.Value)().Value;
        };
    }

    [Pure]
    public static TryOption<R> TriBind<A, R>(this TryOption<A> self, Func<A, TryOption<R>> Some, Func<TryOption<R>> None, Func<Exception, TryOption<R>> Fail)
    {
        return () =>
        {
            var res = self().Value;
            return res.IsSome
                ? Some(res.Value)().Value 
                : None()().Value;
        };
    }

    [Pure]
    public static IEnumerable<Either<Exception, A>> AsEnumerable<A>(this TryOption<A> self)
    {
        var res = TryOptionExtensions.Try(self);

        if (res.IsFaulted)
        {
            yield return res.Exception;
        }
        else if (res.Value.IsSome)
        {
            yield return res.Value.Value;
        }
    }

    [Pure]
    public static Lst<Either<Exception, A>> ToList<A>(this TryOption<A> self) =>
        toList(self.AsEnumerable());

    [Pure]
    public static Arr<Either<Exception, A>> ToArray<A>(this TryOption<A> self) =>
        toArray(self.AsEnumerable());

    [Pure]
    public static TryOptionSomeContext<A, R> Some<A, R>(this TryOption<A> self, Func<A, R> Some) =>
        new TryOptionSomeContext<A, R>(self, Some);

    [Pure]
    public static TryOptionSomeUnitContext<A> Some<A>(this TryOption<A> self, Action<A> Some) =>
        new TryOptionSomeUnitContext<A>(self, Some);

    [Pure]
    public static string AsString<A>(this TryOption<A> self) =>
        match(self,
            Some: v => isnull(v)
                      ? "Some(null)"
                      : $"Some({v})",
            None: () => "None",
            Fail: ex => $"Fail({ex.Message})"
        );

    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TryOption<V> SelectMany<A, U, V>(
        this TryOption<A> self,
        Func<A, TryOption<U>> bind,
        Func<A, U, V> project)
    {
        return () =>
        {
            var resT = self().Value;
            if (resT.IsNone) return Option<V>.None;
            var resU = bind(resT.Value)().Value;
            if (resU.IsNone) return Option<V>.None;
            return project(resT.Value, resU.Value);
        };
    }

    [Pure]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IEnumerable<V> SelectMany<A, U, V>(
        this TryOption<A> self,
        Func<A, IEnumerable<U>> bind,
        Func<A, U, V> project
        )
    {
        var resT = TryOptionExtensions.Try(self);
        if (resT.IsFaulted || resT.Value.IsNone) return new V[0];
        return bind(resT.Value.Value).Map(resU => project(resT.Value.Value, resU));
    }

    public static TryOption<V> Join<A, U, K, V>(
        this TryOption<A> self,
        TryOption<U> inner,
        Func<A, K> outerKeyMap,
        Func<U, K> innerKeyMap,
        Func<A, U, V> project)
    {
        return () =>
        {
            var selfRes = self().Value;
            var innerRes = inner().Value;
            return selfRes.IsSome && innerRes.IsSome && EqualityComparer<K>.Default.Equals(outerKeyMap(selfRes.Value), innerKeyMap(innerRes.Value))
                ? project(selfRes.Value, innerRes.Value)
                : raise<V>(new BottomException());
        };
    }

    /// <summary>
    /// Savely invokes the TryOption computation
    /// </summary>
    /// <typeparam name="T">Bound value of the computation</typeparam>
    /// <param name="self">TryOption to invoke</param>
    /// <returns>TryOptionResult</returns>
    [Pure]
    public static TryOptionResult<T> Try<T>(this TryOption<T> self)
    {
        try
        {
            if (self == null)
            {
                throw new ArgumentNullException("this is null");
            }
            return self();
        }
        catch (Exception e)
        {
            TryConfig.ErrorLogger(e);
            return new TryOptionResult<T>(e);
        }
    }

    [Pure]
    public static TryOption<U> Use<T, U>(this TryOption<T> self, Func<T, U> select)
        where T : IDisposable => () =>
        {
            T t = default(T);
            try
            {
                var opt = self().Value;
                if (opt.IsNone) return opt.Map(select);
                t = opt.Value;
                return select(t);
            }
            finally
            {
                t?.Dispose();
            }
        };

    [Pure]
    public static int Sum(this TryOption<int> self) =>
        self.Try().Value.Sum();

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, R> Some, Func<R> None, Func<Exception, R> Fail) =>
        await self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted
            ? Fail(res.Exception)
            : res.Value.IsSome
                ? Some(res.Value.Value)
                : None();
        });

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, R> Some, Func<R> Fail) =>
        await self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted || res.Value.IsNone
            ? Fail()
            : Some(res.Value.Value);
        });

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, Task<R>> Some, Func<R> None, Func<Exception, R> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted
                ? Task.FromResult(Fail(res.Exception))
                : res.Value.IsSome
                    ? Some(res.Value.Value)
                    : Task.FromResult(None());
        })
        from t in tt
        select t);

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, Task<R>> Some, Func<R> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Task.FromResult(Fail())
                : Some(res.Value.Value);
        })
        from t in tt
        select t);

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, Task<R>> Some, Func<Task<R>> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Some(res.Value.Value);
        })
        from t in tt
        select t);

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, Task<R>> Some, Func<Task<R>> None, Func<Exception, Task<R>> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome 
                    ? Some(res.Value.Value)
                    : None();
        })
        from t in tt
        select t);

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, R> Some, Func<Task<R>> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Task.FromResult(Some(res.Value.Value));
        })
        from t in tt
        select t);

    public static async Task<R> MatchAsync<T, R>(this Task<TryOption<T>> self, Func<T, R> Some, Func<Task<R>> None, Func<Exception, Task<R>> Fail) =>
        await (from tt in self.ContinueWith(trySelf =>
        {
            var res = trySelf.Result.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome
                    ? Task.FromResult(Some(res.Value.Value))
                    : None();
        })
        from t in tt
        select t);

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, R> Some, Func<R> Fail) =>
        self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Some(res.Value.Value);
        });

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, IObservable<R>> Some, Func<R> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Observable.Return(Fail())
                : Some(res.Value.Value);
        })
        from t in tt
        select t;

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, IObservable<R>> Some, Func<IObservable<R>> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Some(res.Value.Value);
        })
        from t in tt
        select t;

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, R> Some, Func<IObservable<R>> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted || res.Value.IsNone
                ? Fail()
                : Observable.Return(Some(res.Value.Value));
        })
        from t in tt
        select t;

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, R> Some, Func<R> None, Func<Exception, R> Fail) =>
        self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome 
                    ? Some(res.Value.Value)
                    : None();
        });

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, IObservable<R>> Some, Func<R> None, Func<Exception, R> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted
                ? Observable.Return(Fail(res.Exception))
                : res.Value.IsSome 
                    ? Some(res.Value.Value)
                    : Observable.Return(None());
        })
        from t in tt
        select t;

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, IObservable<R>> Some, Func<IObservable<R>> None, Func<Exception, IObservable<R>> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome 
                    ? Some(res.Value.Value)
                    : None();
        })
        from t in tt
        select t;

    public static IObservable<R> MatchObservable<T, R>(this IObservable<TryOption<T>> self, Func<T, R> Some, Func<IObservable<R>> None, Func<Exception, IObservable<R>> Fail) =>
        from tt in self.Select(trySelf =>
        {
            var res = trySelf.Try();
            return res.IsFaulted
                ? Fail(res.Exception)
                : res.Value.IsSome 
                    ? Observable.Return(Some(res.Value.Value))
                    : None();
        })
        from t in tt
        select t;

    [Pure]
    public static TryOption<T> Flatten<T>(this TryOption<TryOption<T>> self) =>
        from x in self
        from y in x
        select y;

    [Pure]
    public static TryOption<T> Flatten<T>(this TryOption<TryOption<TryOption<T>>> self) =>
        from x in self
        from y in x
        from z in y
        select z;

    [Pure]
    public static TryOption<T> Flatten<T>(this TryOption<TryOption<TryOption<TryOption<T>>>> self) =>
        from x in self
        from y in x
        from z in y
        from w in z
        select w;

    /// <summary>
    /// Apply a TryOptional argument to a TryOptional function of arity 1
    /// </summary>
    /// <param name="option">Optional function</param>
    /// <param name="x">Argument to apply</param>
    /// <returns>Returns the result of applying the optional argument to the optional function</returns>
    [Pure]
    public static TryOption<B> Apply<A, B>(this TryOption<Func<A, B>> x, TryOption<A> y) =>
        apply<MTryOption<Func<A, B>>, MTryOption<A>, MTryOption<B>, TryOption<Func<A, B>>, TryOption<A>, TryOption<B>, A, B>(x, y);

    /// <summary>
    /// Apply two TryOptional arguments to a TryOptional function of arity 2
    /// </summary>
    /// <param name="TryOption">TryOptional function</param>
    /// <param name="x">Argument to apply</param>
    /// <param name="y">Argument to apply</param>
    /// <returns>Returns the result of applying the TryOptional argument to the TryOptional function</returns>
    [Pure]
    public static TryOption<C> Apply<A, B, C>(this TryOption<Func<A, B, C>> x, TryOption<A> y, TryOption<B> z) =>
        apply<MTryOption<Func<A, B, C>>, MTryOption<A>, MTryOption<B>, MTryOption<C>, TryOption<Func<A, B, C>>, TryOption<A>, TryOption<B>, TryOption<C>, A, B, C>(x, y, z);

    /// <summary>
    /// Apply one TryOptional arguments to a TryOptional function of arity 2
    /// </summary>
    /// <param name="TryOption">TryOptional function</param>
    /// <param name="x">Argument to apply</param>
    /// <returns>Returns the result of applying the TryOptional argument to the TryOptional function</returns>
    [Pure]
    public static TryOption<Func<B, C>> Apply<A, B, C>(this TryOption<Func<A, B, C>> x, TryOption<A> y) =>
        apply<MTryOption<Func<A, B, C>>, MTryOption<A>, MTryOption<Func<B, C>>, TryOption<Func<A, B, C>>, TryOption<A>, TryOption<Func<B, C>>, A, B, C>(x, y);

    /// <summary>
    /// Apply one TryOptional arguments to a TryOptional function of arity 2
    /// </summary>
    /// <param name="TryOption">TryOptional function</param>
    /// <param name="x">Argument to apply</param>
    /// <returns>Returns the result of applying the TryOptional argument to the TryOptional function</returns>
    [Pure]
    public static TryOption<Func<B, C>> Apply<A, B, C>(this TryOption<Func<A, Func<B, C>>> x, TryOption<A> y) =>
        apply2<MTryOption<Func<A, Func<B, C>>>, MTryOption<A>, MTryOption<Func<B, C>>, TryOption<Func<A, Func<B, C>>>, TryOption<A>, TryOption<Func<B, C>>, A, B, C>(x, y);

    /// <summary>
    /// Partially apply a TryOptional argument to a curried TryOptional function
    /// </summary>
    [Pure]
    public static TryOption<B> Action<A, B>(this TryOption<A> x, TryOption<B> y) =>
        action<MTryOption<A>, MTryOption<B>, TryOption<A>, TryOption<B>, A, B>(x, y);

    /// <summary>
    /// Add the bound value of Try(x) to Try(y).  If either of the
    /// Trys are Fail then the result is Fail
    /// </summary>
    /// <param name="lhs">Left-hand side of the operation</param>
    /// <param name="rhs">Right-hand side of the operation</param>
    /// <returns>lhs + rhs</returns>
    [Pure]
    public static TryOption<A> Add<NUM, A>(this TryOption<A> lhs, TryOption<A> rhs) where NUM : struct, Num<A> =>
        from x in lhs
        from y in rhs
        select plus<NUM, A>(x, y);

    /// <summary>
    /// Find the subtract of the bound value of Try(x) and Try(y).  If either of 
    /// the Trys are Fail then the result is Fail
    /// </summary>
    /// <param name="lhs">Left-hand side of the operation</param>
    /// <param name="rhs">Right-hand side of the operation</param>
    /// <returns>lhs + rhs</returns>
    [Pure]
    public static TryOption<A> Subtract<NUM, A>(this TryOption<A> lhs, TryOption<A> rhs) where NUM : struct, Num<A> =>
        from x in lhs
        from y in rhs
        select subtract<NUM, A>(x, y);

    /// <summary>
    /// Multiply the bound value of Try(x) and Try(y).  If either of the
    /// Trys are Fail then the result is Fail
    /// </summary>
    /// <param name="lhs">Left-hand side of the operation</param>
    /// <param name="rhs">Right-hand side of the operation</param>
    /// <returns>lhs + rhs</returns>
    [Pure]
    public static TryOption<A> Product<NUM, A>(this TryOption<A> lhs, TryOption<A> rhs) where NUM : struct, Num<A> =>
        from x in lhs
        from y in rhs
        select product<NUM, A>(x, y);

    /// <summary>
    /// Multiply the bound value of Try(x) and Try(y).  If either of the
    /// Trys are Fail then the result is Fail
    /// </summary>
    /// <param name="lhs">Left-hand side of the operation</param>
    /// <param name="rhs">Right-hand side of the operation</param>
    /// <returns>lhs + rhs</returns>
    [Pure]
    public static TryOption<A> Divide<NUM, A>(this TryOption<A> lhs, TryOption<A> rhs) where NUM : struct, Num<A> =>
        from x in lhs
        from y in rhs
        select divide<NUM, A>(x, y);

    /// <summary>
    /// Convert the Try type to a Nullable of A
    /// </summary>
    /// <typeparam name="A">Type of the bound value</typeparam>
    /// <param name="ma">Try to convert</param>
    /// <returns>Nullable of A</returns>
    [Pure]
    public static A? ToNullable<A>(this TryOption<A> ma) where A : struct
    {
        var x = ma.Try();
        return x.IsFaulted || x.Value.IsNone
            ? (A?)null
            : x.Value.Value;
    }
}