using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.System;
using Moq;

namespace Heracles.Application.Test.AppLayer.Collimators;

public class CollimatorServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task CreateCollimatorAsync_PersistsRequestedActiveState(bool isActive)
    {
        var model = CreateModel(out var head, out var configuration);
        var repository = new Mock<ICollimatorRepository>();
        repository
            .Setup(value => value.CreateCollimatorAsync("serial", head, configuration, isActive))
            .ReturnsAsync(new Collimator
            {
                Id = 10,
                Serial = "serial",
                HeadId = head.Id,
                CollimatorConfigurationId = configuration.Id,
                IsActive = isActive,
            });
        var sut = new CollimatorService(model, repository.Object);

        ICollimator result = await sut.CreateCollimatorAsync(
            "serial",
            configuration.Type,
            configuration.Energy,
            isActive);

        Assert.That(result.IsActive, Is.EqualTo(isActive));
        repository.Verify(
            value => value.CreateCollimatorAsync("serial", head, configuration, isActive),
            Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdateCollimatorAsync_PersistsRequestedActiveState(bool isActive)
    {
        var model = CreateModel(out _, out var configuration);
        var existingCollimator = new Collimator
        {
            Id = 10,
            Serial = "serial",
            CollimatorConfigurationId = configuration.Id,
            IsActive = !isActive,
        };
        model.AddCollimator(existingCollimator);
        var repository = new Mock<ICollimatorRepository>();
        repository
            .Setup(value => value.UpdateCollimatorAsync(existingCollimator, It.IsAny<ICollimator>()))
            .ReturnsAsync((ICollimator _, ICollimator updated) => updated);
        var sut = new CollimatorService(model, repository.Object);

        ICollimator result = await sut.UpdateCollimatorAsync(
            "serial",
            configuration.Type,
            configuration.Energy,
            isActive);

        Assert.That(result.IsActive, Is.EqualTo(isActive));
        repository.Verify(
            value => value.UpdateCollimatorAsync(
                existingCollimator,
                It.Is<ICollimator>(updated => updated.IsActive == isActive)),
            Times.Once);
    }

    private static CollimatorModel CreateModel(
        out IHead head,
        out ICollimatorConfiguration configuration)
    {
        head = new Head { Id = 1, Serial = "head" };
        configuration = new CollimatorConfiguration
        {
            Id = 2,
            Type = TargetType.TargetType_50mm_SSD_13_Fields,
            Energy = Energy.Energy_50,
            SsdType = SsdType.SsdType50mm,
        };
        var model = new CollimatorModel();
        model.Reset(head);
        model.AddConfiguration(configuration);
        return model;
    }
}
