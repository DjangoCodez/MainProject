using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Soe.WebApi.Middlewares
{
    public class CleanerMiddleware : OwinMiddleware
    {
        public CleanerMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            throw new NotImplementedException();
        }
    }
}