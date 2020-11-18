namespace Zoo.Application.Tests.Queries
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using Application.Queries;

    using FizzWare.NBuilder;

    using Moq;

    using NUnit.Framework;

    using Park.BearsAggregate.Models;
    using Park.Common.Adapters;

    [TestFixture]
    public class GetRestrainedAnimalsQueryShould
    {
        [Test]
        public void CallNotFoundWhenCallGetAndNotHzveAnyAnimals()
        {
            var mockedNotFoundCallback = new Mock<NotFoundCallback>();
            var mockedAdapter = new Mock<IRestrainedAnimalAdapter>();
            mockedAdapter.Setup(adapter => adapter.Get<BearRestrained>())
                         .Returns(Enumerable.Empty<BearRestrained>().ToImmutableList());
            var query = new GetRestrainedAnimalsQuery(mockedAdapter.Object);
            
            query.Get<BearRestrained>(null, mockedNotFoundCallback.Object);
            
            mockedNotFoundCallback.Verify(notFound => notFound(), Times.Once);
        }
        
        [Test]
        public void CallFoundWhenCallGetAndHaveAnyAnimals()
        {
            var bears = Builder<BearRestrained>.CreateListOfSize(10).Build().ToImmutableList();
            var mockedFoundCallback = new Mock<FoundCallback<IReadOnlyList<BearRestrained>>>();
            var mockedAdapter = new Mock<IRestrainedAnimalAdapter>();
            mockedAdapter.Setup(adapter => adapter.Get<BearRestrained>())
                         .Returns(bears);
            var query = new GetRestrainedAnimalsQuery(mockedAdapter.Object);
            
            query.Get<BearRestrained>(mockedFoundCallback.Object, null);
            
            mockedFoundCallback.Verify(found => found(bears), Times.Once);
        }
    }
}