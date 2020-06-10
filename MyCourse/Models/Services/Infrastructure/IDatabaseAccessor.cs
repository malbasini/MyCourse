using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MyCourse.Models.ViewModels;

namespace MyCourse.Models.Services.Infrastructure
{
    public interface IDatabaseAccessor
    {
        Task<DataSet> QueryAsync(FormattableString query);
    }
}