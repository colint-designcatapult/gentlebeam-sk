using Heracles.Core.Models.EMR;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xcc.Core.Services;

namespace Heracles.Indoor.AppLayer.DeepColor
{
    public interface IAcquisitionModel
    {
        ObservableCollection<ISeries> Items { get; }

        void AddItem(ISeries item);
        void Clear();
    }

    public class AcquisitionModel(IDispatcherService dispatcherService) : IAcquisitionModel
    {
        public ObservableCollection<ISeries> Items { get; private set; } = new ObservableCollection<ISeries>();

        public void AddItem(ISeries item)
        {
            if (Items.FirstOrDefault(x => x.Id == item.Id) != null)
            {
                throw new ArgumentException($"AcquisitionModel.AddItem error: item {item.Id} already exists");
            }
            dispatcherService.Invoke(() =>
            {
                Items.Add(item);
            });
        }

        public void Clear()
        {
            dispatcherService.Invoke(() =>
            {
                Items.Clear();
            });
        }
    }
}
