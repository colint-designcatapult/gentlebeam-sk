using System.Collections.Generic;
using System.Linq;

namespace Heracles.Application.Helpers
{
    public class ModalityToLabelConverter
    {
        static readonly Dictionary<string, string> _modality2Label = new()
        {
            { "CFM", "RCM" },
            { "XC",  "Photo" },
        };

        static public string ToLabel(string Modality)
        {
            var r = _modality2Label.TryGetValue(Modality, out string Label);
            if (r == true)
                return Label;

            return Modality;
        }

        static public string FromLabel(string Label)
        {
            var r = _modality2Label.FirstOrDefault(x => x.Value == Label).Key;
            if (r == null) 
                return Label;

            return r;
        }

    }
}
