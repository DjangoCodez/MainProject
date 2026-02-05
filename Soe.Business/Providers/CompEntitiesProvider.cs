using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SoftOne.Soe.Business
{
    public static class CompEntitiesProvider
    {
        public const string SCOPED_READONLY_KEY = "CompEntities.ReadOnly";
        private static readonly AsyncLocal<CompEntities> _taskScoped = new();
        public static CompEntities LeaseReadOnlyContext()
        {
            if (_taskScoped.Value is { IsDisposed: false } scoped)
                return scoped;

            if (TryGetEntitiesFromHttpContext(HttpContext.Current, out var entities))
            {
                if (!entities.RequestScoped)
                    throw new InvalidOperationException("The existing CompEntities in HttpContext is not request-scoped and cannot be reused.");

                if (!IsDisposed(entities))
                    return entities;
            }

            var created = Comp.CreateCompEntities(true);
            return created;
        }

        private static bool IsDisposed(this CompEntities entities)
        {
            if (entities.IsDisposed)
                return true;
            return false;
        }

        public static void SetMergeOptionsOnAllTables(CompEntities entities)
        {
            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)entities).ObjectContext;
            var containers = objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace);
            foreach (var set in containers.BaseEntitySets.OfType<System.Data.Entity.Core.Metadata.Edm.EntitySet>())
            {
                var query = objectContext.CreateQuery<System.Data.Entity.Core.Objects.DataClasses.EntityObject>($"{set.Name}");
                query.NoTracking();
            }
        }

        private static bool TryGetEntitiesFromHttpContext(HttpContext http, out CompEntities entities)
        {
            entities = null;
            if (http?.Items != null && http.Items[SCOPED_READONLY_KEY] is CompEntities existing)
            {
                if (existing.ThreadId == Thread.CurrentThread.ManagedThreadId && !existing.IsDisposed)
                {
                    entities = existing;
                    return true;
                }
            }
            return false;
        }

        private static CompEntities CreateTaskScoped()
        {
            if (_taskScoped.Value is { IsDisposed: false } existing)
                return existing;

            var created = Comp.CreateCompEntities(true, false);
            created.IsTaskScoped = true;
            _taskScoped.Value = created;
            return created;
        }

        private static void DisposeTaskScoped(CompEntities entities)
        {
            if (entities.IsTaskScoped)
            {
                if (ReferenceEquals(_taskScoped.Value, entities))
                    _taskScoped.Value = null;
                entities.DisposeNow();
            }
        }

        private static async Task RunWithTaskScopedReadOnlyEntitiesAsync(Func<Task> work)
        {
            var entities = CreateTaskScoped();
            var sysEntities = SysEntitiesProvider.CreateTaskScoped();
            try
            {
                await work();
            }
            finally
            {
                SysEntitiesProvider.DisposeTaskScoped(sysEntities);
                DisposeTaskScoped(entities);
            }
        }

        public static Task RunWithTaskScopedReadOnlyEntities(Action work)
        {
            return RunWithTaskScopedReadOnlyEntitiesAsync(() =>
            {
                work();
                return Task.CompletedTask;
            });
        }

        public static CompEntities CreateOnBeginRequest()
        {
            var http = HttpContext.Current;
            if (http?.Items == null)
                throw new InvalidOperationException("No HttpContext available to create a request-scoped CompEntities.");

            if (TryGetEntitiesFromHttpContext(http, out var existing))
                return existing; // this should never happen, but just in case

            var created = Comp.CreateCompEntities(true, true);
            http.Items[SCOPED_READONLY_KEY] = created; // safe: we just checked missing
            return created;
        }

        public static bool DisposeOnRequestEnd()
        {
            var http = HttpContext.Current;
            if (TryGetEntitiesFromHttpContext(http, out var entities))
            {
                if (!entities.IsTaskScoped)
                {
                    entities.DisposeNow();
                    http.Items.Remove(SCOPED_READONLY_KEY);
                    return true;
                }
            }
            return false;
        }

        public static CompEntities Create()
        {
            return Comp.CreateCompEntities(false);
        }
    }
}
