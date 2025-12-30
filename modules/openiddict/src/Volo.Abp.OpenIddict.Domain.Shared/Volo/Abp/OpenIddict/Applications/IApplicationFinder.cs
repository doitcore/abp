using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.OpenIddict.Applications;

public interface IApplicationFinder
{
    Task<List<ApplicationFinderResult>> SearchAsync(string filter, int page = 1);

    Task<List<ApplicationFinderResult>> SearchByIdsAsync(Guid[] ids);
}
