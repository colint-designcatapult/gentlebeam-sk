using Heracles.Core.Models.EMR;
using System;
using Xcc.Application.Models;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Indoor.Models
{
    public class PatientEntry : ObservableCRUDEntryNew<IPatient>
    {
        public PatientEntry(IAsyncСRUDCommands<IPatient> crudCommands, IPatient patient = null)
            : base(crudCommands, patient)
        {
        }

        public override bool CanDelete()
        {
            throw new NotImplementedException();
        }
    }
}
