using SoftOne.Soe.Data;
using System;
using System.Threading;
using System.Web;

namespace SoftOne.Soe.Business
{
    public static class SysEntitiesProvider
    {
        public const string SCOPED_READONLY_KEY = "SysEntities.ReadOnly";
        private static readonly AsyncLocal<SOESysEntities> _taskScoped = new();

        public static SOESysEntities LeaseReadOnlyContext()
        {
            if (_taskScoped.Value is { IsDisposed: false } scoped)
                return scoped;

            if (TryGetEntitiesFromHttpContext(HttpContext.Current, out var entities))
                return entities;

            var created = Sys.CreateSOESysEntities(true);
            return created;
        }

        private static bool TryGetEntitiesFromHttpContext(HttpContext http, out SOESysEntities entities)
        {
            entities = null;
            if (http?.Items != null && http.Items[SCOPED_READONLY_KEY] is SOESysEntities existing)
            {
                if (existing.ThreadId == Thread.CurrentThread.ManagedThreadId && !existing.IsDisposed)
                {
                    entities = existing;
                    return true;
                }
            }
            return false;
        }


        public static SOESysEntities CreateOnBeginRequest()
        {
            var http = HttpContext.Current;
            if (http?.Items == null)
                throw new InvalidOperationException("No HttpContext available to create a request-scoped SOESysEntities.");

            if (TryGetEntitiesFromHttpContext(http, out var existing))
                return existing; // this should never happen, but just in case

            var created = Sys.CreateSOESysEntities(true, true);
            http.Items[SCOPED_READONLY_KEY] = created; // safe: we just checked missing
            return created;
        }


        public static bool DisposeOnRequestEnd()
        {
            var http = HttpContext.Current;
            if (TryGetEntitiesFromHttpContext(http, out var entities))
            {
                entities.DisposeNow();
                http.Items.Remove(SCOPED_READONLY_KEY);
                return true;
            }
            return false;
        }

        public static SOESysEntities CreateTaskScoped()
        {
            if (_taskScoped.Value is { IsDisposed: false } existing)
                return existing;

            var created = Sys.CreateSOESysEntities(true, false);
            created.IsTaskScoped = true;
            _taskScoped.Value = created;
            return created;
        }

        public static void DisposeTaskScoped(SOESysEntities entities)
        {
            if (entities.IsTaskScoped)
            {
                if (ReferenceEquals(_taskScoped.Value, entities))
                    _taskScoped.Value = null;
                entities.DisposeNow();
            }
        }

        public static SOESysEntities Create()
        {
            return Sys.CreateSOESysEntities(false);
        }
    }
}
