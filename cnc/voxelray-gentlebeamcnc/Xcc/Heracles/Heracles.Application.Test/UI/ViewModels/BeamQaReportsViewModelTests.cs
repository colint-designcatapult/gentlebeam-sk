using System.Collections.ObjectModel;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Application.UI.ViewModels;
using Heracles.Core.Enums;
using Moq;
using Prism.Events;
using Prism.Services.Dialogs;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Application.Test.UI.ViewModels;

internal sealed class BeamQaReportsViewModelTests
{
    [Test]
    public async Task InactiveApplicatorConfiguration_RemainsAvailableForHistoricalQaData()
    {
        var configuration = new CollimatorConfiguration
        {
            Id = 9,
            Type = TargetType.TargetType_50mm_SSD_20mm_Field,
            Energy = Energy.Energy_50,
        };
        var collimatorModel = new CollimatorModel();
        collimatorModel.Reset(
            Mock.Of<IHead>(),
            [configuration],
            []);

        var reportItems = new ObservableCollection<QcSampleBindable>();
        var reportList = new Mock<IQcReportListModel>();
        reportList.SetupGet(model => model.Items).Returns(reportItems);

        var repository = new Mock<IQcRepository>();
        repository.Setup(repo => repo.FetchQcSampleListAsync(configuration.Id))
            .ReturnsAsync([]);

        var viewModel = new BeamQaReportsViewModel(
            new EventAggregator(),
            reportList.Object,
            Mock.Of<IPopUpService>(),
            Mock.Of<IDialogService>(),
            Mock.Of<IDispatcherService>(),
            collimatorModel,
            repository.Object,
            new QcReportListService(reportList.Object, repository.Object),
            Mock.Of<ILogWriter>());

        viewModel.CollimatorType = configuration.Type;
        viewModel.Energy = configuration.Energy;
        await viewModel.CurrentQcTask.Task;

        Assert.That(viewModel.AvailableTargetTypeValues, Does.Contain(configuration.Type));
        repository.Verify(
            repo => repo.FetchQcSampleListAsync(configuration.Id),
            Times.Once);
    }
}
