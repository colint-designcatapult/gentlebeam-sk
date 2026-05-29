using System;
using System.Collections.Generic;
using System.Xml;
using Heracles.Robot.Models.RobotArm.Interfaces;

namespace Heracles.Robot.Models.Sequences
{
    public class SequencesXMLProvider : ISequencesProvider
    {
        private IDictionary<string, ISequence> _sequences;
        private IList<string> _sequenceNames;
        public SequencesXMLProvider(string path_to_xml, IStepFactory stepFactory, ISequenceFactory sequenceFactory)
        {
            _sequences = new Dictionary<string, ISequence>();
            _sequenceNames = new List<string>();

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path_to_xml);
            XmlElement xRoot = xDoc.DocumentElement;

            XmlNodeList sequenceNodes = xRoot?.SelectNodes("sequence");
            if (sequenceNodes is not null)
            {
                foreach (XmlNode sequence in sequenceNodes)
                {
                    var sequenceName = sequence.Attributes["name"].Value.Trim();

                    var stepsList = new List<IStep>();

                    XmlNodeList stepNodes = sequence.SelectNodes("step");
                    foreach (XmlNode step in stepNodes)
                    {
                        // todo save id into step
                        var id = step.Attributes["id"]?.Value.Trim();
                        var nextIdIfOk = step.Attributes["id_ok"]?.Value.Trim();
                        var nextIdIfFailed = step.Attributes["id_failed"]?.Value.Trim();

                        XmlNodeList actuatorsPrecondition = step.SelectNodes("actuators_precondition");
                        XmlNodeList actionNodes = step.SelectNodes("action");
                        XmlNodeList valueNodes = step.SelectNodes("value");


                        if (actionNodes is not null && valueNodes is not null && actionNodes.Count > 0 && valueNodes.Count > 0)
                        {
                            var action = actionNodes[0].InnerText.Trim();
                            var value = valueNodes[0].InnerText;

                            var vals = value.Split(',');
                            var actionValuesList = new List<string>();
                            foreach (var val in vals)
                            {
                                actionValuesList.Add(val.Trim());
                            }

                            var actuatorsPreconditionList = new List<string>();
                            if (actuatorsPrecondition is not null && actuatorsPrecondition.Count > 0)
                            {
                                var preconditions = actuatorsPrecondition[0].InnerText.Trim().Split(',');
                                foreach (var p in preconditions)
                                {
                                    actuatorsPreconditionList.Add(p.Trim());
                                }
                            }

                            stepsList.Add(stepFactory.Create(id, nextIdIfOk, nextIdIfFailed, action, actionValuesList, actuatorsPreconditionList));
                        }
                        else
                        {
                            throw new ArgumentNullException();
                        }
                    }
                    _sequences[sequenceName] = sequenceFactory.Create(sequenceName, stepsList);
                    _sequenceNames.Add(sequenceName);
                }
            }
        }
        public ISequence Provide(string name)
        {
            return _sequences[name];
        }
        public IList<string> SequenceNames { get { return _sequenceNames; } }
    }

}
