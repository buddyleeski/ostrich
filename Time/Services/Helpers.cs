using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Ostrich.Tracker.Model;

namespace Ostrich.Tracker.Services
{
    public class Helpers
    {
        [ThreadStatic]
        private static ISessionFactory factory;

        private static ISessionFactory GetFactory()
        {
            if (factory == null)
            {
                var config = Fluently.Configure();
                var db = config.Database(MySQLConfiguration.Standard.ConnectionString(c => c.Server("izabela.co").Database("izabel6_OstrichProjects").Username("izabel6_dev").Password("france3uk1")));
                var mappings = db.Mappings(m => m.FluentMappings.AddFromAssemblyOf<User>());
                //var configs2 = mappings.ExposeConfiguration(cfg => new SchemaExport(cfg));
                factory = mappings.BuildSessionFactory();
            }

            return factory;
        }

        private static ISession GetSession()
        {
            return GetFactory().OpenSession();
        }

        public static void Transaction(Action<ISession> work)
        {
            using (var s = GetSession())
            {
                using (var t = s.BeginTransaction())
                {
                    work(s);

                    t.Commit();
                }
            }
        }

        public static void Transaction(Action<ISession, ITransaction> work)
        {
            using (var s = GetSession())
            {
                using (var t = s.BeginTransaction())
                {
                    work(s, t);

                    t.Commit();
                }
            }
        }

        public static void NonTransact(Action<ISession> work)
        {
            using (var s = GetSession())
            {
                work(s);
            }
        }

        public static T NonTransact<T>(Func<ISession, T> work)
        {
            using (var s = GetSession())
            {
                return work(s);
            }
        }
    }
}
