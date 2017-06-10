using System;
using PostSharp.Aspects;

namespace AOP
{
    [Serializable]
    public class LoggingPostSharpAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            base.OnEntry(args);
            LoggingHelper.ExecuteBefore(args.Method.Name, args.Arguments.ToArray());
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            base.OnSuccess(args);
            LoggingHelper.ExecuteAfter(args.Method.Name, args.ReturnValue);
        }
    }
}
