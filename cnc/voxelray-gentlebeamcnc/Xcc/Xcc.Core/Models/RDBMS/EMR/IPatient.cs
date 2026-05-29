using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;

namespace Xcc.Core.Models.RDBMS.EMR
{
    public interface IPatient : IEntry
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string PatientId { get; set; }
        PatientIdType PatientIdType { get; set; }
        string MRN { get; set; }        
        Sex? Sex { get; set; }
        DateTime CreationDate { get; set; }
    }
}
