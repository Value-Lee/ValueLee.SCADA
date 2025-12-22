using SCADA.TimerFSM.Interfaces;
using System;
using System.Collections.Generic;

namespace SCADA.TimerFSM
{
    #region StepAction
    public delegate void StepAction(StepContext context);
    public delegate void StepAction<TParam>(StepContext context, TParam param);
    public delegate void StepAction<TParam1, TParam2>(StepContext context, TParam1 param1, TParam2 param2);
    public delegate void StepAction<TParam1, TParam2, TParam3>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3);
    public delegate void StepAction<TParam1, TParam2, TParam3, TParam4>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    public delegate void StepAction<TParam1, TParam2, TParam3, TParam4, TParam5>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);

    public delegate void StepRefAction<TRefParam>(StepContext context, ref TRefParam param);
    public delegate void StepRefAction<TRefParam1, TRefParam2>(StepContext context, ref TRefParam1 param1, ref TRefParam2 param2);
    public delegate void StepRefAction<TRefParam1, TRefParam2, TRefParam3>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3);
    public delegate void StepRefAction<TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3, ref TRefParam4 param4);
    public delegate void StepRefAction<TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3, ref TRefParam4 param4, ref TRefParam5 param5);

    public delegate void Step1Val1RefAction<TParam, TRefParam>(StepContext context, TParam param, ref TRefParam refParam);
    public delegate void Step1Val2RefAction<TParam, TRefParam1, TRefParam2>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate void Step1Val3RefAction<TParam, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate void Step1Val4RefAction<TParam, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate void Step1Val5RefAction<TParam, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    public delegate void Step2Val1RefAction<TParam1, TParam2, TRefParam>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam refParam);
    public delegate void Step2Val2RefAction<TParam1, TParam2, TRefParam1, TRefParam2>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate void Step2Val3RefAction<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate void Step2Val4RefAction<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate void Step2Val5RefAction<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    public delegate void Step3Val1RefAction<TParam1, TParam2, TParam3, TRefParam>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam refParam);
    public delegate void Step3Val2RefAction<TParam1, TParam2, TParam3, TRefParam1, TRefParam2>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate void Step3Val3RefAction<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate void Step3Val4RefAction<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate void Step3Val5RefAction<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    #endregion

    #region StepCheck
    public delegate bool StepCheck(StepContext context);
    public delegate bool StepCheck<TParam>(StepContext context, TParam param);
    public delegate bool StepCheck<TParam1, TParam2>(StepContext context, TParam1 param1, TParam2 param2);
    public delegate bool StepCheck<TParam1, TParam2, TParam3>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3);
    public delegate bool StepCheck<TParam1, TParam2, TParam3, TParam4>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    public delegate bool StepCheck<TParam1, TParam2, TParam3, TParam4, TParam5>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);

    public delegate bool StepRefCheck<TRefParam>(StepContext context, ref TRefParam param);
    public delegate bool StepRefCheck<TRefParam1, TRefParam2>(StepContext context, ref TRefParam1 param1, ref TRefParam2 param2);
    public delegate bool StepRefCheck<TRefParam1, TRefParam2, TRefParam3>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3);
    public delegate bool StepRefCheck<TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3, ref TRefParam4 param4);
    public delegate bool StepRefCheck<TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, ref TRefParam1 param, ref TRefParam2 param2, ref TRefParam3 param3, ref TRefParam4 param4, ref TRefParam5 param5);

    public delegate bool Step1Val1RefCheck<TParam, TRefParam>(StepContext context, TParam param, ref TRefParam refParam);
    public delegate bool Step1Val2RefCheck<TParam, TRefParam1, TRefParam2>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate bool Step1Val3RefCheck<TParam, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate bool Step1Val4RefCheck<TParam, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate bool Step1Val5RefCheck<TParam, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam param, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    public delegate bool Step2Val1RefCheck<TParam1, TParam2, TRefParam>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam refParam);
    public delegate bool Step2Val2RefCheck<TParam1, TParam2, TRefParam1, TRefParam2>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate bool Step2Val3RefCheck<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate bool Step2Val4RefCheck<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate bool Step2Val5RefCheck<TParam1, TParam2, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam1 param1, TParam2 param2, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    public delegate bool Step3Val1RefCheck<TParam1, TParam2, TParam3, TRefParam>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam refParam);
    public delegate bool Step3Val2RefCheck<TParam1, TParam2, TParam3, TRefParam1, TRefParam2>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2);
    public delegate bool Step3Val3RefCheck<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3);
    public delegate bool Step3Val4RefCheck<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3, TRefParam4>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4);
    public delegate bool Step3Val5RefCheck<TParam1, TParam2, TParam3, TRefParam1, TRefParam2, TRefParam3, TRefParam4, TRefParam5>(StepContext context, TParam1 param1, TParam2 param2, TParam3 param3, ref TRefParam1 refParam1, ref TRefParam2 refParam2, ref TRefParam3 refParam3, ref TRefParam4 refParam4, ref TRefParam5 refParam5);

    #endregion

    public class StepContext
    {
        public StepContext()
        {
            _stepData = new Dictionary<string, object>();
        }

        public bool TryGetStepData(string key, out object value)
        {
            if (_stepData.TryGetValue(key, out value))
            {
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetStepData<T>(string key, out T value)
        {
            if (_stepData.TryGetValue(key, out object value1))
            {
                value = (T)value1;
                return true;
            }
            value = default;
            return false;
        }

        public void SetStepData(string key, object value)
        {
            _stepData[key] = value;
        }

        public void ClearStepData()
        {
            _stepData.Clear();
        }

        public string Error { get; set; }=null;
        public Exception Exception { get; set; } = null;
        public object[] MsgArgs { get; set; } = null;

        private readonly Dictionary<string, object> _stepData = null;
        public object SharedVariable { get; set; } = null;
        public IFsmMessenger StateMachine { get; set; } = null;
    }
}