using Dealership.CommandProcessors;
using Dealership.ConsoleClient.Common;
using Dealership.Contracts;
using Dealership.Engine;
using Dealership.Models;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Modules;
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
            Kernel.InterceptReplace<AddCommentCommandProcessor>(x => x.Process(null), invocation => this.AuthorizeUserAspect(invocation, Kernel.Get<IUserService>()));
            Bind<IChainableCommandProcessor>().To<AddVehicleCommandProcessor>().Named(AddVehicleCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<LoginCommandProcessor>().Named(LoginCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<LogoutCommandProcessor>().Named(LogoutCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RegisterCommandProcessor>().Named(RegisterCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RemoveCommentCommandProcessor>().Named(RemoveCommentCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<RemoveVehicleCommandProcessor>().Named(RemoveVehicleCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<ShowUsersCommandProcessor>().Named(ShowUsersCommandProcessorName);
            Bind<IChainableCommandProcessor>().To<ShowVehiclesCommandProcessor>().Named(ShowVehiclesCommandProcessorName);
            Bind<ICommandProcessor>().ToMethod(context =>
            {
                var registerCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(RegisterCommandProcessorName);
                var loginCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(LoginCommandProcessorName);
                var logoutCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(LogoutCommandProcessorName);
                var addVehicleCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(AddVehicleCommandProcessorName);
                var removeVehicleCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(RemoveVehicleCommandProcessorName);
                var addCommentCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(AddCommentCommandProcessorName);
                var removeCommentCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(RemoveCommentCommandProcessorName);
                var showUsersCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(ShowUsersCommandProcessorName);
                var showVehiclesCommandProcessor = context.Kernel.Get<IChainableCommandProcessor>(ShowVehiclesCommandProcessorName);

                registerCommandProcessor.Successor = loginCommandProcessor;
                loginCommandProcessor.Successor = logoutCommandProcessor;
                logoutCommandProcessor.Successor = addVehicleCommandProcessor;
                addVehicleCommandProcessor.Successor = removeVehicleCommandProcessor;
                removeVehicleCommandProcessor.Successor = addCommentCommandProcessor;
                addCommentCommandProcessor.Successor = removeCommentCommandProcessor;
                removeCommentCommandProcessor.Successor = showUsersCommandProcessor;
                showUsersCommandProcessor.Successor = showVehiclesCommandProcessor;
                showVehiclesCommandProcessor.Successor = null;

                return registerCommandProcessor;
            });
        }

        private void AuthorizeUserAspect(IInvocation invocation, IUserService userService)
        {
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
