using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Synnduit
{
    public class SafeMetadataProviderTest
    {
        [Fact]
        public void GetSourceSystemEntityId_Throws_InvalidOperationException_When_Id_Returned_Is_Null()
        {
            this.ThrowsInvalidOperationException<EntityIdentifier>(
                new FakeEntity()
                {
                    SourceSystemEntityId = null,
                    DestinationSystemEntityId = "587485",
                    Label = "Abbb"
                },
                smp => smp.GetSourceSystemEntityId);
        }

        [Fact]
        public void GetSourceSystemEntityId_Throws_InvalidOperationException_When_Id_Changes()
        {
            this.ThrowsInvalidOperationExceptionWhenIdChanges(
                (entity, id) => entity.SourceSystemEntityId = id,
                smp => smp.GetSourceSystemEntityId);
        }

        [Fact]
        public void GetSourceSystemEntityId_Returns_Source_System_Entity_Id()
        {
            this.ReturnsId(
                entity => entity.SourceSystemEntityId,
                smp => smp.GetSourceSystemEntityId);
        }

        [Fact]
        public void GetDestinationSystemEntityId_Throws_InvalidOperationException_When_Id_Returned_Is_Null()
        {
            this.ThrowsInvalidOperationException<EntityIdentifier>(
                new FakeEntity()
                {
                    SourceSystemEntityId = "jadkjs",
                    DestinationSystemEntityId = null,
                    Label = "Honey"
                },
                smp => smp.GetDestinationSystemEntityId);
        }

        [Fact]
        public void GetDestinationSystemEntityId_Throws_InvalidOperationException_When_Id_Changes()
        {
            this.ThrowsInvalidOperationExceptionWhenIdChanges(
                (entity, id) => entity.DestinationSystemEntityId = id,
                smp => smp.GetDestinationSystemEntityId);
        }

        [Fact]
        public void GetDestinationSystemEntityId_Returns_Destination_System_Entity_Id()
        {
            this.ReturnsId(
                entity => entity.DestinationSystemEntityId,
                smp => smp.GetDestinationSystemEntityId);
        }

        [Fact]
        public void GetLabel_Throws_InvalidOperationException_When_Label_Returned_Is_Null()
        {
            this.ThrowsInvalidOperationException<string>(
                new FakeEntity()
                {
                    SourceSystemEntityId = "123",
                    DestinationSystemEntityId = "abc",
                    Label = null
                },
                smp => smp.GetLabel);
        }

        [Fact]
        public void GetLabel_Returns_Label()
        {
            // arrange
            SafeMetadataProvider<FakeEntity>
                safeMetadataProvider = this.CreateSafeMetadataProvider();
            var entity = new FakeEntity()
            {
                SourceSystemEntityId = "876",
                DestinationSystemEntityId = "IHG",
                Label = "VistaVision"
            };

            // act & assert
            safeMetadataProvider
                .GetLabel(entity)
                .Should()
                .Be("VistaVision");
        }

        private void ThrowsInvalidOperationException<TReturn>(
            FakeEntity entity,
            Func<SafeMetadataProvider<FakeEntity>, Func<FakeEntity, TReturn>> getMethod)
        {
            // arrange
            SafeMetadataProvider<FakeEntity>
                safeMetadataProvider = this.CreateSafeMetadataProvider();
            InvalidOperationException exceptionThrown = null;

            // act
            try
            {
                getMethod(safeMetadataProvider)(entity);
            }
            catch(InvalidOperationException exception)
            {
                exceptionThrown = exception;
            }

            // assert
            exceptionThrown.Should().NotBeNull();
        }

        private void ThrowsInvalidOperationExceptionWhenIdChanges(
            Action<FakeEntity, string> setId,
            Func<SafeMetadataProvider<FakeEntity>, Func<FakeEntity, EntityIdentifier>>
                getMethod)
        {
            // arrange
            SafeMetadataProvider<FakeEntity>
                safeMetadataProvider = this.CreateSafeMetadataProvider();
            var entity = new FakeEntity()
            {
                SourceSystemEntityId = Guid.NewGuid().ToString("d"),
                DestinationSystemEntityId = Guid.NewGuid().ToString("d"),
                Label = Guid.NewGuid().ToString("d")
            };
            Func<FakeEntity, EntityIdentifier> method = getMethod(safeMetadataProvider);
            InvalidOperationException exceptionThrown = null;

            // act
            method(entity);
            setId(entity, Guid.NewGuid().ToString("d"));
            try
            {
                method(entity);
            }
            catch(InvalidOperationException exception)
            {
                exceptionThrown = exception;
            }

            // assert
            exceptionThrown.Should().NotBeNull();
        }

        private void ReturnsId(
            Func<FakeEntity, EntityIdentifier> getId,
            Func<SafeMetadataProvider<FakeEntity>, Func<FakeEntity, EntityIdentifier>>
                getMethod)
        {
            // arrange
            var data = new Dictionary<string, object>();
            SafeMetadataProvider<FakeEntity>
                safeMetadataProvider = this.CreateSafeMetadataProvider(data);
            var entityOne = new FakeEntity()
            {
                SourceSystemEntityId = "12345",
                DestinationSystemEntityId = "ABCDE",
                Label = "Mamma Mia"
            };
            var entityTwo = new FakeEntity()
            {
                SourceSystemEntityId = "6789",
                DestinationSystemEntityId = "FGHI",
                Label = "Fernando"
            };
            Func<FakeEntity, EntityIdentifier> method = getMethod(safeMetadataProvider);

            // act & assert 1: two different IDs acceptable in two different entities
            method(entityOne)
                .Should()
                .Be(getId(entityOne));
            method(entityTwo)
                .Should()
                .Be(getId(entityTwo));

            // act & assert 2: ID immutability only enforced within a given operation
            // (transition to a new operation emulated by clearing the data dictionary)
            data.Clear();
            entityOne.SourceSystemEntityId = "54321";
            entityOne.DestinationSystemEntityId = "EDCBA";
            method(entityOne)
                .Should()
                .Be(getId(entityOne));
        }

        private SafeMetadataProvider<FakeEntity>
            CreateSafeMetadataProvider(Dictionary<string, object> data = null)
        {
            var serviceProvider = new Mock<IServiceProvider<FakeEntity>>();
            var operationExecutive = new Mock<IOperationExecutive>();
            var currentOperation = new Mock<IOperationScope>();
            serviceProvider
                .Setup(sp => sp.MetadataProvider)
                .Returns(new FakeEntityMetadataProvider());
            operationExecutive
                .Setup(oe => oe.CurrentOperation)
                .Returns(currentOperation.Object);
            currentOperation
                .Setup(co => co.Data)
                .Returns(data ?? new Dictionary<string, object>());
            return new SafeMetadataProvider<FakeEntity>(
                serviceProvider.Object, operationExecutive.Object);
        }

        public class FakeEntity
        {
            public string SourceSystemEntityId { get; set; }

            public string DestinationSystemEntityId { get; set; }

            public string Label { get; set; }
        }

        private class FakeEntityMetadataProvider : IMetadataProvider<FakeEntity>
        {
            public EntityIdentifier GetSourceSystemEntityId(FakeEntity entity)
            {
                return entity.SourceSystemEntityId;
            }

            public EntityIdentifier GetDestinationSystemEntityId(FakeEntity entity)
            {
                return entity.DestinationSystemEntityId;
            }

            public string GetLabel(FakeEntity entity)
            {
                return entity.Label;
            }
        }
    }
}
