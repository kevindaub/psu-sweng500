﻿namespace Sweng500.Pml.DataAccessLayer
{
    using System;
    using Microsoft.Practices.ServiceLocation;
    using Ninject;

    /// <summary>
    /// Static class used to set up all the various services used for dependency injection
    /// </summary>
    public sealed class Repository : IDisposable
    {
        /// <summary>
        /// Singleton instance of the repository
        /// </summary>
        private static readonly Repository StaticInstance = new Repository();

        /// <summary>
        /// Initializes static members of the Repository class
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1409:RemoveUnnecessaryCode",  Justification = "Explicit static constructor to tell C# compiler not to mark type as beforefieldinit")]
        static Repository()
        {
        }

        /// <summary>
        /// Prevents a default instance of the Repository class from being created except by the singleton
        /// </summary>
        private Repository()
        {
            // Create the concrete kernel
            var kernel = new StandardKernel();

            // Add the various concrete services
            kernel.Bind<ICrudService>().To<EntityCrudService>().InSingletonScope();
            kernel.Bind<ISearchMediaService>().To<SearchMediaService>().InSingletonScope();

            // Create the concrete locator
            var locator = new CommonServiceLocator.NinjectAdapter.NinjectServiceLocator(kernel);

            // Set the delegate that retrieves the service locator
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => locator);
        }

        /// <summary>
        /// Gets the singleton of the repository
        /// </summary>
        public static Repository Instance
        {
            get
            {
                return StaticInstance;
            }
        }

        /// <summary>
        /// Gets the service locator
        /// </summary>
        public IServiceLocator ServiceLocator
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current;
            }
        }

        /// <summary>
        /// Dispose of all the disposable interfaces
        /// </summary>
        public void Dispose()
        {
        }
    }
}
