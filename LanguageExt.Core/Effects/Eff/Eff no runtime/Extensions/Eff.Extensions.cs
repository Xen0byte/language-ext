using System;
using LanguageExt.Effects;
using LanguageExt.Traits;

namespace LanguageExt;

public static partial class EffExtensions
{
    /// <summary>
    /// Cast type to its Kind
    /// </summary>
    public static Eff<A> As<A>(this K<Eff, A> ma) =>
        (Eff<A>)ma;

    /// <summary>
    /// Cast type to its Kind
    /// </summary>
    public static Eff<A> As<A>(this Eff<MinRT, A> ma) =>
        new(ma);
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  Monadic join
    //

    /// <summary>
    /// Monadic join operator 
    /// </summary>
    /// <remarks>
    /// Collapses a nested IO monad so there is no nesting.
    /// </remarks>
    /// <param name="mma">Nest IO monad to flatten</param>
    /// <typeparam name="RT">Runtime</typeparam>
    /// <typeparam name="A">Bound value</typeparam>
    /// <returns>Flattened IO monad</returns>
    public static Eff<A> Flatten<A>(this Eff<Eff<A>> mma) =>
        mma.Bind(ma => ma);

    /// <summary>
    /// Monadic join operator 
    /// </summary>
    /// <remarks>
    /// Collapses a nested IO monad so there is no nesting.
    /// </remarks>
    /// <param name="mma">Nest IO monad to flatten</param>
    /// <typeparam name="RT">Runtime</typeparam>
    /// <typeparam name="A">Bound value</typeparam>
    /// <returns>Flattened IO monad</returns>
    public static Eff<A> Flatten<A>(this Eff<K<Eff, A>> mma) =>
        mma.Bind(ma => ma);
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  SelectMany extensions
    //

    /// <summary>
    /// Monadic bind and project with paired IO monads
    /// </summary>
    public static Eff<D> SelectMany<A, B, C, D>(
        this (K<Eff, A> First, K<Eff, B> Second) self,
        Func<(A First, B Second), K<Eff, C>> bind,
        Func<(A First, B Second), C, D> project) =>
        self.Zip().Bind(ab => bind(ab).Map(c => project(ab, c)));

    /// <summary>
    /// Monadic bind and project with paired IO monads
    /// </summary>
    public static Eff<D> SelectMany<A, B, C, D>(
        this K<Eff, A> self,
        Func<A, (K<Eff, B> First, K<Eff, C> Second)> bind,
        Func<A, (B First, C Second), D> project) =>
        self.As().Bind(a => bind(a).Zip().Map(cd => project(a, cd)));

    /// <summary>
    /// Monadic bind and project with paired IO monads
    /// </summary>
    public static Eff<E> SelectMany<A, B, C, D, E>(
        this (K<Eff, A> First, K<Eff, B> Second, K<Eff, C> Third) self,
        Func<(A First, B Second, C Third), K<Eff, D>> bind,
        Func<(A First, B Second, C Third), D, E> project) =>
        self.Zip().Bind(ab => bind(ab).Map(c => project(ab, c)));

    /// <summary>
    /// Monadic bind and project with paired IO monads
    /// </summary>
    public static Eff<E> SelectMany<A, B, C, D, E>(
        this K<Eff, A> self,
        Func<A, (K<Eff, B> First, K<Eff, C> Second, K<Eff, D> Third)> bind,
        Func<A, (B First, C Second, D Third), E> project) =>
        self.As().Bind(a => bind(a).Zip().Map(cd => project(a, cd)));

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  Zipping
    //

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second)> Zip<A, B>(
         this (K<Eff, A> First, K<Eff, B> Second) tuple) =>
         tuple.First.As()
              .effect
              .Zip(tuple.Second.As().effect)
              .As();

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <typeparam name="C">Third IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second, C Third)> Zip<A, B, C>(
        this (K<Eff, A> First, 
              K<Eff, B> Second, 
              K<Eff, C> Third) tuple) =>
        tuple.First.As()
             .effect
             .Zip(tuple.Second.As().effect, tuple.Third.As().effect)
             .As();

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <typeparam name="C">Third IO monad bound value type</typeparam>
    /// <typeparam name="D">Fourth IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second, C Third, D Fourth)> Zip<A, B, C, D>(
        this (K<Eff, A> First,
            K<Eff, B> Second,
            K<Eff, C> Third,
            K<Eff, D> Fourth) tuple) =>
        tuple.First.As()
             .effect
             .Zip(tuple.Second.As().effect, tuple.Third.As().effect, tuple.Fourth.As().effect)
             .As();

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second)> Zip<A, B>(
        this K<Eff, A> First,
        K<Eff, B> Second) =>
        (First, Second).Zip();

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <typeparam name="C">Third IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second, C Third)> Zip<A, B, C>(
        this K<Eff, A> First, 
        K<Eff, B> Second, 
        K<Eff, C> Third) =>
        (First, Second, Third).Zip();

    /// <summary>
    /// Takes two IO monads and zips their result
    /// </summary>
    /// <remarks>
    /// Asynchronous operations will run concurrently
    /// </remarks>
    /// <param name="tuple">Tuple of IO monads to run</param>
    /// <typeparam name="RT">Runtime type</typeparam>
    /// <typeparam name="E">Error type</typeparam>
    /// <typeparam name="A">First IO monad bound value type</typeparam>
    /// <typeparam name="B">Second IO monad bound value type</typeparam>
    /// <typeparam name="C">Third IO monad bound value type</typeparam>
    /// <typeparam name="D">Fourth IO monad bound value type</typeparam>
    /// <returns>IO monad</returns>
    public static Eff<(A First, B Second, C Third, D Fourth)> Zip<A, B, C, D>(
        this K<Eff, A> First,
        K<Eff, B> Second,
        K<Eff, C> Third,
        K<Eff, D> Fourth) =>
        (First, Second, Third, Fourth).Zip();
}
