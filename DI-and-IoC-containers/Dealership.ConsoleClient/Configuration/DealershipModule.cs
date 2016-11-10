using Dealership.CommandProcessors;
using Dealership.ConsoleClient.Common;
using Dealership.ConsoleClient.Interceptors;
using Dealership.Contracts;
using Dealership.Engine;
using Dealership.Models;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Interception.Infrastructure.Language;
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
        private const string CarName = "Car";
        private const string MotorcycleName = "Motorcycle";
        private const string TruckName = "Truck";

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

            Bind<CommandProcessor>().To<AddCommentCommandProcessor>().Named(AddCommentCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<AddVehicleCommandProcessor>().Named(AddVehicleCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<LoginCommandProcessor>().Named(LoginCommandProcessorName);
            Bind<CommandProcessor>().To<LogoutCommandProcessor>().Named(LogoutCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<RegisterCommandProcessor>().Named(RegisterCommandProcessorName);
            Bind<CommandProcessor>().To<RemoveCommentCommandProcessor>().Named(RemoveCommentCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<RemoveVehicleCommandProcessor>().Named(RemoveVehicleCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<ShowUsersCommandProcessor>().Named(ShowUsersCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<CommandProcessor>().To<ShowVehiclesCommandProcessor>().Named(ShowVehiclesCommandProcessorName);//.Intercept().With<UserAuthorizationInterceptor>();
            Bind<ICommandProcessor>().ToMethod(context =>
            {
                var registerCommandProcessor = context.Kernel.Get<CommandProcessor>(RegisterCommandProcessorName);
                var loginCommandProcessor = context.Kernel.Get<CommandProcessor>(LoginCommandProcessorName);
                var logoutCommandProcessor = context.Kernel.Get<CommandProcessor>(LogoutCommandProcessorName);
                var addVehicleCommandProcessor = context.Kernel.Get<CommandProcessor>(AddVehicleCommandProcessorName);
                var removeVehicleCommandProcessor = context.Kernel.Get<CommandProcessor>(RemoveVehicleCommandProcessorName);
                var addCommentCommandProcessor = context.Kernel.Get<CommandProcessor>(AddCommentCommandProcessorName);
                var removeCommentCommandProcessor = context.Kernel.Get<CommandProcessor>(RemoveCommentCommandProcessorName);
                var showUsersCommandProcessor = context.Kernel.Get<CommandProcessor>(ShowUsersCommandProcessorName);
                var showVehiclesCommandProcessor = context.Kernel.Get<CommandProcessor>(ShowVehiclesCommandProcessorName);

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
    }
}
