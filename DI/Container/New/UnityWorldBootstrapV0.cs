//using Sirenix.Utilities;
//using System;
//using System.Linq;
//using System.Reflection;

//namespace BB.Di
//{
//    public static class UnityWorldBootstrapV0
//    {
//        public static void InitWorld()
//        {
//            if (World.Entity)
//                return;

//            var factoryType = Assembly
//                .GetExecutingAssembly()
//                .DefinedTypes
//                .Where(
//                    t => t.ImplementsOrInherits(typeof(IAutomaticWorldFactory))
//                    && !t.IsAbstract)
//                .FirstOrDefault();

//            if (factoryType is null)
//                throw new DiException(
//                    $"Could not resolve any classes that inherit {typeof(IAutomaticWorldFactory).FullName}." +
//                    $"Aborting world creation.");

//            var factory = (IAutomaticWorldFactory)Activator.CreateInstance(factoryType);
//            var entity = factory.Create();
//            World.Init(entity);
//        }
//    }
//    public interface IAutomaticWorldFactory : IEntityInstaller
//    {
//        IEntity Create();
//    }
//    public abstract class BaseAutomaticWorldFactory : IAutomaticWorldFactory
//    {
//        public IEntity Create()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}