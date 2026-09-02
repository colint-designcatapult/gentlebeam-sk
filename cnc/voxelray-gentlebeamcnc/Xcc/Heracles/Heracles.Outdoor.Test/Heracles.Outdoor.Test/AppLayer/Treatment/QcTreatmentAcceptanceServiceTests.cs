using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using Heracles.External.AppLayer.Treatment;
using Moq;
using NUnit.Framework;

namespace Heracles.Outdoor.Test.AppLayer.Treatment;

internal class QcTreatmentAcceptanceServiceTests
{
    private const long ConfigurationId = 42;

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_NoSamples_ReturnsMissing()
    {
        var repository = ConfigureRepository([]);
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Missing));
        repository.Verify(value => value.FetchQcFieldsAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_LatestSampleOlderThan24Hours_ReturnsMissing()
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-25), referenced: true);
        var repository = ConfigureRepository([reference]);
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Missing));
        repository.Verify(value => value.FetchQcFieldsAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_ApprovedLatestSample_ReturnsAcceptedWithoutReference()
    {
        var approved = CreateHeader(1, DateTime.Now.AddHours(-1), approvedBy: "physicist@example.test");
        var repository = ConfigureRepository([approved]);
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Accepted));
        repository.Verify(value => value.FetchQcFieldsAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_FreshSampleWithoutReference_ReturnsNoReference()
    {
        var latest = CreateHeader(1, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository([latest]);
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.NoReference));
        repository.Verify(value => value.FetchQcFieldsAsync(It.IsAny<long>()), Times.Never);
    }

    [TestCase(102.9, QcAcceptanceStatus.Accepted)]
    [TestCase(103.1, QcAcceptanceStatus.Failed)]
    [TestCase(97.1, QcAcceptanceStatus.Accepted)]
    [TestCase(96.9, QcAcceptanceStatus.Failed)]
    public async Task QcDeviationAcceptanceTestAsync_AppliesSymmetricThreePercentThreshold(
        double latestValue,
        QcAcceptanceStatus expected)
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(2, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [latest, reference],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [reference.Id] = [CreateField(TreatmentFieldName.PlusC, 100)],
                [latest.Id] = [CreateField(TreatmentFieldName.PlusC, latestValue)]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.Multiple(() =>
        {
            Assert.That(QcTreatmentAcceptanceService.QcDeviationThreshold, Is.EqualTo(3));
            Assert.That(result, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_AllQaFieldsMustPass()
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(2, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [reference, latest],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [reference.Id] =
                [
                    CreateField(TreatmentFieldName.Plus1L1, 100, 100),
                    CreateField(TreatmentFieldName.Plus1R1, 100, 100)
                ],
                [latest.Id] =
                [
                    CreateField(TreatmentFieldName.Plus1L1, 101, 99),
                    CreateField(TreatmentFieldName.Plus1R1, 150, 150)
                ]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Failed));
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_ZeroValuedUnusedQaChannels_DoNotFail()
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(2, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [reference, latest],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [reference.Id] =
                [
                    CreateField(TreatmentFieldName.Plus1L1, 100, 100, 0, 0, 0),
                    CreateField(TreatmentFieldName.Plus1R1, 200, 200, 0, 0, 0)
                ],
                [latest.Id] =
                [
                    CreateField(TreatmentFieldName.Plus1L1, 101, 99, 0, 0, 0),
                    CreateField(TreatmentFieldName.Plus1R1, 202, 198, 0, 0, 0)
                ]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Accepted));
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_ZeroReferenceChannelBecomesNonzero_ReturnsFailed()
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(2, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [reference, latest],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [reference.Id] = [CreateField(TreatmentFieldName.Plus1L1, 100, 100, 0, 0, 0)],
                [latest.Id] = [CreateField(TreatmentFieldName.Plus1L1, 101, 99, 1, 0, 0)]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Failed));
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_MissingQaField_ReturnsFailed()
    {
        var reference = CreateHeader(1, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(2, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [reference, latest],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [reference.Id] =
                [
                    CreateField(TreatmentFieldName.Plus1L1, 100, 100),
                    CreateField(TreatmentFieldName.Plus1R1, 100, 100)
                ],
                [latest.Id] = [CreateField(TreatmentFieldName.Plus1L1, 101, 99)]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Failed));
    }

    [Test]
    public async Task QcDeviationAcceptanceTestAsync_MultipleReferences_UsesNewestReferenceShownInHistory()
    {
        var olderReference = CreateHeader(1, DateTime.Now.AddHours(-3), referenced: true);
        var newerReference = CreateHeader(2, DateTime.Now.AddHours(-2), referenced: true);
        var latest = CreateHeader(3, DateTime.Now.AddHours(-1));
        var repository = ConfigureRepository(
            [latest, olderReference, newerReference],
            new Dictionary<long, IEnumerable<QcField>>
            {
                [olderReference.Id] = [CreateField(TreatmentFieldName.PlusC, 100)],
                [newerReference.Id] = [CreateField(TreatmentFieldName.PlusC, 200)],
                [latest.Id] = [CreateField(TreatmentFieldName.PlusC, 202)]
            });
        var service = new QcTreatmentAcceptanceService(repository.Object);

        var result = await service.QcDeviationAcceptanceTestAsync(ConfigurationId);

        Assert.That(result, Is.EqualTo(QcAcceptanceStatus.Accepted));
    }

    private static Mock<IQcRepository> ConfigureRepository(
        IEnumerable<IQcSampleHeader> headers,
        IReadOnlyDictionary<long, IEnumerable<QcField>>? fieldsBySample = null)
    {
        var repository = new Mock<IQcRepository>();
        repository
            .Setup(value => value.FetchQcSampleListAsync(ConfigurationId))
            .ReturnsAsync(headers);
        repository
            .Setup(value => value.FetchQcFieldsAsync(It.IsAny<long>()))
            .Returns((long sampleId) => Task.FromResult(
                fieldsBySample is not null && fieldsBySample.TryGetValue(sampleId, out var fields)
                    ? fields
                    : Enumerable.Empty<QcField>()));
        return repository;
    }

    private static QcSampleHeader CreateHeader(
        long id,
        DateTime creationDate,
        bool referenced = false,
        string approvedBy = "") =>
        new(id)
        {
            CollimatorConfigurationId = ConfigurationId,
            CreationDate = creationDate,
            Referenced = referenced,
            ApprovedBy = approvedBy,
            EmissionCurrent = 1
        };

    private static QcField CreateField(
        TreatmentFieldName name,
        params double?[] values) =>
        new(name, values);
}
