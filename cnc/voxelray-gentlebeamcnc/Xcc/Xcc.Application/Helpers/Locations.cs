namespace Xcc.Application.Helpers
{
    public class Locations
    {
        public static string SeriesLocation(long patientId, long studyId, long seriesId)
        {
            return string.Format("Patient-{0}\\Study-{1}\\Series-{2}\\", patientId, studyId, seriesId);
        }
    }
}
