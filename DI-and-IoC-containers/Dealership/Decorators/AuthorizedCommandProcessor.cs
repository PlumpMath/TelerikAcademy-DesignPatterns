using Dealership.Engine;
using Dealership.Contracts;
using System;
using Dealership.Common;

namespace Dealership.Decorators
{
    public class AuthorizedCommandProcessor : IChainableCommandProcessor
    {
        private const string UserNotLogged = "You are not logged! Please login first!";
        
        private readonly IChainableCommandProcessor commandProcessor;
        private readonly IUserService userService;

        public AuthorizedCommandProcessor(IChainableCommandProcessor commandProcessor, IUserService userService)
        {
            Validator.ValidateNull(commandProcessor, "commandProcessor");
            Validator.ValidateNull(userService, "userService");

            this.commandProcessor = commandProcessor;
            this.userService = userService;
        }

        public IChainableCommandProcessor Successor { get; set; }

        public string Process(ICommand command)
        {
            if (this.userService.LoggedUser != null)
            {
                this.commandProcessor.Successor = this.Successor;

                return this.commandProcessor.Process(command);
            }
            else
            {
                return UserNotLogged;
            }
        }
    }
}
