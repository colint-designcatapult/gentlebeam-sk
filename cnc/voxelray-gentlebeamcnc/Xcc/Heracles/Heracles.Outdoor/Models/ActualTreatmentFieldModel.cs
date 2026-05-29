using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Core.Logging;

namespace Heracles.External.Models
{
    public interface IActualTreatmentFieldModel
    {
        ICollection<IActualTreatmentField> Collection { get; }
        double AverageEnergy { get; }
        
        Task FetchCollection(IList<long> treatmentIds);
        Task<ICollection<IActualTreatmentField>> FetchCollection(long treatmentId);
        Task<IActualTreatmentField> SaveActualTreatmentField(IActualTreatmentField atf);
        void StartCalculatingAverageEnergy();
        double AddEnergyValue(double energy);
    }

    public class ActualTreatmentFieldModel : IActualTreatmentFieldModel
    {
        private double _sumOfEnergyValues = 0.0;
        private int _averageEnergyStepCounter = 0;

        #region Properties
        public ILogRepository LogWriter { get; }
        public IEmrActualTreatmentFieldCommands ActualTreatmentFieldCommands { get; }
        public ICollection<IActualTreatmentField> Collection { get; private set; }
        public double AverageEnergy { get; private set;}
        #endregion

        public ActualTreatmentFieldModel(ILogRepository logWriter,
            IEmrActualTreatmentFieldCommands actualTreatmentFieldCommands)
        {
            LogWriter = logWriter;
            ActualTreatmentFieldCommands = actualTreatmentFieldCommands;
        }

        public async Task<ICollection<IActualTreatmentField>> FetchCollection(long treatmentId)
        {
            Collection = await ActualTreatmentFieldCommands.ReadListAsync(treatmentId);
            return Collection;
        }

        public async Task FetchCollection(IList<long> treatmentIds)
        {
            Collection = await ActualTreatmentFieldCommands.ReadListAsync(treatmentIds[0]);

            for (int i = 1; i < treatmentIds.Count; i++)
            {
                {
                    var treatmentCollection = await ActualTreatmentFieldCommands.ReadListAsync(treatmentIds[i]);
                    foreach (var atf in treatmentCollection)
                    {
                        var existing = Collection.FirstOrDefault(x => x.Name == atf.Name);
                        if (existing == null)
                            Collection.Add(atf);
                        else
                        {
                            existing.ActualCurrent = atf.ActualCurrent;
                            existing.TreatmentId = atf.TreatmentId;
                            existing.CreationDate = atf.CreationDate;
                            existing.Completed = atf.Completed;
                            existing.DisplayValue = atf.DisplayValue;

                            //existing.DwellTime += atf.DwellTime;
                            //existing.ActualDose += atf.ActualDose;
                        }
                    }
                }
            }
        }

        public async Task<IActualTreatmentField> SaveActualTreatmentField(IActualTreatmentField atf)
        {
            var existingEntry = Collection?.FirstOrDefault(x => x.Name == atf.Name);

            if (existingEntry == null)
            {
                var newEntry = await ActualTreatmentFieldCommands.CreateAsync(atf);
                if (newEntry != null)
                {
                    if (Collection == null)
                    {
                        if (atf.TreatmentId > 0L)
                            await FetchCollection(atf.TreatmentId);
                        else 
                            Collection = new List<IActualTreatmentField>();
                    }

                    Collection.Add(newEntry);
                    existingEntry = newEntry;
                }
                else
                {
                    //todo: save data in case when Moses connection is lost
                    throw new Exception("Failed to save new ActualTreatmentField"); // todo: write to a file instead
                }
            }
            else
            {
                atf.Id = existingEntry.Id;
                existingEntry = await ActualTreatmentFieldCommands.UpdateAsync(existingEntry, atf);
            }


            return existingEntry;
        }

        public void StartCalculatingAverageEnergy()
        {
            _sumOfEnergyValues = 0;
            _averageEnergyStepCounter = 0;
        }

        public double AddEnergyValue(double energy)
        {
            _sumOfEnergyValues += energy;
            _averageEnergyStepCounter++;

            AverageEnergy = _sumOfEnergyValues / _averageEnergyStepCounter;
            return AverageEnergy;
        }
    }
}
