using Heracles.Application.DeepColor.DataTypes;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heracles.Indoor.AppLayer.DeepColor
{
    public class AcquisitionService(
        IEmrSeriesCommands seriesCommands,
        IAcquisitionModel acquisitionModel)
    {
        public async Task<IEnumerable<ISeries>> FetchSeriesAsync(long diagnosisId)
        {
            var items = await seriesCommands.ReadListAsync(diagnosisId);

            acquisitionModel.Clear();
            foreach (var item in items.OrderBy(x => x.Id))
            {
                acquisitionModel.AddItem(item);
            }
            return acquisitionModel.Items;
        }

        public async Task<IEnumerable<Acquisition>> FetchAcquisitionsAsync(long diagnosisId)
        {
            var items = await FetchSeriesAsync(diagnosisId);
            // TODO: now we store acquisition values in the only fields we can store it as ISeries:
            return items
                .Where(x => x.Type == Core.Enums.ImageType.Photoacoustic)
                .Select(x => new Acquisition { Id = x.NumberOfInstances, Name = x.Name, Date = DateTimeOffset.Now.ToUnixTimeMilliseconds() });
        }

        public async Task<Acquisition> CreateAcquisitionAsync(Acquisition value, long visitId, long diagnosisId)
        {
            // Convert to series and store:
            var series = new Series()
            {
                DiagnosisId = diagnosisId,
                VisitId = visitId,
                Type = Core.Enums.ImageType.Photoacoustic,
                Location = "pack://application:,,,/Xcc.Application;Component/UI/Resources/Images/DemoImageSet/DeepColorImages/1.png",
                // TODO: now we store acquisition values in the only fields we can store it as ISeries:
                NumberOfInstances = value.Id,
                Name = value.Name,
            };
            var item = await seriesCommands.CreateAsync(series);

            // TODO: now we store acquisition values in the only fields we can store it as ISeries:
            int id = item.NumberOfInstances;
            string name = item.Name;
            
            // Add item to the model:
            acquisitionModel.AddItem(item);

            var storedAcquisition = new Acquisition { Id = id, Name = name, Date = DateTimeOffset.Now.ToUnixTimeMilliseconds() };
            return storedAcquisition;
        }
    }
}
