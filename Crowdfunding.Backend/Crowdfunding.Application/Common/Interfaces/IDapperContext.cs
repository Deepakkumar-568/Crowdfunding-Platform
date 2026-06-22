using System.Data;

namespace Crowdfunding.Application.Common.Interfaces;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}
