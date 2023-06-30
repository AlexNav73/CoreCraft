namespace CoreCraft.Storage.Sqlite.Tests;

public class SqlTypeMapperTests
{
    [Test]
    [TestCase(typeof(string), "TEXT")]
    [TestCase(typeof(byte[]), "BLOB")]
    [TestCase(typeof(bool), "INTEGER")]
    [TestCase(typeof(byte), "INTEGER")]
    [TestCase(typeof(char), "TEXT")]
    [TestCase(typeof(int), "INTEGER")]
    [TestCase(typeof(long), "INTEGER")]
    [TestCase(typeof(sbyte), "INTEGER")]
    [TestCase(typeof(short), "INTEGER")]
    [TestCase(typeof(uint), "INTEGER")]
    [TestCase(typeof(ulong), "INTEGER")]
    [TestCase(typeof(ushort), "INTEGER")]
    [TestCase(typeof(DateTime), "TEXT")]
    [TestCase(typeof(DateTimeOffset), "TEXT")]
    [TestCase(typeof(TimeSpan), "TEXT")]
    [TestCase(typeof(decimal), "TEXT")]
    [TestCase(typeof(double), "REAL")]
    [TestCase(typeof(float), "REAL")]
    [TestCase(typeof(Guid), "TEXT")]
    public void DbTypeNameTest(Type type, string sqliteType)
    {
        Assert.That(SqlTypeMapper.DbTypeName(type), Is.EqualTo(sqliteType));
    }

    [Test]
    public void DbTypeNameUnsupportedTypeTest()
    {
        Assert.Throws<NotSupportedException>(() => SqlTypeMapper.DbTypeName(typeof(SqlTypeMapperTests)));
    }
}
