using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.CRM
{
    public interface ICrmServiceFactory
    {
        IOrganizationService Create();
    }
}
