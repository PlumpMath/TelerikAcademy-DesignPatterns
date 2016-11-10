using Dealership.Contracts;
using Ninject;
using Ninject.Extensions.Interception;

namespace Dealership.ConsoleClient.Interceptors
{
    public class UserAuthorizationInterceptor : IInterceptor
    {
        private const string UserNotLogged = "You are not logged! Please login first!";

        public void Intercept(IInvocation invocation)
        {
            var userService = invocation.Request.Kernel.Get<IUserService>();
            if (userService.LoggedUser == null)
            {
                invocation.ReturnValue = UserNotLogged;
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
