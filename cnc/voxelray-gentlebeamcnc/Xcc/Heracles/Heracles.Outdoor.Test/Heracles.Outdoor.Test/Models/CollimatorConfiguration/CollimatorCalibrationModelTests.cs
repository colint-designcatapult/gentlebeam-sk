using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using Heracles.External.Models.CollimatorConfiguration;
using Moq;
using NUnit.Framework;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using CollimatorConfigurationEntity = Heracles.Application.Domain.DataManagement.System.Collimators.CollimatorConfiguration;

namespace Heracles.Outdoor.Test.Calibration;

[TestFixture]
internal sealed class CollimatorCalibrationModelTests
{
    [Test]
    public async Task FetchCalibrationDataAsync_WhenEveryLoadFails_RetriesAndLogsConfiguration()
    {
        var configuration = CreateConfiguration(1, TargetType.TargetType_50mm_SSD_20mm_Field);
        var model = new CollimatorModel();
        model.Reset(new Head { Id = 1 }, [configuration], []);
        var calibrationInfo = Mock.Of<ICollimatorCalibrationInfo>();
        var repository = new Mock<ICollimatorCalibrationRepository>();
        repository
            .SetupSequence(value => value.FetchConfigurationInfoAsync(configuration))
            .ThrowsAsync(new InvalidOperationException("database unavailable"))
            .ReturnsAsync(calibrationInfo);
        var logWriter = new Mock<ILogWriter>();
        logWriter
            .Setup(value => value.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
            .Returns(Task.CompletedTask);
        var sut = new CollimatorCalibrationModel(
            model,
            repository.Object,
            logWriter.Object,
            new CollimatorCalibrationInfoStore());

        Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.FetchCalibrationDataAsync());
        CollimatorCalibrationInfoStore result = await sut.FetchCalibrationDataAsync();

        Assert.That(result[configuration.Id], Is.SameAs(calibrationInfo));
        repository.Verify(
            value => value.FetchConfigurationInfoAsync(configuration),
            Times.Exactly(2));
        logWriter.Verify(
            value => value.LogAsync(
                It.Is<string>(message => message.Contains("id=1") && message.Contains("database unavailable")),
                LogRecordSeverity.Warn,
                LogRecordType.System),
            Times.Once);
    }

    [Test]
    public async Task FetchCalibrationDataAsync_WhenConfigurationsAreReplaced_InvalidatesCachedSnapshot()
    {
        var firstConfiguration = CreateConfiguration(1, TargetType.TargetType_50mm_SSD_20mm_Field);
        var secondConfiguration = CreateConfiguration(2, TargetType.TargetType_50mm_SSD_50mm_Field);
        var model = new CollimatorModel();
        model.Reset(new Head { Id = 1 }, [firstConfiguration], []);
        var firstInfo = Mock.Of<ICollimatorCalibrationInfo>();
        var secondInfo = Mock.Of<ICollimatorCalibrationInfo>();
        var repository = new Mock<ICollimatorCalibrationRepository>();
        repository
            .Setup(value => value.FetchConfigurationInfoAsync(firstConfiguration))
            .ReturnsAsync(firstInfo);
        repository
            .Setup(value => value.FetchConfigurationInfoAsync(secondConfiguration))
            .ReturnsAsync(secondInfo);
        var sut = new CollimatorCalibrationModel(
            model,
            repository.Object,
            Mock.Of<ILogWriter>(),
            new CollimatorCalibrationInfoStore());

        CollimatorCalibrationInfoStore firstResult = await sut.FetchCalibrationDataAsync();
        Assert.That(firstResult[firstConfiguration.Id], Is.SameAs(firstInfo));

        model.Reset(new Head { Id = 1 }, [secondConfiguration], []);
        CollimatorCalibrationInfoStore secondResult = await sut.FetchCalibrationDataAsync();

        Assert.Multiple(() =>
        {
            Assert.That(secondResult[firstConfiguration.Id], Is.Null);
            Assert.That(secondResult[secondConfiguration.Id], Is.SameAs(secondInfo));
        });
        repository.Verify(
            value => value.FetchConfigurationInfoAsync(firstConfiguration),
            Times.Once);
        repository.Verify(
            value => value.FetchConfigurationInfoAsync(secondConfiguration),
            Times.Once);
    }

    [Test]
    public async Task FetchCalibrationDataAsync_WhenForced_RefreshesSuccessfulCache()
    {
        var configuration = CreateConfiguration(1, TargetType.TargetType_50mm_SSD_20mm_Field);
        var model = new CollimatorModel();
        model.Reset(new Head { Id = 1 }, [configuration], []);
        var repository = new Mock<ICollimatorCalibrationRepository>();
        repository
            .Setup(value => value.FetchConfigurationInfoAsync(configuration))
            .ReturnsAsync(Mock.Of<ICollimatorCalibrationInfo>());
        var sut = new CollimatorCalibrationModel(
            model,
            repository.Object,
            Mock.Of<ILogWriter>(),
            new CollimatorCalibrationInfoStore());

        await sut.FetchCalibrationDataAsync();
        await sut.FetchCalibrationDataAsync(forceRefresh: true);

        repository.Verify(
            value => value.FetchConfigurationInfoAsync(configuration),
            Times.Exactly(2));
    }

    private static CollimatorConfigurationEntity CreateConfiguration(long id, TargetType type) => new()
    {
        Id = id,
        Type = type,
        Energy = Energy.Energy_50,
    };
}
