using NUnit.Framework;

using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Quartz.Spi;
using Quartz.Tests.Integration.Utils;

namespace Quartz.Tests.Integration;

[TestFixture(typeof(BinaryObjectSerializer), TestConstants.DefaultSqlServerProvider, Category = "db-sqlserver")]
[TestFixture(typeof(NewtonsoftJsonObjectSerializer), TestConstants.DefaultSqlServerProvider, Category = "db-sqlserver")]
[TestFixture(typeof(BinaryObjectSerializer), TestConstants.PostgresProvider, Category = "db-postgres")]
[TestFixture(typeof(NewtonsoftJsonObjectSerializer), TestConstants.PostgresProvider, Category = "db-postgres")]
public class AdoSchedulerTest : AbstractSchedulerTest
{
    private readonly IObjectSerializer serializer;

    public AdoSchedulerTest(Type serializerType, string provider) : base(provider, serializerType.Name)
    {
        serializer = (IObjectSerializer) Activator.CreateInstance(serializerType);
        serializer.Initialize();
    }

    protected override async ValueTask<IScheduler> CreateScheduler(string name, int threadPoolSize)
    {
        DatabaseHelper.RegisterDatabaseSettingsForProvider(provider, out var driverDelegateType);

        var jobStore = new JobStoreTX
        {
            DataSource = "default",
            TablePrefix = "QRTZ_",
            InstanceId = "AUTO",
            DriverDelegateType = driverDelegateType,
            ObjectSerializer = serializer
        };

        var schedulerName = CreateSchedulerName(name);
        await DirectSchedulerFactory.Instance.CreateScheduler(schedulerName, "AUTO", new DefaultThreadPool(), jobStore);
        return await SchedulerRepository.Instance.Lookup(schedulerName);
    }
}