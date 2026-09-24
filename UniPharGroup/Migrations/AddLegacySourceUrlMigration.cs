using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UniPharGroup.Migrations;

public class AddLegacySourceUrlMigration : AsyncMigrationBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IShortStringHelper _shortStringHelper;

    public AddLegacySourceUrlMigration(
        IMigrationContext context,
        IContentTypeService contentTypeService,
        IShortStringHelper shortStringHelper) : base(context)
    {
        _contentTypeService = contentTypeService;
        _shortStringHelper = shortStringHelper;
    }

    protected override async Task MigrateAsync()
    {
        var standardPage = _contentTypeService.Get("standardPage");

        if (standardPage == null)
        {
            Logger.LogWarning("standardPage content type not found - skipping migration");
            return;
        }

        if (standardPage.PropertyTypeExists("legacySourceUrl"))
        {
            Logger.LogInformation("legacySourceUrl already exists - skipping");
            return;
        }

        var propertyType = new PropertyType(_shortStringHelper, "Umbraco.TextBox", ValueStorageType.Nvarchar)
        {
            Alias = "legacySourceUrl",
            Name = "Legacy Source URL",
            Mandatory = false
        };

        var added = standardPage.AddPropertyType(propertyType, "contentTab");

        if (!added)
        {
            Logger.LogWarning("AddPropertyType returned false - property group 'Content' likely not found on standardPage");
            return;
        }

        var result = await _contentTypeService.UpdateAsync(standardPage, Constants.Security.SuperUserKey);

        if (!result.Success)
        {
            Logger.LogWarning("Failed to update standardPage content type: {Status}", result.Result);
        }
    }
}

public class AddLegacySourceUrlMigrationFix : AsyncMigrationBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IShortStringHelper _shortStringHelper;

    public AddLegacySourceUrlMigrationFix(
        IMigrationContext context,
        IContentTypeService contentTypeService,
        IShortStringHelper shortStringHelper) : base(context)
    {
        _contentTypeService = contentTypeService;
        _shortStringHelper = shortStringHelper;
    }

    protected override async Task MigrateAsync()
    {
        var standardPage = _contentTypeService.Get("standardPage");

        if (standardPage == null)
        {
            Logger.LogWarning("standardPage content type not found - skipping migration");
            return;
        }

        if (standardPage.PropertyTypeExists("legacySourceUrl"))
        {
            Logger.LogInformation("legacySourceUrl already exists - skipping");
            return;
        }

        var propertyType = new PropertyType(_shortStringHelper, "Umbraco.TextBox", ValueStorageType.Nvarchar)
        {
            Alias = "legacySourceUrl",
            Name = "Legacy Source URL",
            Mandatory = false
        };

        var added = standardPage.AddPropertyType(propertyType, "Content");

        if (!added)
        {
            Logger.LogWarning("AddPropertyType returned false - property group 'Content' likely not found on standardPage");
            return;
        }

        var result = await _contentTypeService.UpdateAsync(standardPage, Constants.Security.SuperUserKey);

        if (!result.Success)
        {
            Logger.LogWarning("Failed to update standardPage content type: {Status}", result.Result);
        }
    }
}

public class AddLegacySourceUrlMigrationFix2 : AsyncMigrationBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IShortStringHelper _shortStringHelper;

    public AddLegacySourceUrlMigrationFix2(
        IMigrationContext context,
        IContentTypeService contentTypeService,
        IShortStringHelper shortStringHelper) : base(context)
    {
        _contentTypeService = contentTypeService;
        _shortStringHelper = shortStringHelper;
    }

    protected override async Task MigrateAsync()
    {
        var standardPage = _contentTypeService.Get("standardPage");

        if (standardPage == null)
        {
            Logger.LogWarning("standardPage content type not found - skipping migration");
            return;
        }

        if (standardPage.PropertyTypeExists("legacySourceUrl"))
        {
            Logger.LogInformation("legacySourceUrl already exists - skipping");
            return;
        }

        var propertyType = new PropertyType(_shortStringHelper, "Umbraco.TextBox", ValueStorageType.Nvarchar)
        {
            Alias = "legacySourceUrl",
            Name = "Legacy Source URL",
            Mandatory = false
        };

        var added = standardPage.AddPropertyType(propertyType, "contentTab");

        if (!added)
        {
            Logger.LogWarning("AddPropertyType still returned false - investigate further");
            return;
        }

        var result = await _contentTypeService.UpdateAsync(standardPage, Constants.Security.SuperUserKey);

        if (!result.Success)
        {
            Logger.LogWarning("Failed to update standardPage content type: {Status}", result.Result);
        }
        else
        {
            Logger.LogInformation("legacySourceUrl property successfully added to standardPage");
        }
    }
}



public class RegisterLegacyRedirectsMigration : AsyncMigrationBase
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IRedirectUrlService _redirectUrlService;

    public RegisterLegacyRedirectsMigration(
        IMigrationContext context,
        IContentService contentService,
        IContentTypeService contentTypeService,
        IRedirectUrlService redirectUrlService) : base(context)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _redirectUrlService = redirectUrlService;
    }

    protected override Task MigrateAsync()
    {
        var standardPageType = _contentTypeService.Get("standardPage");
        if (standardPageType == null)
        {
            Logger.LogWarning("standardPage content type not found - skipping redirect registration");
            return Task.CompletedTask;
        }

        var pages = _contentService.GetPagedOfType(
            standardPageType.Id,
            pageIndex: 0,
            pageSize: 500,
            out long totalRecords,
            filter: null,
            ordering: null);

        var aboutUsPage = pages.FirstOrDefault(c => c.Key == new Guid("db5acdd4-ea38-4ed6-88b7-a04b7ac73bd5"));

        if (aboutUsPage == null)
        {
            Logger.LogWarning("About Us page (Key: ...) not found - skipping redirect registration");
            return Task.CompletedTask;
        }

        _redirectUrlService.Register("/old-site/company-info.html", aboutUsPage.Key, "en-US");

        Logger.LogInformation("Registered legacy redirect: /old-site/company-info.html -> {Name}", aboutUsPage.Name);

        return Task.CompletedTask;
    }
}