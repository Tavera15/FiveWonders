using FiveWonders.core.Contracts;
using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using FiveWonders.DataAccess.SQL;
using FiveWonders.Services;
using System;

using Unity;

namespace FiveWonders.WebUI
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<IRepository<Product>, SQLRepository<Product>>();
            container.RegisterType<IRepository<Category>, SQLRepository<Category>>();
            container.RegisterType<IRepository<SubCategory>, SQLRepository<SubCategory>>();
            container.RegisterType<IRepository<SizeChart>, SQLRepository<SizeChart>>();
            container.RegisterType<IRepository<ServicesMessage>, SQLRepository<ServicesMessage>>();
            container.RegisterType<IRepository<Basket>, SQLRepository<Basket>>();
            container.RegisterType<IRepository<BasketItem>, SQLRepository<BasketItem>>();
            container.RegisterType<IRepository<Customer>, SQLRepository<Customer>>();
            container.RegisterType<IRepository<FWonderOrder>, SQLRepository<FWonderOrder>>();
            container.RegisterType<IRepository<OrderItem>, SQLRepository<OrderItem>>();
            container.RegisterType<IRepository<HomePage>, SQLRepository<HomePage>>();
            container.RegisterType<IRepository<GalleryImg>, SQLRepository<GalleryImg>>();
            container.RegisterType<IRepository<ServicePage>, SQLRepository<ServicePage>>();
            container.RegisterType<IBasketServices, BasketServices>();
            container.RegisterType<IInstagramService, InstagramService>();
        }
    }
}