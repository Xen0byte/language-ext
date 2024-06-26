using LanguageExt.Common;
using LanguageExt.Traits;
using Xunit;
using static LanguageExt.Prelude;

namespace LanguageExt.Tests.Transformer.Traverse.EitherT.Collections;

public class ArrEither
{
    [Fact]
    public void EmptyArrIsRightEmptyArr()
    {
        Arr<Either<Error, int>> ma = Empty;

        var mb = ma.KindT<Arr, Either<Error>, Either<Error, int>, int>()
                   .SequenceM()
                   .AsT<Either<Error>, Arr, Arr<int>, int>()
                   .As();

        Assert.True(mb == Right(Arr<int>.Empty));
    }
        
    [Fact]
    public void ArrRightsIsRightArrs()
    {
        var ma = Array(Right<Error, int>(1), Right<Error, int>(2), Right<Error, int>(3));

        var mb = ma.KindT<Arr, Either<Error>, Either<Error, int>, int>()
                   .SequenceM()
                   .AsT<Either<Error>, Arr, Arr<int>, int>()
                   .As();

        Assert.True(mb == Right(Array(1, 2, 3)));
    }
        
    [Fact]
    public void ArrRightAndLeftIsLeftEmpty()
    {
        var ma = Array(Right<Error, int>(1), Right<Error, int>(2), Left<Error, int>(Error.New("alternative")));

        var mb = ma.KindT<Arr, Either<Error>, Either<Error, int>, int>()
                   .SequenceM()
                   .AsT<Either<Error>, Arr, Arr<int>, int>()
                   .As() ; 

        Assert.True(mb == Left(Error.New("alternative")));
    }
}
