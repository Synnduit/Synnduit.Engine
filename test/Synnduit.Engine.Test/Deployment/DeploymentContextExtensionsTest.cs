using Moq;
using Synnduit.TestHelper;
using System;
using Xunit;

namespace Synnduit.Deployment
{
    public class DeploymentContextExtensionsTest
    {
        [Fact]
        public void DestinationSystemOrphanMappingBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () => DeploymentContextExtensions.DestinationSystemOrphanMappingBehavior(
                    null, Guid.NewGuid(), OrphanMappingBehavior.None),
                "context");
        }

        [Fact]
        public void DestinationSystemOrphanMappingBehavior_Throws_ArgumentException_When_DestinationSystemId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.DestinationSystemOrphanMappingBehavior(
                    Guid.Empty, OrphanMappingBehavior.Remove),
                "destinationSystemId");
        }

        [Fact]
        public void DestinationSystemOrphanMappingBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.DestinationSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.DestinationSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) 57);
            this.DestinationSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) (-1));
        }

        [Fact]
        public void DestinationSystemOrphanMappingBehavior_Creates_Destination_System_Parameter()
        {
            this.DestinationSystemOrphanMappingBehaviorCreatesDestinationSystemParameter(
                OrphanMappingBehavior.None);
            this.DestinationSystemOrphanMappingBehaviorCreatesDestinationSystemParameter(
                OrphanMappingBehavior.Deactivate);
            this.DestinationSystemOrphanMappingBehaviorCreatesDestinationSystemParameter(
                OrphanMappingBehavior.Remove);
        }

        [Fact]
        public void EntityTypeOrphanMappingBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () => DeploymentContextExtensions.EntityTypeOrphanMappingBehavior(
                    null, Guid.NewGuid(), OrphanMappingBehavior.None),
                "context");
        }

        [Fact]
        public void EntityTypeOrphanMappingBehavior_Throws_ArgumentException_When_EntityTypeId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.EntityTypeOrphanMappingBehavior(
                    Guid.Empty, OrphanMappingBehavior.Remove),
                "entityTypeId");
        }

        [Fact]
        public void EntityTypeOrphanMappingBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.EntityTypeOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.EntityTypeOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) 57);
            this.EntityTypeOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) (-1));
        }

        [Fact]
        public void EntityTypeOrphanMappingBehavior_Creates_Entity_Type_Parameter()
        {
            this.EntityTypeOrphanMappingBehaviorCreatesEntityTypeParameter(
                OrphanMappingBehavior.None);
            this.EntityTypeOrphanMappingBehaviorCreatesEntityTypeParameter(
                OrphanMappingBehavior.Deactivate);
            this.EntityTypeOrphanMappingBehaviorCreatesEntityTypeParameter(
                OrphanMappingBehavior.Remove);
        }

        [Fact]
        public void SourceSystemOrphanMappingBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () => DeploymentContextExtensions.SourceSystemOrphanMappingBehavior(
                    null, Guid.NewGuid(), OrphanMappingBehavior.None),
                "context");
        }

        [Fact]
        public void SourceSystemOrphanMappingBehavior_Throws_ArgumentException_When_SourceSystemId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.SourceSystemOrphanMappingBehavior(
                    Guid.Empty, OrphanMappingBehavior.Remove),
                "sourceSystemId");
        }

        [Fact]
        public void SourceSystemOrphanMappingBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.SourceSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.SourceSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) 57);
            this.SourceSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) (-1));
        }

        [Fact]
        public void SourceSystemOrphanMappingBehavior_Creates_Source_System_Parameter()
        {
            this.SourceSystemOrphanMappingBehaviorCreatesSourceSystemParameter(
                OrphanMappingBehavior.None);
            this.SourceSystemOrphanMappingBehaviorCreatesSourceSystemParameter(
                OrphanMappingBehavior.Deactivate);
            this.SourceSystemOrphanMappingBehaviorCreatesSourceSystemParameter(
                OrphanMappingBehavior.Remove);
        }

        [Fact]
        public void OrphanMappingBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () => DeploymentContextExtensions.OrphanMappingBehavior(
                    null, Guid.NewGuid(), Guid.NewGuid(), OrphanMappingBehavior.Remove),
                "context");
        }

        [Fact]
        public void OrphanMappingBehavior_Throws_ArgumentException_When_SourceSystemId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.OrphanMappingBehavior(
                    Guid.NewGuid(), Guid.Empty, OrphanMappingBehavior.Deactivate),
                "sourceSystemId");
        }

        [Fact]
        public void OrphanMappingBehavior_Throws_ArgumentException_When_EntityTypeId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.OrphanMappingBehavior(
                    Guid.Empty, Guid.NewGuid(), OrphanMappingBehavior.Remove),
                "entityTypeId");
        }

        [Fact]
        public void OrphanMappingBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.OrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.OrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) 57);
            this.OrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (OrphanMappingBehavior) (-1));
        }

        [Fact]
        public void OrphanMappingBehavior_Creates_Entity_Type_Source_System_Parameter()
        {
            this.OrphanMappingBehaviorCreatesEntityTypeSourceSystemParameter(
                OrphanMappingBehavior.None);
            this.OrphanMappingBehaviorCreatesEntityTypeSourceSystemParameter(
                OrphanMappingBehavior.Deactivate);
            this.OrphanMappingBehaviorCreatesEntityTypeSourceSystemParameter(
                OrphanMappingBehavior.Remove);
        }

        [Fact]
        public void DestinationSystemGarbageCollectionBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () =>
                    DeploymentContextExtensions.DestinationSystemGarbageCollectionBehavior(
                        null, Guid.NewGuid(), GarbageCollectionBehavior.DeleteCreated),
                "context");
        }

        [Fact]
        public void DestinationSystemGarbageCollectionBehavior_Throws_ArgumentException_When_DestinationSystemId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.DestinationSystemGarbageCollectionBehavior(
                    Guid.Empty, GarbageCollectionBehavior.DeleteMapped),
                "destinationSystemId");
        }

        [Fact]
        public void DestinationSystemGarbageCollectionBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.DestinationSystemGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.DestinationSystemGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (GarbageCollectionBehavior) (-1));
            this.DestinationSystemGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (GarbageCollectionBehavior) 79); 
        }

        [Fact]
        public void DestinationSystemGarbageCollectionBehavior_Creates_Destination_System_Parameter()
        {
            this.DestinationSystemGarbageCollectionBehaviorCreatesDestinationSystemParameter(
                GarbageCollectionBehavior.DeleteCreated);
            this.DestinationSystemGarbageCollectionBehaviorCreatesDestinationSystemParameter(
                GarbageCollectionBehavior.DeleteMapped);
            this.DestinationSystemGarbageCollectionBehaviorCreatesDestinationSystemParameter(
                GarbageCollectionBehavior.DeleteAll);
        }

        [Fact]
        public void EntityTypeGarbageCollectionBehavior_Throws_ArgumentNullException_When_Context_Null()
        {
            ArgumentTester.ThrowsArgumentNullException(
                () => DeploymentContextExtensions.EntityTypeGarbageCollectionBehavior(
                    null, Guid.NewGuid(), GarbageCollectionBehavior.DeleteAll),
                "context");
        }

        [Fact]
        public void EntityTypeGarbageCollectionBehavior_Throws_ArgumentException_When_EntityTypeId_Empty()
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.EntityTypeGarbageCollectionBehavior(
                    Guid.Empty, GarbageCollectionBehavior.DeleteAll),
                "entityTypeId");
        }

        [Fact]
        public void EntityTypeGarbageCollectionBehavior_Throws_ArgumentException_When_Behavior_Invalid()
        {
            this.EntityTypeGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(0);
            this.EntityTypeGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (GarbageCollectionBehavior) (-4));
            this.EntityTypeGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
                (GarbageCollectionBehavior) 100);
        }

        [Fact]
        public void EntityTypeGarbageCollectionBehavior_Creates_Entity_Type_Parameter()
        {
            this.EntityTypeGarbageCollectionBehaviorCreatesEntityTypeParameter(
                GarbageCollectionBehavior.DeleteCreated);
            this.EntityTypeGarbageCollectionBehaviorCreatesEntityTypeParameter(
                GarbageCollectionBehavior.DeleteMapped);
            this.EntityTypeGarbageCollectionBehaviorCreatesEntityTypeParameter(
                GarbageCollectionBehavior.DeleteAll);
        }

        private void DestinationSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            OrphanMappingBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.DestinationSystemOrphanMappingBehavior(
                    Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void DestinationSystemOrphanMappingBehaviorCreatesDestinationSystemParameter(
            OrphanMappingBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid destinationSystemId = Guid.NewGuid();
            context
                .Setup(dc => dc.DestinationSystemParameter(
                    destinationSystemId,
                    DeploymentContextExtensions.OrphanMappingBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context
                .Object
                .DestinationSystemOrphanMappingBehavior(
                    destinationSystemId, behavior);

            // assert
            context.Verify();
        }

        private void EntityTypeOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            OrphanMappingBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.EntityTypeOrphanMappingBehavior(
                    Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void EntityTypeOrphanMappingBehaviorCreatesEntityTypeParameter(
            OrphanMappingBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid entityTypeId = Guid.NewGuid();
            context
                .Setup(dc => dc.EntityTypeParameter(
                    entityTypeId,
                    DeploymentContextExtensions.OrphanMappingBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context
                .Object
                .EntityTypeOrphanMappingBehavior(entityTypeId, behavior);

            // assert
            context.Verify();
        }

        private void SourceSystemOrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            OrphanMappingBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.SourceSystemOrphanMappingBehavior(
                    Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void SourceSystemOrphanMappingBehaviorCreatesSourceSystemParameter(
            OrphanMappingBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid sourceSystemId = Guid.NewGuid();
            context
                .Setup(dc => dc.SourceSystemParameter(
                    sourceSystemId,
                    DeploymentContextExtensions.OrphanMappingBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context
                .Object
                .SourceSystemOrphanMappingBehavior(
                    sourceSystemId, behavior);

            // assert
            context.Verify();
        }

        private void OrphanMappingBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            OrphanMappingBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.OrphanMappingBehavior(
                    Guid.NewGuid(), Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void OrphanMappingBehaviorCreatesEntityTypeSourceSystemParameter(
            OrphanMappingBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid entityTypeId = Guid.NewGuid();
            Guid sourceSystemId = Guid.NewGuid();
            context
                .Setup(dc => dc.EntityTypeSourceSystemParameter(
                    entityTypeId,
                    sourceSystemId,
                    DeploymentContextExtensions.OrphanMappingBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context.Object.OrphanMappingBehavior(entityTypeId, sourceSystemId, behavior);

            // assert
            context.Verify();
        }

        private void DestinationSystemGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            GarbageCollectionBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.DestinationSystemGarbageCollectionBehavior(
                    Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void DestinationSystemGarbageCollectionBehaviorCreatesDestinationSystemParameter(
            GarbageCollectionBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid destinationSystemId = Guid.NewGuid();
            context
                .Setup(dc => dc.DestinationSystemParameter(
                    destinationSystemId,
                    DeploymentContextExtensions.GarbageCollectionBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context.Object.DestinationSystemGarbageCollectionBehavior(
                destinationSystemId, behavior);

            // assert
            context.Verify();
        }

        private void EntityTypeGarbageCollectionBehaviorThrowsArgumentExceptionWhenBehaviorInvalid(
            GarbageCollectionBehavior invalidBehavior)
        {
            var context = new Mock<IDeploymentContext>();
            ArgumentTester.ThrowsArgumentException(
                () => context.Object.EntityTypeGarbageCollectionBehavior(
                    Guid.NewGuid(), invalidBehavior),
                "behavior");
        }

        private void EntityTypeGarbageCollectionBehaviorCreatesEntityTypeParameter(
            GarbageCollectionBehavior behavior)
        {
            // arrange
            var context = new Mock<IDeploymentContext>(MockBehavior.Strict);
            Guid entityTypeId = Guid.NewGuid();
            context
                .Setup(dc => dc.EntityTypeParameter(
                    entityTypeId,
                    DeploymentContextExtensions.GarbageCollectionBehaviorParameterName,
                    behavior.ToString()))
                .Verifiable();

            // act
            context.Object.EntityTypeGarbageCollectionBehavior(entityTypeId, behavior);

            // assert
            context.Verify();
        }
    }
}
