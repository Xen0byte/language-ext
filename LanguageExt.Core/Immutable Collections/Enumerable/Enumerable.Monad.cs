using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt.Traits;

namespace LanguageExt;

public partial class EnumerableM : Monad<EnumerableM>, MonoidK<EnumerableM>, Traversable<EnumerableM>
{
    static K<EnumerableM, B> Monad<EnumerableM>.Bind<A, B>(K<EnumerableM, A> ma, Func<A, K<EnumerableM, B>> f) =>
        ma.As().Bind(f);

    static K<EnumerableM, B> Functor<EnumerableM>.Map<A, B>(Func<A, B> f, K<EnumerableM, A> ma) => 
        ma.As().Map(f);

    static K<EnumerableM, A> Applicative<EnumerableM>.Pure<A>(A value) =>
        singleton(value);

    static K<EnumerableM, B> Applicative<EnumerableM>.Apply<A, B>(K<EnumerableM, Func<A, B>> mf, K<EnumerableM, A> ma)
    {
        return new EnumerableM<B>(Go());
        IEnumerable<B> Go()
        {
            foreach (var f in mf.As())
            {
                foreach (var a in ma.As())
                {
                    yield return f(a);
                }
            }
        }
    }

    static K<EnumerableM, B> Applicative<EnumerableM>.Action<A, B>(K<EnumerableM, A> ma, K<EnumerableM, B> mb)
    {
        ma.As().Consume();
        return mb;
    }

    static K<EnumerableM, A> MonoidK<EnumerableM>.Empty<A>() =>
        EnumerableM<A>.Empty;

    static K<EnumerableM, A> SemigroupK<EnumerableM>.Combine<A>(K<EnumerableM, A> ma, K<EnumerableM, A> mb) =>
        ma.As().ConcatFast(mb.As().runEnumerable);

    static K<F, K<EnumerableM, B>> Traversable<EnumerableM>.Traverse<F, A, B>(Func<A, K<F, B>> f, K<EnumerableM, A> ta) 
    {
        return F.Map<EnumerableM<B>, K<EnumerableM, B>>(
            ks => ks, 
            F.Map(s => new EnumerableM<B>(s.AsEnumerable()), 
                  Foldable.foldBack(cons, F.Pure(Seq.empty<B>()), ta)));

        K<F, Seq<B>> cons(K<F, Seq<B>> ys, A x) =>
            Applicative.lift(Prelude.Cons, f(x), ys);
    }

    static K<F, K<EnumerableM, B>> Traversable<EnumerableM>.TraverseM<F, A, B>(Func<A, K<F, B>> f, K<EnumerableM, A> ta) 
    {
        return F.Map<EnumerableM<B>, K<EnumerableM, B>>(
            ks => ks, 
            F.Map(s => new EnumerableM<B>(s.AsEnumerable()), 
                  Foldable.foldBack(cons, F.Pure(Seq.empty<B>()), ta)));

        K<F, Seq<B>> cons(K<F, Seq<B>> fys, A x) =>
            fys.Bind(ys => f(x).Map(y => y.Cons(ys)));
    }
    
    static S Foldable<EnumerableM>.FoldWhile<A, S>(
        Func<A, Func<S, S>> f,
        Func<(S State, A Value), bool> predicate,
        S state,
        K<EnumerableM, A> ta)
    {
        foreach (var x in ta.As().runEnumerable)
        {
            if (!predicate((state, x))) return state;
            state = f(x)(state);
        }
        return state;
    }
    
    static S Foldable<EnumerableM>.FoldBackWhile<A, S>(
        Func<S, Func<A, S>> f, 
        Func<(S State, A Value), bool> predicate, 
        S state, 
        K<EnumerableM, A> ta)
    {
        foreach (var x in ta.As().runEnumerable.Reverse())
        {
            if (!predicate((state, x))) return state;
            state = f(state)(x);
        }
        return state;
    }
    
    static Arr<A> Foldable<EnumerableM>.ToArr<A>(K<EnumerableM, A> ta) =>
        new(ta.As());

    static Lst<A> Foldable<EnumerableM>.ToLst<A>(K<EnumerableM, A> ta) =>
        new(ta.As());

    static EnumerableM<A> Foldable<EnumerableM>.ToEnumerable<A>(K<EnumerableM, A> ta) =>
        ta.As();
    
    static Seq<A> Foldable<EnumerableM>.ToSeq<A>(K<EnumerableM, A> ta) =>
        new (ta.As().runEnumerable);
}
