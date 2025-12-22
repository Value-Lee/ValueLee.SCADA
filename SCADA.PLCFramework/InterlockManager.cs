using MoreLinq;
using SCADA.Common;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SCADA.PLCFramework
{
    public class InterlockManager
    {
        private Func<string, bool> _ioValueGetter;
        private Action<string, bool> _doValueSetter;

        private readonly Dictionary<InterlockAction, InterlockLimit[]> _dictActionLimits;
        private readonly Dictionary<int, F_TRIG> _triggers;

        public InterlockManager(Func<string, bool> ioValueGetter, Action<string, bool> doValueSetter)
        {
            _dictActionLimits = new Dictionary<InterlockAction, InterlockLimit[]>();
            _triggers = new Dictionary<int, F_TRIG>();
            _ioValueGetter = ioValueGetter;
            _doValueSetter = doValueSetter;
        }

        #region Main Features
        public IEnumerable<string> Monitor()
        {
            foreach (var action in _dictActionLimits)
            {
                if (action.Key.ByPass == false && action.Key.Reserve)
                {
                    foreach (var limit in action.Value)
                    {
                        if (limit.ByPass == false)
                        {
                            var trigger = _triggers[action.Key.GetHashCode() + limit.GetHashCode()];
                            trigger.CLK = _ioValueGetter(limit.Name) != limit.LimitValue;
                            if (trigger.Q)
                            {
                                _doValueSetter(action.Key.Name, !action.Key.ActionValue);
                                yield return "";
                                break;
                            }
                        }
                    }
                }
            }
        }

        public bool CanDO(InterlockAction action, out InterlockLimit interlockLimit)
        {
            interlockLimit = null;
            bool canDo = true;
            if (action.ByPass)
            {
                return canDo;
            }
            foreach (var limit in _dictActionLimits[action])
            {
                if (!limit.ByPass)
                {
                    if (_ioValueGetter(limit.Name) != limit.LimitValue)
                    {
                        canDo = false;
                        interlockLimit = limit;
                        break;
                    }
                }
            }
            return canDo;
        }

        #endregion

        #region Query
        public IReadOnlyDictionary<InterlockAction, InterlockLimit[]> GetAllActions()
        {
            return _dictActionLimits;
        }

        public IEnumerable<InterlockAction> GetAllInterLockActions()
        {
            return _dictActionLimits.Keys.Where(x => !x.ByPass);
        }

        public IEnumerable<InterlockAction> GetAllBypassActions()
        {
            return _dictActionLimits.Keys.Where(x => x.ByPass);
        }

        public IEnumerable<InterlockLimit> GetAllLimits(InterlockAction action)
        {
            return _dictActionLimits[action];
        }

        public IEnumerable<InterlockLimit> GetAllInterLockLimits(InterlockAction action)
        {
            return _dictActionLimits[action].Where(x => !x.ByPass);
        }

        public IEnumerable<InterlockLimit> GetAllBypassLimits(InterlockAction action)
        {
            return _dictActionLimits[action].Where(x => x.ByPass);
        }
        #endregion

        #region Set
        public void BypassInterLock(InterlockAction action)
        {
            action.ByPass = true;
        }

        public void BypassInterLock(InterlockAction action, InterlockLimit limit)
        {
            var target = _dictActionLimits[action].SingleOrDefault(x => x.Equals(limit));
            if (target != null)
            {
                target.ByPass = true;
            }
        }

        public void RestoreInterlock(InterlockAction action)
        {
            action.ByPass = false;
        }

        public void RestoreInterlock(InterlockAction action, InterlockLimit limit)
        {
            var target = _dictActionLimits[action].SingleOrDefault(x => x.Equals(limit));
            if (target != null)
            {
                target.ByPass = false;
            }
        }

        #endregion

        #region Load & Validation
        public void LoadFromFile(string interlockXmlFilePath, Encoding encoding = null)
        {
            Load(File.ReadAllText(interlockXmlFilePath, encoding == null ? Encoding.UTF8 : encoding));
        }

        private void Load(string interlockXmlContent)
        {
            ValidateXml(interlockXmlContent);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(interlockXmlContent);
            xmlDoc.SelectSingleNode("/Interlock").ChildNodes.Cast<XmlNode>().ForEach(actionNode =>
            {
                var interlockLimits = new List<InterlockLimit>();
                var actionName = actionNode.Attributes["do"].Value.Trim();
                var actionValue = bool.Parse(actionNode.Attributes["value"].Value.Trim());
                var reserveAttr = actionNode.Attributes["reserve"];
                var reserve = reserveAttr == null ? true : bool.Parse(reserveAttr.Value.Trim());
                var interlockAction = new InterlockAction(actionName, actionValue, reserve, interlockLimits);

                actionNode.ChildNodes.Cast<XmlNode>().ForEach(limitNode =>
                {
                    var diAttr = limitNode.Attributes["di"];
                    var doAttr = limitNode.Attributes["do"];
                    var limitName = diAttr != null ? diAttr.Value.Trim() : doAttr.Value.Trim();
                    var limitValue = bool.Parse(limitNode.Attributes["value"].Value.Trim());
                    var interlockLimit = new InterlockLimit(limitName, limitValue);
                    interlockLimits.Add(interlockLimit);
                    var triggerKey = interlockAction.GetHashCode() + interlockLimit.GetHashCode();
                    _triggers[triggerKey] = new F_TRIG();
                });

                _dictActionLimits[interlockAction] = interlockLimits.ToArray();
            });
        }

        private void ValidateXml(string xmlContent)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            // Add XML schema validation logic here if needed

            var roots = xmlDoc.SelectNodes("/Interlock");
            if (roots == null || roots.Count != 1)
            {
                throw new Exception("There should be exactly one root Interlock node.");
            }
            var root = roots[0];
            var actionNodes = root.SelectNodes("Action");
        }

        #endregion
    }
}
