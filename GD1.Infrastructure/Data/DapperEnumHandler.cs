using Dapper;
using System.Data;

namespace GD1.Infrastructure.Data
{
    public class DapperEnumHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
    {
        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value.ToString();
        }

        public override T Parse(object value)
        {
            if (value == null || value == DBNull.Value)
                return default;

            if (Enum.TryParse<T>(value.ToString(), true, out var result))
            {
                return result;
            }

            return default;
        }
    }
}
