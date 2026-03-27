using System;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public static class TemplateFactory
    {
        public static ITemplateFactory<T> Create<T>(Func<T> creator) where T : Widget
        {
            return new TemplateFactoryInternal<T>(creator);
        }

        private class TemplateFactoryInternal<T> : ITemplateFactory<T> where T : Widget
        {
            private readonly Func<T> _creator;

            public TemplateFactoryInternal(Func<T> creator)
            {
                _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            }

            public T Create()
            {
                return _creator.Invoke();
            }
        }
    }
}
