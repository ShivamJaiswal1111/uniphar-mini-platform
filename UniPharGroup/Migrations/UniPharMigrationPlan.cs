using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace UniPharGroup.Migrations;

public class UniPharMigrationPlan : MigrationPlan
{
    public UniPharMigrationPlan() : base("UniPharGroup")
    {
        From(string.Empty)
            .To<AddLegacySourceUrlMigration>("add-legacy-source-url")
            .To<AddLegacySourceUrlMigrationFix>("add-legacy-source-url-fix")
            .To<AddLegacySourceUrlMigrationFix2>("add-legacy-source-url-fix-2")
            .To<RegisterLegacyRedirectsMigration>("register-legacy-redirects");
    }
}

public class UniPharMigrationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, UniPharMigrationNotificationHandler>();
    }
}

public class UniPharMigrationNotificationHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IKeyValueService _keyValueService;
    private readonly ICoreScopeProvider _scopeProvider;

    public UniPharMigrationNotificationHandler(
        IMigrationPlanExecutor migrationPlanExecutor,
        IKeyValueService keyValueService,
        ICoreScopeProvider scopeProvider)
    {
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _scopeProvider = scopeProvider;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        var plan = new UniPharMigrationPlan();

        using var scope = _scopeProvider.CreateCoreScope();
        var upgrader = new Upgrader(plan);
        upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
        scope.Complete();
    }
}