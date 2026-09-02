using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using Heracles.External.ViewModels.QualityCheck;
using Moq;
using NUnit.Framework;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Logging;

namespace Heracles.Outdoor.Test.ViewModels;

internal class QcReportServiceTests
{
    [Test]
    public async Task SaveQcSampleReportAsync_PersistsOnlySuppliedField()
    {
        const long configurationId = 44;
        var configuration = new Mock<ICollimatorConfiguration>();
        configuration.SetupGet(value => value.Id).Returns(configurationId);
        configuration.SetupGet(value => value.Type).Returns(TargetType.TargetType_30mm_SSD_7_Fields);
        configuration.SetupGet(value => value.Energy).Returns(Energy.Energy_50);
        var field = new QcSampleFieldEntry(configuration.Object, filamentSetpoint: 1.0)
        {
            Name = TreatmentFieldName.PlusC,
            Intensities = new QcReadings([10, 20, 30, 40, 50]),
        };

        var reportList = new Mock<IQcReportListModel>();
        reportList.SetupGet(value => value.CurrentCollimatorConfigurationId)
            .Returns(configurationId);
        var repository = new Mock<IQcRepository>();
        repository
            .Setup(value => value.CreateQcSampleAsync(
                It.IsAny<IQcSampleHeader>(),
                It.IsAny<IEnumerable<IQcSampleFieldEntry>>()))
            .Returns((IQcSampleHeader header, IEnumerable<IQcSampleFieldEntry> fields) =>
                Task.FromResult<(IQcSampleHeader, IEnumerable<QcField>)>((
                    header,
                    fields.Select(value => new QcField(
                        value.Name,
                        value.Intensities.Data
                            .Select(reading => (double?)reading)
                            .ToArray())))));
        var userStore = new Mock<IAuthorizedUserStore>();
        userStore.SetupGet(value => value.AuthorizedUser)
            .Returns(Mock.Of<IUser>(value => value.EmailAddress == "qa@example.test"));
        var service = new QcReportService(
            reportList.Object,
            repository.Object,
            userStore.Object,
            Mock.Of<ILogWriter>());

        await service.SaveQcSampleReportAsync(field);

        repository.Verify(value => value.CreateQcSampleAsync(
            It.IsAny<IQcSampleHeader>(),
            It.Is<IEnumerable<IQcSampleFieldEntry>>(fields =>
                fields.Count() == 1 && ReferenceEquals(fields.Single(), field))),
            Times.Once);
        reportList.Verify(value => value.AddNewSample(It.IsAny<QcSampleBindable>()), Times.Once);
    }
}
