using Castle.DynamicProxy;

namespace AOP
{
    public class LoggingInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            ExecuteBefore(invocation);
            invocation.Proceed();
            ExecuteAfter(invocation);
        }

        private void ExecuteAfter(IInvocation invocation)
        {
            LoggingHelper.ExecuteAfter(invocation.Method.Name, invocation.ReturnValue);
        }

        private void ExecuteBefore(IInvocation invocation)
        {
            LoggingHelper.ExecuteBefore(invocation.Method.Name, invocation.Arguments);
        }
    }
}
