using System.Data;

namespace Antigravity.Application.Common.Interfaces;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}
