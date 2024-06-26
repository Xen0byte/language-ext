﻿#nullable enable
namespace LanguageExt;

/// <summary>
/// Ignore transducer.  Lifts a unit accepting transducer, ignores the input value.
/// </summary>
record IgnoreTransducer<A, B>(Transducer<Unit, B> Transducer) : 
    Transducer<A, B>
{
    public override Reducer<A, S> Transform<S>(Reducer<B, S> reduce) =>
        new Ignore<S>(Transducer.Transform(reduce));

    record Ignore<S>(Reducer<Unit, S> Reducer) : Reducer<A, S>
    {
        public override TResult<S> Run(TState state, S stateValue, A value) =>
            Reducer.Run(state, stateValue, default);
    }
            
    public override string ToString() =>  
        "ignore";
}
