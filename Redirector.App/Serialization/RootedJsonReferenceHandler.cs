using System.Text.Json.Serialization;

namespace Redirector.App.Serialization
{
    public class RootedJsonReferenceHandler : ReferenceHandler
    {
        public RootedJsonReferenceHandler() => Reset();

        private ReferenceResolver _rootedResolver;
        public override ReferenceResolver CreateResolver() => _rootedResolver;
        public void Reset() => _rootedResolver = new RootedJsonReferenceResolver();
    }
}
