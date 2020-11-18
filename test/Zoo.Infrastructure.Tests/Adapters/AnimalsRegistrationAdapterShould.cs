using NUnit.Framework;

namespace Zoo.Infrastructure.Tests.Adapters
{
    using System.Threading.Tasks;

    using Administration.AnimalsRegistrationAggregate.Models;

    using AutoMapper;

    using FizzWare.NBuilder;

    using FluentAssertions;

    using Infrastructure.Adapters;
    using Infrastructure.Entities;
    using Infrastructure.Store;

    using Moq;

    using Park.BearsAggregate.Models;

    [TestFixture]
    public class AnimalsRegistrationAdapterShould
    {
        [Test]
        public async Task ReturnDetailsOfCreatedAnimalsWhenCallRegister()
        {
            var bearCreating = Builder<BearCreating>.CreateNew().Build();
            var mappedAnimal = Builder<Animal>.CreateNew().Build();
            var bearDetails = Builder<BearDetails>.CreateNew().Build();
            var mockedMapper = new Mock<IMapper>();
            mockedMapper.Setup(mapper => mapper.Map<Animal>(bearCreating))
                        .Returns(mappedAnimal);
            mockedMapper.Setup(mapper => mapper.Map<BearDetails>(mappedAnimal))
                        .Returns(bearDetails);
            var mockedWriter = new Mock<IWriter>();
            mockedWriter.Setup(writer => writer.Create(mappedAnimal));
            mockedWriter.Setup(writer => writer.SaveAsync(default))
                        .Returns(Task.CompletedTask);
            var adapter = new AnimalsRegistrationAdapter(mockedMapper.Object, mockedWriter.Object);

            var actualValue = await adapter.RegisterAsync<BearCreating, BearDetails>(bearCreating);

            actualValue.Should().Be(bearDetails);
        }
    }
}