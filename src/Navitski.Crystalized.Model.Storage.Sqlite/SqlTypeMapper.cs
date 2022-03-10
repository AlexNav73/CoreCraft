namespace Navitski.Crystalized.Model.Storage.Sqlite;

internal static class SqlTypeMapper
{
    private const string _integerType = "INTEGER";
    private const string _realType = "REAL";
    private const string _textType = "TEXT";
    private const string _blobType = "BLOB";

    public static string DbTypeName(Type type)
    {
        if (_types.TryGetValue(type, out var dbType))
        {
            return dbType;
        }

        throw new NotSupportedException($"Type [{type}] is not supported");
    }

    private static readonly IDictionary<Type, string> _types = new Dictionary<Type, string>()
    {
        { typeof(string), _textType },
        { typeof(byte[]), _blobType },
        { typeof(bool), _integerType },
        { typeof(byte), _integerType },
        { typeof(char), _textType },
        { typeof(int), _integerType },
        { typeof(long), _integerType },
        { typeof(sbyte), _integerType },
        { typeof(short), _integerType },
        { typeof(uint), _integerType },
        { typeof(ulong), _integerType },
        { typeof(ushort), _integerType },
        { typeof(DateTime), _textType },
        { typeof(DateTimeOffset), _textType },
        { typeof(TimeSpan), _textType },
        { typeof(decimal), _textType },
        { typeof(double), _realType },
        { typeof(float), _realType },
        { typeof(Guid), _textType }
    };
}
