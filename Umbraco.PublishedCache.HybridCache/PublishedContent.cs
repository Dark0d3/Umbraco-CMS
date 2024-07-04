﻿using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class PublishedContent : PublishedContentBase
{
    private readonly IPublishedSnapshotAccessor? _publishedSnapshotAccessor;
    private readonly IVariationContextAccessor? _variationContextAccessor;
    private readonly IPublishedModelFactory? _publishedModelFactory;
    private readonly ContentData _contentData;
    private readonly ContentNode _contentNode;
    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;
    private readonly string? _urlSegment;

    // [JsonConstructor]
    public PublishedContent(
        ContentNode contentNode,
        ContentData contentData,
        IPublishedSnapshotAccessor? publishedSnapshotAccessor,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedModelFactory? publishedModelFactory)
        : base(variationContextAccessor)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;
        _contentNode = contentNode;

        _contentData = contentData ?? throw new ArgumentNullException(nameof(contentData));
        _urlSegment = _contentData.UrlSegment;

        // // TODO: Implement properties
        // var properties = new IPublishedProperty[_contentNode.ContentType.PropertyTypes.Count()];
        // var i = 0;
        // foreach (IPublishedPropertyType propertyType in _contentNode.ContentType.PropertyTypes)
        // {
        //     // add one property per property type - this is required, for the indexing to work
        //     // if contentData supplies pdatas, use them, else use null
        //     contentData.Properties.TryGetValue(propertyType.Alias, out PropertyData[]? pdatas); // else will be null
        //     properties[i++] = new Property(propertyType, this, pdatas, _publishedSnapshotAccessor, propertyType.CacheLevel);
        // }
        //
        // Properties = properties;
    }

    public override IPublishedContentType ContentType => _contentNode.ContentType;

    public override Guid Key { get; }

    public override IEnumerable<IPublishedProperty> Properties { get; } = null!;

    public override IPublishedProperty? GetProperty(string alias) => throw new NotImplementedException();

    public override int Id { get; }

    public override int SortOrder { get; }

    public override int Level { get; }

    public override string Path => _contentNode.Path;

    public override int? TemplateId { get; }

    public override int CreatorId { get; }

    public override DateTime CreateDate { get; }

    public override int WriterId { get; }

    public override DateTime UpdateDate { get; }

    public bool IsPreviewing { get; }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, PublishedCultureInfo> Cultures
    {
        get
        {
            if (_cultures != null)
            {
                return _cultures;
            }

            if (!ContentType.VariesByCulture())
            {
                return _cultures = new Dictionary<string, PublishedCultureInfo>
                {
                    { string.Empty, new PublishedCultureInfo(string.Empty, _contentData.Name, _urlSegment, CreateDate) },
                };
            }

            if (_contentData.CultureInfos == null)
            {
                throw new PanicException("_contentDate.CultureInfos is null.");
            }

            return _cultures = _contentData.CultureInfos
                .ToDictionary(
                    x => x.Key,
                    x => new PublishedCultureInfo(x.Key, x.Value.Name, x.Value.UrlSegment, x.Value.Date),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    public override PublishedItemType ItemType { get; }

    public override bool IsDraft(string? culture = null) => throw new NotImplementedException();

    public override bool IsPublished(string? culture = null) => throw new NotImplementedException();

    public override IEnumerable<IPublishedContent> ChildrenForAllCultures { get; } = null!;

    public override IPublishedContent? Parent { get; } = null!;
}
