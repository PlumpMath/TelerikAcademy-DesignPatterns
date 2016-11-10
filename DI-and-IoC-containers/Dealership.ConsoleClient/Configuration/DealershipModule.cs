using Dealership.CommandProcessors;
using Dealership.ConsoleClient.Common;
using Dealership.Contracts;
using Dealership.Decorators;
using Dealership.Engine;
using Dealership.Models;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dealership.ConsoleClient.Configuration
{
    public class DealershipModule : NinjectModule
    {
        private const string AddCommentCommandProcessorName = "AddCommentCommandProcessor";
        private const string AddVehicleCommandProcessorName = "AddVehicleCommandProcessor";
        private const string LoginCommandProcessorName = "LoginCommandProcessor";
        private const string LogoutCommandProcessorName = "LogoutCommandProcessor";
        private const string RegisterCommandProcessorName = "RegisterCommandProcessor";
        private const string RemoveCommentCommandProcessorName = "RemoveCommentCommandProcessor";
        private const string RemoveVehicleCommandProcessorName = "RemoveVehicleCommandProcessor";
        private const string ShowUsersCommandProcessorName = "ShowUsersCommandProcessor";
        private const string ShowVehiclesCommandProcessorName = "ShowVehiclesCommandProcessorName";
        private const string AuthorizedCommandProcessorName = "AuthorizedCommandProcessor";

        private const string CommanProcessorContructorArgumentName = "commandProcessor";
        private const string UserServiceContructorArgumentName = "userService";

        private const string CarName = "Car";
        private const string MotorcycleName = "Motorcycle";
        private const string TruckName = "Truck";

        private const string UserNotLogged = "You are not logged! Please login first!";

        public override void Load()
        {
            var typesToExclude = new List<Type>() { typeof(UserService) };
            Kernel.Bind(x =>
            {
                x.FromAssembliesInPath(Path.GetDirectoryName(Assembly.GetAssembly(typeof(IEngine)).Location))
                .SelectAllClasses()
                .Excluding(typesToExclude)
                .BindDefaultInterface();
            });

            Bind<IEngine>().To<DealershipEngine>().InSingletonScope();

            Bind<IUserService>().To<UserService>().InSingletonScope();

            Bind<IInputReader>().To<ConsoleInputReader>();
            Bind<IOutputWriter>().To<ConsoleOutputWriter>();

            Bind<IUserFactory>().ToFactory().InSingletonScope();
            Bind<IVehicleFactory>().ToFactory().InSingletonScope();
            Bind<ICommentFactory>().ToFactory().InSingletonScope();
            Bind<ICommandFactory>().ToFactory().InSingletonScope();

            Bind<IVehicle>().To<Car>().Named(CarName);
            Bind<IVehicle>().To<Motorcycle>().Named(MotorcycleName);
            Bind<IVehicle>().To<Truck>().Named(TruckName);

            Bind<IChainableCommandProcessor>().To<AddCommentCommandProcessor>().Named(AddCommentCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<AddVehicleCommandProcessor>().Named(AddVehicleCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<LoginCommandProcessor>().Named(LoginCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<LogoutCommandProcessor>().Named(LogoutCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RegisterCommandProcessor>().Named(RegisterCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RemoveCommentCommandProcessor>().Named(RemoveCommentCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RemoveVehicleCommandProcessor>().Named(RemoveVehicleCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<ShowUsersCommandProcessor>().Named(ShowUsersCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<ShowVehiclesCommandProcessor>().Named(ShowVehiclesCommandProcessorName);

            Bind<IChainableCommandProcessor>().To<AuthorizedCommandProcessor>().Named(AuthorizedCommandProcessorName);

            Bind<ICommandProcessor>().ToMethod(context =>
            {
                var registerCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(RegisterCommandProcessorName);
                var loginCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(LoginCommandProcessorName);
                var authorizedLogoutCommandProcessor = this.GetAuthorizedCommandProcessor(context, LogoutCommandProcessorName);
                var authorizedAddVehicleCommandProcessor = this.GetAuthorizedCommandProcessor(context, AddVehicleCommandProcessorName);
                var authorizedRemoveVehicleCommandProcessor = this.GetAuthorizedCommandProcessor(context, RemoveVehicleCommandProcessorName);
                var authorizedAddCommentCommandProcessor = this.GetAuthorizedCommandProcessor(context, AddCommentCommandProcessorName);
                var authorizedRemoveCommentCommandProcessor = this.GetAuthorizedCommandProcessor(context, RemoveCommentCommandProcessorName);
                var authorizedShowUsersCommandProcessor = this.GetAuthorizedCommandProcessor(context, ShowUsersCommandProcessorName);
                var authorizedShowVehiclesCommandProcessor = this.GetAuthorizedCommandProcessor(context, ShowVehiclesCommandProcessorName);

                registerCommandProcessor.Successor = loginCommandProcessor;
                loginCommandProcessor.Successor = authorizedLogoutCommandProcessor;
                authorizedLogoutCommandProcessor.Successor = authorizedAddVehicleCommandProcessor;
                authorizedAddVehicleCommandProcessor.Successor = authorizedRemoveVehicleCommandProcessor;
                authorizedRemoveVehicleCommandProcessor.Successor = authorizedAddCommentCommandProcessor;
                authorizedAddCommentCommandProcessor.Successor = authorizedRemoveCommentCommandProcessor;
                authorizedRemoveCommentCommandProcessor.Successor = authorizedShowUsersCommandProcessor;
                authorizedShowUsersCommandProcessor.Successor = authorizedShowVehiclesCommandProcessor;
                authorizedShowVehiclesCommandProcessor.Successor = null;

                return registerCommandProcessor;
            });
        }

        private IChainableCommandProcessor GetAuthorizedCommandProcessor(IContext context, string commandProcessorName)
        {
            return context.Kernel.Get<IChainableCommandProcessor>(
                    AuthorizedCommandProcessorName,
                    new ConstructorArgument(CommanProcessorContructorArgumentName, context.Kernel.Get<IChainableCommandProcessor>(commandProcessorName)),
                    new ConstructorArgument(UserServiceContructorArgumentName, context.Kernel.Get<IUserService>()));
        }
    }
}
