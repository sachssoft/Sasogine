using System;

namespace Sachssoft.Sasogine.Common
{
    public class Template<T> : ITemplate where T : class
    {
        private readonly TemplateType _templateType;
        private readonly Func<T>? _factory;
        private readonly IFactoryRegistry? _referenceRegistry;
        private readonly string? _targetId;
        private readonly Type? _targetType;

        private enum TemplateType
        {
            Factory,
            Registry
        }

        public Template(Func<T> factory)
        {
            _templateType = TemplateType.Factory;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Template(IFactoryRegistry registry, string targetId, Type targetType)
        {
            _templateType = TemplateType.Registry;
            _referenceRegistry = registry ?? throw new ArgumentNullException(nameof(registry));
            _targetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
            _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        }

        public virtual T Create()
        {
            switch (_templateType)
            {
                case TemplateType.Factory:
                    return _factory?.Invoke() ?? throw new InvalidOperationException("Factory delegate is null");
                case TemplateType.Registry:
                    return _referenceRegistry?.Create(_targetId!, _targetType!) as T
                           ?? throw new InvalidOperationException("Reference registry or target info is null");
                default:
                    throw new NotSupportedException($"Creation mode {_templateType} is not supported");
            }
        }

        object ITemplate.Create() => Create();
    }
}