﻿
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Snapshot;


namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco's NuCache
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco NuCache dependencies
    /// </summary>
    public static IUmbracoBuilder AddUmbracoHybridCache(this IUmbracoBuilder builder)
    {
        builder.Services.AddHybridCache();
        builder.Services.AddUnique<IPublishedContentTypeCacheAccessor, PublishedContentTypeTypeCacheAccessor>();
        builder.Services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        builder.Services.AddSingleton<IPublishedContentCache, ContentCache>();
        builder.Services.AddSingleton<IPublishedMediaCache, MediaCache>();
        builder.Services.AddTransient<IPublishedMemberCache, MemberCache>();
        builder.Services.AddSingleton<IDomainCache>(serviceProvider =>
        {
            IDefaultCultureAccessor defaultCultureAccessor = serviceProvider.GetRequiredService<IDefaultCultureAccessor>();
            var domainStore = new SnapDictionary<int, Domain>();

            // TODO: Remove all this snapshotting stuff from domain cache.
            SnapDictionary<int, Domain>.Snapshot domainSnap = domainStore.CreateSnapshot();
            var defaultCulture = defaultCultureAccessor.DefaultCulture;
            return new DomainCache(domainSnap, defaultCulture);
        });

        builder.Services.AddSingleton<IElementsCache, ElementsDictionaryAppCache>();
        builder.Services.AddSingleton<IContentCacheService, ContentCacheService>();
        builder.Services.AddSingleton<IMediaCacheService, MediaCacheService>();
        builder.Services.AddTransient<IMemberCacheService, MemberCacheService>();
        builder.Services.AddTransient<IPublishedContentFactory, PublishedContentFactory>();
        builder.Services.AddTransient<ICacheNodeFactory, CacheNodeFactory>();
        builder.Services.AddSingleton<ICacheManager, CacheManager>();
        builder.Services.AddSingleton<IContentCacheDataSerializerFactory>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();
            switch (options.Value.NuCacheSerializerType)
            {
                case NuCacheSerializerType.JSON:
                    return new JsonContentNestedDataSerializerFactory();
                case NuCacheSerializerType.MessagePack:
                    return ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(s);
                default:
                    throw new IndexOutOfRangeException();
            }
        });
        builder.Services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        builder.AddNotificationAsyncHandler<ContentRefreshNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaRefreshNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, SeedingNotificationHandler>();
        return builder;
    }
}
